using Microsoft.Extensions.Logging;

namespace GradsSharp.Drawing.Grads;

internal class s2pbuf
{
    public int len; /* Number of polygon points */
    public int color, index; /* Output options for this polygon */
    public double[] xy; /* Line points, x,y number len */
    public double clev1, clev2; /* Fill range values */
};

internal class GxShad2
{
    private DrawingContext _drawingContext;

    static int np;
    static double[] xp = new double[50], yp = new double[50];
    static int[] tp = new int[50], sp = new int[50];
    static int typ1, typ2, typ3;
    static int numpoly, polyside;

    static double[] xpo = new double[50], ypo = new double[50];
    static int npo;

/* Following variables used for gxshad2  */

    static byte[] pflg, s1flg, s2flg, s3flg, s4flg;
    static byte[] uu;
    static int isize, jsize, pnum, gindex;
    private static double[] rr;
    static double alev, blev;

    static double[] xxyy; /* Holds one polygon */
    static int xynum;


    static List<s2pbuf> s2pbufanch; /* Anchor for polygon buffer */
    static bool nodraw = false; /* If 1, polygons are not drawn */

    static bool bufopt = false; /* Buffer or not, default is not */

    /* If buffering is enabled, someone needs to call s2frepbuf from somewhere */
    static int bufcnt; /* Number of polys buff'd */
    static string pout; /* Build strings for KML here */

/* Debug variable, used by both */

    static int bug;

