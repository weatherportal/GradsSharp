using GradsSharp.Models;
using GradsSharp.Models.Internal;
using Microsoft.Extensions.Logging;

namespace GradsSharp.Drawing.Grads;

internal class GaIO
{
    private bool cflag = false; /* cache flag */
    private GradsGrid pgr;
    private gavar pvr;
    private GradsFile pfi;
    private int timerr;
    private int msgflg = 1;

    private gaindx pindx;
    private gaindxb pindxb;
    private gag2indx g2indx;

    private DrawingContext _drawingContext;
    
    /* Routine to obtain a grid.  The addresses of the gagrid
        structure is passed to this routine.  The storage for the
        grid is obtained and the grid is filled with data.                 */
    public GaIO(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public int gaggrd(GradsGrid pgrid)
    {
        double[] gr;
        byte[] gru;
        int x, i, id, jd;
        int[] d = new int[5], dx = new int[5];
        int incr, rc, dflag, size;
        long sz;
        long dsid, vid;
        string vname;

        if (cflag)
        {
            //gree(cache, "f105");
        }

        //cache = NULL;
        cflag = false;
        // if (bcflag) {
        //     gree(bcache, "f106");
        //     gree(bpcach, "f107");
        // }
        // bcache = NULL;
        // bpcach = NULL;
        // bcflag = 0;
        // bssav = -999;
        // bpsav = (long) - 999;

        pgr = pgrid;
        pvr = pgr.pvar;
        pfi = pgr.pfile;
        timerr = 0;
        if (pfi.idxflg == 1)
        {
            pindx = pfi.pindx;
            pindxb = pfi.pindxb;
        }

        if (pfi.idxflg == 2) g2indx = pfi.g2indx;
        if (pfi.ppflag > 0 && msgflg > 0)
        {
            _drawingContext.Logger?.LogInformation("Notice:  Automatic Grid Interpolation Taking Place");
            msgflg = 0;
        }

        if (pfi.type == 4)
        {
            rc = gagdef();
            return (rc);
        }

        /* Check dimensions we were given */
        if (pgr.IDimension < -1 || pgr.IDimension > 4 ||
            pgr.JDimension < -1 || pgr.JDimension > 4 ||
            (pgr.IDimension == -1 && pgr.JDimension != -1))
        {
            _drawingContext.Logger?.LogInformation("Internal logic check 16:  {pgr.idim} {pgr.jdim}");
            return (16);
        }
        

        /* Calc sizes and get storage for the grid */
        id = pgr.IDimension;
        jd = pgr.JDimension;
        if (id > -1) pgr.ISize = pgr.DimensionMaximum[id] - pgr.DimensionMinimum[id] + 1;
        else pgr.ISize = 1;
        if (jd > -1) pgr.JSize = pgr.DimensionMaximum[jd] - pgr.DimensionMinimum[jd] + 1;
        else pgr.JSize = 1;
        size = pgr.ISize * pgr.JSize;
        if (size > 1)
        {
            /* this is for the grid */
            gr = new double[size];
            pgr.GridData = gr;
            /* this is for the undef mask */
            gru = new byte[size];
            pgr.UndefinedMask = gru;
        }
        else
        {
            pgr.GridData = new double[] { pgr.MinimumGridValue };
            gr = pgr.GridData;
            pgr.UndefinedMask = new byte[] { pgr.umin };
            gru = pgr.UndefinedMask;
        }

        /* Handle predefined variable */
        if (pvr.levels < -900)
        {
            rc = gagpre();
            return (rc);
        }

        /* set minimum and maximum grid indices */
        for (i = 0; i < 5; i++)
        {
            d[i] = pgr.DimensionMinimum[i];
            dx[i] = pfi.dnum[i];
        }

        /* adjust max Z index so it doesn't exceed the number of levels for this variable */
        dx[2] = pvr.levels;
        if (dx[2] == 0)
        {
            if (id == 2 || jd == 2) goto nozdat;
            dx[2] = 1;
            d[2] = 1;
        }

        incr = pgr.ISize;

        /* If X does not vary, make sure the X coordinate is normalized.    */
        if (id != 0 && pfi.wrap > 0)
        {
            x = pgr.DimensionMinimum[0];
            while (x < 1) x = x + dx[0];
            while (x > dx[0]) x = x - dx[0];
            pgr.DimensionMinimum[0] = x;
            pgr.DimensionMaximum[0] = x;
            d[0] = x;
        }

        /* If any of the non-varying dimensions are out of bounds of the
         file dimension limits, then we have a grid of missing data.
         Check for this.                                                  */
        for (i = 0; i < 5; i++)
        {
            if (id != i && jd != i && /* dim i is non-varying */
                (d[i] < 1 || d[i] > dx[i]))
            {
                /* dim index is <1 or >max dim size */
                /* returned grid will be missing, except for one special case ... */
                /* ... allow a time index offset (offt) equal to 0 */
                if (i != 3 || pgr.toff != 1 || d[i] != 0) /* same as !(i==3 && pgr.toff==1 && d[i]==0) */
                    goto nodat;
            }
        }

        /* Break out point for reading 2D netcdf grids (for special cases) */
        /* JMA still need to optimize handling of OPeNDAP pre-projected grids */
        // if ((pgr.toff != 1) &&          /* if t value is not an offset */
        //     (pfi.ncflg == 1) &&           /* format is netcdf */
        //     (pfi.ppflag == 0) &&          /* no pdef */
        //     (pfi.tmplat == 0)) {          /* not templated */
        //     /* check the variable id, get variable attributes */
        //     rc = gancsetup();
        //     if (rc>0) return (rc);
        //     /* get the 2D grid */
        //     rc = gancgrid(gr, gru, id, jd);
        //     if (rc < 0) goto nodat;
        //     return (rc);
        // }

// #if USEHDF5 == 1
//                                                                                                                             /* For non-templated HDF5 data sets, file is already open,
//      but we still need to open the variable and set it up */
//   if (pfi.tmplat==0 && pfi.ncflg==3) {
//     /* check of variable is already opened */
//     if (pvr.h5varflg < 0) {
//       /* get the variable name */
//       if (pvr.longnm[0] != '\0')
// 	vname = pvr.longnm;
//       else
// 	vname = pvr.abbrv;
//       /* open the variable */
//       rc = h5openvar(pfi.h5id,vname,&dsid,&vid);
//       if (rc) {
// 	pvr.h5vid = -888;
// 	snprintf(pout,1255,"Error: Variable %s not in HDF5 file\n",vname);
// 	gaprnt(0,pout);
// 	return (rc);
//       }
//       /* No errors, so continue with variable set up */
//       pvr.dataspace = dsid;
//       pvr.h5varflg = vid;
//       /* if we haven't looked at this variable before ... */
//       if (pvr.h5vid == -999) {
// 	/* get undef & packing attributes, check cache size */
// 	rc = h5setup();
// 	if (rc) return (rc);
//       }
//       /* set h5-relevant variables in the gavar structure */
//       pvr.h5vid = (int)vid;
//     }
//   }
// #endif

        /* Handle case where X varies. */
        dflag = 0;
        if (id == 0) // idim == 0
        {
            // if (jd < 0) jd = 1;
            //
            // int row = 0;
            // for (d[jd] = pgr.dimmin[jd]; d[jd] <= pgr.dimmax[jd]; d[jd]++)
            // {
            //     if (d[jd] < 1 || d[jd] > dx[jd])
            //     {
            //         for (i = 0; i < incr; i++)
            //         {
            //             gr[row + i] = pgr.undef;
            //             gru[row + i] = 0;
            //         }
            //     }
            //     else
            //     {
            //         rc = gagrow(ref gr, ref gru, d);
            //         if (rc > 0) return (1);
            //         if (rc == 0) dflag = 1;
            //     }
            //
            //     row += incr;
            // if (pgrid.pvar.variableDefinition.HeightType == FixedSurfaceType.IsobaricSurface)
            // {
            //     pgrid.pvar.variableDefinition.HeightValue = GaUtil.gr2lev(_drawingContext.CommonData.pfid.abvals[2], pgr.DimensionMinimum[2]) * 100;
            // }

            // convert our grid coordinates back to absolute coordinates in X & Y direction for correct fetching of data
            pgr.WorldDimensionMinimum[0] =
                _drawingContext.CommonData.pfid.gr2ab[0](_drawingContext.CommonData.pfid.grvals[0],
                    pgr.DimensionMinimum[0]);
            
            pgr.WorldDimensionMinimum[1] =
                _drawingContext.CommonData.pfid.gr2ab[1](_drawingContext.CommonData.pfid.grvals[1],
                    pgr.DimensionMinimum[1]);
            
            
            pgr.WorldDimensionMaximum[0] =
                _drawingContext.CommonData.pfid.gr2ab[0](_drawingContext.CommonData.pfid.grvals[0],
                    pgr.DimensionMaximum[0]);
            
            pgr.WorldDimensionMaximum[1] =
                _drawingContext.CommonData.pfid.gr2ab[1](_drawingContext.CommonData.pfid.grvals[1],
                    pgr.DimensionMaximum[1]);

            var vardef = pgrid.pvar.variableDefinition;
            if (vardef.HeightType == FixedSurfaceType.IsobaricSurface)
            {
                vardef.HeightValue = _drawingContext.CommonData.pfid.gr2ab[2](_drawingContext.CommonData.pfid.grvals[2],
                    pgr.DimensionMinimum[2]);
            }

            
            pgr.WorldDimensionMinimum[3] = pgr.DimensionMinimum[3];
            pgr.WorldDimensionMaximum[3] = pgr.DimensionMaximum[3];
            pgr.DimensionSize[0] = pgr.pfile.dnum[0];
            pgr.DimensionSize[1] = pgr.pfile.dnum[1];

            pgr.DataReader.ReadData(pgr, pgrid.pvar.variableDefinition);

            
            return (0);
        }

        /* Handle cases where X does not vary. Read each point in the grid seperately. */
        if (jd < 0)
        {
            if (id < 0)
            {
                id = 0;
                jd = 1;
            }
            else jd = 0;
        }

        int grpos = 0;

        for (d[jd] = pgr.DimensionMinimum[jd]; d[jd] <= pgr.DimensionMaximum[jd]; d[jd]++)
        {
            if (d[jd] < 1 || d[jd] > dx[jd])
            {
                for (i = 0; i < incr; i++, grpos++)
                {
                    gr[grpos] = pgr.Undef;
                    gru[grpos] = 0;
                }
            }
            else
            {
                for (d[id] = pgr.DimensionMinimum[id]; d[id] <= pgr.DimensionMaximum[id]; d[id]++)
                {
                    if (d[id] < 1 || d[id] > dx[id])
                    {
                        gr[grpos] = pgr.Undef;
                        gru[grpos] = 0;
                    }
                    else
                    {
                        rc = garrow(d[0], d[1], d[2], d[3], d[4], 1, ref gr, ref gru, pgr.toff);
                        if (rc != 0) return (1);
                        dflag = 1;
                    }

                    grpos++;
                }
            }
        }

        if (dflag == 0) goto nodatmsg;

        
        
        return (0);

        nozdat:
        // if (mfcmn.warnflg > 0)
        // {
        //     _drawingContext.Logger?.LogInformation("Data Request Warning:  Varying Z dimension environment...\n");
        //     _drawingContext.Logger?.LogInformation("  but the requested variable has no Z dimension\n");
        //     gaprnt(2, "  Entire grid contents are set to missing data \n");
        // }

        int grpos2 = 0;
        for (i = 0; i < size; i++, grpos2++)
        {
            gr[grpos2] = pgr.Undef;
            gru[grpos2] = 0;
        }

        return (-1);

        nodat:
        for (i = 0; i < size; i++)
        {
            gr[i] = pgr.Undef;
            gru[i] = 0;
        }

        nodatmsg:
        // if (mfcmn.warnflg > 0)
        // {
        //     _drawingContext.Logger?.LogInformation("Data Request Warning:  Request is completely outside file limits\n");
        //     gaprnt(2, "  Entire grid contents are set to missing data \n");
        //     snprintf(pout, 1255, "  Grid limits of file:     X = 1 %i  Y = 1 %i  Z = 1 %i  T = 1 %i  E = 1 %i \n",
        //         pfi.dnum[0], pfi.dnum[1], pfi.dnum[2], pfi.dnum[3], pfi.dnum[4]);
        //     gaprnt(2, pout);
        //     snprintf(pout, 1255, "  Grid limits of request:  X = %i %i  Y = %i %i  Z = %i %i  T = %i %i  E = %i %i \n",
        //         pgr.dimmin[0], pgr.dimmax[0],
        //         pgr.dimmin[1], pgr.dimmax[1],
        //         pgr.dimmin[2], pgr.dimmax[2],
        //         pgr.dimmin[3], pgr.dimmax[3],
        //         pgr.dimmin[4], pgr.dimmax[4]);
        //     gaprnt(2, pout);
        // }

        return (-1);
    }


/* gagrow gets a row of data from the file.  The row of data can
   be 'wrapped' if the x direction of the grid spans the globe.
   return codes:
    0 if no errors
   -1 if out of bounds
    1 if errors

      */

    int gagrow(ref double[] gr, ref byte[] gru, int[] d)
    {
        int rc, i, x, j;
        int y, z, t, e;

        y = d[1];
        z = d[2];
        t = d[3];
        e = d[4];

        /* If the needed data is within the bounds of the file dimensions
         then read the data directly.                                     */
        if (pgr.DimensionMinimum[0] >= 1 && pgr.DimensionMaximum[0] <= pfi.dnum[0])
        {
            rc = garrow(pgr.DimensionMinimum[0], y, z, t, e, (pgr.DimensionMaximum[0] - pgr.DimensionMinimum[0] + 1), ref gr, ref gru, pgr.toff);
            if (rc != 0) return (1);
            return (0);
        }

        /* If the file does not wrap, then read the data directly, if possible.
         If the requested data lies outside the file's bounds,
         fill in with missing data where appropriate.                   */
        if (pfi.wrap == 0)
        {
            if (pgr.DimensionMinimum[0] >= 1 && pgr.DimensionMaximum[0] <= pfi.dnum[0])
            {
                rc = garrow(pgr.DimensionMinimum[0], y, z, t, e, (pgr.DimensionMaximum[0] - pgr.DimensionMinimum[0] + 1), ref gr, ref gru, pgr.toff);
                if (rc != 0) return (1);
                return (0);
            }

            for (i = 0; i < pgr.ISize; i++)
            {
                gr[i] = pgr.Undef;
                gru[i] = 0;
            }

            if (pgr.DimensionMinimum[0] < 1 && pgr.DimensionMaximum[0] < 1) return (-1);
            if (pgr.DimensionMinimum[0] > pfi.dnum[0] &&
                pgr.DimensionMaximum[0] > pfi.dnum[0])
                return (-1);
            i = 1 - pgr.DimensionMinimum[0];
            if (i > 0)
            {
                // gr += i;
                // gru += i;
            }

            i = 1;
            if (pgr.DimensionMinimum[0] > 1) i = pgr.DimensionMinimum[0];
            j = pgr.DimensionMaximum[0];
            if (j > pfi.dnum[0]) j = pfi.dnum[0];
            j = 1 + (j - i);
            rc = garrow(i, y, z, t, e, j, ref gr, ref gru, pgr.toff);
            if (rc != 0) return (1);
            return (0);
        }

        /* When the file wraps, we read the entire row into the row buffer, and
         copy the values as needed into locations in the requested row.    */
        rc = garrow(1, y, z, t, e, pfi.dnum[0], ref pfi.rbuf, ref pfi.ubuf, pgr.toff);
        if (rc != 0) return (1);
        for (x = pgr.DimensionMinimum[0]; x <= pgr.DimensionMaximum[0]; x++)
        {
            i = x;
            while (i < 1) i = i + pfi.dnum[0];
            while (i > pfi.dnum[0]) i = i - (pfi.dnum[0]); /* Best way??? */
            // *gr = *((pfi.rbuf) + i - 1);
            // *gru = *((pfi.ubuf) + i - 1);
            // gr++;
            // gru++;
        }

        return (0);
    }


/*  Basic read of a row of data elements -- a row is always
    in the X direction, which for grads binary is the fastest
    varying dimension */

    int garrow(int x, int y, int z, int t, int e,
        int len, ref double[] gr, ref byte[] gru, int toff)
    {
        gaens ens;
        int rc, i = 0, tt, ee, oflg;
        long fposlf;
// #if USEHDF5 == 1
//         char* vname;
//         hid_t dsid, vid;
// #endif

        /* change t value if offset flag is set */
        if (toff>0)
        {
            throw new NotImplementedException();
            /* advance through chain of ensemble structure to get to ensemble 'e' */
            // ens = pfi.ens1;
            // i = 1;
            // while (i < e)
            // {
            //     i++;
            //     ens++;
            // }
            //
            // /* add the forecast offset to the start time for this ensemble */
            // t = ens.gt + t;
            // /* if new t value is outside file's bounds, populate with undefs */
            // if (t < 1 || t > pfi.dnum[3])
            // {
            //     for (i = 0; i < len; i++)
            //     {
            //         *(gr + i) = pfi.undef;
            //         *(gru + i) = 0;
            //     }
            //
            //     return (0);
            // }
        }

        tt = t;
        if (pfi.tmplat>0)
        {
//             tt = gaopfn(t, e, &ee, &oflg, pfi);
//             if (tt == -99999) return (1);
//             if (tt == -88888)
//             {
//                 for (i = 0; i < len; i++)
//                 {
//                     *(gr + i) = pfi.undef;
//                     *(gru + i) = 0;
//                 }
//
//                 return (0);
//             }
//
//             if (oflg)
//             {
//                 /* Force new bit map cache if new file opened */
//                 bpsav = (long) - 999;
//             }
// #if USEHDF5 == 1
//             /* For HDF5, call h5setup and h5openvar if new file opened or if a new variable */
//             if (pfi.ncflg == 3)
//             {
//                 if (oflg || pvr.h5varflg < 0)
//                 {
//                     /* get the variable name */
//                     if (pvr.longnm[0] != '\0')
//                         vname = pvr.longnm;
//                     else
//                         vname = pvr.abbrv;
//                     /* open the variable */
//                     rc = h5openvar(pfi.h5id, vname, &dsid, &vid);
//                     if (rc)
//                     {
//                         pvr.h5vid = -888;
//                         snprintf(pout, 1255, "Error: Variable %s not in HDF5 file\n", vname);
//                         _drawingContext.Logger?.LogInformation(pout);
//                         return (rc);
//                     }
//
//                     /* No errors, so continue with variable set up */
//                     pvr.dataspace = dsid;
//                     pvr.h5varflg = vid;
//                     /* if we haven't looked at this variable before ... */
//                     if (pvr.h5vid == -999)
//                     {
//                         /* get undef & packing attributes, check cache size */
//                         rc = h5setup();
//                         if (rc) return (rc);
//                     }
//
//                     /* set h5-relevant variables in the gavar structure */
//                     pvr.h5vid = (int)vid;
//                 }
//             }
//#endif
        }
        else
        {
            ee = e; /* set relative ensemble number to e for non-templated data sets */
        }

        /* Preprojected (pdef) */
        if (pfi.ppflag>0)
        {
            throw new NotImplementedException();
            // if (pfi.idxflg)
            //     rc = gaprow(x, y, z, t, e, tt, len, gr, gru); /* Grib uses e to read index file */
            // else
            //     rc = gaprow(x, y, z, t, ee, tt, len, gr, gru); /* All other data types use ee */
            // return (rc);
        }

        /* netcdf */
        if (pfi.ncflg == 1)
        {
            throw new NotImplementedException();
            // rc = gancsetup();
            // if (rc) return (rc);
            // rc = gancrow(x, y, z, tt, ee, len, gr, gru);
            // return (rc);
            
        }

        /* HDF-SDS */
        if (pfi.ncflg == 2)
        {
            throw new NotImplementedException();
            // rc = gahrow(x, y, z, tt, ee, len, gr, gru);
            // return (rc);
        }

        /* HDF5 grids */
        if (pfi.ncflg == 3)
        {
            throw new NotImplementedException();
            // rc = gah5row(x, y, z, tt, ee, len, gr, gru);
            // return (rc);

        }

        /* Indexed (grib) */
        if (pfi.idxflg>1)
        {
            //rc = gairow(x, y, z, t, e, i, len, gr, gru);
            rc = 1;
            return (rc);
        }

        /* if none of the above... binary */
        // fposlf = gafcorlf(x, y, z, tt, ee);
        // rc = garead(fposlf, len, gr, gru);
        // return (rc);
        return 1;
    }

    /*  Obtain user requested grid from defined variable */

    int gagdef()
    {
        int id, jd, i, flag;
        int ys, zs, ts, es, siz;
        long pos;
        int[] d = new int[5];
        int d1min = 0, d1max = 0, d2min = 0, d2max = 0, xt, yt;
        double[] v;
        byte[] vmask;
        long sz;

        /* If a dimension is a fixed dimension in the defined
         variable, it must be a fixed dimension in the output
         grid.  */

        id = pgr.IDimension;
        jd = pgr.JDimension;
        if (jd > -1)
        {
            if (pfi.dnum[jd] == 1)
            {
                jd = -1;
                pgr.JDimension = -1;
                pgr.JSize = -1;
            }
        }

        if (id > -1)
        {
            if (pfi.dnum[id] == 1)
            {
                id = jd;
                pgr.IDimension = pgr.JDimension;
                pgr.ISize = pgr.JSize;
                pgr.igrab = pgr.jgrab;
                pgr.iabgr = pgr.jabgr;
                pgr.ivals = pgr.jvals;
                pgr.iavals = pgr.javals;
                pgr.ilinr = pgr.jlinr;
                jd = -1;
                pgr.JDimension = -1;
                pgr.JSize = 1;
            }
        }

        /* Set up constants for array subscripting */

        ys = pfi.dnum[0];
        zs = ys * pfi.dnum[1];
        ts = zs * pfi.dnum[2];
        es = ts * pfi.dnum[3];

        /* Set up dimension ranges */

        for (i = 0; i < 5; i++) d[i] = pgr.DimensionMinimum[i] - pfi.dimoff[i] - 1;
        for (i = 0; i < 5; i++)
            if (pfi.dnum[i] == 1)
                d[i] = 0;
        if (id > -1)
        {
            d1min = d[id];
            d1max = pgr.DimensionMaximum[id] - pfi.dimoff[id] - 1;
        }

        if (jd > -1)
        {
            d2min = d[jd];
            d2max = pgr.DimensionMaximum[jd] - pfi.dimoff[jd] - 1;
        }

        /* Get storage for output grid */

        pgr.ISize = 1;
        pgr.JSize = 1;
        if (id > -1) pgr.ISize = 1 + d1max - d1min;
        if (jd > -1) pgr.JSize = 1 + d2max - d2min;
        siz = pgr.ISize * pgr.JSize;
        if (siz > 1)
        {
            pgr.GridData = new double[siz];
            pgr.UndefinedMask = new byte[siz];
        }
        else
        {
            pgr.GridData = new double[] {pgr.MinimumGridValue};
            pgr.UndefinedMask = new byte[] {pgr.umin};
        }

        /* Normalize time coordinate if not varying */
        /* JMA: This does not handle leap years properly!!!!  Gotta fix this someday */

        //if (pfi.climo && id != 3 && jd != 3) clicyc(d + 3);

        /* Check for entirely undefined grid */

        flag = 0;
        for (i = 0; i < 5; i++)
        {
            if (i != id && i != jd && (d[i] < 0 || d[i] >= pfi.dnum[i])) flag = 1;
        }

        if (flag>0)
        {
            for (i = 0; i < siz; i++)
            {
                pgr.GridData[i] = pfi.undef;
                pgr.UndefinedMask[i] = 0;
            }

            return (0);
        }

        /* Move appropriate grid values */

        if (id == -1 && jd == -1)
        {
            pos = (long)d[0] + (long)d[1] * (long)ys + (long)d[2] * (long)zs + (long)d[3] * (long)ts +
                  (long)d[4] * (long)es;
            pgr.MinimumGridValue = pfi.rbuf[pos];
            pgr.umin = pfi.ubuf[pos];
            return (0);
        }

        int vpos = 0;
        
        v = pgr.GridData;
        vmask = pgr.UndefinedMask;

        if (jd == -1)
        {
            for (xt = d1min; xt <= d1max; xt++)
            {
                d[id] = xt;
                //if (id == 3 && pfi.climo) clicyc(d + 3);
                if (d[id] < 0 || d[id] >= pfi.dnum[id])
                {
                    v[vpos] = pfi.undef;
                    vmask[vpos] = 0;
                }
                else
                {
                    pos = (long)d[0] + (long)d[1] * (long)ys + (long)d[2] * (long)zs + (long)d[3] * (long)ts +
                          (long)d[4] * (long)es;
                    v[vpos] = pfi.rbuf[pos];
                    vmask[vpos] = pfi.ubuf[pos];
                }

                vpos++;
            }

            return (0);
        }

        for (yt = d2min; yt <= d2max; yt++)
        {
            d[jd] = yt;
            //if (jd == 3 && pfi.climo) clicyc(d + 3);
            for (d[id] = d1min; d[id] <= d1max; d[id]++)
            {
                if (d[jd] < 0 || d[jd] >= pfi.dnum[jd] ||
                    d[id] < 0 || d[id] >= pfi.dnum[id])
                {
                    v[vpos] = pfi.undef;
                    vmask[vpos] = 0;
                }
                else
                {
                    pos = (long)d[0] + (long)d[1] * (long)ys + (long)d[2] * (long)zs + (long)d[3] * (long)ts +
                          (long)d[4] * (long)es;
                    v[vpos] = pfi.rbuf[pos];
                    vmask[vpos] = pfi.ubuf[pos];
                }

                vpos++;
            }
        }

        return (0);
    }

    int gagpre()
    {
        Func<double[], double, double>? conv;
        int[] d = new int[5];
        int id, jd, i, dim;
        double[] gr;  
        double t;
        byte[] gru;
        double[] vals;

        id = pgr.IDimension;
        jd = pgr.JDimension;
        for (i = 0; i < 5; i++) d[i] = pgr.DimensionMinimum[i];

        dim = (int)pvr.offset;
        conv = pfi.gr2ab[dim];
        vals = pfi.grvals[dim];

        gr = pgr.GridData;
        gru = pgr.UndefinedMask;

        int pos = 0;

        if (id > -1 && jd > -1)
        {
            for (d[jd] = pgr.DimensionMinimum[jd]; d[jd] <= pgr.DimensionMaximum[jd]; d[jd]++)
            {
                for (d[id] = pgr.DimensionMinimum[id]; d[id] <= pgr.DimensionMaximum[id]; d[id]++)
                {
                    t = (double)(d[dim]);
                    gr[pos] = conv(vals, t);
                    gru[pos] = 1;
                    pos++;
                }
            }
        }
        else if (id > -1)
        {
            for (d[id] = pgr.DimensionMinimum[id]; d[id] <= pgr.DimensionMaximum[id]; d[id]++)
            {
                t = (double)(d[dim]);
                gr[pos] = conv(vals, t);
                gru[pos] = 1;
                pos++;
            }
        }
        else
        {
            t = (double)(d[dim]);
            gr[pos] = conv(vals, t);
            gru[pos] = 1;
        }

        return (0);
    }
}