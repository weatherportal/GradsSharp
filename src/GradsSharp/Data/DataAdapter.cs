using GradsSharp.Drawing;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data;

internal class DataAdapter : IDataAdapter
{
    private const double FUZZ_SCALE = 1e-5;
    private DrawingContext _drawingContext;

    public DataAdapter(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public double[] GetVariable(VariableDefinition definition)
    {
        if (_drawingContext.CommonData.pdf1 != null)
        {
            foreach (var dfn in _drawingContext.CommonData.pdf1)
            {
                if (dfn.abbrv == definition.VariableName)
                {
                    return dfn.pfi.rbuf;
                }
            }
        }

        if (_drawingContext.CommonData.fnum == 0)
        {
            throw new Exception("No files open yet");
        }

        if (_drawingContext.CommonData.pfid != null)
        {
            return _drawingContext.CommonData.pfid.DataReader.ReadData(_drawingContext.CommonData,
                _drawingContext.CommonData.pfid, definition);
        }

        return Array.Empty<double>();
    }

    public void DefineVariable(string name, double[] data)
    {
        _drawingContext.GradsCommandInterface.Define(name, data);
    }

    public double GetLevMin()
    {
        return _drawingContext.CommonData.dmin[2];
    }

    public double GetLevMax()
    {
        return _drawingContext.CommonData.dmax[2];
    }

    public double[] GetMultiLevelData(VariableDefinition definition, double startlevel, double endLevel,
        MultiLevelFunction function)
    {
        int sel = (int)function;
        Func<double[], double, double>? conv;
        double d2r = Math.PI / 180.0;
        double gr1 = startlevel;
        double gr2 = endLevel;
        

        GradsFile? pfi = _drawingContext.CommonData.pfid;
        int dim = 2; // always Z dimension
        conv = pfi.ab2gr[dim];
        var cvals = pfi.abvals[dim];
        gr1 = conv(cvals, gr1);
        gr2 = conv(cvals, gr2);

        if (gr2 < gr1)
        {
            throw new Exception(
                "Error:  2nd dimension expression is invalid (end grid point is less than start grid point");
        }
        
        bool bndflg = false;
        int incr = 1;


        double d1 = Math.Ceiling(gr1 - 0.001); /* Ave limits are integers    */
        double d2 = Math.Floor(gr2 + 0.001);
        double wlo = 0, whi = 0;
        if (bndflg)
        {
            d1 = Math.Floor(gr1 + 0.5);
            d2 = Math.Ceiling(gr2 - 0.5);
            if (dim != 3)
            {
                conv = pfi.gr2ab[dim];
                wlo = conv(pfi.grvals[dim], gr1);
                whi = conv(pfi.grvals[dim], gr2);
            }
        }

        double wt1 = 1.0;
        double abs = 0, alo, ahi, alen;
        double[] dmin = new double[5], dmax = new double[5];

        if (dim == 3)
        {
            // gr2t (pfi.grvals[3],d1,&(pst.tmin));
            // pst.tmax = pst.tmin;
            // if (bndflg) {
            //     rd1 = d1;
            //     if (gr1 < rd1+0.5) wt1 = (rd1+0.5)-gr1;
            //     if (gr2 > rd1-0.5) wt1 = gr2 + 0.5 - rd1;
            //     if (wt1<0.0) wt1=0.0;
            // }
        }
        /*-----  lon,lat,lev,ens */
        else
        {
            conv = pfi.gr2ab[dim];
            abs = conv(pfi.grvals[dim], d1);
            alo = conv(pfi.grvals[dim], d1 - 0.5);
            ahi = conv(pfi.grvals[dim], d1 + 0.5);
            alen = Math.Abs(ahi - alo);
            dmin[dim] = abs;
            dmax[dim] = abs;
            if (bndflg)
            {
                if (whi < wlo)
                {
                    if (alo > wlo) alo = wlo;
                    if (ahi > wlo) ahi = wlo;
                    if (alo < whi) alo = whi;
                    if (ahi < whi) ahi = whi;
                }
                else
                {
                    if (alo < wlo) alo = wlo;
                    if (ahi < wlo) ahi = wlo;
                    if (alo > whi) alo = whi;
                    if (ahi > whi) ahi = whi;
                }
            }

            /*-----  lat scaling */
            if (dim == 1)
            {
                // if (alo >  90.0) alo =  90.0;
                // if (ahi >  90.0) ahi =  90.0;
                // if (alo < -90.0) alo = -90.0;
                // if (ahi < -90.0) ahi = -90.0;
                // if (sel==1) {                                                   /* ave */
                //     wt1 = Math.Abs(sin(ahi*d2r)-sin(alo*d2r));
                // } else if (sel==2) {                                            /* mean */
                //     wt1 = Math.Abs(ahi-alo);
                // } else if (sel==3) {                                            /* sum */
                //     if (alen > FUZZ_SCALE) {
                //         wt1=Math.Abs(ahi-alo)/alen;
                //     } else {
                //         wt1=0.0;
                //     }
                // } else if (sel==4) {                                            /* sumg */
                //     wt1=1.0;
                // }
            }
            /* -----   lon,lev,ens scaling */
            else
            {
                if (sel <= 2)
                {
                    /* ave, mean */
                    wt1 = ahi - alo;
                }
                else if (sel == 3)
                {
                    /* sum */
                    if (alen > FUZZ_SCALE)
                    {
                        wt1 = Math.Abs(ahi - alo) / alen;
                    }
                    else
                    {
                        wt1 = 0.0;
                    }
                }
                else if (sel == 4)
                {
                    /* sumg */
                    wt1 = 1.0;
                }
            }
        }


        double[] pgr1 = GetVariable(new VariableDefinition()
        {
            HeightType = FixedSurfaceType.IsobaricSurface,
            VariableType = definition.VariableType,
            HeightValue = abs * 100
        });

        double d = d1 + incr;
        if (d > d2)
        {
            return pgr1;
        }

        double wt = 1.0;

        conv = pfi.gr2ab[dim];
        abs = conv(pfi.grvals[dim], d);
        alo = conv(pfi.grvals[dim], d - 0.5);
        ahi = conv(pfi.grvals[dim], d + 0.5);
        alen = Math.Abs(ahi - alo);
        dmin[dim] = abs;
        dmax[dim] = abs;
        if (bndflg)
        {
            if (whi < wlo)
            {
                if (alo > wlo) alo = wlo;
                if (ahi > wlo) ahi = wlo;
                if (alo < whi) alo = whi;
                if (ahi < whi) ahi = whi;
            }
            else
            {
                if (alo < wlo) alo = wlo;
                if (ahi < wlo) ahi = wlo;
                if (alo > whi) alo = whi;
                if (ahi > whi) ahi = whi;
            }
        }

        /* ---- lat scaling 2222222222222*/
        if (dim == 1)
        {
            if (alo > 90.0) alo = 90.0;
            if (ahi > 90.0) ahi = 90.0;
            if (alo < -90.0) alo = -90.0;
            if (ahi < -90.0) ahi = -90.0;
            if (sel == 1)
            {
                /* ave */
                wt = Math.Abs(Math.Sin(ahi * d2r) - Math.Sin(alo * d2r));
            }
            else if (sel == 2)
            {
                /* mean */
                wt = Math.Abs(ahi - alo);
            }
            else if (sel == 3)
            {
                /* sum */
                if (alen > FUZZ_SCALE)
                {
                    wt = Math.Abs(ahi - alo) / alen;
                }
                else
                {
                    wt = 0.0;
                }
            }
            else if (sel == 4)
            {
                /* sumg */
                wt = 1.0;
            }
        }
        /* ---- lon,lev,ens  scaling 2222222222222*/
        else
        {
            if (sel <= 2)
            {
                /* ave, mean */
                wt = ahi - alo;
            }
            else if (sel == 3)
            {
                /* sum */
                if (alen > FUZZ_SCALE)
                {
                    wt = Math.Abs(ahi - alo) / alen;
                }
                else
                {
                    wt = 0.0;
                }
            }
            else if (sel == 4)
            {
                /* sumg */
                wt = 1.0;
            }
        }

        double[] pgr2 = GetVariable(new VariableDefinition()
        {
            HeightType = FixedSurfaceType.IsobaricSurface,
            VariableType = definition.VariableType,
            HeightValue = abs * 100
        });

        int siz = pgr1.Length;
        int sum = 0;
        int cnt = 0;
        int sumu = 0;
        int cntu = 0;
        byte[] umask1 = new byte[siz], umask2 = new byte[siz];
        for (int j = 0; j < siz; j++)
        {
            umask1[j] = 1;
            umask2[j] = 1;
        }

        for (int i = 0; i < siz; i++)
        {
            if (sel >= 5 && sel <= 8)
            {
                if (umask1[sumu] == 0 || umask2[cntu] == 0)
                {
                    if (umask2[cntu] != 0)
                    {
                        pgr1[sum] = pgr2[cnt];
                        umask1[sumu] = 1;
                        pgr2[cnt] = d;
                    }
                    else if (umask1[sumu] != 0)
                    {
                        pgr2[cnt] = d1;
                        umask2[cntu] = 1;
                    }
                }
                else
                {
                    if (sel == 5 || sel == 7)
                    {
                        if (pgr2[cnt] < pgr1[sum])
                        {
                            pgr1[sum] = pgr2[cnt];
                            pgr2[cnt] = d;
                        }
                        else pgr2[cnt] = d1;
                    }

                    if (sel == 6 || sel == 8)
                    {
                        if (pgr2[cnt] > pgr1[sum])
                        {
                            pgr1[sum] = pgr2[cnt];
                            pgr2[cnt] = d;
                        }
                        else
                            pgr2[cnt] = d1;
                    }
                }
            }
            else
            {
                if (umask1[sumu] == 0)
                {
                    if (umask2[cntu] == 0)
                    {
                        pgr2[cnt] = 0.0;
                        umask2[cntu] = 1;
                    }
                    else
                    {
                        if (sel <= 3)
                        {
                            /* ave, mean sum */
                            pgr1[sum] = pgr2[cnt] * wt;
                            umask1[sumu] = 1;
                            pgr2[cnt] = wt;
                        }
                        else if (sel == 4)
                        {
                            /* sumg */
                            pgr1[sum] = pgr2[cnt];
                            umask1[sumu] = 1;
                        }
                    }
                }
                else if (umask2[cntu] == 0 && (sel <= 3))
                {
                    /* ave, mean sum */
                    pgr2[cnt] = wt1;
                    umask2[cntu] = 1;
                    pgr1[sum] = pgr1[sum] * wt1;
                }
                else
                {
                    if (sel <= 3)
                    {
                        pgr1[sum] = pgr1[sum] * wt1 + pgr2[cnt] * wt; /* ave, mean sum */
                    }
                    else if (sel == 4)
                    {
                        pgr1[sum] = pgr1[sum] + pgr2[cnt];
                    }

                    pgr2[cnt] = wt1 + wt;
                    umask2[cntu] = 1;
                }
            }

            cnt++;
            sum++;
            cntu++;
            sumu++;
        }
        
        d += incr;
    int rc = 0;
    for (d = d; d <= d2 && rc==0; d += incr) {
        /* Get weight for this grid */
        wt = 1.0;

        /*---- time 3333333*/
        if (dim == 3) {
            // gr2t(pfi.grvals[3], d, &(pst.tmin));
            // pst.tmax = pst.tmin;
            // if (bndflg) {
            //     rd1 = d;
            //     if (gr1 < rd1 + 0.5) wt = (rd1 + 0.5) - gr1;
            //     if (gr2 > rd1 - 0.5) wt = gr2 + 0.5 - rd1;
            //     if (wt < 0.0) wt = 0.0;
            // }
        }
            /*---- lat,lon,lev,ens 3333333*/
        else {
            conv = pfi.gr2ab[dim];
            abs = conv(pfi.grvals[dim], d);
            alo = conv(pfi.grvals[dim], d - 0.5);
            ahi = conv(pfi.grvals[dim], d + 0.5);
            alen = Math.Abs(ahi - alo);
            dmin[dim] = abs;
            dmax[dim] = abs;
            if (bndflg) {
                if (whi < wlo) {
                    if (alo > wlo) alo = wlo;
                    if (ahi > wlo) ahi = wlo;
                    if (alo < whi) alo = whi;
                    if (ahi < whi) ahi = whi;
                } else {
                    if (alo < wlo) alo = wlo;
                    if (ahi < wlo) ahi = wlo;
                    if (alo > whi) alo = whi;
                    if (ahi > whi) ahi = whi;
                }
            }
            /*---- lat 3333333*/
            if (dim == 1) {
                if (alo > 90.0) alo = 90.0;
                if (ahi > 90.0) ahi = 90.0;
                if (alo < -90.0) alo = -90.0;
                if (ahi < -90.0) ahi = -90.0;
                if (sel == 1) {                                                  /* ave */
                    wt = Math.Abs(Math.Sin(ahi * d2r) - Math.Sin(alo * d2r));
                } else if (sel == 2) {                                          /* mean */
                    wt = Math.Abs(ahi - alo);
                } else if (sel == 3) {                                          /* sum */
                    if (alen > FUZZ_SCALE) {
                        wt = Math.Abs(ahi - alo) / alen;
                    } else {
                        wt = 0.0;
                    }
                } else if (sel == 4) {                                          /* sumg */
                    wt = 1.0;
                }
            }
                /*---- lon,lev,ens 3333333*/
            else {
                if (sel <= 2) {                        /* ave, mean */
                    wt = ahi - alo;
                } else if (sel == 3) {                 /* sum */
                    if (alen > FUZZ_SCALE) {
                        wt = Math.Abs(ahi - alo) / alen;
                    } else {
                        wt = 0.0;
                    }
                } else if (sel == 4) {                 /* sumg */
                    wt = 1.0;
                }
            }
        }

        
        double[] pgr = GetVariable(new VariableDefinition()
        {
            HeightType = FixedSurfaceType.IsobaricSurface,
            VariableType = definition.VariableType,
            HeightValue = abs * 100
        });
        int val = 0;
        cnt = 0;
        sum = 0;
        int valu = 0;
        cntu = 0;
        sumu = 0;
        for (int i = 0; i < siz; i++) {
            if (sel >= 5 && sel <= 8) {
                if (umask1[sumu] == 0 || 1 == 0) {
                    if (1 != 0) {
                        pgr1[sum] = pgr[val];
                        pgr2[cnt] = d;
                        umask1[sumu] = 1;
                        umask2[cntu] = 1;
                    }
                } else {
                    if ((sel == 5 || sel == 7) && pgr[val] < pgr1[sum]) {
                        pgr1[sum] = pgr[val];
                        pgr2[cnt] = d;
                    }
                    if ((sel == 6 || sel == 8) && pgr[val] > pgr1[sum]) {
                        pgr1[sum] = pgr[val];
                        pgr2[cnt] = d;
                    }
                }
            } else {
                if (1 != 0) {
                    /* weight for ave,mean,sum  for sumg just accum */
                    if (sel <= 3) {
                        pgr[val] = pgr[val] * wt;
                    }
                    if (umask1[sumu] == 0) {
                        pgr1[sum] = pgr[val];
                        umask1[sumu] = 1;
                        pgr2[cnt] += wt;
                    } else {
                        pgr1[sum] += pgr[val];
                        pgr2[cnt] += wt;
                    }
                }
            }
            sum++;
            cnt++;
            val++;
            sumu++;
            cntu++;
            valu++;
        }
        
    }
    

    if (rc==1) {
        
        
    } else {
        cnt = 0;         /* Normalize if needed */
        sum = 0;
        cntu = 0;
        sumu = 0;
        if (sel == 1 || sel == 2 || sel == 7 || sel == 8) {
            for (int i = 0; i < siz; i++) {
                if (umask1[sumu] != 0) {
                    if (sel < 3 && pgr2[cnt] == 0.0)
                    {
                        return Array.Empty<double>();
                    }
                    if (sel > 6 && umask2[cntu] == 0) {
                        return Array.Empty<double>();
                    }
                    if (sel == 1 || sel == 2) {
                        pgr1[sum] = pgr1[sum] / pgr2[cnt];
                    } else {
                        pgr1[sum] = pgr2[cnt];
                    }
                }
                sum++;
                cnt++;
                sumu++;
                cntu++;
            }
        }
    }

    return pgr1;

    
    // err3:
    // snprintf(pout, 1255, "Error from %s: Invalid time increment argument\n", fnam);
    // gaprnt(0, pout);
    // return (1);
    }
}