    public GxShad2(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public void gxshad2(double[] r, int iis, int jjs, double[] vs, double rmax, int[] clrs, int lvs, byte[] u)
    {
        double clev1, clev2;
        int k;

        bug = 0;
        if (bug > 0) _drawingContext.Logger?.LogInformation("in gxshad2");

        rr = r;
        isize = iis;
        jsize = jjs;
        uu = u;
/*   bufopt = 0; */

        pflg = new byte[iis * jjs];
        s1flg = new byte[iis * jjs];
        s2flg = new byte[iis * jjs];
        s3flg = new byte[iis * jjs];
        s4flg = new byte[iis * jjs];

        k = iis;
        if (jjs > iis) k = jjs;
        k = k * 5;
        xxyy = new double[k * 2];

        xynum = k;

        /* Loop through the shade levels and set the color.  Skip the level
           if the color is less than zero.  */

        for (k = 0; k < lvs; k++)
        {
            if (clrs[k] < 0) continue;
            gindex = k;
            clev1 = vs[k];
            if (k < lvs - 1) clev2 = vs[k + 1];
            else
            {
                if (clev1 <= rmax) clev2 = rmax + Math.Abs(vs[k] - vs[k - 1]);
                else clev2 = clev1 + Math.Abs(vs[k] - vs[k - 1]);
            }

            _drawingContext.GradsDrawingInterface.SetDrawingColor(clrs[k]);
            blev = clev1;
            alev = clev2;
            if (bug > 0) _drawingContext.Logger?.LogInformation($"clevs {clev1} {clev2}");
            s2flags(r, u, iis, jjs, clev1, clev2);
            s2poly(r, iis, jjs, clev1, clev2);
            if (bug > 0) s2debug();
        }


        if (bug > 0 && bufopt) _drawingContext.Logger?.LogInformation($"---. bufcnt = {bufcnt}");
    }
/* The grid r is shaded.  Size is by js.  lvs indicates the number of 
   shaded levels.  vs contains the values bounding the shaded regions.
   clrs contains lvs+1 colors for the shaded regions.  u is the
   undefined grid data value.                                        */

    public void gxshad2b(double[] r, int iis, int jjs, double[] vs, double rmax, int[] clrs, int lvs, byte[] u)
    {
        double v1, v2, v3, v4, clev1, clev2;
        int i, k, ig, jg, ijg;

        bug = 0;

        /* Loop through the shade levels and set the color.  Skip the level
           if the color is less than zero.  */

        for (k = 0; k < lvs; k++)
        {
            if (clrs[k] < 0) continue;
            clev1 = vs[k];
            if (k < lvs - 1) clev2 = vs[k + 1];
            else
            {
                if (clev1 <= rmax) clev2 = rmax + Math.Abs(vs[k] - vs[k - 1]);
                else clev2 = clev1 + Math.Abs(vs[k] - vs[k - 1]);
            }

            _drawingContext.GradsDrawingInterface.SetDrawingColor(clrs[k]);

            /*  Loop through all the grid boxes.  Skip box if any missing values */

            for (jg = 0; jg < jjs - 1; jg++)
            {
                for (ig = 0; ig < iis - 1; ig++)
                {
                    ijg = jg * iis + ig;
                    if (u[ijg] == 0 || u[ijg + 1] == 0 ||
                        u[ijg + iis] == 0 || u[ijg + iis + 1] == 0) continue;
                    v1 = r[ijg];
                    v2 = r[ijg + 1];
                    v4 = r[ijg + iis];
                    v3 = r[ijg + iis + 1];
                    if (bug > 0) _drawingContext.Logger?.LogInformation($"{v1},{v2},{v3},{v4}");

                    /* Find all the intersect points for this box, and if it
                       is a col, determine the nature of the col and the 
                       number of polygons to be output */

                    s2box(v1, v2, v3, v4, clev1, clev2);

                    /* Join the intersect points into polygons.  This is simple
                       if it is just one polygon.  Otherwise a determination must
                       be made how to draw the two polygons. */

                    if (np > 1)
                    {
                        npo = 0;
                        for (i = 0; i < np; i++)
                        {
                            /* Put points into grid coords */
                            xp[i] = xp[i] + (float)(ig + 1);
                            yp[i] = yp[i] + (float)(jg + 1);
                        }

                        if (numpoly == 1)
                        {
                            /* Only one polygon.  Easy.  */
                            for (i = 0; i < np; i++)
                            {
                                xpo[npo] = xp[i];
                                ypo[npo] = yp[i];
                                npo++;
                            }

                            s2outpoly();
                        }

                        if (numpoly == 2)
                        {
                            /* Two polygons */
                            if (polyside == 1)
                            {
                                /* Poly1: sides 1 and 2.  */
                                for (i = 0; i < np; i++)
                                {
                                    if (sp[i] == 1 || sp[i] == 2)
                                    {
                                        xpo[npo] = xp[i];
                                        ypo[npo] = yp[i];
                                        npo++;
                                    }
                                }

                                s2outpoly();
                                for (i = 0; i < np; i++)
                                {
                                    if (sp[i] == 3 || sp[i] == 4)
                                    {
                                        xpo[npo] = xp[i];
                                        ypo[npo] = yp[i];
                                        npo++;
                                    }
                                }

                                s2outpoly();
                            }
                            else
                            {
                                for (i = 0; i < np; i++)
                                {
                                    /* Poly1:  sides 2 and 3 */
                                    if (sp[i] == 2 || sp[i] == 3)
                                    {
                                        xpo[npo] = xp[i];
                                        ypo[npo] = yp[i];
                                        npo++;
                                    }
                                }

                                s2outpoly();
                                for (i = 0; i < np; i++)
                                {
                                    if (sp[i] == 1 || sp[i] == 4)
                                    {
                                        xpo[npo] = xp[i];
                                        ypo[npo] = yp[i];
                                        npo++;
                                    }
                                }

                                s2outpoly();
                            }
                        }
                    }
                }
            }
        }

        return;
    }

/* Output a single polygon. Remove duplicate points. */

    void s2outpoly()
    {
        double[] xy = new double[50];
        double[] pxy;
        int i, j;

        if (bug > 0) _drawingContext.Logger?.LogInformation($"  xxx> {npo}");
        if (npo < 3)
        {
            /* At least 3 points needed */
            npo = 0;
            return;
        }

        j = 0;
        for (i = 1; i < npo; i++)
        {
            /* Remove dups */
            if (Math.Abs(xpo[i] - xpo[j]) > 1e-5 || Math.Abs(ypo[i] - ypo[j]) > 1e-5)
            {
                if (i != j + 1)
                {
                    xpo[j + 1] = xpo[i];
                    ypo[j + 1] = ypo[i];
                }

                j++;
            }
        }

        j++;
        if (j < 3)
        {
            npo = 0;
            return;
        }

        npo = j;

        pxy = xy;
        int cntpxy = 0;
        for (i = 0; i < npo; i++)
        {
            /* Scale from grid to xy */
            GradsDrawingInterface.gxconv(xpo[i], ypo[i], out pxy[cntpxy], out pxy[cntpxy + 1], 3);
            if (bug > 0) _drawingContext.Logger?.LogInformation($"    {xpo[i]} {ypo[i]} {pxy[cntpxy]} {pxy[cntpxy + 1]}");
            cntpxy += 2;
        }

        pxy[cntpxy] = xy[0];
        pxy[cntpxy + 1] = xy[1];
        npo++;
        if (!nodraw)
        {
            _drawingContext.GradsDrawingInterface.gxfill(xy, npo); /* Output polygon */
        }

        npo = 0;
    }

/* Pre-determine flags for the entire grid
            s2       
         v2 --- v3
          |     |  s3
      s1  |     |
         v1 --- v4
       p1    s4

*/

/* flag values:  1 -- c1 intersects, v1<v2 (or v2<v3, v4<v3, v1<v4)
                 2 -- c1 intersects, v1>v2
                 3 -- c2 intersects, v1<v2
                 4 -- c2 intersects, v1>v2
                 5 -- both intersect, v1<v2
                 6 -- both intersect, v1>v2
               +10 -- boundary with an intersect (ie, the polygon edge
                         is from an intersect point to a corner)
               =10 -- boundary, no intersect (ie, the polygon
                         edge is along the entire box side, corner to corner)
         
       note: a boundary can be along the outside edge, along the edge of missing 
       data, and along artificial internal boundaries introduced to 
       insure polygon closure or to avoid col problems.  */

    void s2flags(double[] r, byte[] u, int iis, int jjs, double c1, double c2)
    {
        int ig, jg, ijg, flag, jj;
        double v1, v2, v3, v4;

        for (jg = 0; jg < jjs; jg++)
        {
            ijg = jg * iis;
            for (ig = 0; ig < iis; ig++)
            {
                pflg[ijg] = 0;
                if (u[ijg]>0)
                {
                    if (r[ijg] > c1 && r[ijg] <= c2) pflg[ijg] = 1; /* pflg true if within shade range */
                }

                ijg++;
            }
        }

        for (jg = 0; jg < jjs - 1; jg++)
        {
            ijg = jg * iis;
            for (ig = 0; ig < iis - 1; ig++)
            {
                s1flg[ijg] = 0;
                s2flg[ijg] = 0;
                s3flg[ijg] = 0;
                s4flg[ijg] = 0;

                if (u[ijg] == 0 || u[ijg + 1] == 0 || u[ijg + iis] == 0 || u[ijg + iis + 1] == 0)
                {
                    ijg++;
                    continue;
                }

                v1 = r[ijg];
                v2 = r[ijg + iis];
                v3 = r[ijg + iis + 1];
                v4 = r[ijg + 1];

                /* side1 */
                if (v1 <= c1 && v2 > c1) s1flg[ijg] = 1;
                if (v1 > c1 && v2 <= c1) s1flg[ijg] = 2;
                if (v1 <= c2 && v2 > c2)
                {
                    if (s1flg[ijg]>0) s1flg[ijg] = 5;
                    else s1flg[ijg] = 3;
                }

                if (v1 > c2 && v2 <= c2)
                {
                    if (s1flg[ijg]>0) s1flg[ijg] = 6;
                    else s1flg[ijg] = 4;
                }

                /* side2 */
                if (v2 <= c1 && v3 > c1) s2flg[ijg] = 1;
                if (v2 > c1 && v3 <= c1) s2flg[ijg] = 2;
                if (v2 <= c2 && v3 > c2)
                {
                    if (s2flg[ijg]>0) s2flg[ijg] = 5;
                    else s2flg[ijg] = 3;
                }

                if (v2 > c2 && v3 <= c2)
                {
                    if (s2flg[ijg]>0) s2flg[ijg] = 6;
                    else s2flg[ijg] = 4;
                }

                /* side3 */
                if (v3 <= c1 && v4 > c1) s3flg[ijg] = 2;
                if (v3 > c1 && v4 <= c1) s3flg[ijg] = 1;
                if (v3 <= c2 && v4 > c2)
                {
                    if (s3flg[ijg]>0) s3flg[ijg] = 6;
                    else s3flg[ijg] = 4;
                }

                if (v3 > c2 && v4 <= c2)
                {
                    if (s3flg[ijg]>0) s3flg[ijg] = 5;
                    else s3flg[ijg] = 3;
                }

                /* side4 */
                if (v4 <= c1 && v1 > c1) s4flg[ijg] = 2;
                if (v4 > c1 && v1 <= c1) s4flg[ijg] = 1;
                if (v4 <= c2 && v1 > c2)
                {
                    if (s4flg[ijg]>0) s4flg[ijg] = 6;
                    else s4flg[ijg] = 4;
                }

                if (v4 > c2 && v1 <= c2)
                {
                    if (s4flg[ijg]>0) s4flg[ijg] = 5;
                    else s4flg[ijg] = 3;
                }

                /* set side flag to 10 if side is completely in the range and  
                   the side is a boundary  */

                if (pflg[ijg]>0 && pflg[ijg + iis]>0)
                {
                    if (ig == 0) s1flg[ijg] = 10;
                    else if (u[ijg - 1] == 0 || u[ijg + iis - 1] == 0) s1flg[ijg] = 10;
                }

                if (pflg[ijg + iis] > 0 && pflg[ijg + iis + 1] > 0)
                {
                    if (jg == jjs - 2) s2flg[ijg] = 10;
                    else if (u[ijg + iis * 2] == 0 || u[ijg + iis * 2 + 1] == 0) s2flg[ijg] = 10;
                }

                if (pflg[ijg + iis + 1] > 0 && pflg[ijg + 1] > 0)
                {
                    if (ig == iis - 2) s3flg[ijg] = 10;
                    else if (u[ijg + 2] == 0 || u[ijg + iis + 2] == 0) s3flg[ijg] = 10;
                }

                if (pflg[ijg + 1] > 0 && pflg[ijg] > 0)
                {
                    if (jg == 0) s4flg[ijg] = 10;
                    else if (u[ijg - iis] == 0 || u[ijg + 1 - iis] == 0) s4flg[ijg] = 10;
                }

                ijg++;
            }
        }

        /* If a side flag indicates an intersect (value of 1 to 6), but the 
           side flag "next" to it does not, then the intersect is also a 
           boundary.  Indicate this with a +10.  */

        for (jg = 0; jg < jjs - 1; jg++)
        {
            ijg = jg * iis;
            for (ig = 0; ig < iis - 1; ig++)
            {
                if (s1flg[ijg] > 0 && s1flg[ijg] < 7)
                {
                    if (ig == 0) s1flg[ijg] += 10;
                    else if (s3flg[ijg - 1] == 0) s1flg[ijg] += 10;
                }

                if (s2flg[ijg] > 0 && s2flg[ijg] < 7)
                {
                    if (jg == jjs - 2) s2flg[ijg] += 10;
                    else if (s4flg[ijg + iis] == 0) s2flg[ijg] += 10;
                }

                if (s3flg[ijg] > 0 && s3flg[ijg] < 7)
                {
                    if (ig == iis - 2) s3flg[ijg] += 10;
                    else if (s1flg[ijg + 1] == 0) s3flg[ijg] += 10;
                }

                if (s4flg[ijg] > 0 && s4flg[ijg] < 7)
                {
                    if (jg == 0) s4flg[ijg] += 10;
                    else if (s2flg[ijg - iis] == 0) s4flg[ijg] += 10;
                }

                ijg++;
            }
        }

        /* Determine some needed internal boundaries.  Set flag to +10 for these.
           Be careful not to change existing boundary values. */

        for (jg = 0; jg < jjs - 1; jg++)
        {
            flag = 0;
            ijg = jg * iis + 1;
            for (ig = 1; ig < iis - 1; ig++)
            {
                if (pflg[ijg] == 0 && s1flg[ijg] > 0 && s1flg[ijg] < 7)
                {
                    if (s4flg[ijg - 1] > 0 && s4flg[ijg - 1] < 7) flag = 1;
                    if (flag == 1 && s4flg[ijg] > 0 && s4flg[ijg] < 7)
                    {
                        jj = jg;
                        while (jj < jjs - 1)
                        {
                            if (u[(jj + 1) * iis + ig] == 0) break;
                            s1flg[jj * iis + ig] += 10;
                            s3flg[jj * iis + ig - 1] += 10;
                            jj++;
                            if (pflg[jj * iis + ig] == 0) break;
                            if (s1flg[jj * iis + ig] > 9) break;
                            if (s3flg[jj * iis + ig - 1] > 9) break;
                        }

                        flag = 0;
                    }
                }
                else flag = 0;

                ijg++;
            }
        }

        /* Handle any holes resulting from missing data */

        for (jg = 1; jg < jjs - 1; jg++)
        {
            ijg = jg * iis + 1;
            for (ig = 1; ig < iis - 1; ig++)
            {
                if ((pflg[ijg] > 0 && s4flg[ijg - 1] > 9 && s4flg[ijg] < 10
                     && s3flg[ijg - 1] < 10 && s1flg[ijg] < 10) ||
                    (pflg[ijg] == 0 && s4flg[ijg - 1] > 9 && s4flg[ijg] < 10
                     && s1flg[ijg] < 10 && s1flg[ijg] > 0))
                {
                    jj = jg;
                    while (jj < jjs - 1)
                    {
                        if (u[(jj + 1) * iis + ig] == 0) break;
                        s1flg[jj * iis + ig] += 10;
                        s3flg[jj * iis + ig - 1] += 10;
                        jj++;
                        if (pflg[jj * iis + ig] == 0) break;
                        if (s1flg[jj * iis + ig] > 9) break;
                        if (s3flg[jj * iis + ig - 1] > 9) break;
                    }
                }

                ijg++;
            }
        }

        /* Above logic may put a flag in a box with an undefined corner.
           Remove any such flags */

        for (jg = 0; jg < jjs - 1; jg++)
        {
            ijg = jg * iis;
            for (ig = 0; ig < iis - 1; ig++)
            {
                if (u[ijg] == 0 || u[ijg + 1] == 0 || u[ijg + iis] == 0 || u[ijg + iis + 1] == 0)
                {
                    s1flg[ijg] = 0;
                    s2flg[ijg] = 0;
                    s3flg[ijg] = 0;
                    s4flg[ijg] = 0;
                }

                ijg++;
            }
        }
    }

/* Use the flags to create closed polygons of the shaded area */

    void s2poly(double[] r, int iis, int jjs, double c1, double c2)
    {
        int ig, jg, ijg, rc;

        /* Loop thru grid boxes to find unused polygon edges.  */

        for (jg = 0; jg < jjs - 1; jg++)
        {
            ijg = jg * iis;
            for (ig = 0; ig < iis -1; ig++)
            {
                rc = 0;
                if (s1flg[ijg]>0) rc = s2follow(r,  iis,
                jjs,c1,c2,1,ig,jg);
                if (s2flg[ijg]>0) rc = s2follow(r,  iis,
                jjs,c1,c2,2,ig,jg);
                if (s3flg[ijg]>0) rc = s2follow(r,  iis,
                jjs,c1,c2,3,ig,jg);
                if (s4flg[ijg]>0) rc = s2follow(r,  iis,
                jjs,c1,c2,4,ig,jg);
                if (rc > 0) return;
                ijg++;
            }
        }
    }

    int s2follow(double[] r, int iis, int jjs, double c1, double c2, int side,
        int ig, int jg)
    {
        double p1, p2, p3, p4;
        int ii, jj, ij, fl, sflg, n, k, rc;
        int f1, f2, f3, f4;

        pnum = 0;

        /* save starting point */

        ii = ig;
        jj = jg;
        sflg = 0;

        /* Follow the polygon edge that begins with the indicated box and side.  
           Keep the interior to the right hand side.  
           It seems natural here to go to the next intersection point by 
             using goto's.  A rare case where goto's make sense.
           The labels with "side" are of the form sidexyz, where x is the side number, 
             y is either i for intersect or b for boundary, and z is 1 if we are intersecting
             with c1 or 2 for a c2 intersect.  
           The labels with "corner" indicate we are following a boundary and are 
             going along sides of boxes from corner to corner.                   
           Yes, there are 20 cases in all.
           Cols are handled as individual grid boxes, due to their complexity. */


        ij = jj * iis +ii;

        if (side == 1)
        {
            fl = s1flg[ij];
            if (bug>0) _drawingContext.Logger?.LogInformation($"entering side 1 {ii} {jj} {fl}");
            if (fl == 5 || fl == 6) goto skip;
            if (fl == 1)
            {
                ii--;
                ig--;
                side = 3;
                goto side3i1;
            }

            if (fl == 2 || fl == 12 || fl == 16) goto side1i1;
            if (fl == 11 || fl == 15) goto side1b1;
            if (fl == 3 || fl == 13) goto side1i2;
            if (fl == 4)
            {
                ii--;
                jg--;
                side = 3;
                goto side3i2;
            }

            if (fl == 14) goto side1b2;
            if (fl == 10) goto corner2;
        }

        if (side == 2)
        {
            fl = s2flg[ij];
            if (bug > 0) _drawingContext.Logger?.LogInformation($"entering side 2 {ii} {jj} {fl}");
            if (fl == 5 || fl == 6) goto skip;
            if (fl == 1)
            {
                jj++;
                jg++;
                side = 4;
                goto side4i1;
            }

            if (fl == 2 || fl == 12 || fl == 16) goto side2i1;
            if (fl == 11 || fl == 15) goto side2b1;
            if (fl == 3 || fl == 13) goto side2i2;
            if (fl == 4)
            {
                jj++;
                jg++;
                side = 4;
                goto side4i2;
            }

            if (fl == 14) goto side2b2;
            if (fl == 10) goto corner3;
        }

        if (side == 3)
        {
            fl = s3flg[ij];
            if (bug > 0) _drawingContext.Logger?.LogInformation($"entering side 3 {ii} {jj} {fl}");
            if (fl == 5 || fl == 6) goto skip;
            if (fl == 2)
            {
                ii++;
                ig++;
                side = 1;
                goto side1i1;
            }

            if (fl == 1 || fl == 11 || fl == 15) goto side3i1;
            if (fl == 12 || fl == 16) goto side3b1;
            if (fl == 4 || fl == 14) goto side3i2;
            if (fl == 3)
            {
                ii++;
                ig++;
                side = 1;
                goto side1i2;
            }

            if (fl == 13) goto side3b2;
            if (fl == 10) goto corner4;
        }

        if (side == 4)
        {
            fl = s4flg[ij];
            if (bug > 0) _drawingContext.Logger?.LogInformation($"entering side 4 {ii} {jj} {fl}");
            if (fl == 5 || fl == 6) goto skip;
            if (fl == 2)
            {
                jj--;
                jg--;
                side = 2;
                goto side2i1;
            }

            if (fl == 1 || fl == 11 || fl == 15) goto side4i1;
            if (fl == 12 || fl == 16) goto side4b1;
            if (fl == 4 || fl == 14) goto side4i2;
            if (fl == 3)
            {
                jj--;
                jg--;
                side = 2;
                goto side2i2;
            }

            if (fl == 13) goto side4b2;
            if (fl == 10) goto corner1;
        }

        /* we should not get here */

        goto err4;

        side1i1: /* Enter on side 1; c1 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side1 c1 {ii} {jj}{ig} {jg}");
        ij = jj * iis + ii;
        p1 = rr[ij];
        p2 = rr[ij + isize];
        s2ppnt((double)(ii + 1), (double)(jj + 1) + (c1 - p1) / (p2 - p1));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f1 < 10 && f1 != s3flg[ij - 1]) goto cerr;
        if (sflg>0)
        {
            if (f1 == 2)
            {
                s1flg[ij] = 0;
                s3flg[ij - 1] = 0;
            }
            else if (f1 == 12 || f1 == 14) s1flg[ij] = 0;
            else if (f1 == 6)
            {
                s1flg[ij] = 4;
                s3flg[ij - 1] = 4;
            }
            else if (f1 == 16) s1flg[ij] = 14;

            if (ii == ig && jj == jg && side == 1) goto done;
        }

        sflg = 1;
        if ((f4 == 2 || f4 == 6 || f4 == 12 || f4 == 16) &&
            (f2 == 1 || f2 == 5 || f2 == 11 || f2 == 15))
        {
            /* col */
            if (s2col(c1, ii, jj)>0)
            {
                if (f2 == 1 || f2 == 5)
                {
                    jj++;
                    goto side4i1;
                }
                else goto side2b1;
            }
            else
            {
                if (f4 == 2 || f4 == 6)
                {
                    jj--;
                    goto side2i1;
                }
                else goto side4b1;
            }
        }

        if (f4 == 2 || f4 == 6)
        {
            jj--;
            goto side2i1;
        }

        if (f3 == 2 || f3 == 6)
        {
            ii++;
            goto side1i1;
        }

        if (f2 == 1 || f2 == 5)
        {
            jj++;
            goto side4i1;
        }

        if (f4 == 12 || f4 == 16) goto side4b1;
        if (f3 == 12 || f3 == 16) goto side3b1;
        if (f2 == 11 || f2 == 15) goto side2b1;
        goto err1;

        side2i1: /* Enter on side 2; c1 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side2 c1 {ii} {jj}");
        ij = jj * iis +ii;
        p2 = rr[ij + isize];
        p3 = rr[ij + isize + 1];
        s2ppnt((double)(ii + 1) + (c1 - p2) / (p3 - p2), (double)(jj + 2));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f2 < 10 && f2 != s4flg[ij + iis]) goto cerr;
        if (sflg>0)
        {
            if (f2 == 2)
            {
                s2flg[ij] = 0;
                s4flg[ij + iis] = 0;
            }
            else if (f2 == 12 || f2 == 14) s2flg[ij] = 0;
            else if (f2 == 6)
            {
                s2flg[ij] = 4;
                s4flg[ij + iis] = 4;
            }
            else if (f2 == 16) s2flg[ij] = 14;

            if (ii == ig && jj == jg && side == 2) goto done;
        }

        sflg = 1;
        if ((f1 == 1 || f1 == 5 || f1 == 11 || f1 == 15) &&
            (f3 == 2 || f3 == 6 || f3 == 12 || f3 == 16))
        {
            /* col */
            if (s2col(c1, ii, jj)>0)
            {
                if (f1 == 1 || f1 == 5)
                {
                    ii--;
                    goto side3i1;
                }
                else goto side1b1;
            }
            else
            {
                if (f3 == 2 || f3 == 6)
                {
                    ii++;
                    goto side1i1;
                }
                else goto side3b1;
            }
        }

