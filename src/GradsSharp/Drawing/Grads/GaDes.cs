using GradsSharp.Models.Internal;

namespace GradsSharp.Drawing.Grads;

internal class GaDes
{
    public static int gappcn(GradsFile pfi, int pdefop1, int pdefop2)
    {
        int size, i, j, ii, jj;
        double lat = 0, lon = 0, rii = 0, rjj = 0;
        double[] dx, dy;
        double[] dw = Array.Empty<double>();
        double dum;
        double pi;
        int[] ioff;
        float?[] fvals;
        int rdw, rc, pnum, wflg;
        int sz;

        dw = null;
        size = pfi.dnum[0] * pfi.dnum[1];

        /* Allocate space needed for the ppi and ppf grids */
        if (pfi.ppflag != 8)
        {
            pfi.ppi.Add(new int[size]);
            pfi.ppf.Add(new double[size]);
            pfi.ppf.Add(new double[size]);
            if (pfi.ppwrot > 0)
            {
                pfi.ppw = new double[size];
            }
        }

        /* pdef bilin */
        if (pfi.ppflag == 7)
        {
            /* allocate memory to temporarily store array of floats to be read from pdef file */
            // sz = sizeof(gafloat) * size;
            // if ((fvals = (gafloat *) galloc(sz, "ppfvals")) == NULL) goto merr;
            //
            // if (pdefop1 == 2) {  /* sequential -- read the 4-byte header */
            //     rc = fread(&rdw, sizeof(int), 1, pdfi);
            //     if (rc != 1) goto merr2;
            // }
            //
            // /* read the grid of pdef ivals into fvals array */
            // rc = fread(fvals, sizeof(gafloat), size, pdfi);
            // if (rc != size) goto merr2;
            // /* byte swap if necessary */
            // if ((pdefop2 == 2 && !BYTEORDER) || (pdefop2 == 3 && BYTEORDER)) gabswp(fvals, size);
            // /* cast to doubles */
            // for (i = 0; i < size; i++) *(pfi.ppf[0] + i) = (double) fvals[i];
            //
            // if (pdefop1 == 2) {  /* sequential -- read the 4-byte footer and next header */
            //     rc = fread(&rdw, sizeof(int), 1, pdfi);
            //     if (rc != 1) goto merr2;
            //     rc = fread(&rdw, sizeof(int), 1, pdfi);
            //     if (rc != 1) goto merr2;
            // }
            //
            // /* read the grid of pdef jvals into fvals array */
            // rc = fread(fvals, sizeof(gafloat), size, pdfi);
            // if (rc != size) goto merr2;
            // /* byte swap if necessary */
            // if ((pdefop2 == 2 && !BYTEORDER) || (pdefop2 == 3 && BYTEORDER)) gabswp(fvals, size);
            // /* cast to doubles */
            // for (i = 0; i < size; i++) *(pfi.ppf[1] + i) = (double) fvals[i];
            //
            // if (pdefop1 == 2) {  /* sequential -- read the 4-byte footer and next header */
            //     rc = fread(&rdw, sizeof(int), 1, pdfi);
            //     if (rc != 1) goto merr2;
            //     rc = fread(&rdw, sizeof(int), 1, pdfi);
            //     if (rc != 1) goto merr2;
            // }
            //
            // /* read the grid of wind rotation vals */
            // rc = fread(fvals, sizeof(gafloat), size, pdfi);
            // if (rc != size) goto merr2;
            // /* byte swap if necessary */
            // if ((pdefop2 == 2 && !BYTEORDER) || (pdefop2 == 3 && BYTEORDER)) gabswp(fvals, size);
            // /* cast to doubles */
            // for (i = 0; i < size; i++) *(pfi.ppw + i) = (double) fvals[i];
            //
            // /* Fill grids of file offsets and weights (dx,dy) for pdef grid interpolation */
            // ioff = pfi.ppi[0];
            // dx = pfi.ppf[0];
            // dy = pfi.ppf[1];
            // dw = pfi.ppw;
            // wflg = 0;
            // for (j = 0; j < pfi.dnum[1]; j++) {
            //     for (i = 0; i < pfi.dnum[0]; i++) {
            //         if (*dx < 0.0) *ioff = -1;
            //         else {
            //             /* ii and jj are integer parts of i and j values read from pdef bilin file */
            //             ii = (int) (*dx);
            //             jj = (int) (*dy);
            //             /* dx and dy are now the remainder after the integer part is subtracted out */
            //             *dx = *dx - (double) ii;
            //             *dy = *dy - (double) jj;
            //             /* if ii and jj values are outside the native grid, they are not used */
            //             if (ii < 1 || ii > pfi.ppisiz - 1 || jj < 1 || jj > pfi.ppjsiz - 1) {
            //                 *ioff = -1;
            //             } else {
            //                 /* ioff index values (pfi.ppi) start from 0 instead of 1 */
            //                 *ioff = (jj - 1) * pfi.ppisiz + ii - 1;
            //             }
            //         }
            //         if (fabs(*dw) > 0.00001) wflg = 1;
            //         ioff++;
            //         dx++;
            //         dy++, dw++;
            //     }
            // }
            // pfi.ppwrot = wflg;

            /* When pdef is a file, read in the offsets of the points to use and their weights,
         as well as the array of wind rotation values to use */
        }
        else if (pfi.ppflag == 8)
        {
            //  pnum = (int) (pfi.ppvals[0] + 0.1);
            //  /* allocate memory for array of floats to be read from pdef file */
            //  sz = sizeof(gafloat) * size;
            //  if ((fvals = (gafloat *) galloc(sz, "ppfvals")) == NULL) goto merr;
            //
            //  /* get weights and offsets from pdef file */
            //  for (i = 0; i < pnum; i++) {
            //      /* allocate memory for the array of offsets */
            //      sz = sizeof(int) * size;
            //      if ((pfi.ppi[i] = (int *) galloc(sz, "ppi3")) == NULL) goto merr;
            //      /* sequential -- header */
            //      if (pdefop1 == 2) {
            //          rc = fread(&rdw, sizeof(int), 1, pdfi);
            //          if (rc != 1) goto merr2;
            //      }
            //      /* read the offsets */
            //      rc = fread(pfi.ppi[i], sizeof(int), size, pdfi);
            //      if (rc != size) goto merr2;
            //      /* byte swap if necessary */
            //      if ((pdefop2 == 2 && !BYTEORDER) || (pdefop2 == 3 && BYTEORDER))
            //          gabswp((gafloat *) (pfi.ppi[i]), size);
            //      /* sequential -- footer */
            //      if (pdefop1 == 2) {
            //          rc = fread(&rdw, sizeof(int), 1, pdfi);
            //          if (rc != 1) goto merr2;
            //      }
            //
            //      /* allocate memory for array of weights */
            //      sz = sizeof(double) * size;
            //      if ((pfi.ppf[i] = (double *) galloc(sz, "ppf2")) == NULL) goto merr;
            //      /* sequential -- header */
            //      if (pdefop1 == 2) {
            //          rc = fread(&rdw, sizeof(int), 1, pdfi);
            //          if (rc != 1) goto merr2;
            //      }
            //      /* read the floating-point weights */
            //      rc = fread(fvals, sizeof(gafloat), size, pdfi);
            //      if (rc != size) goto merr2;
            //      /* byte swap if necessary */
            //      if ((pdefop2 == 2 && !BYTEORDER) || (pdefop2 == 3 && BYTEORDER)) gabswp(fvals, size);
            //      /* cast to doubles */
            //      for (j = 0; j < size; j++) *(pfi.ppf[i] + j) = (double) fvals[j];
            //      /* sequential -- footer */
            //      if (pdefop1 == 2) {
            //          rc = fread(&rdw, sizeof(int), 1, pdfi);
            //          if (rc != 1) goto merr2;
            //      }
            //  }
            //
            //  /* allocate memory and read in the wind rotation values */
            //  sz = sizeof(double) * size;
            //  if ((pfi.ppw = (double *) galloc(sz, "ppw2")) == NULL) goto merr;
            //  /* sequential -- header */
            //  if (pdefop1 == 2) {
            //      rc = fread(&rdw, sizeof(int), 1, pdfi);
            //      if (rc != 1) goto merr2;
            //  }
            //  rc = fread(fvals, sizeof(gafloat), size, pdfi);
            //  if (rc != size) goto merr2;
            //  /* byte swap if necessary */
            //  if ((pdefop2 == 2 && !BYTEORDER) || (pdefop2 == 3 && BYTEORDER)) gabswp(fvals, size);
            //  /* cast to doubles */
            //  for (i = 0; i < size; i++) *(pfi.ppw + i) = (double) fvals[i];
            //
            //  /* set wind rotation flag */
            //  dw = pfi.ppw;
            //  wflg = 0;
            //  for (i = 0; i < size; i++) {
            //      if (fabs(*dw) > 0.00001) wflg = 1;
            //      dw++;
            //  }
            //  pfi.ppwrot = wflg;
            //
            //  /* If native data is grib, and the "pdef file" keyword is used,
            // then the offsets in the file are assumed to be 0-based.
            // The code in gaprow() expects 1-based offsets, so we add 1
            // and check to make sure offsets don't exceed isize*jsize. */
            //  if (pfi.idxflg && pfi.type == 1 && pfi.pdefgnrl == 0) {
            //      for (i = 0; i < pnum; i++) {
            //          for (j = 0; j < size; j++) {
            //              if (*(pfi.ppi[i] + j) == pfi.ppisiz * pfi.ppjsiz) {
            //                  gaprnt(0, "PDEF FILE Error: The offsets in the pdef file for native \n");
            //                  gaprnt(0, "  GRIB data must be 0-based (i.e., >= 0 and < isize*jsize). \n");
            //                  gaprnt(0, "  Use the PDEF GENERAL keyword for 1-based file offsets.\n");
            //                  goto err;
            //              }
            //              *(pfi.ppi[i] + j) = 1 + *(pfi.ppi[i] + j);
            //          }
            //      }
            //  }
            //  /* If native data is NOT grib, and the "pdef file" keyword is used,
            // then the offsets in the file are assumed to be 1-based.
            // The code in gaprow() expects 1-based offsets, so we just
            // check to make sure offsets don't equal 0. */
            //  if (pfi.idxflg == 0 && pfi.type == 1 && pfi.pdefgnrl == 0) {
            //      for (i = 0; i < pnum; i++) {
            //          for (j = 0; j < size; j++) {
            //              if (*(pfi.ppi[i] + j) == 0) {
            //                  gaprnt(0, "PDEF FILE Error: The offsets in the pdef file \n");
            //                  gaprnt(0, "  must be 1-based (i.e., > 0 and <= isize*jsize). \n");
            //                  goto err;
            //              }
            //          }
            //      }
            //  }
            //  /* The "pdef general" keyword means the offsets in the file are always 1-based.
            // Check to make sure offsets don't equal 0. */
            //  if (pfi.pdefgnrl == 1) {
            //      for (i = 0; i < pnum; i++) {
            //          for (j = 0; j < size; j++) {
            //              if (*(pfi.ppi[i] + j) == 0) {
            //                  gaprnt(0, "PDEF GENERAL Error: The offsets in the pdef file \n");
            //                  gaprnt(0, "  must be 1-based (i.e., > 0 and <= isize*jsize). \n");
            //                  goto err;
            //              }
            //          }
            //      }
            //  }
        } /* matches  else if (pfi.ppflag==8) */

        else
        {
            /* When a supported projection is specified, calculate
           three constants at each lat-lon grid point: offset
           of the ij gridpoint, and the delta x and delta y values. */

            pi = Math.PI;
            ioff = pfi.ppi[0];
            dx = pfi.ppf[0];
            dy = pfi.ppf[1];
            if (pfi.ppwrot > 0) dw = pfi.ppw;

            /* get i,j values in preprojected grid for each lat/lon point */
            for (j = 0; j < pfi.dnum[1]; j++)
            {
                lat = pfi.gr2ab[1](pfi.grvals[1], (double)(j + 1));
                for (i = 0; i < pfi.dnum[0]; i++)
                {
                    lon = pfi.gr2ab[0](pfi.grvals[0], (double)(i + 1));
                    if (pfi.ppflag == 3)
                    {
                        if (pfi.ppwrot > 0)
                        {
                            /* PDEF lccr */
                            ll2lc(pfi.ppvals, lat, lon, ref rii, ref rjj, dw);
                        }
                        else
                        {
                            /* PDEF lcc */
                            ll2lc(pfi.ppvals, lat, lon, ref rii, ref rjj, ref dum);
                        }
                    }
                    else if (pfi.ppflag == 4)
                    {
                        /* PDEF eta.u */
                        ll2eg(pfi.ppisiz, pfi.ppjsiz, pfi.ppvals, lon, lat, ref rii, ref rjj, dw);
                    }
                    else if (pfi.ppflag == 5)
                    {
                        /* PDEF pse */
                        ll2pse(pfi.ppisiz, pfi.ppjsiz, pfi.ppvals, lon, lat, ref rii, ref rjj);
                    }
                    else if (pfi.ppflag == 6)
                    {
                        /* PDEF ops */
                        ll2ops(pfi.ppvals, lon, lat, ref rii, ref rjj);
                    }
                    else if (pfi.ppflag == 9)
                    {
                        if (pfi.ppwrot > 0)
                        {
                            /* PDEF rotllr */
                            ll2rotll(pfi.ppvals, lat, lon, ref rii, ref rjj, dw);
                        }
                        else
                        {
                            /* PDEF rotll */
                            ll2rotll(pfi.ppvals, lat, lon, ref rii, ref rjj, &dum);
                        }
                    }
                    else
                    {
                        /* PDEF nps and sps */
                        w3fb04(lat, -1.0 * lon, pfi.ppvals[3], -1.0 * pfi.ppvals[2], ref rii, ref rjj);
                        rii = rii + pfi.ppvals[0]; /* Normalize based on pole point */
                        rjj = rjj + pfi.ppvals[1];
                        *dw = (pfi.ppvals[2] - lon) * pi / 180.0; /* wind rotation amount */
                        if (pfi.ppflag == 2) *dw = pi - *dw;
                    }

                    ii = (int)rii;
                    jj = (int)rjj;
                    *dx = rii - (double)ii;
                    *dy = rjj - (double)jj;
                    if (ii < 1 || ii > pfi.ppisiz - 1 ||
                        jj < 1 || jj > pfi.ppjsiz - 1)
                    {
                        *ioff = -1;
                    }
                    else
                    {
                        *ioff = (jj - 1) * pfi.ppisiz + ii - 1;
                    }

                    ioff++;
                    dx++;
                    dy++;
                    if (pfi.ppwrot) dw++;
                }
            }
        }

        //if (fvals != NULL) gree(fvals, "f80g");
        return (0);

        merr:
        //gaprnt(0, "Open Error:  Memory allocation error in pdef handler\n");
        goto err;
        merr2:
        //gaprnt(0, "Open Error:  I/O Error on pdef file read\n");
        goto err;

        err:
        // if (pfi.ppi[0] != NULL) {
        //     gree(pfi.ppi[0], "f80a");
        //     pfi.ppi[0] = NULL;
        // }
        // if (pfi.ppf[0] != NULL) {
        //     gree(pfi.ppf[0], "f80c");
        //     pfi.ppf[0] = NULL;
        // }
        // if (pfi.ppf[1] != NULL) {
        //     gree(pfi.ppf[1], "f80d");
        //     pfi.ppf[1] = NULL;
        // }
        // if (pfi.ppwrot && pfi.ppw != NULL) {
        //     gree(pfi.ppw, "f80e");
        //     pfi.ppw = NULL;
        // }
        // if (fvals != NULL) gree(fvals, "f80f");
        return (1);
    }


