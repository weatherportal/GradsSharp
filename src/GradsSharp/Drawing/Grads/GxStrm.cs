namespace GradsSharp.Drawing.Grads;

internal class GxStrm
{

    private DrawingContext _drawingContext;


    public GxStrm(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public void gxstrm(double[] u, double[] v, double[] c, int iis, int jjs,
        byte[] umask, byte[] vmask, byte[] cmask, bool flag, double[] shdlvs,
        int[] shdcls, int shdcnt, int den, double strmarrd, double strmarrsz,
        int strmarrt)
    {
        double x, y, xx, yy;
        int up, vp, cp;
        double cv1,cv2,cv;
        double uv1, uv2, uv, vv1, vv2, vv, auv, avv, xsav, ysav, xold = 0.0, yold = 0.0;
        double fact, rscl, xxsv, yysv, xstrt, ystrt;
        double xx1, yy1, xx2, yy2, adj, dacum, tacum;
        int i, ii, jj, ii1, ij1, i2, j2, ipt, acnt, icol, scol, dis;
        int[] it;
        int siz, iacc, iisav, iscl, imn, imx, jmn, jmx, iz, jz, iss, jss;
        bool bflg;
        int upmask, vpmask, cpmask;

        scol = -9;
        icol = 1;

        /* Figure out the interval for the flag grid */

        i =  iis;
        if (jjs > i) i = jjs;
        iscl = 200 / i;
        iscl = iscl + den - 5;
        if (iscl < 1) iscl = 1;
        if (iscl > 10) iscl = 10;
        ii = 1;
        if (den < 0)
        {
            /* Support very high resolution grids */
            ii = -1 * (den - 1);
            if (ii > 10) ii = 10;
            if ((iis<200 || jjs < 100) && ii > 2) ii = 2; /* Limit downscaling to only high res */
            if ((iis<500 || jjs < 250) && ii > 5) ii = 5;
            if ((iis<1000 || jjs < 500) && ii > 10) ii = 10;
            if ((iis<1500 || jjs < 750) && ii > 15) ii = 15;
            if (ii > 20) ii = 20;
        }

        rscl = (double)iscl / (double)ii;
        fact = 0.5 / rscl;
        /* if (fact<0.3) fact = 0.3; */

        /* Allocate memory for the flag grid */

        iss =  iis*iscl / ii;
        jss = jjs * iscl / ii;
        siz = iss * jss;
        it = new int[siz];
        
        for (i = 0; i < siz; i++) it[i] = 0;

        /* Loop through flag grid to look for start of streamlines.  
           To start requires no streams drawn within surrounding 
           flag boxes.  */

        i2 = 0;
        j2 = 0;
        for (i = 0; i < siz; i++)
        {
            dis = 2;
            if (den < 5) dis = 3;
            if (den > 5) dis = 1;
            if (den < 0) dis = 1;
            if (den < -5) dis = 2;
            imn = i2 - dis;
            imx = i2 + dis + 1;
            jmn = j2 - dis;
            jmx = j2 + dis + 1;
            if (imn < 0) imn = 0;
            if (imx > iss) imx = iss;
            if (jmn < 0) jmn = 0;
            if (jmx > jss) jmx = jss;
            iacc = 0;
            for (jz = jmn; jz < jmx; jz++)
            {
                ipt = jz * iss + imn;
                for (iz = imn; iz < imx; iz++)
                {
                    iacc = iacc + it[ipt];
                    ipt++;
                }
            }

            if (iacc == 0)
            {
                x = ((double)i2) / rscl;
                y = ((double)j2) / rscl;
                xsav = x;
                ysav = y;
                xstrt = x;
                ystrt = y;
                GradsDrawingInterface.gxconv(x + 1.0, y + 1.0, out xx, out yy, 3);
                _drawingContext.GradsDrawingInterface.gxplot(xx, yy, 3);
                xxsv = xx;
                yysv = yy;
                iisav = -999;
                iacc = 0;
                acnt = 0;
                dacum = 0.0;
                tacum = 0.0;
                bflg = false;
                while (x >= 0.0 && x < (double)( iis - 1) && y >= 0.0 && y < (double)(jjs - 1)) {
                    ii = (int)x;
                    jj = (int)y;
                    xx = x - (double)ii;
                    yy = y - (double)jj;
                    up = jj * iis +ii;
                    upmask = jj * iis +ii;
                    vp = jj * iis +ii;
                    vpmask = jj * iis +ii;
                    if (umask[upmask] == 0 ||
                        umask[upmask + 1]== 0 ||
                        umask[upmask + iis] == 0 ||
                        umask[upmask + iis +1] == 0) break;
                    if (vmask[vpmask] == 0 ||
                        vmask[vpmask + 1]== 0 ||
                        vmask[vpmask + iis] == 0 ||
                        vmask[vpmask + iis +1] == 0) break;
                    if (flag)
                    {
                        cp = jj * iis +ii;
                        cpmask = jj * iis +ii;
                        if (cmask[cpmask] == 0 ||
                            cmask[cpmask + 1] == 0 ||
                            cmask[cpmask + iis] == 0 ||
                            cmask[cpmask + iis +1] == 0) icol = 15;
                        else
                        {
                            cv1 = c[cp] + (c[cp + 1] - c[cp]) * xx;
                            cv2 = c[cp + iis] + (c[cp + iis +1]- c[cp + iis]) * xx;
                            cv = cv1 + (cv2 - cv1) * yy;
                            icol = gxshdc(shdlvs, shdcls, shdcnt, cv);
                        }

                        if (icol != scol && icol > -1) _drawingContext.GradsDrawingInterface.gxcolr(icol);
                        scol = icol;
                    }

                    uv1 = u[up] + (u[up + 1] - u[up]) * xx;
                    uv2 = u[up + iis] + (u[up + iis +1] - u[up + iis]) * xx;
                    uv = uv1 + (uv2 - uv1) * yy;
                    vv1 = v[vp] + (v[vp + 1] - v[vp]) * xx;
                    vv2 = v[vp + iis] + (v[vp + iis +1]- v[vp + iis]) * xx;
                    vv = vv1 + (vv2 - vv1) * yy;
                    auv = Math.Abs(uv);
                    avv = Math.Abs(vv);
                    if (auv < 0.1 && avv < 0.1) break;
                    if (auv > avv)
                    {
                        vv = vv * fact / auv;
                        uv = uv * fact / auv;
                    }
                    else
                    {
                        uv = uv * fact / avv;
                        vv = vv * fact / avv;
                    }

                    GradsDrawingInterface.gxconv(x + 1.0, y + 1.0, out xx, out yy, 3); /* account for localized grid distortions */
                    GradsDrawingInterface.gxconv(x + 1.1, y + 1.0, out xx1, out yy1, 3);
                    GradsDrawingInterface.gxconv(x + 1.0, y + 1.1, out xx2, out yy2, 3);
                    adj = GaUtil.hypot(xx - xx1, yy - yy1) / GaUtil.hypot(xx - xx2, yy - yy2);
                    if (adj > 1.0) uv = uv / adj;
                    else vv = vv * adj;
                    if (Math.Abs(uv) < 1e-6 && Math.Abs(vv) < 1e-6) break;
                    x = x + uv;
                    y = y + vv;
                    ii1 = (int)(x * rscl);
                    ij1 = (int)(y * rscl);
                    ii1 = ij1 * iss + ii1;
                    if (ii1 < 0 || ii1 >= siz) break;
                    if (it[ii1] == 1) break;
                    if (ii1 != iisav && iisav > -1) it[iisav] = 1;
                    if (ii1 == iisav) iacc++;
                    else
                    {
                        iacc = 0;
                        tacum = 0;
                    }

                    if (iacc > 10 && tacum < 0.1) break;
                    if (iacc > 100) break;
                    iisav = ii1;
                    GradsDrawingInterface.gxconv(x + 1.0, y + 1.0, out xx, out yy, 3);
                    if (icol > -1)
                    {
                        if (bflg)
                        {
                            _drawingContext.GradsDrawingInterface.gxplot(xold, yold, 3);
                            bflg = false;
                        }

                        _drawingContext.GradsDrawingInterface.gxplot(xx, yy, 2);
                    }
                    else bflg = true;

                    dacum += GaUtil.hypot(xx - xold, yy - yold);
                    tacum += GaUtil.hypot(xx - xold, yy - yold);
                    acnt++;
                    if (dacum > strmarrd)
                    {
                        if (icol > -1) strmar(xxsv, yysv, xx, yy, strmarrsz, strmarrt);
                        acnt = 0;
                        dacum = 0.0;
                    }

                    xold = xx;
                    yold = yy;
                    xxsv = xx;
                    yysv = yy;
                }
                bflg = false;
                x = xsav;
                y = ysav;
                GradsDrawingInterface.gxconv(x + 1.0, y + 1.0, out xx, out yy, 3);
                _drawingContext.GradsDrawingInterface.gxplot(xx, yy, 3);
                xxsv = xx;
                yysv = yy;
                iisav = -999;
                iacc = 0;
                acnt = 19;
                dacum = 0.0;
                tacum = 0.0;
                while (x >= 0.0 && x < (double)( iis -1) && y >= 0.0 && y < (double)(jjs - 1)) {
                    ii = (int)x;
                    jj = (int)y;
                    xx = x - (double)ii;
                    yy = y - (double)jj;
                    up = jj * iis +ii;
                    upmask = jj * iis +ii;
                    vp = jj * iis +ii;
                    vpmask = jj * iis +ii;
                    if (umask[upmask] == 0 ||
                        umask[upmask + 1] == 0 ||
                        umask[upmask + iis] == 0 ||
                        umask[upmask + iis +1] == 0) break;
                    if (vmask[vpmask] == 0 ||
                        vmask[vpmask + 1] == 0 ||
                        vmask[vpmask + iis] == 0 ||
                        vmask[vpmask + iis +1] == 0) break;
                    if (flag)
                    {
                        cp = jj * iis +ii;
                        cpmask = jj * iis +ii;
                        if (cmask[cpmask] == 0 ||
                            cmask[cpmask + 1] == 0 ||
                            cmask[cpmask + iis] == 0 ||
                            cmask[cpmask + iis +1] == 0) icol = 15;
                        else
                        {
                            cv1 = c[cp] + (c[cp + 1] - c[cp]) * xx;
                            cv2 = c[cp + iis] + (c[cp + iis +1] - c[cp + iis]) * xx;
                            cv = cv1 + (cv2 - cv1) * yy;
                            icol = gxshdc(shdlvs, shdcls, shdcnt, cv);
                        }

                        if (icol != scol && icol > -1) _drawingContext.GradsDrawingInterface.gxcolr(icol);
                        scol = icol;
                    }

                    uv1 = u[up] + (u[up + 1] - u[up]) * xx;
                    uv2 = u[up + iis] + (u[up + iis +1] - u[up + iis]) * xx;
                    uv = uv1 + (uv2 - uv1) * yy;
                    vv1 = v[vp] + (v[vp + 1] - v[vp]) * xx;
                    vv2 = v[vp + iis] + (v[vp + iis +1] - v[vp + iis]) * xx;
                    vv = vv1 + (vv2 - vv1) * yy;
                    auv = Math.Abs(uv);
                    avv = Math.Abs(vv);
                    if (auv < 0.1 && avv < 0.1) break;
                    if (auv > avv)
                    {
                        vv = vv * fact / auv;
                        uv = uv * fact / auv;
                    }
                    else
                    {
                        uv = uv * fact / avv;
                        vv = vv * fact / avv;
                    }

                    GradsDrawingInterface.gxconv(x + 1.0, y + 1.0, out xx, out yy, 3); /* account for localized grid distortions */
                    GradsDrawingInterface.gxconv(x + 1.1, y + 1.0, out xx1, out yy1, 3);
                    GradsDrawingInterface.gxconv(x + 1.0, y + 1.1, out xx2, out yy2, 3);
                    adj = GaUtil.hypot(xx - xx1, yy - yy1) / GaUtil.hypot(xx - xx2, yy - yy2);
                    if (adj > 1.0) uv = uv / adj;
                    else vv = vv * adj;
                    if (Math.Abs(uv) < 1e-6 && Math.Abs(vv) < 1e-6) break;
                    x = x - uv;
                    y = y - vv;
                    ii1 = (int)(x * rscl);
                    ij1 = (int)(y * rscl);
                    ii1 = ij1 * iss + ii1;
                    if (ii1 < 0 || ii1 >= siz) break;
                    if (it[ii1] == 1) break;
                    if (ii1 != iisav && iisav > -1) it[iisav] = 1;
                    if (ii1 == iisav) iacc++;
                    else iacc = 0;
                    if (iacc > 10 && tacum < 0.1) break;
                    if (iacc > 100) break;
                    iisav = ii1;
                    GradsDrawingInterface.gxconv(x + 1.0, y + 1.0, out xx, out yy, 3);
                    if (icol > -1)
                    {
                        if (bflg)
                        {
                            _drawingContext.GradsDrawingInterface.gxplot(xold, yold, 3);
                            bflg = false;
                        }

                        _drawingContext.GradsDrawingInterface.gxplot(xx, yy, 2);
                    }
                    else bflg = true;

                    dacum += GaUtil.hypot(xx - xold, yy - yold);
                    tacum += GaUtil.hypot(xx - xold, yy - yold);
                    xold = xx;
                    yold = yy;
                    acnt++;
                    if (dacum > strmarrd)
                    {
                        if (icol > -1) strmar(xx, yy, xxsv, yysv, strmarrsz, strmarrt);
                        acnt = 0;
                        dacum = 0.0;
                    }

                    xxsv = xx;
                    yysv = yy;
                }
                ii1 = (int)(xstrt * rscl);
                ij1 = (int)(ystrt * rscl);
                ii1 = ij1 * iss + ii1;
                if (ii1 >= 0 || ii1 < siz) it[ii1] = 1;
            }

            i2++;
            if (i2 == iss)
            {
                i2 = 0;
                j2++;
            }
        }

        
    }

    static double a150 = 150.0 * Math.PI / 180;

    void strmar(double xx1, double yy1, double xx2, double yy2, double sz, int type)
    {
        double dir;
        double[] xy = new double[8];

        if (sz < 0.0001) return;
        dir = Math.Atan2(yy2 - yy1, xx2 - xx1);
        xy[0] = xx2;
        xy[1] = yy2;
        xy[2] = xx2 + sz * Math.Cos(dir + a150);
        xy[3] = yy2 + sz * Math.Sin(dir + a150);
        xy[4] = xx2 + sz * Math.Cos(dir - a150);
        xy[5] = yy2 + sz * Math.Sin(dir - a150);
        xy[6] = xx2;
        xy[7] = yy2;
        if (type == 1)
        {
            _drawingContext.GradsDrawingInterface.gxplot(xx2, yy2, 3);
            _drawingContext.GradsDrawingInterface.gxplot(xy[2], xy[3], 2);
            _drawingContext.GradsDrawingInterface.gxplot(xx2, yy2, 3);
            _drawingContext.GradsDrawingInterface.gxplot(xy[4], xy[5], 2);
            _drawingContext.GradsDrawingInterface.gxplot(xx2, yy2, 3);
        }

        if (type == 2)
        {
            _drawingContext.GradsDrawingInterface.gxfill(xy, 4);
        }
    }

/* Given a shade value, return the relevent color */

    int gxshdc(double[] shdlvs, int[] shdcls, int shdcnt, double val)
    {
        int i;

        if (shdcnt == 0) return (1);
        if (shdcnt == 1) return (shdcls[0]);
        if (val <= shdlvs[1]) return (shdcls[0]);
        for (i = 1; i < shdcnt - 1; i++)
        {
            if (val > shdlvs[i] && val <= shdlvs[i + 1]) return (shdcls[i]);
        }

        return (shdcls[shdcnt - 1]);
    }
}