        if (f1 == 1 || f1 == 5)
        {
            ii--;
            goto side3i1;
        }

        if (f4 == 2 || f4 == 6)
        {
            jj--;
            goto side2i1;
        }

        if (f3 == 2 || f3 == 6)
        {
            ii++;
            goto side1i1;
        }

        if (f1 == 11 || f1 == 15) goto side1b1;
        if (f4 == 12 || f4 == 16) goto side4b1;
        if (f3 == 12 || f3 == 16) goto side3b1;
        goto err1;

        side3i1: /* Enter on side 3; c1 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side3 c1 {ii} {jj}");
        ij = jj * iis +ii;
        p3 = rr[ij + isize + 1];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 2), (double)(jj + 1) + (c1 - p4) / (p3 - p4));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f3 < 10 && f3 != s1flg[ij + 1]) goto cerr;
        if (sflg > 0)
        {
            if (f3 == 1)
            {
                s3flg[ij] = 0;
                s1flg[ij + 1] = 0;
            }
            else if (f3 == 11 || f3 == 13) s3flg[ij] = 0;
            else if (f3 == 5)
            {
                s3flg[ij] = 3;
                s1flg[ij + 1] = 3;
            }
            else if (f3 == 15) s3flg[ij] = 13;

            if (ii == ig && jj == jg && side == 3) goto done;
        }

        sflg = 1;
        if ((f2 == 1 || f2 == 5 || f2 == 11 || f2 == 15) &&
            (f4 == 2 || f4 == 6 || f4 == 12 || f4 == 16))
        {
            /* col */
            if (s2col(c1, ii, jj)>0)
            {
                if (f4 == 2 || f4 == 6)
                {
                    jj--;
                    goto side2i1;
                }
                else goto side4b1;
            }
            else
            {
                if (f2 == 1 || f2 == 5)
                {
                    jj++;
                    goto side4i1;
                }
                else goto side2b1;
            }
        }

        if (f2 == 1 || f2 == 5)
        {
            jj++;
            goto side4i1;
        }

        if (f1 == 1 || f1 == 5)
        {
            ii--;
            goto side3i1;
        }

        if (f4 == 2 || f4 == 6)
        {
            jj--;
            goto side2i1;
        }

        if (f2 == 11 || f2 == 15) goto side2b1;
        if (f1 == 11 || f1 == 15) goto side1b1;
        if (f4 == 12 || f4 == 16) goto side4b1;
        goto err1;

        side4i1: /* Enter on side 4; c1 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side4 c1 {ii} {jj}");
        ij = jj * iis +ii;
        p1 = rr[ij];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 1) + (c1 - p1) / (p4 - p1), (double)(jj + 1));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f4 < 10 && f4 != s2flg[ij - iis]) goto cerr;
        if (sflg>0)
        {
            if (f4 == 1)
            {
                s4flg[ij] = 0;
                s2flg[ij - iis] = 0;
            }
            else if (f4 == 11 || f4 == 13) s4flg[ij] = 0;
            else if (f4 == 5)
            {
                s4flg[ij] = 3;
                s2flg[ij - iis] = 3;
            }
            else if (f4 == 15) s4flg[ij] = 13;

            if (ii == ig && jj == jg && side == 4) goto done;
        }

        sflg = 1;
        if ((f3 == 2 || f3 == 6 || f3 == 12 || f3 == 16) &&
            (f1 == 1 || f1 == 5 || f1 == 11 || f1 == 15))
        {
            /* col */
            if (s2col(c1, ii, jj)>0)
            {
                if (f3 == 2 || f3 == 6)
                {
                    ii++;
                    goto side1i1;
                }
                else goto side3b1;
            }
            else
            {
                if (f1 == 1 || f1 == 5)
                {
                    ii--;
                    goto side3i1;
                }
                else goto side1b1;
            }
        }

        if (f3 == 2 || f3 == 6)
        {
            ii++;
            goto side1i1;
        }

        if (f2 == 1 || f2 == 5)
        {
            jj++;
            goto side4i1;
        }

        if (f1 == 1 || f1 == 5)
        {
            ii--;
            goto side3i1;
        }

        if (f3 == 12 || f3 == 16) goto side3b1;
        if (f2 == 11 || f2 == 15) goto side2b1;
        if (f1 == 11 || f1 == 15) goto side1b1;
        goto err1;

        side1i2: /* Enter on side 1; c2 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side1 c2 {ii} {jj}");
        ij = jj * iis +ii;
        p1 = rr[ij];
        p2 = rr[ij + isize];
        s2ppnt((double)(ii + 1), (double)(jj + 1) + (c2 - p1) / (p2 - p1));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f1 < 10 && f1 != s3flg[ij - 1]) goto cerr;
        if (sflg > 0)
        {
            if (f1 == 3)
            {
                s1flg[ij] = 0;
                s3flg[ij - 1] = 0;
            }
            else if (f1 == 13 || f1 == 11) s1flg[ij] = 0;
            else if (f1 == 5)
            {
                s1flg[ij] = 1;
                s3flg[ij - 1] = 1;
            }
            else if (f1 == 15) s1flg[ij] = 11;

            if (ii == ig && jj == jg && side == 1) goto done;
        }

        sflg = 1;
        if ((f4 == 3 || f4 == 5 || f4 == 13 || f4 == 15) &&
            (f2 == 4 || f2 == 6 || f2 == 14 || f2 == 16))
        {
            /* col */
            if (s2col(c2, ii, jj)>0)
            {
                if (f2 == 4 || f2 == 6)
                {
                    jj++;
                    goto side4i2;
                }
                else goto side2b2;
            }
            else
            {
                if (f4 == 3 || f4 == 5)
                {
                    jj--;
                    goto side2i2;
                }
                else goto side4b2;
            }
        }

        if (f4 == 3 || f4 == 5)
        {
            jj--;
            goto side2i2;
        }

        if (f3 == 3 || f3 == 5)
        {
            ii++;
            goto side1i2;
        }

        if (f2 == 4 || f2 == 6)
        {
            jj++;
            goto side4i2;
        }

        if (f4 == 13 || f4 == 15) goto side4b2;
        if (f3 == 13 || f3 == 15) goto side3b2;
        if (f2 == 14 || f2 == 16) goto side2b2;
        goto err2;

        side2i2: /* Enter on side 2; c2 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side2 c2 {ii} {jj}");
        ij = jj * iis +ii;
        p2 = rr[ij + isize];
        p3 = rr[ij + isize + 1];
        s2ppnt((double)(ii + 1) + (c2 - p2) / (p3 - p2), (double)(jj + 2));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f2 < 10 && f2 != s4flg[ij + iis]) goto cerr;
        if (sflg > 0)
        {
            if (f2 == 3)
            {
                s2flg[ij] = 0;
                s4flg[ij + iis] = 0;
            }
            else if (f2 == 13 || f2 == 11) s2flg[ij] = 0;
            else if (f2 == 5)
            {
                s2flg[ij] = 1;
                s4flg[ij + iis] = 1;
            }
            else if (f2 == 15) s2flg[ij] = 11;

            if (ii == ig && jj == jg && side == 2) goto done;
        }

        sflg = 1;
        if ((f1 == 4 || f1 == 6 || f1 == 14 || f1 == 16) &&
            (f3 == 3 || f3 == 5 || f3 == 13 || f3 == 15))
        {
            /* col */
            if (s2col(c2, ii, jj)>0)
            {
                if (f1 == 4 || f1 == 6)
                {
                    ii--;
                    goto side3i2;
                }
                else goto side1b2;
            }
            else
            {
                if (f3 == 3 || f3 == 5)
                {
                    ii++;
                    goto side1i2;
                }
                else goto side3b2;
            }
        }

        if (f1 == 4 || f1 == 6)
        {
            ii--;
            goto side3i2;
        }

        if (f4 == 3 || f4 == 5)
        {
            jj--;
            goto side2i2;
        }

        if (f3 == 3 || f3 == 5)
        {
            ii++;
            goto side1i2;
        }

        if (f1 == 14 || f1 == 16) goto side1b2;
        if (f4 == 13 || f4 == 15) goto side4b2;
        if (f3 == 13 || f3 == 15) goto side3b2;
        goto err2;

        side3i2: /* Enter on side 3; c2 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side3 c2 {ii} {jj}");
        ij = jj * iis +ii;
        p3 = rr[ij + isize + 1];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 2), (double)(jj + 1) + (c2 - p4) / (p3 - p4));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f3 < 10 && f3 != s1flg[ij + 1]) goto cerr;
        if (sflg > 0)
        {
            if (f3 == 4)
            {
                s3flg[ij] = 0;
                s1flg[ij + 1] = 0;
            }
            else if (f3 == 14 || f3 == 12) s3flg[ij] = 0;
            else if (f3 == 6)
            {
                s3flg[ij] = 2;
                s1flg[ij + 1] = 2;
            }
            else if (f3 == 16) s3flg[ij] = 12;

            if (ii == ig && jj == jg && side == 3) goto done;
        }

        sflg = 1;
        if ((f2 == 4 || f2 == 6 || f2 == 14 || f2 == 16) &&
            (f4 == 3 || f4 == 5 || f4 == 13 || f4 == 15))
        {
            /* col */
            if (s2col(c2, ii, jj)>0)
            {
                if (f4 == 3 || f4 == 5)
                {
                    jj--;
                    goto side2i2;
                }
                else goto side4b2;
            }
            else
            {
                if (f2 == 4 || f2 == 6)
                {
                    jj++;
                    goto side4i2;
                }
                else goto side2b2;
            }
        }

        if (f2 == 4 || f2 == 6)
        {
            jj++;
            goto side4i2;
        }

        if (f1 == 4 || f1 == 6)
        {
            ii--;
            goto side3i2;
        }

        if (f4 == 3 || f4 == 5)
        {
            jj--;
            goto side2i2;
        }

        if (f2 == 14 || f2 == 16) goto side2b2;
        if (f1 == 14 || f1 == 16) goto side1b2;
        if (f4 == 13 || f4 == 15) goto side4b2;
        goto err2;

        side4i2: /* Enter on side 4; c2 intersect */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side4 c2 {ii} {jj}");
        ij = jj * iis +ii;
        p1 = rr[ij];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 1) + (c2 - p1) / (p4 - p1), (double)(jj + 1));
        f1 = s1flg[ij];
        f2 = s2flg[ij];
        f3 = s3flg[ij];
        f4 = s4flg[ij];
        if (f4 < 10 && f4 != s2flg[ij - iis]) goto cerr;
        if (sflg > 0)
        {
            if (f4 == 4)
            {
                s4flg[ij] = 0;
                s2flg[ij - iis] = 0;
            }
            else if (f4 == 14 || f4 == 12) s4flg[ij] = 0;
            else if (f4 == 6)
            {
                s4flg[ij] = 2;
                s2flg[ij - iis] = 2;
            }
            else if (f4 == 16) s4flg[ij] = 12;

            if (ii == ig && jj == jg && side == 4) goto done;
        }

        sflg = 1;
        if ((f3 == 3 || f3 == 5 || f3 == 13 || f3 == 15) &&
            (f1 == 4 || f1 == 6 || f1 == 14 || f1 == 16))
        {
            /* col */
            if (s2col(c2, ii, jj)>0)
            {
                if (f3 == 3 || f3 == 5)
                {
                    ii++;
                    goto side1i2;
                }
                else goto side3b2;
            }
            else
            {
                if (f1 == 4 || f1 == 6)
                {
                    ii--;
                    goto side3i2;
                }
                else goto side1b2;
            }
        }

        if (f3 == 3 || f3 == 5)
        {
            ii++;
            goto side1i2;
        }

        if (f2 == 4 || f2 == 6)
        {
            jj++;
            goto side4i2;
        }

        if (f1 == 4 || f1 == 6)
        {
            ii--;
            goto side3i2;
        }

        if (f3 == 13 || f3 == 15) goto side3b2;
        if (f2 == 14 || f2 == 16) goto side2b2;
        if (f1 == 14 || f1 == 16) goto side1b2;
        goto err2;

        side1b1: /* Hit boundary on side1; c1 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side1 b c1 {ii} {jj}");
        p1 = rr[ij];
        p2 = rr[ij + isize];
        s2ppnt((double)(ii + 1), (double)(jj + 1) + (c1 - p1) / (p2 - p1));
        if (sflg>0 && ii == ig && jj == jg && side == 1)
        {
            s1flg[ij] = 0;
            goto done;
        }

        if (s1flg[ij] == 15)
        {
            s1flg[ij] = 11;
            goto side1i2;
        }

        goto corner2;

        side2b1: /* Hit boundary on side2; c1 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side2 b c1 {ii} {jj}");
        p2 = rr[ij + isize];
        p3 = rr[ij + isize + 1];
        s2ppnt((double)(ii + 1) + (c1 - p2) / (p3 - p2), (double)(jj + 2));
        if (sflg > 0 && ii == ig && jj == jg && side == 2)
        {
            s2flg[ij] = 0;
            goto done;
        }

        if (s2flg[ij] == 15)
        {
            s2flg[ij] = 11;
            goto side2i2;
        }

        goto corner3;

        side3b1: /* Hit boundary on side3; c1 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side3 b c1 {ii} {jj}");
        p3 = rr[ij + isize + 1];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 2), (double)(jj + 1) + (c1 - p4) / (p3 - p4));
        if (sflg>0 && ii == ig && jj == jg && side == 3)
        {
            s3flg[ij] = 0;
            goto done;
        }

        if (s3flg[ij] == 16)
        {
            s3flg[ij] = 12;
            goto side3i2;
        }

        goto corner4;

        side4b1: /* Hit boundary on side4; c1 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side4 b c1 {ii} {jj}");
        p1 = rr[ij];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 1) + (c1 - p1) / (p4 - p1), (double)(jj + 1));
        if (sflg>0 && ii == ig && jj == jg && side == 4)
        {
            s4flg[ij] = 0;
            goto done;
        }

        if (s4flg[ij] == 16)
        {
            s4flg[ij] = 12;
            goto side4i2;
        }

        goto corner1;

        side1b2: /* Hit boundary on side1; c2 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side1 b c2 {ii} {jj}");
        p1 = rr[ij];
        p2 = rr[ij + isize];
        s2ppnt((double)(ii + 1), (double)(jj + 1) + (c2 - p1) / (p2 - p1));
        p1 = rr[ij];
        p2 = rr[ij + isize];
        s2ppnt((double)(ii + 1), (double)(jj + 1) + (c2 - p1) / (p2 - p1));
        if (sflg>0 && ii == ig && jj == jg && side == 1)
        {
            s1flg[ij] = 0;
            goto done;
        }

        if (s1flg[ij] == 16)
        {
            s1flg[ij] = 14;
            goto side1i1;
        }

        goto corner2;

        side2b2: /* Hit boundary on side2; c2 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side2 b c2 {ii} {jj}");
        p2 = rr[ij + isize];
        p3 = rr[ij + isize + 1];
        s2ppnt((double)(ii + 1) + (c2 - p2) / (p3 - p2), (double)(jj + 2));
        if (sflg>0 && ii == ig && jj == jg && side == 2)
        {
            s2flg[ij] = 0;
            goto done;
        }

        if (s2flg[ij] == 16)
        {
            s2flg[ij] = 14;
            goto side2i1;
        }

        goto corner3;

        side3b2: /* Hit boundary on side3; c2 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side3 b c2 {ii} {jj}");
        p3 = rr[ij + isize + 1];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 2), (double)(jj + 1) + (c2 - p4) / (p3 - p4));
        if (sflg>0 && ii == ig && jj == jg && side == 3)
        {
            s3flg[ij] = 0;
            goto done;
        }

        if (s3flg[ij] == 15)
        {
            s3flg[ij] = 13;
            goto side3i1;
        }

        goto corner4;

        side4b2: /* Hit boundary on side4; c2 intersect  */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"side4 b c2 {ii} {jj}");
        p1 = rr[ij];
        p4 = rr[ij + 1];
        s2ppnt((double)(ii + 1) + (c2 - p1) / (p4 - p1), (double)(jj + 1));
        if (sflg>0 && ii == ig && jj == jg && side == 4)
        {
            s4flg[ij] = 0;
            goto done;
        }

        if (s4flg[ij] == 15)
        {
            s4flg[ij] = 13;
            goto side4i1;
        }

        goto corner1;

        corner1: /* Arriving at corner 1 from the right */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"corner 1 {ii} {jj}");
        s2ppnt((double)(ii + 1), (double)(jj + 1));
        ij = jj * iis +ii;
        if (sflg > 0)
        {
            s4flg[ij] = 0;
            if (ii == ig && jj == jg && side == 4) goto done;
        }

        sflg = 1;
        if (s1flg[ij] == 10) goto corner2;
        if (s1flg[ij] == 12) goto side1i1;
        if (s1flg[ij] == 13) goto side1i2;
        ii--;
        if (s4flg[ij - 1] == 10) goto corner1;
        if (s4flg[ij - 1] == 11) goto side4i1;
        if (s4flg[ij - 1] == 14) goto side4i2;
        jj--;
        ij = jj * iis +ii;
        if (s3flg[ij] == 10) goto corner4;
        if (s3flg[ij] == 11) goto side3i1;
        if (s3flg[ij] == 14) goto side3i2;
        ii++;
        jj++;
        _drawingContext.Logger?.LogInformation($"err 3 corner 1 {ii} {jj}");
        goto err3;

        corner2: /* Arriving at corner 2 from below */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"corner 2 {ii} {jj}");
        s2ppnt((double)(ii + 1), (double)(jj + 2));
        ij = jj * iis +ii;
        if (sflg > 0)
        {
            s1flg[ij] = 0;
            if (ii == ig && jj == jg && side == 1) goto done;
        }

        sflg = 1;
        if (s2flg[ij] == 10) goto corner3;
        if (s2flg[ij] == 12) goto side2i1;
        if (s2flg[ij] == 13) goto side2i2;
        jj++;
        ij = jj * iis +ii;
        if (s1flg[ij] == 10) goto corner2;
        if (s1flg[ij] == 12) goto side1i1;
        if (s1flg[ij] == 13) goto side1i2;
        ii--;
        if (s4flg[ij - 1] == 10) goto corner1;
        if (s4flg[ij - 1] == 11) goto side4i1;
        if (s4flg[ij - 1] == 14) goto side4i2;
        ii++;
        jj--;
        _drawingContext.Logger?.LogInformation($"err 3 corner 2 {ii} {jj}");
        goto err3;

        corner3: /* Arriving at corner 3 from the left */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"corner 3 {ii} {jj}");
        s2ppnt((double)(ii + 2), (double)(jj + 2));
        ij = jj * iis +ii;
        if (sflg > 0)
        {
            s2flg[ij] = 0;
            if (ii == ig && jj == jg && side == 2) goto done;
        }

        sflg = 1;
        if (s3flg[ij] == 10) goto corner4;
        if (s3flg[ij] == 11) goto side3i1;
        if (s3flg[ij] == 14) goto side3i2;
        ii++;
        if (s2flg[ij + 1] == 10) goto corner3;
        if (s2flg[ij + 1] == 12) goto side2i1;
        if (s2flg[ij + 1] == 13) goto side2i2;
        jj++;
        ij = jj * iis +ii;
        if (s1flg[ij] == 10) goto corner2;
        if (s1flg[ij] == 12) goto side1i1;
        if (s1flg[ij] == 13) goto side1i2;
        ii--;
        jj--;
        _drawingContext.Logger?.LogInformation($"err 3 corner 3 {ii} {jj}");
        goto err3;

        corner4: /* Arriving at corner 4 from above */
        if (bug > 0) _drawingContext.Logger?.LogInformation($"corner 4 {ii} {jj}");
        s2ppnt((double)(ii + 2), (double)(jj + 1));
        ij = jj * iis +ii;
        if (sflg > 0)
        {
            s3flg[ij] = 0;
            if (ii == ig && jj == jg && side == 3) goto done;
        }

        sflg = 1;
        if (s4flg[ij] == 10) goto corner1;
        if (s4flg[ij] == 11) goto side4i1;
        if (s4flg[ij] == 14) goto side4i2;
        jj--;
        ij = jj * iis +ii;
        if (s3flg[ij] == 10) goto corner4;
        if (s3flg[ij] == 11) goto side3i1;
        if (s3flg[ij] == 14) goto side3i2;
        ii++;
        if (s2flg[ij + 1] == 10) goto corner3;
        if (s2flg[ij + 1] == 12) goto side2i1;
        if (s2flg[ij + 1] == 13) goto side2i2;
        ii--;
        jj++;
        _drawingContext.Logger?.LogInformation($"err 3 corner 4 {ii} {jj}");
        goto err3;

        done:
        if (bug > 0) _drawingContext.Logger?.LogInformation($"done\n");

        if (xynum < 0) return (1);

        xxyy[pnum * 2] = xxyy[0]; /* Insure polygon closure */
        xxyy[pnum * 2 + 1] = xxyy[1];
        pnum++;
        k = 0;
        for (n = 1; n < pnum; n++)
        {
            /* Remove adjacent dups */
            if (Math.Abs(xxyy[n * 2] - xxyy[k * 2]) > 1e-5 || Math.Abs(xxyy[n * 2 + 1] - xxyy[k * 2 + 1]) > 1e-5)
            {
                if (n != k + 1)
                {
                    xxyy[(k + 1) * 2] = xxyy[n * 2];
                    xxyy[(k + 1) * 2 + 1] = xxyy[n * 2 + 1];
                }

                k++;
            }
        }

        k++;
        if (bug > 0) _drawingContext.Logger?.LogInformation($"pnum {pnum} {k}");
        if (k < 3) return (0);

        if (!nodraw)
        {
            _drawingContext.GradsDrawingInterface.gxfill(xxyy, k);
        }

        if (bufopt)
        {
            rc = s2bufpoly(k);
            if (rc > 0)
            {
                s2frepbuf();
                _drawingContext.Logger?.LogInformation($"Memory error in shade2: Unable to allocate Polygon Buffer\n");
                bufopt = false;
            }
        }

        if (bug > 0)
        {
            n = GradsDrawingInterface.lcolor;
            _drawingContext.GradsDrawingInterface.SetDrawingColor(0);
            _drawingContext.GradsDrawingInterface.gxplot(xxyy[0], xxyy[1], 3);
            for (ii = 1; ii < k; ii++)
            {
                _drawingContext.GradsDrawingInterface.gxplot(xxyy[ii * 2], xxyy[ii * 2 + 1], 2);
            }

            _drawingContext.GradsDrawingInterface.SetDrawingColor(n);
        }

        return (0);

        skip:
        return (0);

        err1:
        _drawingContext.Logger?.LogInformation($"Logic Error 1 in gxshad2 s2follow\n");
        return (1);
        err2:
        _drawingContext.Logger?.LogInformation($"Logic Error 2 in gxshad2 s2follow\n");
        return (1);
        err3:
        _drawingContext.Logger?.LogInformation($"Logic Error 3 in gxshad2 s2follow\n");
        return (1);
        err4:
        _drawingContext.Logger?.LogInformation($"Logic Error 4 in gxshad2 s2follow\n");
        return (1);
        cerr:
        _drawingContext.Logger?.LogInformation($"Logic Error 5 in gxshad2 s2follow\n");
        return (1);
    }