    public static void w3fb04(double alat, double along, double xmeshl, double orient, ref double xi, ref double xj)
    {
        const double d2r = Math.PI / 180.0;
        const double earthr = 6371.2;

        double re = (earthr * 1.86603) / xmeshl;
        double xlat = alat * d2r;

        if (xmeshl > 0.0)
        {
            double wlong = (along + 180.0 - orient) * d2r;
            double r = (re * Math.Cos(xlat)) / (1.0 + Math.Sin(xlat));
            xi = r * Math.Sin(wlong);
            xj = r * Math.Cos(wlong);
        }
        else
        {
            re = -re;
            xlat = -xlat;
            double wlong = (along - orient) * d2r;
            double r = (re * Math.Cos(xlat)) / (1.0 + Math.Sin(xlat));
            xi = r * Math.Sin(wlong);
            xj = -r * Math.Cos(wlong);
        }
    }

/* Lambert conformal conversion */

    public static void ll2lc(double[] vals, double grdlat, double grdlon, ref double grdi, ref double grdj, ref double wrot)
    {
        const double pi = Math.PI;
        const double pi2 = pi / 2.0;
        const double pi4 = pi / 4.0;
        const double d2r = pi / 180.0;
        const double r2d = 180.0 / pi;
        const double radius = 6371229.0;
        const double omega4 = 4.0 * pi / 86400.0;

        double gcon, ogcon, H, deg, cn1, cn2, cn3, cn4, rih, xih, yih, rrih, check;
        double alnfix, alon, x, y, windrot;
        double latref, lonref, iref, jref, stdlt1, stdlt2, stdlon, delx, dely;

        latref = vals[0];
        lonref = vals[1];
        iref = vals[2];
        jref = vals[3];
        stdlt1 = vals[4];
        stdlt2 = vals[5];
        stdlon = vals[6];
        delx = vals[7];
        dely = vals[8];

        if (stdlt1 == stdlt2)
        {
            gcon = Math.Sin(d2r * (Math.Abs(stdlt1)));
        }
        else
        {
            gcon = (Math.Log(Math.Sin((90.0 - Math.Abs(stdlt1)) * d2r))
                    - Math.Log(Math.Sin((90.0 - Math.Abs(stdlt2)) * d2r)))
                   / (Math.Log(Math.Tan((90.0 - Math.Abs(stdlt1)) * 0.5 * d2r))
                      - Math.Log(Math.Tan((90.0 - Math.Abs(stdlt2)) * 0.5 * d2r)));
        }

        ogcon = 1.0 / gcon;
        H = Math.Abs(stdlt1) / (stdlt1);
        cn1 = Math.Sin((90.0 - Math.Abs(stdlt1)) * d2r);
        cn2 = radius * cn1 * ogcon;
        deg = (90.0 - Math.Abs(stdlt1)) * d2r * 0.5;
        cn3 = Math.Tan(deg);
        deg = (90.0 - Math.Abs(latref)) * d2r * 0.5;
        cn4 = Math.Tan(deg);
        rih = cn2 * Math.Pow((cn4 / cn3), gcon);

        xih = rih * Math.Sin((lonref - stdlon) * d2r * gcon);
        yih = -rih * Math.Cos((lonref - stdlon) * d2r * gcon) * H;
        deg = (90.0 - grdlat * H) * 0.5 * d2r;
        cn4 = Math.Tan(deg);
        rrih = cn2 * Math.Pow((cn4 / cn3), gcon);
        check = 180.0 - stdlon;
        alnfix = stdlon + check;
        alon = grdlon + check;

        while (alon < 0.0) alon = alon + 360.0;
        while (alon > 360.0) alon = alon - 360.0;

        deg = (alon - alnfix) * gcon * d2r;
        x = rrih * Math.Sin(deg);
        y = -rrih * Math.Cos(deg) * H;
        grdi = iref + (x - xih) / delx;
        grdj = jref + (y - yih) / dely;
        windrot = gcon * (stdlon - grdlon) * d2r;
        wrot = windrot;
    }

/* NMC eta ll to xy map  */

