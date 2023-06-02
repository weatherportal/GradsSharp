using System.Reflection;
using System.Reflection.Metadata;
using GradsSharp.Models.Internal;
using Microsoft.Extensions.Logging;

namespace GradsSharp.Drawing.Grads;

struct mapcache
{
    public int size; /* size of the file cached here */
    public string name; /* file name cached here */
    public byte[] data; /* contents of the file cached here */
};

internal class GxWmap
{
    private DrawingContext _drawingContext;

    private static int CACHEMAX = 2000000;
    private static List<mapcache> manchor = new();
    private mapcache cmc;
    
    static int imap;
    static double lomin, lomax, lamin, lamax;
    static double lonref; /* Reference longitude for adjustment */
    static int adjtyp = 0; /* Direction adjustment class */

    static int mcpos; /* current position in cached file */
    static int mclen; /* length of the data in current cache */
    static Stream mfile; /* for file i/o instead of caching */
    static int cflag; /* indicate if i/o from cache or file */

    public GxWmap(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public void gxrsmapt()
    {
        adjtyp = 0;
    }

    public void gxdmap(mapopt mopt)
    {
        double[] lon = new double[255], lat = new double[255];
        double xx, yy, lnmin, lnmax, ltmin, ltmax, lnfact;
        int num, i, ipen, rc, type, ilon, ilat, rnum, flag, st1, st2, spos;
        float sln1, sln2, slt1, slt2;
        float lnsav, ltsav, lndif, ltdif, lntmp, lttmp, llinc, llsum, lldist;
        string fname;
        byte[] hdr = new byte[3], rec = new byte[1530];

        llinc = (float)GaUtil.hypot(mopt.lnmax - mopt.lnmin, mopt.ltmax - mopt.ltmin);
        llinc = llinc / 200;
        if (llinc < 0.0001) llinc = 0.0001f;

        /* Open the map data set */

        if (mopt.mpdset[0] == '/' || mopt.mpdset[0] == '\\')
        {
            imap = gxwopen(mopt.mpdset, "rb");
            if (imap == 0)
            {
                _drawingContext.Logger?.LogInformation($"Open Error on Map Data Set: {mopt.mpdset}");
                return;
            }
        }
        else
        {
            fname = mopt.mpdset;
            imap = gxwopen(fname, "rb");
            if (imap == 0)
            {
                imap = gxwopen(mopt.mpdset, "rb", true);
                if (imap == 0)
                {
                    throw new FileNotFoundException($"Open Error on Map Data Set: {fname}");
                    return;
                }
            }
        }

        /* Read and process each record */

        rnum = 0;
        while (true)
        {
            if (cflag>0) rc = gxwread(out hdr, 3);
            else rc = mfile.Read(hdr, 0, 3);
            if (rc != 3) break;
            rnum++;
            i = gagby(hdr, 0, 1);
            if (i < 1 || i > 3)
            {
                throw new Exception("Map file format error: Invalid rec type {i} rec num {rnum}");
            }

            if (i == 2)
            {
                st1 = gagby(hdr, 1, 1);
                st2 = gagby(hdr, 2, 1);
                if (cflag>0) gxwread(out rec, 16);
                else mfile.Read(rec, 0, 16);
                spos = gagby(rec, 0, 4);
                ilon = gagby(rec, 4, 3);
                sln1 = (float)(((float)ilon) / 1e4);
                ilon = gagby(rec, 7, 3);
                sln2 = (float)(((float)ilon) / 1e4);
                ilat = gagby(rec, 10, 3);
                slt1 = (float)(((float)ilat) / 1e4 - 90.0);
                ilat = gagby(rec, 13, 3);
                slt2 = (float)(((float)ilat) / 1e4 - 90.0);
                flag = 0;
                for (i = 0; i < 256; i++)
                {
                    if (mopt.mcol[i] != -9 && i >= st1 && i <= st2) flag = 1;
                }

                if (flag == 0)
                {
                    if (spos == 0)
                    {
                        if (cflag>0) gxwclose(imap);
                        else mfile.Close();
                        return;
                    }

                    if (cflag > 0) gxwseek(spos);
                    else mfile.Seek(spos, SeekOrigin.Begin);
                    continue;
                }

                flag = 0;
                if (sln1 > 360.0) flag = 1;
                else
                {
                    if (slt2 <= mopt.ltmin || slt1 >= mopt.ltmax) flag = 0;
                    else
                    {
                        lnfact = 0.0;
                        while (sln2 + lnfact > mopt.lnmin) lnfact -= 360.0;
                        lnfact += 360.0;
                        if (sln1 + lnfact >= mopt.lnmax) flag = 0;
                        else flag = 1;
                    }
                }

                if (flag == 0)
                {
                    if (spos == 0)
                    {
                        if (cflag > 0) gxwclose(imap);
                        else mfile.Close();
                        return;
                    }

                    if (cflag > 0) gxwseek(spos);
                    else mfile.Seek(spos, SeekOrigin.Begin);
                }

                continue;
            }

            type = gagby(hdr, 1, 1);
            num = gagby(hdr, 2, 1);

            /* The lowres map has only one type:
             1 -- coastlines.
           The mres and hires maps have three types:
             0 -- coastlines
         1 -- political boundaries
         2 -- US state boundaries
        */


            /* Read the next record; convert the data points;
           and get the lat/lon bounds for this line segment */

            if (cflag > 0) gxwread(out rec, num * 6);
            else mfile.Read(rec, 0, num * 6);
            if (mopt.mcol[type] == -9) continue;
            if (mopt.mcol[type] == -1)
            {
                _drawingContext.GradsDrawingInterface.SetDrawingColor(mopt.dcol);
                _drawingContext.GradsDrawingInterface.gxstyl(mopt.dstl);
                _drawingContext.GradsDrawingInterface.gxwide(mopt.dthk);
            }
            else
            {
                _drawingContext.GradsDrawingInterface.SetDrawingColor(mopt.mcol[type]);
                _drawingContext.GradsDrawingInterface.gxstyl(mopt.mstl[type]);
                _drawingContext.GradsDrawingInterface.gxwide(mopt.mthk[type]);
            }

            lnmin = 9999.9;
            lnmax = -9999.9;
            ltmin = 9999.9;
            ltmax = -9999.9;
            for (i = 0; i < num; i++)
            {
                ilon = gagby(rec, i * 6, 3);
                ilat = gagby(rec, i * 6 + 3, 3);
                lat[i] = ((float)ilat) / 1e4 - 90.0;
                lon[i] = ((float)ilon) / 1e4;
                if (lat[i] < ltmin) ltmin = lat[i];
                if (lat[i] > ltmax) ltmax = lat[i];
                if (lon[i] < lnmin) lnmin = lon[i];
                if (lon[i] > lnmax) lnmax = lon[i];
            }

            /* Plot this line segment if it falls within the
           appropriate lat/lon bounds */

            if (ltmax < mopt.ltmin) continue;
            if (ltmin > mopt.ltmax) continue;

            lnfact = 0.0;
            while (lnmax + lnfact > mopt.lnmin) lnfact -= 360.0;
            lnfact += 360.0;

            while (lnmin + lnfact < mopt.lnmax)
            {
                if (lnmax + lnfact < mopt.lnmin)
                {
                    lnfact += 360.0;
                    continue;
                }

                /* Split long lines into shorter segments and limit
             drawing at lat-lon bounds */

                ipen = 3;
                lnsav = (float)lon[0];
                ltsav = (float)lat[0];
                for (i = 1; i < num; i++)
                {
                    lndif = (float)Math.Abs(lon[i] - lon[i - 1]);
                    ltdif = (float)Math.Abs(lat[i] - lat[i - 1]);
                    if (lndif > ltdif) lldist = lndif;
                    else lldist = ltdif;
                    llsum = llinc;
                    lntmp = lnsav;
                    lttmp = ltsav;
                    while (llsum < lldist + llinc)
                    {
                        if (llsum >= lldist - llinc / 4.0)
                        {
                            lntmp = (float)lon[i];
                            lttmp = (float)lat[i];
                            llsum += llinc; /* Insure loop dropout */
                        }
                        else
                        {
                            if (lndif > ltdif)
                            {
                                if (lon[i - 1] < lon[i])
                                {
                                    lntmp += llinc;
                                    lttmp += (float)(llinc * (lat[i] - lat[i - 1]) / (lon[i] - lon[i - 1]));
                                }
                                else
                                {
                                    lntmp -= llinc;
                                    lttmp -= (float)(llinc * (lat[i] - lat[i - 1]) / (lon[i] - lon[i - 1]));
                                }
                            }
                            else
                            {
                                if (lat[i - 1] < lat[i])
                                {
                                    lttmp += llinc;
                                    lntmp += (float)(llinc * (lon[i] - lon[i - 1]) / (lat[i] - lat[i - 1]));
                                }
                                else
                                {
                                    lttmp -= llinc;
                                    lntmp -= (float)(llinc * (lon[i] - lon[i - 1]) / (lat[i] - lat[i - 1]));
                                }
                            }
                        }

                        if (lntmp + lnfact < mopt.lnmin ||
                            lntmp + lnfact > mopt.lnmax ||
                            lttmp < mopt.ltmin || lttmp > mopt.ltmax)
                        {
                            if (ipen == 2)
                            {
                                GradsDrawingInterface.gxconv(lntmp + lnfact, lttmp, out xx, out yy, 2);
                                _drawingContext.GradsDrawingInterface.gxplot(xx, yy, ipen);
                            }

                            ipen = 3;
                        }
                        else
                        {
                            if (ipen == 3)
                            {
                                GradsDrawingInterface.gxconv(lnsav + lnfact, ltsav, out xx, out yy, 2);
                                _drawingContext.GradsDrawingInterface.gxplot(xx, yy, ipen);
                            }

                            ipen = 2;
                            GradsDrawingInterface.gxconv(lntmp + lnfact, lttmp, out xx, out yy, 2);
                            _drawingContext.GradsDrawingInterface.gxplot(xx, yy, ipen);
                        }

                        lnsav = lntmp;
                        ltsav = lttmp;
                        llsum += llinc;
                    }
                }

                lnfact += 360.0;
            }
        }

        if (cflag > 0) gxwclose(imap);
        else mfile.Close();
    }

/* Routine to set up scaling for lat-lon projection.  The aspect
   ratio is *not* maintained.                                   */

    internal int gxscld(mapprj mpj, bool xflip, bool yflip)
    {
        float x1, x2, y1, y2;

        if (mpj.lnmn >= mpj.lnmx) return (1);
        if (mpj.ltmn >= mpj.ltmx) return (1);
        if (mpj.xmn >= mpj.xmx) return (1);
        if (mpj.ymn >= mpj.ymx) return (1);
        mpj.axmn = mpj.xmn;
        mpj.axmx = mpj.xmx;
        mpj.aymn = mpj.ymn;
        mpj.aymx = mpj.ymx;
        x1 = (float)mpj.lnmn;
        x2 = (float)mpj.lnmx;
        y1 = (float)mpj.ltmn;
        y2 = (float)mpj.ltmx;
        if (xflip)
        {
            x1 = (float)mpj.lnmx;
            x2 = (float)mpj.lnmn;
        }

        if (yflip)
        {
            y1 = (float)mpj.ltmx;
            y2 = (float)mpj.ltmn;
        }

        _drawingContext.GradsDrawingInterface.gxscal(mpj.axmn, mpj.axmx, mpj.aymn, mpj.aymx, x1, x2, y1, y2);
        _drawingContext.GradsDrawingInterface.gxproj(null);
        adjtyp = 0;
        return (0);
    }

/* Routine to set up scaling for lat-lon projection.  Aspect
   ratio of the projection is maintained as a constant, and it
   fills the plotting area as much as possible.                 */

    public int gxltln(mapprj mpj)
    {
        float lndif, ltdif, aspect, aspect2, xdif, xlo, xhi, ydif, ylo, yhi;

        if (mpj.lnmn >= mpj.lnmx) return (1);
        if (mpj.ltmn >= mpj.ltmx) return (1);
        if (mpj.xmn >= mpj.xmx) return (1);
        if (mpj.ymn >= mpj.ymx) return (1);

        lndif = (float)(mpj.lnmx - mpj.lnmn);
        ltdif = (float)(mpj.ltmx - mpj.ltmn);
        aspect = (float)(1.2 * ltdif / lndif);
        aspect2 = (float)((mpj.ymx - mpj.ymn) / (mpj.xmx - mpj.xmn));
        if (aspect > aspect2)
        {
            xdif = (float)((mpj.xmx - mpj.xmn) * aspect2 / aspect);
            xlo = (float)(((mpj.xmx - mpj.xmn) / 2.0) - (xdif * 0.5));
            xhi = (float)(((mpj.xmx - mpj.xmn) / 2.0) + (xdif * 0.5));
            mpj.axmx = mpj.xmn + xhi;
            mpj.axmn = mpj.xmn + xlo;
            mpj.aymn = mpj.ymn;
            mpj.aymx = mpj.ymx;
        }
        else
        {
            ydif = (float)((mpj.ymx - mpj.ymn) * aspect / aspect2);
            ylo =(float)(((mpj.ymx - mpj.ymn) / 2.0) - (ydif * 0.5));
            yhi = (float)(((mpj.ymx - mpj.ymn) / 2.0) + (ydif * 0.5));
            mpj.aymx = mpj.ymn + yhi;
            mpj.aymn = mpj.ymn + ylo;
            mpj.axmn = mpj.xmn;
            mpj.axmx = mpj.xmx;
        }

        _drawingContext.GradsDrawingInterface.gxscal(mpj.axmn, mpj.axmx, mpj.aymn, mpj.aymx,
            mpj.lnmn, mpj.lnmx, mpj.ltmn, mpj.ltmx);
        _drawingContext.GradsDrawingInterface.gxproj(null);
        adjtyp = 0;
        return (0);
    }

/* Routine for north polar stereographic.  Projection scaling
   is set along with level 1 linear scaling.   The only difficult
   aspect to this is to set the level 1 linear scaling such that
   the proper aspect ratio is maintained.   */

    static float londif;

    public int gxnste(mapprj mpj)
    {
        double x1, x2, y1, y2, dum, lonave;
        double w1, xave, yave;
        double lonmn, lonmx, latmn, latmx, xmin, xmax, ymin, ymax;

        lonmn = mpj.lnmn;
        lonmx = mpj.lnmx;
        latmn = mpj.ltmn;
        latmx = mpj.ltmx;
        xmin = mpj.xmn;
        xmax = mpj.xmx;
        ymin = mpj.ymn;
        ymax = mpj.ymx;

        if ((lonmx - lonmn) > 360.0)
        {
            return (1);
        }

        if (lonmn < -360.0 || lonmx > 360.0)
        {
            return (1);
        }

        if (latmn < -80.0 || latmx > 90.0)
        {
            return (1);
        }

        if (latmn >= latmx || lonmn >= lonmx || xmin >= xmax || ymin >= ymax)
        {
            return (1);
        }

        lonave = (lonmx + lonmn) / 2.0; /* Longitude adjustment to put */
        londif = (float)(-90.0 - lonave); /*  central meridian at bottom.*/
        lonref = lonave;

        /* Plotting limits depend on how much of the hemisphere we are
         actually plotting.  */

        if ((lonmx - lonmn) < 180.0)
        {
            gxnpst(lonmn, latmn, out x1, out dum); /* Left side coord  */
            gxnpst(lonmx, latmn, out x2, out dum); /* Right side coord */
            gxnpst(lonmn, latmx, out dum, out y2); /* Top coord        */
            gxnpst(lonave, latmn, out dum, out y1); /* Bottom coord     */
        }
        else
        {
            gxnpst(lonave - 90.0, latmn, out x1, out dum); /* Left side coord  */
            gxnpst(lonave + 90.0, latmn, out x2, out dum); /* Right side coord */
            gxnpst(lonmn, latmn, out dum, out y2); /* Top coord        */
            gxnpst(lonave, latmn, out dum, out y1); /* Bottom coord     */
        }

        /* Set up linear level scaling while maintaining aspect ratio.   */

        if (((xmax - xmin) / (ymax - ymin)) > ((x2 - x1) / (y2 - y1)))
        {
            w1 = 0.5 * (ymax - ymin) * (x2 - x1) / (y2 - y1);
            xave = (xmax + xmin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xave - w1, xave + w1, ymin, ymax, x1, x2, y1, y2);
            mpj.axmn = xave - w1;
            mpj.axmx = xave + w1;
            mpj.aymn = ymin;
            mpj.aymx = ymax;
        }
        else
        {
            w1 = 0.5 * (xmax - xmin) * (y2 - y1) / (x2 - x1);
            yave = (ymax + ymin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            mpj.axmn = xmin;
            mpj.axmx = xmax;
            mpj.aymn = yave - w1;
            mpj.aymx = yave + w1;
        }

        _drawingContext.GradsDrawingInterface.gxproj(gxnpst);
        _drawingContext.GradsDrawingInterface.gxback(gxnrev);
        adjtyp = 1;
        return (0);
    }

    public Tuple<double, double> gxnpst(double rlon, double rlat)
    {
        double x, y;
        gxnpst(rlon, rlat, out x, out y);
        return new Tuple<double, double>(x, y);
    }
    void gxnpst(double rlon, double rlat, out double x, out double y)
    {
        double radius, theta;

        radius = Math.Tan(0.785315 - (0.00872572 * rlat));
        theta = (rlon + londif) * 0.0174514;
        x = radius * Math.Cos(theta);
        y = radius * Math.Sin(theta);
    }

/* Routine for back transform for npst */
    public Tuple<double, double> gxnrev(double x, double y )
    {
        double rlon, rlat;
        gxnrev(x, y, out rlon, out rlat);
        return new Tuple<double, double>(rlon, rlat);
    }
    void gxnrev(double x, double y, out double rlon, out double rlat)
    {
        double rad, alpha;

        rad = GaUtil.hypot(x, y);
        alpha = 180.0 * Math.Atan(rad) / Math.PI;
        rlat = 90.0 - 2.0 * alpha;

        if (x == 0.0 && y == 0.0) rlon = 0.0;
        else
        {
            rlon = (180.0 * Math.Atan2(y, x) / Math.PI) - londif;
            while (rlon < lonref - 180.0) rlon += 360.0;
            while (rlon > lonref + 180.0) rlon -= 360.0;
        }
    }

/* Routine for south polar stereographic.  Projection scaling
   is set along with level 1 linear scaling.   The only difficult
   aspect to this is to set the level 1 linear scaling such that
   the proper aspect ratio is maintained.   */

    public int gxsste(mapprj mpj)
    {
        double x1, x2, y1, y2, dum, lonave;
        double w1, xave, yave;
        double lonmn, lonmx, latmn, latmx, xmin, xmax, ymin, ymax;

        lonmn = mpj.lnmn;
        lonmx = mpj.lnmx;
        latmn = mpj.ltmn;
        latmx = mpj.ltmx;
        xmin = mpj.xmn;
        xmax = mpj.xmx;
        ymin = mpj.ymn;
        ymax = mpj.ymx;

        if ((lonmx - lonmn) > 360.0)
        {
            return (1);
        }

        if (lonmn < -360.0 || lonmx > 360.0)
        {
            return (1);
        }

        if (latmn < -90.0 || latmx > 80.0)
        {
            return (1);
        }

        if (latmn >= latmx || lonmn >= lonmx || xmin >= xmax || ymin >= ymax)
        {
            return (1);
        }

        lonave = (lonmx + lonmn) / 2.0; /* Longitude adjustment to put */
        londif = (float)(-90.0 - lonave); /*  central meridian at bottom.*/
        lonref = lonave;

        /* Plotting limits depend on how much of the hemisphere we are
         actually plotting.  */

        if ((lonmx - lonmn) < 180.0)
        {
            gxspst(lonmn, latmx, out x1, out dum); /* Left side coord  */
            gxspst(lonmx, latmx, out x2, out dum); /* Right side coord */
            gxspst(lonmn, latmn, out dum, out y1); /* Top coord        */
            gxspst(lonave, latmx, out dum, out y2); /* Bottom coord     */
        }
        else
        {
            gxspst(lonave - 90.0, latmx, out x1, out dum); /* Left side coord  */
            gxspst(lonave + 90.0, latmx, out x2, out dum); /* Right side coord */
            gxspst(lonmn, latmx, out dum, out y1); /* Top coord        */
            gxspst(lonave, latmx, out dum, out y2); /* Bottom coord     */
        }

        /* Set up linear level scaling while maintaining aspect ratio.   */

        if (((xmax - xmin) / (ymax - ymin)) > ((x2 - x1) / (y2 - y1)))
        {
            w1 = 0.5 * (ymax - ymin) * (x2 - x1) / (y2 - y1);
            xave = (xmax + xmin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xave - w1, xave + w1, ymin, ymax, x1, x2, y1, y2);
            mpj.axmn = xave - w1;
            mpj.axmx = xave + w1;
            mpj.aymn = ymin;
            mpj.aymx = ymax;
        }
        else
        {
            w1 = 0.5 * (xmax - xmin) * (y2 - y1) / (x2 - x1);
            yave = (ymax + ymin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            mpj.axmn = xmin;
            mpj.axmx = xmax;
            mpj.aymn = yave - w1;
            mpj.aymx = yave + w1;
        }

        _drawingContext.GradsDrawingInterface.gxproj(gxspst);
        _drawingContext.GradsDrawingInterface.gxback(gxsrev);
        adjtyp = 2;
        return (0);
    }

    public Tuple<double, double> gxspst(double rlon, double rlat)
    {
        double x=0;
        double y = 0;
        gxspst(rlon, rlat, out x, out y);
        return new Tuple<double, double>(x, y);
    }
    public void gxspst(double rlon, double rlat, out double x, out double y)
    {
        double radius, theta;

        radius = Math.Tan(0.785315 + (0.00872572 * rlat));
        theta = (rlon + londif) * (-0.0174514);
        x = radius * Math.Cos(theta);
        y = radius * Math.Sin(theta);
    }

/* Routine for back transform for spst */
    public Tuple<double, double> gxsrev(double x, double y)
    {
        double rlon = 0;
        double rlat = 0;
        gxsrev(x, y, out rlon, out rlat);
        return new Tuple<double, double>(rlon, rlat);
    }
    public void gxsrev(double x, double y, out double rlon, out double rlat)
    {
        double rad, alpha;

        rad = GaUtil.hypot(x, y);
        alpha = 180.0 * Math.Atan(rad) / Math.PI;
        rlat = 2.0 * alpha - 90.0;

        if (x == 0.0 && y == 0.0) rlon = 0.0;
        else
        {
            rlon = (-180.0 * Math.Atan2(y, x) / Math.PI) - londif;
            while (rlon < lonref - 180.0) rlon += 360.0;
            while (rlon > lonref + 180.0) rlon -= 360.0;
        }
    }

/* Return adjustment angle (in radians) to apply to a wind direction
   to correct for current map projection and position. */

    public double gxaarw(double lon, double lat)
    {
        double xx1, yy1, xx2, yy2, dir;

        if (adjtyp == 0) return (0.0);
        if (adjtyp == 1)
        {
            lon = (lon - lonref) * Math.PI / 180.0;
            return (lon);
        }

        if (adjtyp == 2)
        {
            lon = (lonref - lon) * Math.PI / 180.0;
            return (lon);
        }

        /* For type 3 map projections that lack back transforms, estimate the north
         direction using finite difference. */

        if (adjtyp == 3)
        {
            if (lat > 89.9)
            {
                /* back difference if near np */
                GradsDrawingInterface.gxconv(lon, lat - 0.05, out xx1, out yy1, 2);
                GradsDrawingInterface.gxconv(lon, lat, out xx2, out yy2, 2);
            }
            else if (lat < -89.9)
            {
                /* forward difference if near sp */
                GradsDrawingInterface.gxconv(lon, lat, out xx1, out yy1, 2);
                GradsDrawingInterface.gxconv(lon, lat + 0.05, out xx2, out yy2, 2);
            }
            else
            {
                /* otherwise centered diff */
                GradsDrawingInterface.gxconv(lon, lat - 0.03, out xx1, out yy1, 2);
                GradsDrawingInterface.gxconv(lon, lat + 0.03, out xx2, out yy2, 2);
            }

            dir = Math.Atan2(xx1 - xx2, yy2 - yy1);
            return (dir);
        }

        /* type 4 map projections do not have lat/lon lines that cross at
         right angles (non-conformal).  This is too hard to deal with.  */

        return (-999.9);
    }

/*  Set up Robinson Projection */

    static double fudge;

    public int gxrobi(mapprj mpj)
    {
        double lonmn, lonmx, latmn, latmx, xmin, xmax, ymin, ymax;
        double x1, x2, y1, y2, xd, yd, xave, yave, w1;

        lonmn = mpj.lnmn;
        lonmx = mpj.lnmx;
        latmn = mpj.ltmn;
        latmx = mpj.ltmx;
        xmin = mpj.xmn;
        xmax = mpj.xmx;
        ymin = mpj.ymn;
        ymax = mpj.ymx;

        /* Check for errors */

        fudge = 0.0;
        if (lonmn < -180.0 || lonmx > 180.0 || latmn < -90.0 || latmx > 90.0)
        {
            if (GaUtil.dequal(lonmn, 0.0, 1e-7)>0 || GaUtil.dequal(lonmx, 360.0, 1e-7)>0) return (1);
            else fudge = 180.0;
        }

        if (latmn >= latmx || lonmn >= lonmx || xmin >= xmax || ymin >= ymax)
        {
            return (1);
        }

        /* Get bounds of the map in linear units */

        gxrobp(lonmn, latmn, out x1, out y1); /* Lower Left       */
        gxrobp(lonmn, latmx, out xd, out y2); /* Upper Left       */
        if (xd < x1) x1 = xd;
        if (latmn < 0.0 && latmx > 0.0)
        {
            gxrobp(lonmn, 0.0, out xd, out yd); /* Left Middle      */
            if (xd < x1) x1 = xd;
        }

        gxrobp(lonmx, latmn, out x2, out y1); /* Lower Right      */
        gxrobp(lonmx, latmx, out xd, out y2); /* Upper Right      */
        if (xd > x2) x2 = xd;
        if (latmn < 0.0 && latmx > 0.0)
        {
            gxrobp(lonmx, 0.0, out xd, out yd); /* Right Middle     */
            if (xd > x2) x2 = xd;
        }

        /* Set up linear level scaling while maintaining aspect ratio.   */

        if (((xmax - xmin) / (ymax - ymin)) > ((x2 - x1) / (y2 - y1)))
        {
            w1 = 0.5 * (ymax - ymin) * (x2 - x1) / (y2 - y1);
            xave = (xmax + xmin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xave - w1, xave + w1, ymin, ymax, x1, x2, y1, y2);
            mpj.axmn = xave - w1;
            mpj.axmx = xave + w1;
            mpj.aymn = ymin;
            mpj.aymx = ymax;
        }
        else
        {
            w1 = 0.5 * (xmax - xmin) * (y2 - y1) / (x2 - x1);
            yave = (ymax + ymin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            mpj.axmn = xmin;
            mpj.axmx = xmax;
            mpj.aymn = yave - w1;
            mpj.aymx = yave + w1;
        }

        _drawingContext.GradsDrawingInterface.gxproj(gxrobp);
        _drawingContext.GradsDrawingInterface.gxback(gxrobb);
        adjtyp = 4;
        return (0);
    }

/* Transform routine for Robinson Projection */

    double[] rob1 =
    {
        -1.349, -1.317, -1.267, -1.206, -1.138, -1.066, -0.991,
        -0.913, -0.833, -0.752, -0.669, -0.586, -0.502, -0.418, -0.334, -0.251,
        -0.167, -0.084, 0.000, 0.084, 0.167, 0.251, 0.334, 0.418, 0.502, 0.586,
        0.669, 0.752, 0.833, 0.913, 0.991, 1.066, 1.138, 1.206, 1.267, 1.317, 1.349
    };

    double[] rob2 =
    {
        1.399, 1.504, 1.633, 1.769, 1.889, 1.997, 2.099,
        2.195, 2.281, 2.356, 2.422, 2.478, 2.532, 2.557, 2.582, 2.602, 2.616,
        2.625, 2.628, 2.625, 2.616, 2.602, 2.582, 2.557, 2.532, 2.478, 2.422,
        2.356, 2.281, 2.195, 2.099, 1.997, 1.889, 1.769, 1.633, 1.504, 1.399
    };

    public Tuple<double, double> gxrobp(double rlon, double rlat)
    {
        double x = 0;
        double y = 0;
        gxrobp(rlon, rlat, out x, out y);
        return new Tuple<double, double>(x, y);
    }
    public void gxrobp(double rlon, double rlat, out double x, out double y)
    {
        int i;
        rlat = (rlat + 90.0) / 5.0;
        i = (int)rlat;
        rlat = rlat - (double)i;
        if (i < 0)
        {
            y = -1.349;
            x = 1.399 * (rlon - fudge) / 180.0;
        }

        if (i >= 36)
        {
            y = 1.349;
            x = 1.399 * (rlon - fudge) / 180.0;
        }

        y = rob1[i] + rlat * (rob1[i + 1] - rob1[i]);
        x = rob2[i] + rlat * (rob2[i + 1] - rob2[i]);
        x = x * (rlon - fudge) / 180.0;
    }

/* Back Transform for Robinson Projection */

    public Tuple<double, double> gxrobb(double x, double y)
    {
        return new Tuple<double, double>(-999.9, -999.9);
    }

/*------------------------------------------------------------------
     DKRZ appends: Mollweide Projection
     10.08.95   Karin Meier (karin.meier@dkrz.de)
  ------------------------------------------------------------------*/

    public int gxmoll(mapprj mpj)
    {
        double lonmn, lonmx, latmn, latmx, xmin, xmax, ymin, ymax;
        double x1, x2, y1, y2, xd, yd, xave, yave, w1;

        lonmn = mpj.lnmn;
        lonmx = mpj.lnmx;
        latmn = mpj.ltmn;
        latmx = mpj.ltmx;
        xmin = mpj.xmn;
        xmax = mpj.xmx;
        ymin = mpj.ymn;
        ymax = mpj.ymx;
        lomin = lonmn;
        lomax = lonmx;
        lamin = latmn;
        lamax = latmx;

/* Check for errors */

        if (latmn < -90.0 || latmx > 90.0)
        {
            return (1);
        }

        if (latmn >= latmx || lonmn >= lonmx || xmin >= xmax || ymin >= ymax)
        {
            return (1);
        }

        /* Get bounds of the map in linear units */

        gxmollp(lonmn, latmn, out x1, out y1); /* Lower Left       */
        gxmollp(lonmn, latmx, out xd, out y2); /* Upper Left       */
        if (xd < x1) x1 = xd;
        if (latmn < 0.0 && latmx > 0.0)
        {
            gxmollp(lonmn, 0.0, out xd, out yd); /* Left Middle      */
            if (xd < x1) x1 = xd;
        }

        gxmollp(lonmx, latmn, out x2, out y1); /* Lower Right      */
        gxmollp(lonmx, latmx, out xd, out y2); /* Upper Right      */
        if (xd > x2) x2 = xd;
        if (latmn < 0.0 && latmx > 0.0)
        {
            gxmollp(lonmx, 0.0, out xd, out yd); /* Right Middle     */
            if (xd > x2) x2 = xd;
        }

        /* Set up linear level scaling while maintaining aspect ratio.   */

        if (((xmax - xmin) / (ymax - ymin)) > ((x2 - x1) / (y2 - y1)))
        {
            w1 = 0.5 * (ymax - ymin) * (x2 - x1) / (y2 - y1);
            xave = (xmax + xmin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xave - w1, xave + w1, ymin, ymax, x1, x2, y1, y2);
            mpj.axmn = xave - w1;
            mpj.axmx = xave + w1;
            mpj.aymn = ymin;
            mpj.aymx = ymax;
        }
        else
        {
            w1 = 0.5 * (xmax - xmin) * (y2 - y1) / (x2 - x1);
            yave = (ymax + ymin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            mpj.axmn = xmin;
            mpj.axmx = xmax;
            mpj.aymn = yave - w1;
            mpj.aymx = yave + w1;
        }

        _drawingContext.GradsDrawingInterface.gxproj(gxmollp);
        _drawingContext.GradsDrawingInterface.gxback(gxmollb);
        adjtyp = 4;
        return (0);
    }

    public Tuple<double, double> gxmollp(double rlon, double rlat)
    {
        double x, y;
        gxmollp(rlon, rlat, out x, out y);
        return new Tuple<double, double>(x, y);
    }
    void gxmollp(double rlon, double rlat, out double x, out double y)
    {
        double diff, radlat, radlon;

        if (lomin != -180.0)
        {
            diff = -180.0 - lomin;
            rlon = rlon + diff;
        }

        radlat = (Math.PI * rlat) / 180.0;
        radlon = (Math.PI * rlon) / 180.0;

        x = Math.Cos(radlat);
        y = Math.Cos(radlat) / 2.0;
        x = x * rlon / 180.0;

        return;
    }

/* Back Transform for Mollweide Projection */
    public Tuple<double, double> gxmollb(double x, double y)
    {
        return new Tuple<double, double>(-999.9, -999.9);
    }
    void gxmollb(double x, double y, out double rlon, out double rlat)
    {
        rlon = -999.9;
        rlat = -999.9;
    }

/* Orthographic projection.  Requires exact setup with the lat/lon range
   being exactly what is visible.  lat -90 to 90 and lon diff exactly 180. */

/* A secret mpvals mod, where the area can be clipped by x1,y1,x2,y2 where the
   values are in the range of -1 to 1 */

    public int gxortg(mapprj mpj)
    {
        double lonmn, lonmx, latmn, latmx, xmin, xmax, ymin, ymax;
        double x1, x2, y1, y2, xd, yd, xave, yave, w1;
        double xlmn, xlmx, ylmn, ylmx;
        int lflg;

        lflg = 0;
        xlmn = xlmx = ylmn = ylmx = -999;
        lonmn = mpj.lnmn;
        lonmx = mpj.lnmx;
        latmn = mpj.ltmn;
        latmx = mpj.ltmx;
        xmin = mpj.xmn;
        xmax = mpj.xmx;
        ymin = mpj.ymn;
        ymax = mpj.ymx;
        lomin = lonmn;
        lomax = lonmx;
        lamin = latmn;
        lamax = latmx;
        lonref = (lonmx + lonmn) / 2.0;
        if (mpj.axmn > -999.0)
        {
            xlmn = mpj.axmn;
            xlmx = mpj.axmx;
            ylmn = mpj.aymn;
            ylmx = mpj.aymx;
            if (xlmn >= -1.0 && xlmn <= 1.0 && xlmx >= -1.0 && xlmx <= 1.0 &&
                ylmn >= -1.0 && ylmn <= 1.0 && ylmx >= -1.0 && ylmx <= 1.0 &&
                ylmx > ylmn && xlmx > xlmn)
                lflg = 1;
        }

        /* Check boundaries */

        if (latmn != -90.0 || latmx != 90.0)
        {
            Console.WriteLine("Map Projection Error:  Latitude must be in range -90 90\n");
            return (1);
        }

        if ((lonmx - lonmn) > 180.001)
        {
            Console.WriteLine($"Map Projection Error:  {lonmx} - {lonmn}  > 180.0");
            return (1);
        }

        if ((lonmx - lonmn) < 179.999)
        {
            Console.WriteLine($"Map Projection Error:  {lonmx} - {lonmn}  > 180.0");
            return (1);
        }

        if (latmn >= latmx || lonmn >= lonmx || xmin >= xmax || ymin >= ymax) return (1);

        if (lonmn < -180.0)
        {
            mpj.lnmn = lonmn + 360.0;
            mpj.lnmx = lonmx + 360.0;
            lonmn = mpj.lnmn;
            lonmx = mpj.lnmx;
        }

        if (lonmx > 180.0)
        {
            mpj.lnmn = lonmn - 360.0;
            mpj.lnmx = lonmx - 360.0;
            lonmn = mpj.lnmn;
            lonmx = mpj.lnmx;
        }

        /* Get bounds of the map in linear units */

        gxortgp(lonmn, latmn, out x1, out y1);
        gxortgp(lonmn, latmx, out xd, out y2);
        if (xd < x1) x1 = xd;
        if (latmn < 0.0 && latmx > 0.0)
        {
            gxortgp(lonmn, 0.0, out xd, out yd);
            if (xd < x1) x1 = xd;
        }

        gxortgp(lonmx, latmn, out x2, out y1);
        gxortgp(lonmx, latmx, out xd, out y2);
        if (xd > x2) x2 = xd;
        if (latmn < 0.0 && latmx > 0.0)
        {
            gxortgp(lonmx, 0.0, out xd, out yd);
            if (xd > x2) x2 = xd;
        }

        if (lflg>0)
        {
            x1 = xlmn;
            x2 = xlmx;
            y1 = ylmn;
            y2 = ylmx;
        }

        /* Set up linear level scaling while maintaining aspect ratio.   */

        if (((xmax - xmin) / (ymax - ymin)) > ((x2 - x1) / (y2 - y1)))
        {
            w1 = 0.5 * (ymax - ymin) * (x2 - x1) / (y2 - y1);
            xave = (xmax + xmin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xave - w1, xave + w1, ymin, ymax, x1, x2, y1, y2);
            mpj.axmn = xave - w1;
            mpj.axmx = xave + w1;
            mpj.aymn = ymin;
            mpj.aymx = ymax;
        }
        else
        {
            w1 = 0.5 * (xmax - xmin) * (y2 - y1) / (x2 - x1);
            yave = (ymax + ymin) / 2.0;
            _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            mpj.axmn = xmin;
            mpj.axmx = xmax;
            mpj.aymn = yave - w1;
            mpj.aymx = yave + w1;
        }

        _drawingContext.GradsDrawingInterface.gxproj(gxortgp);
        _drawingContext.GradsDrawingInterface.gxback(gxortgb);
        adjtyp = 4;
        return (0);
    }

    public Tuple<double, double> gxortgp(double rlon, double rlat)
    {
        double x, y;
        gxortgp(rlon, rlat, out x, out y);
        return new Tuple<double, double>(x, y);
    }

    void gxortgp(double rlon, double rlat, out double x, out double y)
    {
        double radlat, radlon, diff;

        if (lomin != -90.0)
        {
            diff = -90.0 - lomin;
            rlon = rlon + diff;
        }

        radlat = (Math.PI * rlat) / 180.0;
        radlon = (Math.PI * rlon) / 180.0;

        x = Math.Cos(radlat);
        y = Math.Cos(radlat);
        x = x * Math.Cos(radlon);
        
    }

/* Back Transform for Orthographic Projection */
    public Tuple<double, double> gxortgb(double x, double y)
    {
        return new Tuple<double, double>(-999.9, -999.9);
    }
    void gxortgb(double x, double y, out double rlon, out double rlat)
    {
        rlon = -999.9;
        rlat = -999.9;
    }

/*------------------------------------------------------------------
     DKRZ appends: Lambert conformal conic Projection
     15.03.96                       Karin Meier (karin.meier@dkrz.de)
  ------------------------------------------------------------------*/
    static double hemi, r;

    public int gxlamc(mapprj mpj)
    {
        double lonmn, lonmx, latmn, latmx, dlat, dlon, dx, dy;
        double xave, yave, w1, lonave, xmin, xmax, ymin, ymax, x1, x2, y1, y2, xd, yd;

        lonmn = mpj.lnmn;
        lonmx = mpj.lnmx;
        latmn = mpj.ltmn;
        latmx = mpj.ltmx;
        xmin = mpj.xmn;
        xmax = mpj.xmx;
        ymin = mpj.ymn;
        ymax = mpj.ymx;
        lomin = lonmn;
        lomax = lonmx;
        lamin = latmn;
        lamax = latmx;
        lonave = (lonmx + lonmn) / 2.0;
        dlat = lamax - lamin;
        dlon = lomax - lomin;
        dx = xmax - xmin;
        dy = ymax - ymin;

        if ((lonmn >= lonmx) || (latmn >= latmx) || (xmin >= xmax) || (ymin >= ymax))
        {
            return (1);
        }

        if (((latmn > 0.0) && (latmx < 0.0)) || ((latmn < 0.0) && (latmx > 0.0)))
        {
            throw new Exception("Map Projection Error:  Latitude must be in range -90 0 or 0 90");
            return (1);
        }

/*--- set constant for northern or southern hemisphere  ---*/

        if (latmn >= 0.0)
        {
            hemi = 1.0; /** northern hemisphere **/
        }
        else
        {
            hemi = -1.0; /** southern hemisphere **/
        }

/*--- reset 90.0/-90.0 degrees to 89.99/-89.99 because of tangent  ---*/

        if (latmn == -90.0) latmn = -89.99;
        if (latmx == 90.0) latmx = 89.99;

/*--- get viewport coordinates  x1, x2, y1, y2---*/

        gxlamcp(lonmn, latmn, out x1, out y1);
        gxlamcp(lonmn, latmx, out xd, out y2);
        if (xd < x1) x1 = xd;
        if (y2 < y1)
        {
            yd = y2;
            y2 = y1;
            y1 = yd;
        }

        if (latmn >= 0.0 && latmx > 0.0)
        {
            gxlamcp(lonmn, 0.0, out xd, out yd);
            if (xd < x1) x1 = xd;
        }

        gxlamcp(lonmx, latmn, out x2, out y1);
        gxlamcp(lonmx, latmx, out xd, out y2);
        if (xd > x2) x2 = xd;
        if (y2 < y1)
        {
            yd = y2;
            y2 = y1;
            y1 = yd;
        }

        if (latmn < 0.0 && latmx <= 0.0)
        {
            gxlamcp(lonmx, 0.0, out xd, out yd);
            if (xd > x2) x2 = xd;
        }

/*--- determining terms for scaling  ---*/

        xave = (xmin + xmax) / 2.0;
        yave = (ymin + ymax) / 2.0;

        if (((xmax - xmin) / (ymax - ymin)) > ((x2 - x1) / (y2 - y1)))
        {
            if (hemi == -1.0 && 180.0 < (lomax - lomin) && (lomax - lomin) <= 270.0)
                yave -= 1.5;
            else if (hemi == 1.0 && 180.0 < (lomax - lomin) && (lomax - lomin) <= 270.0)
                yave += 1.5;
            else if (hemi == -1.0 && 270.0 <= (lomax - lomin) && (lomax - lomin) <= 360.0)
                yave -= 1.2;
            else if (hemi == 1.0 && 270.0 <= (lomax - lomin) && (lomax - lomin) <= 360.0)
                yave += 1.2;
            else if (hemi == -1.0 && 90.0 < (lomax - lomin) && (lomax - lomin) <= 180.0)
                yave -= 0.5;
            else if (hemi == 1.0 && 90.0 < (lomax - lomin) && (lomax - lomin) <= 180.0)
                yave += 1.0;
            else if (hemi == -1.0 && (lomax - lomin) <= 90.0)
                yave += 0.0;
            else if (hemi == 1.0 && (lomax - lomin) <= 90.0)
                yave += 1.0;

            w1 = 0.5 * (ymax - ymin) * (x2 - x1) / (y2 - y1);
            if (w1 < 1.0)
                _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            else if (w1 < 2.0)
                _drawingContext.GradsDrawingInterface.gxscal(xave - 0.5 * (w1), xave + 0.5 * w1, yave - w1, yave + w1,
                    x1, x2, y1, y2);
            else if (w1 < 3.0)
                _drawingContext.GradsDrawingInterface.gxscal(xave - 0.5 * w1, xave + 0.5 * w1, yave - w1, yave + w1,
                    x1, x2, y1, y2);
            else if (w1 > 3.0)
                _drawingContext.GradsDrawingInterface.gxscal(xave - 0.75 * w1, xave + 0.75 * w1, yave - 0.75 * w1,
                    yave + 0.75 * w1, x1, x2, y1, y2);
            else
                _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
        }
        else
        {
            if (hemi == -1.0 && 180.0 < (lomax - lomin) && (lomax - lomin) <= 270.0)
                yave -= 1.0;
            else if (hemi == 1.0 && 180.0 < (lomax - lomin) && (lomax - lomin) <= 270.0)
                yave += 1.5;
            else if (hemi == -1.0 && 270.0 <= (lomax - lomin) && (lomax - lomin) <= 360.0)
                yave -= 1.0;
            else if (hemi == 1.0 && 270.0 <= (lomax - lomin) && (lomax - lomin) <= 360.0)
                yave += 1.0;
            else if (hemi == -1.0 && 90.0 < (lomax - lomin) && (lomax - lomin) <= 180.0)
                yave -= 0.5;
            else if (hemi == 1.0 && 90.0 < (lomax - lomin) && (lomax - lomin) <= 180.0)
                yave += 1.0;
            else if (hemi == -1.0 && (lomax - lomin) <= 90.0)
                yave += 0.0;
            else if (hemi == 1.0 && (lomax - lomin) <= 90.0)
                yave += 1.0;

            w1 = 0.5 * (xmax - xmin) * (y2 - y1) / (x2 - x1);
            if (w1 < 1.0)
                _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            else if (w1 < 2.0)
                _drawingContext.GradsDrawingInterface.gxscal(xmin + 0.5 * w1, xmax - 0.5 * w1, yave - 1.25 * w1,
                    yave + 1.25 * w1, x1, x2, y1, y2);
            else if (w1 < 3.0)
                _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
            else if (w1 > 3.0)
                _drawingContext.GradsDrawingInterface.gxscal(xave - 0.5 * w1, xave + 0.5 * w1, yave - 0.5 * w1,
                    yave + 0.5 * w1, x1, x2, y1, y2);
            else
                _drawingContext.GradsDrawingInterface.gxscal(xmin, xmax, yave - w1, yave + w1, x1, x2, y1, y2);
        }

        mpj.axmn = xmin;
        mpj.axmx = xmax;
        mpj.aymn = ymin;
        mpj.aymx = ymax;

        _drawingContext.GradsDrawingInterface.gxproj(gxlamcp);
        _drawingContext.GradsDrawingInterface.gxback(gxlamcb);
        adjtyp = 3;
        return (0);
    }


/*--- transform routine for lambert conformal conic projection  ---*/

    Tuple<double, double> gxlamcp(double rlon, double rlat)
    {
        double x, y;
        gxlamcp(rlon, rlat, out x, out y);
        return new Tuple<double, double>(x, y);
    }
    void gxlamcp(double rlon, double rlat, out double x, out double y)
    {
        double d2r, cone, phis, phin, clon, term1, term2;

        d2r = Math.PI / 180.0;

/*--- standard latitudes:  north - phin;  south - phis  ---*/
        phis = lamin;
        phin = lamax;

/*--- reset 90.0/-90.0 degrees to 89.99/-89.99 because of tangent  ---*/
        if (phis == -90.0) phis = -89.99;
        if (phin == 90.0) phin = 89.99;

/*--- calculate the constant of the cone +++ radius, x, y ---*/
/*--- clon -  central meridian;    cone -  cone constant  ---*/
        clon = Math.Floor((lomax + lomin) / 2.0);
        term1 = Math.Tan((45.0 - hemi * phis / 2.0) * d2r);
        term2 = Math.Tan((45.0 - hemi * phin / 2.0) * d2r);

        if (phis != phin)
            cone = (Math.Log10(Math.Cos(phis * d2r)) - Math.Log10(Math.Cos(phin * d2r))) /
                   (Math.Log10(term1) - Math.Log10(term2));
        else
            cone = Math.Cos((90.0 - hemi * phis) * d2r);

        r = Math.Pow(Math.Tan((45.0 - hemi * rlat / 2.0) * d2r), cone);
        x = r * Math.Cos((rlon - clon) * d2r * cone);
        y = -hemi * r * Math.Cos((rlon - clon) * d2r * cone);

        
    }


/*--- Back Transform for Lambert conformal Projection ---*/

    Tuple<double, double> gxlamcb(double x, double y) => new Tuple<double, double>(-999.9, -999.9);
    
    void gxlamcb(double x, double y, out double rlon, out double rlat)
    {
        rlon = -999.9;
        rlat = -999.9;
    }

/* Interpolate lat/lon boundaries, and convert to xy, on
   behalf of 'draw mappoly' .  For most part, the same
   code as in gxdmap  */

    double[] gxmpoly(double[] xy, int cnt, double llinc, out int newcnt)
    {
        double ln1, ln2, lt1, lt2, lnsav, ltsav, llsum;
        double lndif, ltdif, lldist, lntmp, lttmp, xx, yy;
        double[] newxy;
        int i, j, ip, ncnt;

        /* Determine total 'path' length */

        llsum = 0.0;
        for (i = 1; i < cnt; i++)
        {
            ip = (i - 1) * 2;
            lndif = Math.Abs(xy[ip + 2]- xy[ip]);
            ltdif = Math.Abs(xy[ip + 3]- xy[ip + 1]);
            if (lndif > ltdif) lldist = lndif;
            else lldist = ltdif;
            llsum += lldist;
        }

        /* Estimate number of output points, and allocate storage for them. */
        /* add one more point in case polygon doesn't close,
         an extra point (to close polygon) needs to be added by calling routine
         add one more point to include the very first point, before interpolation begins */

        ncnt = (int)(cnt + llsum / llinc + 2);
        newxy = new double[ncnt * 2];
        
        /* Write out the very first point, before interpolation begins (this is for j=0) */
        GradsDrawingInterface.gxconv(xy[0], xy[1], out xx, out yy, 2);
        newxy[0] = xx;
        newxy[1] = yy;
        /* Now interpolate each point, convert to x,y, and put in list */
        j = 1;
        lnsav = xy[0];
        ltsav = xy[1];
        for (i = 1; i < cnt; i++)
        {
            ip = (i - 1) * 2;
            ln1 = xy[ip];
            ln2 = xy[ip + 2];
            lt1 = xy[ip + 1];
            lt2 = xy[ip + 3];
            lndif = Math.Abs(ln2 - ln1);
            ltdif = Math.Abs(lt2 - lt1);
            if (lndif > ltdif) lldist = lndif;
            else lldist = ltdif;
            llsum = llinc;
            lntmp = lnsav;
            lttmp = ltsav;
            while (llsum < lldist + llinc)
            {
                if (llsum >= lldist - llinc / 4.0)
                {
                    lntmp = ln2;
                    lttmp = lt2;
                    llsum += llinc; /* Insure loop dropout */
                }
                else
                {
                    if (lndif > ltdif)
                    {
                        if (ln1 < ln2)
                        {
                            lntmp += llinc;
                            lttmp += llinc * (lt2 - lt1) / (ln2 - ln1);
                        }
                        else
                        {
                            lntmp -= llinc;
                            lttmp -= llinc * (lt2 - lt1) / (ln2 - ln1);
                        }
                    }
                    else
                    {
                        if (lt1 < lt2)
                        {
                            lttmp += llinc;
                            lntmp += llinc * (ln2 - ln1) / (lt2 - lt1);
                        }
                        else
                        {
                            lttmp -= llinc;
                            lntmp -= llinc * (ln2 - ln1) / (lt2 - lt1);
                        }
                    }
                }

                GradsDrawingInterface.gxconv(lntmp, lttmp, out xx, out yy, 2);
                newxy[j * 2] = xx;
                newxy[j * 2 + 1] = yy;
                j++;
                if (j >= ncnt)
                {
                    Console.WriteLine("Logic Error in gxmpoly");
                    newcnt = 0;
                    return (null);
                }

                lnsav = lntmp;
                ltsav = lttmp;
                llsum += llinc;
            }
        }

        newcnt = j;
        return (newxy);
    }

/* If the file has not been read into cache, read it.  If the file
   is alrady cached, set the pointer to it. */

    int gxwopen(string name, string opts, bool fromAssembly = false)
    {
        Stream ifile;
        mapcache tmppmc, pmc;
        int i, rc, flen;
        string ch,fname;
        byte[] cdat; 

        cflag = 0; /* indicate i/o from file; change later */

        /* traverse link list to find this file */

        foreach(var c in manchor)
        {
            ch = c.name;
            if (ch == name) {
                cmc = c;
                mcpos = 0;
                mclen = cmc.size;
                cflag = 1;
                return (1);
            }
        }

        /* this file is not in the cache.  try to open it.  */

        if (fromAssembly)
        {
            try
            {
                ifile = Assembly.GetExecutingAssembly().GetManifestResourceStream("GradsSharp.Data." + name) ??
                        throw new FileNotFoundException(name);
            }
            catch (Exception ex)
            {
                return (0);    
            }
        }
        else
        {
            try
            {
                ifile = new FileStream(name, FileMode.Open);
            }
            catch (Exception ex)
            {
                return (0);    
            }

        }
        
        
        /* check size of file */

        flen = (int)ifile.Length;
        
        /* if file is too big, do regular file i/o.  set this up.  */

        if (flen > CACHEMAX)
        {
            mfile = ifile;
            return (2);
        }

        /* allocate memory for all the cache items */

        cdat = new byte[flen];

        i = 0;
        fname = name;
        
        pmc = new mapcache();
        
        /* read in the file.  on error, fall back to file i/o. */

        rc = ifile.Read(cdat, 0, flen);
        if (rc != flen)
        {
            mfile = ifile;
            return (2);
        }

        ifile.Close();

        /* chain it up, set it up, and return */
        
        
        pmc.size = flen;
        pmc.name = fname;
        pmc.data = cdat;
        manchor.Add(pmc);
        
        mcpos = 0; /* initial position in cache */
        mclen = flen;
        cflag = 1; /* indicate cache i/o */
        cmc = pmc;
        return (1);
    }

/* pull requested length of data from cache from the current cache location.
   update the cache location.  Return the length of the data -- this can be
   less than the requested length if the end of buffer is hit.  */

    int gxwread(out byte[] rec, int len)
    {
        int i;
        byte[] cdat;
        rec = new byte[len];

        cdat = cmc.data;
        i = 0;
        while (i < len && mcpos < mclen)
        {
            rec[i] = cdat[mcpos];
            i++;
            mcpos++;
        }

        return (i);
    }

    void gxwseek(int pos)
    {
        mcpos = pos;
    }

    void gxwclose(int flag)
    {
        cflag = 0;
        mcpos = 0;
        mclen = 0;
    }

    private int gagby(byte[] data, int offset, int len)
    {

        byte[] toConvert = {0,0,0,0};

        if (len == 1)
        {
            toConvert = new byte[] { data[0+offset], 0, 0, 0 };
        }

        if (len == 2)
        {
            toConvert = new byte[] { data[1+offset], data[0+offset],0,0 };
        }

        if (len == 3)
        {
            toConvert = new byte[] { data[2+offset], data[1+offset], data[0+offset], 0 };
        }

        if (len > 3)
        {
            toConvert = new byte[] { data[3+offset], data[2+offset], data[1+offset], data[0+offset] };
        }
        
        if (BitConverter.IsLittleEndian)
        {
            //Array.Reverse(toConvert);
        }

        return BitConverter.ToInt32(toConvert);
    }
}