/* Determine the intersect points in a single grid box for the polygons
   that are in this grid box for the requested shading range.  The intersect
   points are provided in the range of 0 to 1.  Check for a col, and if there
   is a col, determine if there should be one polygon or two, and if two, 
   determine what sides each polygon intersects.  

       v4 --- v3
        |     |
        |     |
       v1 --- v2

   c1: lower contour level
   c2: upper contour level

   No undefs in this box.  Check before calling.
*/

    void s2box(double v1, double v2, double v3, double v4, double c1, double c2)
    {
        int fl1a, fl1b, fl2a, fl2b, fl3a, fl3b, fl4a, fl4b, col1, col2;
        int path1, path2;

        numpoly = 0;

        np = 0;
        typ1 = 0;
        typ2 = 0;
        typ3 = 0;

        fl1a = 0;
        fl1b = 0;
        fl2a = 0;
        fl2b = 0;
        fl3a = 0;
        fl3b = 0;
        fl4a = 0;
        fl4b = 0;

        /* Determine if a contour intersects a side. 
           flag a = c1 intersects.  flab b = c2 intersects. */

        if ((v1 <= c1 && v2 > c1) || (v1 > c1 && v2 <= c1)) fl1a = 1;
        if ((v1 <= c2 && v2 > c2) || (v1 > c2 && v2 <= c2)) fl1b = 1;

        if ((v2 <= c1 && v3 > c1) || (v2 > c1 && v3 <= c1)) fl2a = 1;
        if ((v2 <= c2 && v3 > c2) || (v2 > c2 && v3 <= c2)) fl2b = 1;

        if ((v3 <= c1 && v4 > c1) || (v3 > c1 && v4 <= c1)) fl3a = 1;
        if ((v3 <= c2 && v4 > c2) || (v3 > c2 && v4 <= c2)) fl3b = 1;

        if ((v4 <= c1 && v1 > c1) || (v4 > c1 && v1 <= c1)) fl4a = 1;
        if ((v4 <= c2 && v1 > c2) || (v4 > c2 && v1 <= c2)) fl4b = 1;

        /* Travel around the box and find all the intersect points within
           our contour range */

        if (v1 > c1 && v1 <= c2) s2pdrop(0.0, 0.0, 1, 1);
        if (fl1a > 0 && fl1b > 0)
        {
            /* insure points are in order */
            if (v2 >= v1)
            {
                s2pdrop((c1 - v1) / (v2 - v1), 0.0, 2, 1);
                s2pdrop((c2 - v1) / (v2 - v1), 0.0, 3, 1);
            }
            else
            {
                s2pdrop((c2 - v1) / (v2 - v1), 0.0, 3, 1);
                s2pdrop((c1 - v1) / (v2 - v1), 0.0, 2, 1);
            }
        }
        else if (fl1a > 0) s2pdrop((c1 - v1) / (v2 - v1), 0.0, 2, 1);
        else if (fl1b > 0) s2pdrop((c2 - v1) / (v2 - v1), 0.0, 3, 1);

        if (v2 > c1 && v2 <= c2) s2pdrop(1.0, 0.0, 1, 2);
        if (fl2a > 0 && fl2b > 0)
        {
            if (v3 >= v2)
            {
                s2pdrop(1.0, (c1 - v2) / (v3 - v2), 2, 2);
                s2pdrop(1.0, (c2 - v2) / (v3 - v2), 3, 2);
            }
            else
            {
                s2pdrop(1.0, (c2 - v2) / (v3 - v2), 3, 2);
                s2pdrop(1.0, (c1 - v2) / (v3 - v2), 2, 2);
            }
        }
        else if (fl2a > 0) s2pdrop(1.0, (c1 - v2) / (v3 - v2), 2, 2);
        else if (fl2b > 0) s2pdrop(1.0, (c2 - v2) / (v3 - v2), 3, 2);

        if (v3 > c1 && v3 <= c2) s2pdrop(1.0, 1.0, 1, 3);
        if (fl3a > 0 && fl3b > 0)
        {
            if (v4 >= v3)
            {
                s2pdrop((c1 - v4) / (v3 - v4), 1.0, 2, 3);
                s2pdrop((c2 - v4) / (v3 - v4), 1.0, 3, 3);
            }
            else
            {
                s2pdrop((c2 - v4) / (v3 - v4), 1.0, 3, 3);
                s2pdrop((c1 - v4) / (v3 - v4), 1.0, 2, 3);
            }
        }
        else if (fl3a > 0) s2pdrop((c1 - v4) / (v3 - v4), 1.0, 2, 3);
        else if (fl3b > 0) s2pdrop((c2 - v4) / (v3 - v4), 1.0, 3, 3);

        if (v4 > c1 && v4 <= c2) s2pdrop(0.0, 1.0, 1, 4);
        if (fl4a > 0 && fl4b > 0)
        {
            if (v1 >= v4)
            {
                s2pdrop(0.0, (c1 - v1) / (v4 - v1), 2, 4);
                s2pdrop(0.0, (c2 - v1) / (v4 - v1), 3, 4);
            }
            else
            {
                s2pdrop(0.0, (c2 - v1) / (v4 - v1), 3, 4);
                s2pdrop(0.0, (c1 - v1) / (v4 - v1), 2, 4);
            }
        }
        else if (fl4a > 0) s2pdrop(0.0, (c1 - v1) / (v4 - v1), 2, 4);
        else if (fl4b > 0) s2pdrop(0.0, (c2 - v1) / (v4 - v1), 3, 4);

        if (np == 0) return; /* If no intersects, just return */

        numpoly = 1;
        polyside = 1;

        /* Check for col  */

        col1 = 0;
        col2 = 0;
        if (fl1a > 0 && fl2a > 0 && fl3a > 0 && fl4a > 0) col1 = 1;
        if (fl1b > 0 && fl2b > 0 && fl3b > 0 && fl4b > 0) col2 = 1;

        if (col1 > 0 && col2 > 0)
        {
            /* both levels are cols */
            path1 = s2pathln(c1, v1, v2, v3, v4);
            path2 = s2pathln(c2, v1, v2, v3, v4);
            if (path1 == path2)
            {
                numpoly = 2;
                if (path1 == 0) polyside = 2;
            }
        }
        else if (col1 > 0)
        {
            /* only lower level is a col */
            path1 = s2pathln(c1, v1, v2, v3, v4);
            if (path1 == 1)
            {
                if (v1 <= c1)
                {
                    numpoly = 2;
                }
            }
            else
            {
                if (v2 <= c1)
                {
                    numpoly = 2;
                    polyside = 2;
                }
            }
        }
        else if (col2 > 0)
        {
            /* only upper level is a col */
            path2 = s2pathln(c2, v1, v2, v3, v4);
            if (path2 == 1)
            {
                if (v1 > c2)
                {
                    numpoly = 2;
                }
            }
            else
            {
                if (v2 > c2)
                {
                    numpoly = 2;
                    polyside = 2;
                }
            }
        }
    }

    void s2ppnt(double x, double y)
    {
        double xx, yy;
        double[] xynew;
        int i;

        if (xynum < 0) return;

        /* Increase polygon buffer size if necessary */

        if (pnum > xynum - 3)
        {
            xynum = xynum * 2;
            if (bug > 0) _drawingContext.Logger?.LogInformation($"Poly buff memory for {xynum} points");
            xynew = new double[xynum * 2];
            
            for (i = 0; i < pnum * 2; i++) xynew[i] = xxyy[i];
            xxyy = xynew;
        }

        GradsDrawingInterface.gxconv(x, y, out xx, out yy, 3);
        xxyy[pnum * 2 ] = xx;
        xxyy[pnum * 2 + 1 ]= yy;

        pnum++;
    }