    public static void ll2eg(int im, int jm, double[] vals, double grdlon, double grdlat, ref double grdi,
        ref double grdj, ref double alpha)
    {
        const double d2r = Math.PI / 180.0;
        const double r2d = 1.0 / d2r;
        const double earthr = 6371.2;

        double tlm0d = -vals[0]; // convert + W to + E, the grads standard for longitude
        double tph0d = vals[1];
        double dlam = vals[2] * 0.5;
        double dphi = vals[3] * 0.5;

        // convert to radians
        double phi = grdlat * d2r; // grid latitude
        double lam = -grdlon * d2r; // grid longitude, convert +W to +E, the grads standard
        double phi0 = tph0d * d2r; // center latitude
        double lam0 = tlm0d * d2r; // center longitude

        // Transform grid lat/lon
        double x = Math.Cos(phi0) * Math.Cos(phi) * Math.Cos(lam - lam0) + Math.Sin(phi0) * Math.Sin(phi);
        double y = -Math.Cos(phi) * Math.Sin(lam - lam0);
        double z = -Math.Sin(phi0) * Math.Cos(phi) * Math.Cos(lam - lam0) + Math.Cos(phi0) * Math.Sin(phi);
        double biglam = Math.Atan2(y, x) / d2r; // transformed lon in degrees
        double bigphi = Math.Atan2(z, Math.Sqrt(x * x + y * y)) / d2r; // transformed lat in degrees

        // Convert transformed lat/lon -> i,j
        double dlmd = vals[2];
        double dphd = vals[3];
        double wbd = (-1) * 0.5 * (im - 1) * dlmd; // western boundary of transformed grid
        double sbd = (-1) * 0.5 * (jm - 1) * dphd; // southern boundary of transformed grid
        grdi = 1.0 + (biglam - wbd) / dlmd;
        grdj = 1.0 + (bigphi - sbd) / dphd;

        // params for wind rotation alpha, alpha>0 ==> counter clockwise rotation
        double xx = Math.Sin(phi0) * Math.Sin(biglam * d2r) / Math.Cos(phi);
        if (xx < -1.0) xx = -1.0;
        else if (xx > 1.0) xx = 1.0;
        alpha = (-1) * Math.Asin(xx);
    }