/* Add an intersect point to the array of points. Also add a corner point
   if it is inside the range of clevs.  Keep track of what type of point it
   it is: an intersect with the lower clev, upper clev, or a corner point */

    void s2pdrop(double x, double y, int type, int side)
    {
        if (type == 1) typ1++;
        if (type == 2) typ2++;
        if (type == 3) typ3++;
        xp[np] = x;
        yp[np] = y;
        tp[np] = type;
        sp[np] = side;
        np++;
    }

/* Interface to s2pathln -- the convenction for ordering of the points in a
   box is not the same between gxcntr and gxshad2  */

    int s2col(double vv, int ii, int jj)
    {
        double p1, p2, p3, p4;
        int ij, rc;

        ij = jj * isize + ii;
        p1 = rr[ij];
        p2 = rr[ij + 1];
        p3 = rr[ij + isize + 1];
        p4 = rr[ij + isize];

        rc = s2pathln(vv, p1, p2, p3, p4);

        return (rc);
    }

/* Calculate shortest combined path length through a col point.
   Return true if shortest path is side 1/2,3/4, else false.  */

/*  THIS IS THE SAME AS IN GXCNTR, except the contour level is 
     passed as an arg. THIS NEEDS TO BE THE SAME AS THE VERSION IN GXCNTR
     SO THE SHADE BOUNDARIES ALIGN WITH LINE CONTOURS. */

    int s2pathln(double vv, double p1, double p2, double p3, double p4)
    {
        double v1, v2, v3, v4, d1, d2;

        v1 = (vv - p1) / (p2 - p1);
        v2 = (vv - p2) / (p3 - p2);
        v3 = (vv - p4) / (p3 - p4);
        v4 = (vv - p1) / (p4 - p1);
        d1 = GaUtil.hypot(1.0 - v1, v2) + GaUtil.hypot(1.0 - v4, v3);
        d2 = GaUtil.hypot(v1, v4) + GaUtil.hypot(1.0 - v2, 1.0 - v3);
        if (d2 < d1) return (0);
        return (1);
    }