    public static void ll2pse(int im, int jm, double[] vals, double lon, double lat, ref double grdi, ref double grdj)
    {
        const double rearth = 6378.273e3;
        const double eccen2 = 0.006693883;
        const double pi = Math.PI;

        double cdr, alat, along, e, e2;
        double t, x, y, rho, sl, tc, mc;
        double slat, slon, xorig, yorig, sgn, polei, polej, dx, dy;

        slat = vals[0];
        slon = vals[1];
        polei = vals[2];
        polej = vals[3];
        dx = vals[4] * 1000;
        dy = vals[5] * 1000;
        sgn = vals[6];

        xorig = -polei * dx;
        yorig = -polej * dy;

        cdr = 180.0 / pi;
        alat = lat / cdr;
        along = lon / cdr;
        e2 = eccen2;
        e = Math.Sqrt(eccen2);

        if (Math.Abs(lat) > 90.0)
        {
            grdi = -1;
            grdj = -1;
            return;
        }
        else
        {
            t = Math.Tan(pi / 4.0 - alat / 2.0) /
                Math.Pow((1.0 - e * Math.Sin(alat)) / (1.0 + e * Math.Sin(alat)), e / 2.0);

            if (Math.Abs(90.0 - slat) < 0.0001)
            {
                rho = 2.0 * rearth * t /
                      Math.Pow(Math.Pow(1.0 + e, 1.0 + e) * Math.Pow(1.0 - e, 1.0 - e), e / 2.0);
            }
            else
            {
                sl = slat / cdr;
                tc = Math.Tan(pi / 4.0 - sl / 2.0) /
                     Math.Pow((1.0 - e * Math.Sin(sl)) / (1.0 + e * Math.Sin(sl)), (e / 2.0));
                mc = Math.Cos(sl) / Math.Sqrt(1.0 - e2 * Math.Sin(sl) * Math.Sin(sl));
                rho = rearth * mc * t / tc;
            }

            x = rho * sgn * Math.Cos(sgn * (along + slon / cdr));
            y = rho * sgn * Math.Sin(sgn * (along + slon / cdr));

            grdi = (x - xorig) / dx + 1;
            grdj = (y - yorig) / dy + 1;

            return;
        }
    }

    public static void ll2ops(double[] vals, double lni, double lti, ref double grdi, ref double grdj)
    {
        const double radius = 6371229.0;

        double stdlat, stdlon, xref, yref, xiref, yjref, delx, dely;
        double plt, pln;
        double pi180, c1, c2, c3, c4, c5, c6, arg2a, bb, plt1, alpha, pln1, plt90, argu1, argu2;
        double hsign, glor, rstdlon, glolim, facpla, x, y;

        stdlat = vals[0];
        stdlon = vals[1];
        xref = vals[2];
        yref = vals[3];
        xiref = vals[4];
        yjref = vals[5];
        delx = vals[6];
        dely = vals[7];

        c1 = 1.0;
        pi180 = Math.Asin(c1) / 90.0;

        /* set flag for n/s hemisphere and convert longitude to <0 ; 360> gainterval */
        if (stdlat >= 0.0)
        {
            hsign = 1.0;
        }
        else
        {
            hsign = -1.0;
        }

        /* set flag for n/s hemisphere and convert longitude to <0 ; 360> interval */
        glor = lni;
        if (glor <= 0.0) glor = 360.0 + glor;
        rstdlon = stdlon;
        if (rstdlon < 0.0) rstdlon = 360.0 + stdlon;

        /* test for a n/s pole case */
        if (stdlat == 90.0)
        {
            plt = lti;
            pln = (glor + 270.0) % 360.0;
            goto l2000;
        }

        if (stdlat == -90.0)
        {
            plt = -lti;
            pln = (glor + 270.0) % 360.0;
            goto l2000;
        }

        /* test for longitude on 'greenwich or date line' */
        if (glor == rstdlon)
        {
            if (lti > stdlat)
            {
                plt = 90.0 - lti + stdlat;
                pln = 90.0;
            }
            else
            {
                plt = 90.0 - stdlat + lti;
                pln = 270.0;
            }

            goto l2000;
        }

        if ((glor + 180.0) % 360.0 == rstdlon)
        {
            plt = stdlat - 90.0 + lti;
            if (plt < -90.0)
            {
                plt = -180.0 - plt;
                pln = 270.0;
            }
            else
            {
                pln = 90.0;
            }

            goto l2000;
        }

        /* determine longitude distance relative to rstdlon so it belongs to
         the absolute interval 0 - 180 */
        argu1 = glor - rstdlon;
        if (argu1 > 180.0) argu1 = argu1 - 360.0;
        if (argu1 < -180.0) argu1 = argu1 + 360.0;

        /* 1. get the help circle bb and angle alpha (legalize arguments) */

        c2 = lti * pi180;
        c3 = argu1 * pi180;
        arg2a = Math.Cos(c2) * Math.Cos(c3);
        if (-c1 > arg2a) arg2a = -c1; /* arg2a = max1(arg2a,-c1)  */
        if (c1 < arg2a) arg2a = c1; /* min1(arg2a, c1)         */
        bb = Math.Acos(arg2a);

        c4 = hsign * lti * pi180;
        arg2a = Math.Sin(c4) / Math.Sin(bb);
        if (-c1 > arg2a) arg2a = -c1; /* arg2a = dmax1(arg2a,-c1) */
        if (c1 < arg2a) arg2a = c1; /* arg2a = dmin1(arg2a, c1) */
        alpha = Math.Asin(arg2a);

        /* 2. get plt and pln (still legalizing arguments) */
        c5 = stdlat * pi180;
        c6 = hsign * stdlat * pi180;
        arg2a = Math.Cos(c5) * Math.Cos(bb) + Math.Sin(c6) * Math.Sin(c4);
        if (-c1 > arg2a) arg2a = -c1; /* arg2a = dmax1(arg2a,-c1) */
        if (c1 < arg2a) arg2a = c1; /* arg2a = dmin1(arg2a, c1) */
        plt1 = Math.Asin(arg2a);

        arg2a = Math.Sin(bb) * Math.Cos(alpha) / Math.Cos(plt1);

        if (-c1 > arg2a) arg2a = -c1; /* arg2a = dmax1(arg2a,-c1) */
        if (c1 < arg2a) arg2a = c1; /* arg2a = dmin1(arg2a, c1) */
        pln1 = Math.Asin(arg2a);


        /* test for passage of the 90 degree longitude (duallity in pln)
         get plt for which pln=90 when lti is the latitude */
        arg2a = Math.Sin(c4) / Math.Sin(c6);
        if (-c1 > arg2a) arg2a = -c1; /* arg2a = dmax1(arg2a,-c1) */
        if (c1 < arg2a) arg2a = c1; /* arg2a = dmin1(arg2a, c1) */
        plt90 = Math.Asin(arg2a);

        /* get help arc bb and angle alpha */
        arg2a = Math.Cos(c5) * Math.Sin(plt90);
        if (-c1 > arg2a) arg2a = -c1; /* arg2a = dmax1(arg2a,-c1) */
        if (c1 < arg2a) arg2a = c1; /* arg2a = dmin1(arg2a, c1) */
        bb = Math.Acos(arg2a);

        arg2a = Math.Sin(c4) / Math.Sin(bb);
        if (-c1 > arg2a) arg2a = -c1; /* arg2a = dmax1(arg2a,-c1) */
        if (c1 < arg2a) arg2a = c1; /* arg2a = dmin1(arg2a, c1) */
        alpha = Math.Asin(arg2a);

        /* get glolim - it is nesc. to test for the existence of solution */
        argu2 = Math.Cos(c2) * Math.Cos(bb) / (1.0 - Math.Sin(c4) * Math.Sin(bb) * Math.Sin(alpha));
        if (Math.Abs(argu2) > c1)
        {
            glolim = 999.0;
        }
        else
        {
            glolim = Math.Acos(argu2) / (Math.PI / 180.0);
        }

        /* modify (if nesc.) the pln solution */
        if ((Math.Abs(argu1) > glolim && lti <= stdlat) || (lti > stdlat))
        {
            pln1 = (Math.PI / 180.0) * 180.0 - pln1;
        }

        /* the solution is symmetric so the direction must be if'ed */
        if (argu1 < 0.0)
        {
            pln1 = -pln1;
        }

        /* convert the radians to degrees */
        plt = plt1 / (Math.PI / 180.0);
        pln = pln1 / (Math.PI / 180.0);

        /* to obtain a rotated value (ie so x-axis in pol.ste. points east)
         add 270 to longitude */
        pln = (pln + 270.0) % 360.0;

        l2000:

        /*
        c     this program convert polar stereographic coordinates to x,y ditto
        c     longitude:   0 - 360  ; positive to the east
        c     latitude : -90 -  90  ; positive for northern hemisphere
        c     it is assumed that the x-axis point towards the east and
        c     corresponds to longitude = 0
        c
        c     tsp 20/06-89
        c
        c     constants and functions
        */
        facpla = radius * 2.0 / (1.0 + Math.Sin(plt * (Math.PI / 180.0))) * Math.Cos(plt * (Math.PI / 180.0));
        x = facpla * Math.Cos(pln * (Math.PI / 180.0));
        y = facpla * Math.Sin(pln * (Math.PI / 180.0));

        grdi = (x - xref) / delx + xiref;
        grdj = (y - yref) / dely + yjref;

        return;
    }

/* Projection definition for rotated lat/lon
 *
 * The transformation is done as described in the
 * COSMO documentation, Part 1, chapter 3.3.
 * http://www.cosmo-model.org/public/documentation.htm
 */