/* for debugging... plot flags */

    void s2debug()
    {
        int ig, jg, offset, mk = 0;
        double sz, xxx, yyy;

        sz = 0.03;
        _drawingContext.GradsDrawingInterface.gxclip(0.0, 11.0, 0.0, 8.5);
        for (jg = 0; jg < jsize - 1; jg++)
        {
            for (ig = 0; ig < isize - 1; ig++)
            {
                offset = jg * isize + ig;

                if (pflg[offset]>0) _drawingContext.GradsDrawingInterface.SetDrawingColor(1);
                else _drawingContext.GradsDrawingInterface.SetDrawingColor(2);
                GradsDrawingInterface.gxconv((double)ig + 1.0, (double)jg + 1.0, out xxx, out yyy, 3);
                _drawingContext.GradsDrawingInterface.gxmark(3, xxx, yyy, sz);
                if (s1flg[offset]>0)
                {
                    _drawingContext.GradsDrawingInterface.SetDrawingColor(3);
                    /*       if (*(s1flg+offset)>7) gxcolr(12);
                             if (*(s1flg+offset)>10 && *(s1flg+offset)<20) gxcolr(6); */
                    if (s1flg[offset] > 9) _drawingContext.GradsDrawingInterface.SetDrawingColor(6);
                    if (s1flg[offset] == 88)
                    {
                        _drawingContext.GradsDrawingInterface.SetDrawingColor(6);
                        mk = 1;
                        sz = 0.06;
                    }

                    GradsDrawingInterface.gxconv((double)ig + 1.1, (double)jg + 1.5, out xxx, out yyy, 3);
                    _drawingContext.GradsDrawingInterface.gxmark(mk, xxx, yyy, sz);
                }

                if (s2flg[offset]>0)
                {
                    _drawingContext.GradsDrawingInterface.SetDrawingColor(3);
                    /* if (*(s2flg+offset)>7) gxcolr(12);
                    if (*(s2flg+offset)>10 && *(s2flg+offset)<20) gxcolr(6); */
                    if (s2flg[offset] > 9) _drawingContext.GradsDrawingInterface.SetDrawingColor(6);
                    GradsDrawingInterface.gxconv((double)ig + 1.5, (double)jg + 1.9, out xxx, out yyy, 3);
                    _drawingContext.GradsDrawingInterface.gxmark(3, xxx, yyy, sz);
                }

                if (s3flg[offset]>0)
                {
                    _drawingContext.GradsDrawingInterface.SetDrawingColor(3);
                    /* if (*(s3flg+offset)>7) gxcolr(12);
                    if (*(s3flg+offset)>90) gxcolr(7);
                    if (*(s3flg+offset)>10 && *(s3flg+offset)<20) gxcolr(6); */
                    if (s3flg[offset] > 9) _drawingContext.GradsDrawingInterface.SetDrawingColor(6);
                    GradsDrawingInterface.gxconv((double)ig + 1.9, (double)jg + 1.5, out xxx, out yyy, 3);
                    _drawingContext.GradsDrawingInterface.gxmark(3, xxx, yyy, sz);
                }

                if (s4flg[offset]>0)
                {
                    _drawingContext.GradsDrawingInterface.SetDrawingColor(3);
                    /* if (*(s4flg+offset)>7) gxcolr(12);
                    if (*(s4flg+offset)>90) gxcolr(7);
                    if (*(s4flg+offset)>10 && *(s4flg+offset)<20) gxcolr(6); */
                    if (s4flg[offset] > 9) _drawingContext.GradsDrawingInterface.SetDrawingColor(6);
                    GradsDrawingInterface.gxconv((double)ig + 1.5, (double)jg + 1.1, out xxx, out yyy, 3);
                    _drawingContext.GradsDrawingInterface.gxmark(3, xxx, yyy, sz);
                }
            }
        }
    }