    public static void ll2rotll(double[] vals, double grdlat, double grdlon, ref double grdi, ref double grdj, ref double wrot)
    {
        const double pi = Math.PI;
        double lon_pole; // longitude of the pole in radians
        double lat_pole; // latitude of the pole in radians
        double dlon; // longitude increment in radians
        double dlat; // latitude increment in radians
        double lon_ll_corner; // longitude of the lower left corner in radians
        double lat_ll_corner; // latitude of the lower left corner in radians
        double lon_rotated; // rotated longitude in radians
        double lat_rotated; // rotated latitude in radians
        double lon_RW; // real world longitude in radians
        double lat_RW; // real world latitude in radians

        // grab projection parameters from the pdef line
        lon_pole = vals[0] / 180.0 * pi;
        lat_pole = vals[1] / 180.0 * pi;
        dlon = vals[2] / 180.0 * pi;
        dlat = vals[3] / 180.0 * pi;
        lon_ll_corner = vals[4] / 180.0 * pi;
        lat_ll_corner = vals[5] / 180.0 * pi;

        lat_RW = grdlat / 180 * pi;
        lon_RW = grdlon / 180 * pi;

        // calculate rotated longitude and latitude
        lat_rotated = Math.Asin(
            Math.Sin(lat_RW) * Math.Sin(lat_pole)
            + Math.Cos(lat_RW) * Math.Cos(lat_pole)
                               * Math.Cos(lon_RW - lon_pole)
        );
        lon_rotated = Math.Atan(
            Math.Cos(lat_RW) * Math.Sin(lon_RW - lon_pole)
            / (Math.Cos(lat_RW) * Math.Sin(lat_pole)
                                * Math.Cos(lon_RW - lon_pole)
               - Math.Sin(lat_RW) * Math.Cos(lat_pole))
        );

        // calculate grid point number
        grdj = (lat_rotated - lat_ll_corner) / dlat + 1;
        grdi = (lon_rotated - lon_ll_corner) / dlon + 1;

        // calculate wind rotation angle
        wrot = -Math.Atan(
            Math.Cos(lat_pole) * Math.Sin(lon_pole - lon_RW)
            / (Math.Cos(lat_RW) * Math.Sin(lat_pole)
               - Math.Sin(lat_RW) * Math.Cos(lat_pole) * Math.Cos(lon_pole - lon_RW)
            )
        );
    }
}