/* When polygon buffering is requested, put the current polygon into 
   the s2pbuf chain */

    int s2bufpoly(int pcnt)
    {
        s2pbuf ppbuf = new s2pbuf();
        int sz, i;

        if (s2pbufanch == null)
        {

            s2pbufanch = new List<s2pbuf>();
            s2pbufanch.Add(ppbuf);
            bufcnt = 0;
        }
        else s2pbufanch.Add( ppbuf);

        /* Allocate space for the poly points */

        ppbuf.len = pcnt;
        sz = pcnt * 2;
        ppbuf.xy = new double[sz];
        
        /* Copy the poly points and info */

        for (i = 0; i < pcnt * 2; i++) ppbuf.xy[i] = xxyy[i];

        ppbuf.color = GradsDrawingInterface.lcolor;
        ppbuf.index = gindex;
        ppbuf.clev1 = blev;
        ppbuf.clev2 = alev;

        bufcnt++;
        return (0);
    }

/* Free the polygon buffer */

    void s2frepbuf()
    {
        s2pbufanch = null;
    }

/* Turn buffering on/off */

    void s2setbuf(bool flg)
    {
        bufopt = flg;
    }

/* Turn drawing of polygons on/off. If 1, polygons are not drawn */

    void s2setdraw(bool flg)
    {
        nodraw = flg;
    }


/* When gxout shape is in use, this routine is called 
   to dump all the polygon vertices to the shapefile.
   For each polygon in the buffer: 
     - get the vertex x/y coordinates 
     - convert them to lon/lat 
     - write out the vertices ('measured' value for each polygon is color #)
     - write out the attributes (clev1 and clev2 are the dynamic values) 
   The polygon buffer is released in gagx, inside the gashpwrt() routine. 

   Returns -1 on error, otherwise returns number of shapes written to file.
*/
// #if USESHP==1
//     int s2shpwrt(SHPHandle sfid, DBFHandle dbfid,  struct dbfld *dbanch) {
//         int i, rc, ival;
//         struct dbfld* fld;
//         struct s2pbuf* pbuf = NULL;
//         int shpid, *pstart = NULL,nParts,nFields;
//         SHPObject* shp;
//         double x, y, *lons = NULL,*lats = NULL,*vals = NULL,lon,lat,val,dval;
//
//         nParts = 1;
//         nFields = 1;
//         pstart = (int*)galloc(nParts * sizeof(int), "pstart");
//         *pstart = 0;
//         shpid = 0;
//
//         pbuf = s2pbufanch;
//         if (pbuf == NULL)
//         {
//             _drawingContext.Logger?.LogInformation($"Error in s2shpwrt: polygon buffer is empty\n");
//             rc = -1;
//             goto cleanup;
//         }
//
//         while (pbuf)
//         {
//             if (pbuf.xy)
//             {
//                 /* allocate memory for lons and lats of the vertices in polygon */
//                 if ((lons = (double*)galloc(pbuf.len * sizeof(double), "shplons")) == NULL)
//                 {
//                     _drawingContext.Logger?.LogInformation($"Error in s2shpwrt: unable to allocate memory for lon array\n");
//                     rc = -1;
//                     goto cleanup;
//                 }
//
//                 if ((lats = (double*)galloc(pbuf.len * sizeof(double), "shplats")) == NULL)
//                 {
//                     _drawingContext.Logger?.LogInformation($"Error in s2shpwrt: unable to allocate memory for lat array\n");
//                     rc = -1;
//                     goto cleanup;
//                 }
//
//                 if ((vals = (double*)galloc(pbuf.len * sizeof(double), "shpvals")) == NULL)
//                 {
//                     _drawingContext.Logger?.LogInformation($"Error in s2shpwrt: unable to allocate memory for val array\n");
//                     rc = -1;
//                     goto cleanup;
//                 }
//
//                 /* get x,y values and convert them to lon,lat */
//                 for (i = 0; i < pbuf.len; i++)
//                 {
//                     x = *(pbuf.xy + (2 * i));
//                     y = *(pbuf.xy + (2 * i + 1));
//                     gxxy2w(x, y, &lon, &lat);
//                     *(lons + i) = lon;
//                     *(lats + i) = lat;
//                     *(vals + i) = (double)pbuf.index; /* the index number is used as the polygon's measure value */
//                 }
//
//                 /* create the shape, write it out, then release it */
//                 shp = SHPCreateObject(SHPT_POLYGONM, shpid, nParts, pstart, NULL, pbuf.len, lons, lats, NULL, vals);
//                 i = SHPWriteObject(sfid, -1, shp);
//                 SHPDestroyObject(shp);
//                 if (i != shpid)
//                 {
//                     _drawingContext.Logger?.LogInformation($"Error in s2shpwrt: SHPWriteObject returned %d, shpid=%d\n", i, shpid);
//                     rc = -1;
//                     goto cleanup;
//                 }
//
//                 gree(lons, "c10");
//                 lons = NULL;
//                 gree(lats, "c11");
//                 lats = NULL;
//                 gree(vals, "c12");
//                 vals = NULL;
//                 /* write out the attribute fields for this shape */
//                 fld = dbanch; /* point to the first one */
//                 while (fld != NULL)
//                 {
//                     if (fld.flag == 0)
//                     {
//                         /* static fields */
//                         if (fld.type == FTString)
//                         {
//                             DBFWriteStringAttribute(dbfid, shpid, fld.index, (const char* )fld.value);
//                         }
//                         else if (fld.type == FTInteger)
//                         {
//                             intprs(fld.value, &ival);
//                             DBFWriteIntegerAttribute(dbfid, shpid, fld.index, ival);
//                         }
//                         else if (fld.type == FTDouble)
//                         {
//                             getdbl(fld.value, &dval);
//                             DBFWriteDoubleAttribute(dbfid, shpid, fld.index, dval);
//                         }
//                     }
//                     else
//                     {
//                         /* dynamic fields */
//                         if (strcmp(fld.name, "INDEX") == 0)
//                         {
//                             val = pbuf.index;
//                             DBFWriteDoubleAttribute(dbfid, shpid, fld.index, val);
//                         }
//                         else if (strcmp(fld.name, "MIN_VALUE") == 0)
//                         {
//                             val = pbuf.clev1;
//                             DBFWriteDoubleAttribute(dbfid, shpid, fld.index, val);
//                         }
//                         else if (strcmp(fld.name, "MAX_VALUE") == 0)
//                         {
//                             val = pbuf.clev2;
//                             DBFWriteDoubleAttribute(dbfid, shpid, fld.index, val);
//                         }
//                     }
//
//                     fld = fld.next; /* advance to next field */
//                 }
//
//                 shpid++;
//             }
//
//             pbuf = pbuf.fpbuf;
//         }
//
//         /* if no errors, return the number of polygons written to the file */
//         rc = shpid;
//
//         cleanup:
//         if (lons) gree(lons, "c7");
//         if (lats) gree(lats, "c8");
//         if (vals) gree(vals, "c8");
//         if (pstart) gree(pstart, "c9");
//
//         return (rc);
//     }
// #endif

/* Routine to write out polygon vertices to a KML file. 
   For each polygon in the buffer: 
     get the vertex x/y coordinates, 
     convert them to lon/lat, 
     write out the coordinates to the kmlfile,
     release storage and return. 
   Returns -1 on error, otherwise the number of polygons written. 
*/
    int s2polyvert(string kmlfp)
    {
        // struct s2pbuf* pbuf = NULL;
        // double lon, lat, x, y;
        // int i, j, c, err;
        //
        // err = 0;
        // c = 0;
        // pbuf = s2pbufanch;
        // if (pbuf == NULL)
        // {
        //     _drawingContext.Logger?.LogInformation($"Error in s2polyvert: polygon buffer is empty\n");
        //     err = 1;
        //     goto cleanup;
        // }
        //
        // while (pbuf)
        // {
        //     if (pbuf.xy)
        //     {
        //         /* write out headers for each polygon */
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "    <Placemark>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 1;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "      <styleUrl>#%d</styleUrl>\n", pbuf.color);
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 2;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "      <name>%g to %g</name>\n", pbuf.clev1, pbuf.clev2);
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 3;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "      <Polygon>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 4;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "        <altitudeMode>clampToGround</altitudeMode>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 5;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "        <tessellate>1</tessellate>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 6;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "        <outerBoundaryIs>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 7;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "          <LinearRing>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 8;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "            <coordinates>\n              ");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 9;
        //             goto cleanup;
        //         }
        //
        //         /* get x,y values and convert them to lon,lat */
        //         j = 1;
        //         for (i = 0; i < pbuf.len; i++)
        //         {
        //             x = *(pbuf.xy + (2 * i));
        //             y = *(pbuf.xy + (2 * i + 1));
        //             gxxy2w(x, y, &lon, &lat);
        //             if (lat > 90) lat = 90;
        //             if (lat < -90) lat = -90;
        //
        //             sn_drawingContext.Logger?.LogInformation($pout, 511, "%g,%g,0 ", lon, lat);
        //             if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //             {
        //                 err = 10;
        //                 goto cleanup;
        //             }
        //
        //             if (j == 6 || i == (pbuf.len - 1))
        //             {
        //                 if (j == 6) sn_drawingContext.Logger?.LogInformation($pout, 511, "\n              ");
        //                 else sn_drawingContext.Logger?.LogInformation($pout, 511, "\n");
        //                 if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //                 {
        //                     err = 11;
        //                     goto cleanup;
        //                 }
        //
        //                 j = 0;
        //             }
        //
        //             j++;
        //         }
        //
        //         /* write out footers for each polygon */
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "            </coordinates>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 12;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "          </LinearRing>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 13;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "        </outerBoundaryIs>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 14;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "      </Polygon>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 15;
        //             goto cleanup;
        //         }
        //
        //         sn_drawingContext.Logger?.LogInformation($pout, 511, "    </Placemark>\n");
        //         if ((fwrite(pout, sizeof(char), strlen(pout), kmlfp)) != strlen(pout))
        //         {
        //             err = 16;
        //             goto cleanup;
        //         }
        //
        //         c++;
        //     }
        //
        //     pbuf = pbuf.fpbuf;
        // }
        //
        // cleanup:
        // if (err) return (-1);
        // else
        return (0);
    }
}