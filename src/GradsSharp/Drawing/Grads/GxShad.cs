using Microsoft.Extensions.Logging;

namespace GradsSharp.Drawing.Grads;

internal class GxShad
{
    private DrawingContext _drawingContext;

    private const int XYBMAX = 5000;

    private int[] flgh, flgv; /* Pointer to flags arrays                 */
    private List<double[]> xystk = new List<double[]>(XYBMAX); /* Pointers to xy stack buffers            */
    private int stkcnt; /* Current number of stacked buffers       */
    private double[] xypnt; /* Pointer into a stack buffer             */
    private double[] xybuf; /* Pointer to xy coord buffer              */
    private int xycnt; /* Current count in xy coord buffer        */
    private int imax, jmax; /* grid size                               */
    private int imn, imx, jmn, jmx; /* Current grid bounds                     */
    private double[] gr; /* Pointer to grid                         */
    private byte[] gru; /* Pointer to grid undef mask              */
    private int grsize; /* Number of elements in grid              */
    private int color; /* Current color to use for shading        */
    private int prvclr; /* Color of one level lower                */
    private double val; /* Current shading level value             */
    private int bndflg; /* Current coutour hit a boundry           */

    public GxShad(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }


    public void gxshad(double[] r, int iis, int jjs, double[] vs, int[] clrs, int lvs, byte[] u)
    {
        double x, y, rmin, rmax;
        int i, j, k, rc;
        
        /* Make some stuff global within this file     */
        imax = iis;
        jmax = jjs;
        gr = r;
        gru = u;

        /* Initialize xy coord buffer and stack buffer setup.             */
        stkcnt = 0;
        for (i = 0; i < XYBMAX; i++) xystk[i] = null;

        grsize = iis * jjs;
        xybuf = new double[grsize * 2];

        xycnt = 0;

        /* Alocate flag work area.                                         */
        flgh = new int[grsize];
        flgv = new int[grsize];

        /* Loop through bands of the grid.  Each band is three grid boxes
           wide, which avoids any problems with imbedded max's within min's
           or with imbedded complex undefined regions.                     */
        imn = 1;
        imx = imax;
        for (jmn = 1; jmn < jmax; jmn += 3)
        {
            jmx = jmn + 3;
            if (jmx > jmax) jmx = jmax;

            rmin = 9.99e33;
            rmax = -9.99e33;
            int pcnt = 0;
            int pucnt = 0;
            for (j = jmn; j <= jmx; j++)
            {
                pcnt = (j - 1) * imax + imn - 1;
                pucnt = (j - 1) * imax + imn - 1;
                for (i = imn; i <= imx; i++)
                {
                    if (gru[pucnt] != 0)
                    {
                        if (gr[pcnt] > rmax) rmax = gr[pcnt];
                        if (gr[pcnt] < rmin) rmin = gr[pcnt];
                    }

                    pcnt++;
                    pucnt++;
                }
            }

            /* Loop through shade levels.  */
            prvclr = 0;
            for (k = 0; k < lvs; k++)
            {
                val = vs[k];
                color = clrs[k];
                if (k < lvs - 1 && vs[k + 1] < rmin)
                {
                    prvclr = color;
                    continue;
                }

                if (val > rmax) continue;

                /* Set up flags to indicate which grid boxes contain missing data values
                   and where the grid boundries are.  Flag values are:
                       0 - nothing yet
                       1 - contour has been drawn through this side
                       7 - contour drawn through missing data box side
                       8 - boundry between missing data value box and non-missing data value box
                       9 - missing data value box side                                             */

                int f1cnt = (jmn - 1) * imax;
                int f4cnt = (jmn - 1) * imax;
                for (j = jmn; j <= jmx; j++)
                {
                    for (i = 1; i <= imax; i++)
                    {
                        flgh[f1cnt] = 0;
                        flgv[f4cnt] = 0;
                        f1cnt++;
                        f4cnt++;
                    }
                }

                for (j = jmn; j < jmx; j++)
                {
                    int p1cnt = (j - 1) * imax + imn - 1;
                    int p2cnt = p1cnt + 1;
                    int p3cnt = p2cnt + imax;
                    int p4cnt = p1cnt + imax;

                    f1cnt = (j - 1) * imax + imn - 1;
                    int f2cnt = (j - 1) * imax + imn;
                    int f3cnt = f1cnt + imax;
                    f4cnt = f2cnt - 1;

                    for (i = imn; i < imx; i++)
                    {
                        if (gru[p1cnt] == 0 || gru[p2cnt] == 0 || gru[p3cnt] == 0 || gru[p4cnt] == 0)
                        {
                            flgh[f1cnt] = 9;
                            flgv[f2cnt] = 9;
                            flgh[f3cnt] = 9;
                            flgv[f4cnt] = 9;
                        }

                        p1cnt++;
                        p2cnt++;
                        p3cnt++;
                        p4cnt++;
                        f1cnt++;
                        f2cnt++;
                        f3cnt++;
                        f4cnt++;
                    }
                }

                for (j = jmn; j < jmx; j++)
                {
                    int p1cnt = (j - 1) * imax + imn - 1;
                    int p2cnt = p1cnt + 1;
                    int p3cnt = p2cnt + imax;
                    int p4cnt = p1cnt + imax;

                    f1cnt = (j - 1) * imax + imn - 1;
                    int f2cnt = (j - 1) * imax + imn;
                    int f3cnt = f1cnt + imax;
                    f4cnt = f2cnt - 1;

                    for (i = imn; i < imx; i++)
                    {
                        if (gru[p1cnt] != 0 || gru[p2cnt] != 0 || gru[p3cnt] != 0 || gru[p4cnt] != 0)
                        {
                            if (flgh[f1cnt] == 9) flgh[f1cnt] = 8;
                            if (flgv[f2cnt] == 9) flgv[f2cnt] = 8;
                            if (flgh[f3cnt] == 9) flgh[f3cnt] = 8;
                            if (flgv[f4cnt] == 9) flgv[f4cnt] = 8;
                        }

                        p1cnt++;
                        p2cnt++;
                        p3cnt++;
                        p4cnt++;
                        f1cnt++;
                        f2cnt++;
                        f3cnt++;
                        f4cnt++;
                    }
                }

                /* Loop through grid, finding starting locations for a contour
                   line.  Once found, call gxsflw to follow the contour until
                   it is closed.  The contour is closed by following the grid
                   boundry (and missing-data-value boundries) if necessary.      */

                for (j = jmn; j <= jmx; j++)
                {
                    int p1cnt = (j - 1) * imax + imn - 1;
                    int p2cnt = p1cnt + 1;
                    int p4cnt = p1cnt + imax;

                    f1cnt = (j - 1) * imax + imn - 1;
                    f4cnt = (j - 1) * imax + imn - 1;

                    for (i = imn; i <= imx; i++)
                    {
                        if (i < imx && (flgh[f1cnt] == 0 || flgh[f1cnt] == 8) &&
                            ((r[p1cnt] <= val && r[p2cnt] > val) || (r[p1cnt] > val && r[p2cnt] <= val)))
                        {
                            if (j == jmx) rc = gxsflw(i, j - 1, 3);
                            else rc = gxsflw(i, j, 1);
                            if (rc > 0) goto err;
                        }

                        if (j < jmx && (flgv[f4cnt] == 0 || flgv[f4cnt] == 8) &&
                            ((r[p1cnt] <= val && r[p4cnt] > val) || (r[p1cnt] > val && r[p4cnt] <= val)))
                        {
                            if (i == imx) rc = gxsflw(i - 1, j, 2);
                            else rc = gxsflw(i, j, 4);
                            if (rc > 0) goto err;
                        }

                        p1cnt++;
                        p2cnt++;
                        p4cnt++;
                        f1cnt++;
                        f4cnt++;
                    }
                }


                /* Check for any unfilled regions by looking for any unfollowed
                   boundry or missing-data-value sides that have point values
                   that are both greater than the current shade value.  This
                   indicates a possible closed region (closed by missing data
                   value boundries) that we have not yet picked up.  We will
                   bound that region and fill it.                               */

                for (j = jmn; j <= jmx; j++)
                {
                    int p1cnt = (j - 1) * imax + imn - 1;
                    int p2cnt = p1cnt + 1;
                    int p4cnt = p1cnt + imax;

                    f1cnt = (j - 1) * imax + imn - 1;
                    f4cnt = (j - 1) * imax + imn - 1;

                    for (i = imn; i <= imx; i++)
                    {
                        rc = 0;
                        if (i < imx)
                        {
                            if (j == jmn && flgh[f1cnt] == 0 && r[p1cnt] > val && r[p2cnt] > val) rc = gxsflw(i, j, 5);
                            if (j == jmx && flgh[f1cnt] == 0 && r[p1cnt] > val && r[p2cnt] > val) rc = gxsflw(i, j, 6);
                            if (flgh[f1cnt] == 8 && r[p1cnt] > val && r[p2cnt] > val) rc = gxsflw(i + 1, j, 9);
                        }

                        if (j < jmx)
                        {
                            if (i == imn && flgv[f4cnt] == 0 && r[p1cnt] > val && r[p4cnt] > val) rc = gxsflw(i, j, 7);
                            if (i == imx && flgv[f4cnt] == 0 && r[p1cnt] > val && r[p4cnt] > val) rc = gxsflw(i, j, 8);
                            if (flgv[f4cnt] == 8 && r[p1cnt] > val && r[p4cnt] > val) rc = gxsflw(i, j + 1, 10);
                        }

                        if (rc > 0) goto err;
                        f1cnt++;
                        f4cnt++;
                        p1cnt++;
                        p2cnt++;
                        p4cnt++;
                    }
                }

                prvclr = color;
            }

            /* All closed maximas have been filled, and all closed minimas
               have been stacked.  Fill minimas in reverse order.           */

            /* Note: to insure the various bands 'fit' together properly,
               the boundry points are adjusted outward slightly.  This due to
               the Xserver not filling out to the boundry in poly fills.    */

            for (i = stkcnt - 1; i >= 0; i--)
            {
                xypnt = xystk[i];
                xycnt = (int)(xypnt[0]);
                color = (int)(xypnt[1]);
                int xypntc = 2;
                for (j = 0; j < xycnt; j++)
                {
                    _drawingContext.GradsDrawingInterface.gxconv(xypnt[2 + (j * 2)], xypnt[2+ (j * 2 + 1)], out x, out y, 3);
                    xypnt[2 + (j * 2)] = x;
                    xypnt[2 + (j * 2 + 1)] = y;
                }

                _drawingContext.GradsDrawingInterface.SetDrawingColor(color);
                _drawingContext.GradsDrawingInterface.gxfill(xypnt, xycnt);
                
            }

            stkcnt = 0;
            xycnt = 0;
        }


        return;

        err:
        throw new Exception("Error in gxshad");
    }

    int gxsflw(int i, int j, int iside)
    {
/* Follow a shaded outline to the end.  Close it if necessary by 
   following around undef areas and around the grid border.         */

/* The grid box:

              (f3)
     p4      side 3     p3
        x ------------ x
        |              |
 side 4 |              | side 2
  (f4)  |              |  (f2)
        |              |
        x ------------ x
      p1    side 1      p2
             (f1)
                                                                     */

        int[] f1,  f2,  f3, f4, ff, fu, fd, fl, fr;
        int cnt, rc, isave, jsave, ucflg, k;
        bool bflag, uflag, ucflag;
        double[] p1,  p2, p3, p4;
        double x, y;

        isave = i;
        jsave = j;
        uflag = false;
        bndflg = 0;

        bflag = false;
        if (iside == 1) goto side1; /* Jump in based on side    */
        if (iside == 2) goto side2;
        if (iside == 3) goto side3;
        if (iside == 4) goto side4;
        bflag = true;
        if (iside == 5) goto br;
        if (iside == 6) goto tr;
        if (iside == 7) goto lu;
        if (iside == 8) goto ru;
        if (iside == 9) goto ur;
        if (iside == 10) goto uu;
        _drawingContext.Logger?.LogInformation("Logic error 40 in gxshad");
        return (1);

        /* Calculate entry point in the current grid box, then move to the
           next grid box based on the exit side.                           */

        side1: /* Enter side 1             */

        if (i < imn || i > (imx - 1) || j < jmn || j > jmx)
        {
            _drawingContext.Logger?.LogInformation("logic error 12 in gxshad");
            _drawingContext.Logger?.LogInformation($"  side1, {i} {j} ");
            return (1);
        }

        int p1cnt = 0, p2cnt = 0, p3cnt = 0, p4cnt;
        int f1cnt = 0, f2cnt = 0, f3cnt = 0, f4cnt;
        int fucnt = 0, ffcnt = 0, fdcnt = 0, flcnt = 0, frcnt = 0;
        
        p1cnt = imax * (j - 1) + i - 1;
        p2cnt = p1cnt + 1;
        x = (double)i + (val - gr[p1cnt]) / (gr[p2cnt] - gr[p1cnt]); /* Calculate entry point    */
        y = (double)j;
        rc = putxy(x, y); /* Put points in buffer     */
        if (rc > 0) return (rc);
        f1cnt = (imax * (j - 1) + i - 1);
        if (flgh[f1cnt] == 1 || flgh[f1cnt] == 7) goto done; /* We may be done           */
        if (flgh[f1cnt] > 5 && !uflag)
        {
            /* Entered an undef box?    */
            if (flgh[f1cnt] == 9)
            {
                _drawingContext.Logger?.LogInformation($"Logic error 4 in gxshad: {i} {j}");
                return (1);
            }

            flgh[f1cnt] = 7; /* Indicate we were here    */
            if (gr[p1cnt] > val)
            {
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto uleft;
            }
            else
            {
                i++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto uright;
            }
        }

        if (flgh[f1cnt] == 8) flgh[f1cnt] = 7; /* Indicate we were here    */
        else flgh[f1cnt] = 1;
        uflag = false;
        if (j + 1 > jmx)
        {
            /* At top boundry?          */
            if (gr[p1cnt] > val)
            {
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto tleft;
            }
            else
            {
                i++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto tright;
            }
        }

        /* Check for exit side.  Also check for col.                    */

        p3cnt = p2cnt + imax;
        p4cnt = p3cnt - 1;
        if ((gr[p2cnt] <= val && gr[p3cnt] > val) || (gr[p2cnt] > val && gr[p3cnt] <= val))
        {
            if ((gr[p3cnt] <= val && gr[p4cnt] > val) || (gr[p3cnt] > val && gr[p4cnt] <= val))
            {
                if (spathl(gr[p1cnt], gr[p2cnt], gr[p3cnt], gr[p4cnt])==0)
                {
                    i--;
                    goto side2; /* Exiting 4, go enter 2  */
                }
            }

            i++;
            goto side4; /* Exiting 2, go enter 4  */
        }

        if ((gr[p3cnt] <= val && gr[p4cnt] > val) || (gr[p3cnt] > val && gr[p4cnt] <= val))
        {
            j++;
            goto side1; /* Exiting 3, go enter 1  */
        }

        if ((gr[p4cnt] <= val && gr[p1cnt] > val) || (gr[p4cnt] > val && gr[p1cnt] <= val))
        {
            i--;
            goto side2; /* Exiting 4, go enter 2  */
        }

        _drawingContext.Logger?.LogInformation($"Logic error 8 in gxshad\n");
        return (1);

        side2: /* Enter side 2           */

        if (i < (imn - 1) || i > (imx - 1) || j < jmn || j > (jmx - 1))
        {
            _drawingContext.Logger?.LogInformation($"logic error 12 in gxshad\n");
            _drawingContext.Logger?.LogInformation($"  side2, {i} {j}");
            return (1);
        }

        p2cnt = imax * (j - 1) + i;
        p3cnt = p2cnt + imax;
        x = (double)(i + 1);
        y = (double)j + (val - gr[p2cnt]) / (gr[p3cnt] - gr[p2cnt]); /* Calculate entry point    */
        rc = putxy(x, y); /* Put points in buffer     */
        if (rc > 0) return (rc);
        f2cnt = imax * (j - 1) + i;
        if (flgv[f2cnt] == 1 || flgv[f2cnt] == 7) goto done; /* We may be done           */
        if (flgv[f2cnt] > 5 && !uflag)
        {
            /* Entered an undef box?    */
            if (flgv[f2cnt] == 9)
            {
                _drawingContext.Logger?.LogInformation($"Logic error 4 in gxshad: {i} {j}");
                _drawingContext.Logger?.LogInformation($"Side 2, entered {iside}");
                return (1);
            }

            flgv[f2cnt] = 7; /* Indicate we were here    */
            if (gr[p2cnt] > val)
            {
                i++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto udown;
            }
            else
            {
                i++;
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto uup;
            }
        }

        if (flgv[f2cnt] == 8) flgv[f2cnt] = 7; /* Indicate we were here    */
        else flgv[f2cnt] = 1;
        uflag = false;
        if (i < imn)
        {
            /* At left boundry?         */
            if (gr[p2cnt] > val)
            {
                i++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto ldown;
            }
            else
            {
                i++;
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto lup;
            }
        }

        /* Check for exit side.  Also check for col.                    */

        p1cnt = p2cnt - 1;
        p4cnt = p3cnt - 1;
        if ((gr[p3cnt] <= val && gr[p4cnt] > val) || (gr[p3cnt] > val && gr[p4cnt] <= val))
        {
            if ((gr[p4cnt] <= val && gr[p1cnt] > val) || (gr[p4cnt] > val && gr[p1cnt] <= val))
            {
                if (spathl(gr[p1cnt], gr[p2cnt], gr[p3cnt], gr[p4cnt])>0)
                {
                    j--;
                    goto side3; /* Exiting 1, go enter 3  */
                }
            }

            j++;
            goto side1; /* Exiting 3, go enter 1  */
        }

        if ((gr[p4cnt] <= val && gr[p1cnt] > val) || (gr[p4cnt] > val && gr[p1cnt] <= val))
        {
            i--;
            goto side2; /* Exiting 4, go enter 2  */
        }

        if ((gr[p1cnt] <= val && gr[p2cnt] > val) || (gr[p1cnt] > val && gr[p2cnt] <= val))
        {
            j--;
            goto side3; /* Exiting 1, go enter 3  */
        }

        _drawingContext.Logger?.LogInformation($"Logic error 8 in gxshad");
        return (1);

        side3: /* Enter side 3             */

        if (i < imn || i > (imx - 1) || j < (jmn - 1) || j > (jmx - 1))
        {
            _drawingContext.Logger?.LogInformation($"logic error 12 in gxshad");
            _drawingContext.Logger?.LogInformation($"  side3, {i} {j}");
            return (1);
        }

        p3cnt = (imax * (j) + i);
        p4cnt = p3cnt - 1;
        x = (double)i + (val - gr[p4cnt]) / (gr[p3cnt] - gr[p4cnt]); /* Calculate entry point    */
        y = (double)(j + 1);
        rc = putxy(x, y); /* Put points in buffer     */
        if (rc > 0) return (rc);
        f3cnt = (imax * (j) + i - 1);
        if (flgh[f3cnt] == 1 || flgh[f3cnt] == 7) goto done; /* We may be done           */
        if (flgh[f3cnt] > 5 && !uflag)
        {
            /* Entered an undef box?    */
            if (flgh[f3cnt] == 9)
            {
                _drawingContext.Logger?.LogInformation($"Logic error 4 in gxshad: {i} {j}");
                _drawingContext.Logger?.LogInformation($"Side 3, entered {iside}");
                return (1);
            }

            flgh[f3cnt] = 7; /* Indicate we were here    */
            if (gr[p3cnt] > val)
            {
                i++;
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto uright;
            }
            else
            {
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto uleft;
            }
        }

        if (flgh[f3cnt] == 8) flgh[f3cnt] = 7; /* Indicate we were here    */
        else flgh[f3cnt] = 1;
        uflag = false;
        if (j < jmn)
        {
            /* At bottom boundry?       */
            if (gr[p3cnt] > val)
            {
                i++;
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto bright;
            }
            else
            {
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto bleft;
            }
        }

        /* Check for exit side.  Also check for col.                    */

        p1cnt = p4cnt - imax;
        p2cnt = p1cnt + 1;
        if ((gr[p1cnt] <= val && gr[p4cnt] > val) || (gr[p1cnt] > val && gr[p4cnt] <= val))
        {
            if ((gr[p1cnt] <= val && gr[p2cnt] > val) || (gr[p1cnt] > val && gr[p2cnt] <= val))
            {
                if (spathl(gr[p1cnt], gr[p2cnt], gr[p3cnt], gr[p4cnt])==0)
                {
                    i++;
                    goto side4; /* Exiting 2, go enter 4  */
                }
            }

            i--;
            goto side2; /* Exiting 4, go enter 2  */
        }

        if ((gr[p1cnt] <= val && gr[p2cnt] > val) || (gr[p1cnt] > val && gr[p2cnt] <= val))
        {
            j--;
            goto side3; /* Exiting 1, go enter 3  */
        }

        if ((gr[p2cnt] <= val && gr[p3cnt] > val) || (gr[p2cnt] > val && gr[p3cnt] <= val))
        {
            i++;
            goto side4; /* Exiting 2, go enter 4  */
        }

        _drawingContext.Logger?.LogInformation($"Logic error 8 in gxshad");
        return (1);

        side4: /* Enter side 4           */

        if (i < 1 || i > imax || j < 1 || j > (jmax - 1))
        {
            _drawingContext.Logger?.LogInformation($"logic error 12 in gxshad");
            _drawingContext.Logger?.LogInformation($"  side4, {i} {j}");
            _drawingContext.Logger?.LogInformation($" imax, jmax = {imax} {jmax}");
            return (1);
        }

        p1cnt = imax * (j - 1) + i - 1;
        p4cnt = p1cnt + imax;
        x = (double)i;
        y = (double)j + (val - gr[p1cnt]) / (gr[p4cnt] - gr[p1cnt]); /* Calculate entry point    */
        rc = putxy(x, y); /* Put points in buffer     */
        if (rc > 0) return (rc);
        f4cnt = ((j - 1) * imax + i - 1);
        if (flgv[f4cnt] == 1 || flgv[f4cnt] == 7) goto done; /* We may be done           */
        if (flgv[f4cnt] > 5 && !uflag)
        {
            /* Entered an undef box?    */
            if (flgv[f4cnt] == 9)
            {
                _drawingContext.Logger?.LogInformation($"Logic error 4 in gxshad: {i} {j}");
                _drawingContext.Logger?.LogInformation($"Side 4, entered {iside}");
                return (1);
            }

            flgv[f4cnt] = 7; /* Indicate we were here    */
            if (gr[p1cnt] > val)
            {
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto udown;
            }
            else
            {
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto uup;
            }
        }

        if (flgv[f4cnt] == 8) flgv[f4cnt] = 7; /* Indicate we were here    */
        else flgv[f4cnt] = 1;
        uflag = false;
        if (i + 1 > imx)
        {
            /* At right boundry?        */
            if (gr[p1cnt] > val)
            {
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto rdown;
            }
            else
            {
                j++;
                rc = putxy((double)i, (double)j);
                if (rc > 0) return (rc);
                goto rup;
            }
        }

        /* Check for exit side.  Also check for col.                    */

        p2cnt = p1cnt + 1;
        p3cnt = p4cnt + 1;
        if ((gr[p1cnt] <= val && gr[p2cnt] > val) || (gr[p1cnt] > val && gr[p2cnt] <= val))
        {
            if ((gr[p2cnt] <= val && gr[p3cnt] > val) || (gr[p2cnt] > val && gr[p3cnt] <= val))
            {
                if (spathl(gr[p1cnt], gr[p2cnt], gr[p3cnt], gr[p4cnt])>0)
                {
                    j++;
                    goto side1; /* Exiting 3, go enter 1  */
                }
            }

            j--;
            goto side3; /* Exiting 1, go enter 3  */
        }

        if ((gr[p2cnt] <= val && gr[p3cnt] > val) || (gr[p2cnt] > val && gr[p3cnt] <= val))
        {
            i++;
            goto side4; /* Exiting 2, go enter 4  */
        }

        if ((gr[p3cnt] <= val && gr[p4cnt] > val) || (gr[p3cnt] > val && gr[p4cnt] <= val))
        {
            j++;
            goto side1; /* Exiting 3, go enter 1  */
        }

        _drawingContext.Logger?.LogInformation($"Logic error 8 in gxshad");
        return (1);

        /* At an undefined boundry and last moved towards the left.  */

        uleft:

        bndflg = 1;
        if (bflag && i == isave && j == jsave) goto done;
        if (j < (jmn + 1) || j > jmx - 1)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 16 in gxshad");
            return (1);
        }

        fucnt = ((j - 1) * imax + i - 1);
        fdcnt = fucnt - imax;
        if (i == imn)
        {
            if ((flgv[fucnt] > 5 && flgv[fdcnt] > 5) || (flgv[fucnt] < 5 && flgv[fdcnt] < 5))
            {
                _drawingContext.Logger?.LogInformation($"Logic error 20 in gxshad");
                return (1);
            }

            if (flgv[fucnt] > 5) goto ldown;
            else goto lup;
        }

        ffcnt = ((j - 1) * imax + i - 2);
        cnt = 0;
        if (flgh[ffcnt] == 7 || flgh[ffcnt] == 8) cnt++;
        if (flgv[fucnt] == 7 || flgv[fucnt] == 8) cnt++;
        if (flgv[fdcnt] == 7 || flgv[fdcnt] == 8) cnt++;
        if (cnt == 2 || cnt == 0)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 24 in gxshad");
            return (1);
        }

        ucflg = 0;
        if (cnt == 3) ucflg = undcol(i, j);
        if (ucflg == 9) return (1);
        if (ucflg == 0 && (flgh[ffcnt] == 7 || flgh[ffcnt] == 8))
        {
            i--;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (flgv[fucnt] > 5)
                {
                    j--;
                    goto side3;
                }
                else goto side1;
            }

            flgh[ffcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uleft;
        }

        if (ucflg != 2 && (flgv[fdcnt] == 7 || flgv[fdcnt] == 8))
        {
            j--;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg>0 || flgv[fucnt] == 9)
                {
                    goto side4;
                }
                else
                {
                    i--;
                    goto side2;
                }
            }

            flgv[fdcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto udown;
        }

        if (ucflg != 1 && (flgv[fucnt] == 7 || flgv[fucnt] == 8))
        {
            j++;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg > 0 || flgv[fdcnt] == 9)
                {
                    j--;
                    goto side4;
                }
                else
                {
                    i--;
                    j--;
                    goto side2;
                }
            }

            flgv[fucnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uup;
        }

        _drawingContext.Logger?.LogInformation($"Logic error 28 in gxshad");
        return (1);

        /* At an undefined boundry and last moved towards the right. */

        uright:

        if (bflag && i == isave && j == jsave) goto done;

        ur:

        bndflg = 1;
        if (j < (jmn + 1) || j > jmx - 1)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 16 in gxshad");
            return (1);
        }

        fucnt = ((j - 1) * imax + i - 1);
        fdcnt = fucnt - imax;
        if (i == imx)
        {
            if ((flgv[fucnt] > 5 && flgv[fdcnt] > 5) || (flgv[fucnt] < 5 && flgv[fdcnt] < 5))
            {
                _drawingContext.Logger?.LogInformation($"Logic error 20 in gxshad");
                return (1);
            }

            if (flgv[fucnt] > 5) goto rdown;
            else goto rup;
        }

        ffcnt = ((j - 1) * imax + i - 1);
        cnt = 0;
        if (flgh[ffcnt] == 7 || flgh[ffcnt] == 8) cnt++;
        if (flgv[fdcnt] == 7 || flgv[fdcnt] == 8) cnt++;
        if (flgv[fucnt] == 7 || flgv[fucnt] == 8) cnt++;
        if (cnt == 2 || cnt == 0)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 24 in gxshad");
            return (1);
        }

        ucflg = 0;
        if (cnt == 3) ucflg = undcol(i, j);
        if (ucflg == 9) return (1);
        if (ucflg == 0 && (flgh[ffcnt] == 7 || flgh[ffcnt] == 8))
        {
            i++;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                i--;
                if (flgv[fucnt] > 5)
                {
                    j--;
                    goto side3;
                }
                else goto side1;
            }

            flgh[ffcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uright;
        }

        if (ucflg != 1 && (flgv[fdcnt] == 7 || flgv[fdcnt] == 8))
        {
            j--;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg > 0 || flgv[fucnt] == 9)
                {
                    i--;
                    goto side2;
                }
                else
                {
                    goto side4;
                }
            }

            flgv[fdcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto udown;
        }

        if (ucflg != 2 && (flgv[fucnt] == 7 || flgv[fucnt] == 8))
        {
            j++;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg>0 || flgv[fdcnt] == 9)
                {
                    i--;
                    j--;
                    goto side2;
                }
                else
                {
                    j--;
                    goto side4;
                }
            }

            flgv[fucnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uup;
        }

        _drawingContext.Logger?.LogInformation($"Logic error 28 in gxshad");
        return (1);

        /* At an undefined boundry and last moved towards the top.   */

        uup:

        if (bflag && i == isave && j == jsave) goto done;

        uu:

        bndflg = 1;
        if (i < (imn + 1) || i > imx - 1)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 16 in gxshad");
            return (1);
        }

        frcnt = ((j - 1) * imax + i - 1);
        flcnt = frcnt - 1;
        if (j == jmx)
        {
            if ((flgh[frcnt] > 5 && flgh[flcnt] > 5) || (flgh[frcnt] < 5 && flgh[flcnt] < 5))
            {
                _drawingContext.Logger?.LogInformation($"Logic error 20 in gxshad");
                return (1);
            }

            if (flgh[frcnt] > 5) goto tleft;
            else goto tright;
        }

        ffcnt = ((j - 1) * imax + i - 1);
        cnt = 0;
        if (flgv[ffcnt] == 7 || flgv[ffcnt] == 8) cnt++;
        if (flgh[frcnt] == 7 || flgh[frcnt] == 8) cnt++;
        if (flgh[flcnt] == 7 || flgh[flcnt] == 8) cnt++;
        if (cnt == 2 || cnt == 0)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 24 in gxshad");
            return (1);
        }

        ucflg = 0;
        if (cnt == 3) ucflg = undcol(i, j);
        if (ucflg == 9) return (1);
        if (ucflg == 0 && (flgv[ffcnt] == 7 || flgv[ffcnt] == 8))
        {
            j++;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                j--;
                uflag = true;
                if (flgh[frcnt] > 5)
                {
                    i--;
                    goto side2;
                }
                else goto side4;
            }

            flgv[ffcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uup;
        }

        if (ucflg != 2 && (flgh[frcnt] == 7 || flgh[frcnt] == 8))
        {
            i++;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg > 0 || flgh[flcnt] == 9)
                {
                    i--;
                    j--;
                    goto side3;
                }
                else
                {
                    i--;
                    goto side1;
                }
            }

            flgh[frcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uright;
        }

        if (ucflg != 1 && (flgh[flcnt] == 7 || flgh[flcnt] == 8))
        {
            i--;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg > 0 || flgh[frcnt] == 9)
                {
                    j--;
                    goto side3;
                }
                else
                {
                    goto side1;
                }
            }

            flgh[flcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uleft;
        }

        _drawingContext.Logger?.LogInformation($"Logic error 28 in gxshad");
        return (1);

        /* At an undefined boundry and last moved towards the bottom.  */

        udown:

        bndflg = 1;
        if (bflag && i == isave && j == jsave) goto done;
        if (i < (imn + 1) || i > imx - 1)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 16 in gxshad");
            return (1);
        }

        frcnt = ((j - 1) * imax + i - 1);
        flcnt = frcnt - 1;
        if (j == jmn)
        {
            if ((flgh[frcnt] > 5 && flgh[flcnt] > 5) || (flgh[frcnt] < 5 && flgh[flcnt] < 5))
            {
                _drawingContext.Logger?.LogInformation($"Logic error 20 in gxshad");
                return (1);
            }

            if (flgh[frcnt] > 5) goto bleft;
            else goto bright;
        }

        ffcnt = ((j - 2) * imax + i - 1);
        cnt = 0;
        if (flgv[ffcnt] == 7 || flgv[ffcnt] == 8) cnt++;
        if (flgh[frcnt] == 7 || flgh[frcnt] == 8) cnt++;
        if (flgh[flcnt] == 7 || flgh[flcnt] == 8) cnt++;
        if (cnt == 2 || cnt == 0)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 24 in gxshad");
            return (1);
        }

        ucflg = 0;
        if (cnt == 3) ucflg = undcol(i, j);
        if (ucflg == 9) return (1);
        if (ucflg == 0 && (flgv[ffcnt] == 7 || flgv[ffcnt] == 8))
        {
            j--;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (flgh[frcnt] > 5)
                {
                    i--;
                    goto side2;
                }
                else goto side4;
            }

            flgv[ffcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto udown;
        }

        if (ucflg != 1 && (flgh[frcnt] == 7 || flgh[frcnt] == 8))
        {
            i++;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg > 0 || flgh[flcnt] == 9)
                {
                    i--;
                    goto side1;
                }
                else
                {
                    i--;
                    j--;
                    goto side3;
                }
            }

            flgh[frcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uright;
        }

        if (ucflg != 2 && (flgh[flcnt] == 7 || flgh[flcnt] == 8))
        {
            i--;
            p1cnt = ((j - 1) * imax + i - 1);
            if (gr[p1cnt] <= val)
            {
                uflag = true;
                if (ucflg>0 || flgh[frcnt] == 9)
                {
                    goto side1;
                }
                else
                {
                    j--;
                    goto side3;
                }
            }

            flgh[flcnt] = 7;
            rc = putxy((double)i, (double)j);
            if (rc > 0) return (rc);
            goto uleft;
        }

        _drawingContext.Logger?.LogInformation($"Logic error 28 in gxshad");
        return (1);

        /* Follow grid boundry until we hit a missing data area, or until
           we hit the restart of the contour line.                         */

        tright:

        if (bflag && i == isave && j == jsave) goto done;

        tr:

        bndflg = 1;
        if (i == imx) goto rdown;
        ffcnt = ((j - 1) * imax + i - 1);
        if (flgh[ffcnt] > 5) goto udown;
        i++;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            j--;
            i--;
            goto side3;
        }

        flgh[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto tright;

        tleft:

        bndflg = 1;
        if (bflag && i == isave && j == jsave) goto done;
        if (i == imn) goto ldown;
        ffcnt = ((j - 1) * imax + i - 2);
        if (flgh[ffcnt] > 5) goto udown;
        i--;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            j--;
            goto side3;
        }

        flgh[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto tleft;

        bright:

        if (bflag && i == isave && j == jsave) goto done;

        br:

        bndflg = 1;
        if (i == imx) goto rup;
        ffcnt = ((j - 1) * imax + i - 1);
        if (flgh[ffcnt] > 5) goto uup;
        i++;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            i--;
            goto side1;
        }

        flgh[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto bright;

        bleft:

        bndflg = 1;
        if (bflag && i == isave && j == jsave) goto done;
        if (i == imn) goto lup;
        ffcnt = ((j - 1) * imax + i - 2);
        if (flgh[ffcnt] > 5) goto uup;
        i--;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            goto side1;
        }

        flgh[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto bleft;

        rup:

        if (bflag && i == isave && j == jsave) goto done;

        ru:

        bndflg = 1;
        if (j == jmx) goto tleft;
        ffcnt = ((j - 1) * imax + i - 1);
        if (flgv[ffcnt] > 5) goto uleft;
        j++;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            j--;
            i--;
            goto side2;
        }

        flgv[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto rup;

        rdown:

        bndflg = 1;
        if (bflag && i == isave && j == jsave) goto done;
        if (j == jmn) goto bleft;
        ffcnt = ((j - 2) * imax + i - 1);
        if (flgv[ffcnt] > 5) goto uleft;
        j--;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            i--;
            goto side2;
        }

        flgv[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto rdown;

        lup:

        if (bflag && i == isave && j == jsave) goto done;

        lu:

        bndflg = 1;
        if (j == jmx) goto tright;
        ffcnt = ((j - 1) * imax + i - 1);
        if (flgv[ffcnt] > 5) goto uright;
        j++;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            j--;
            goto side4;
        }

        flgv[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto lup;

        ldown:

        bndflg = 1;
        if (bflag && i == isave && j == jsave) goto done;
        if (j == jmn) goto bright;
        ffcnt = ((j - 2) * imax + i - 1);
        if (flgv[ffcnt] > 5) goto uright;
        j--;
        p1cnt = ((j - 1) * imax + i - 1);
        if (gr[p1cnt] <= val)
        {
            goto side4;
        }

        flgv[ffcnt] = 1;
        rc = putxy((double)i, (double)j);
        if (rc > 0) return (rc);
        goto ldown;

        done:

        shdcmp();
        if (xycnt < 4) goto cont;
        if (shdmax()>0)
        {
            for (k = 0; k < xycnt; k++)
            {
                _drawingContext.GradsDrawingInterface.gxconv(xybuf[(k * 2)], xybuf[(k * 2 + 1)], out x, out y, 3);
                xybuf[(k * 2)] = x;
                xybuf[(k * 2 + 1)] = y;
            }

            _drawingContext.GradsDrawingInterface.SetDrawingColor(color);
            _drawingContext.GradsDrawingInterface.gxfill(xybuf, xycnt);
        }
        else
        {
            xystk[stkcnt] = new double[(xycnt + 1) * 2];
            
            xypnt = xystk[stkcnt];
            xypnt[0] = (double)(xycnt) + 0.1;
            xypnt[1] = (double)(prvclr) + 0.1;
            
            for (k = 0; k < xycnt; k++)
            {
                xypnt[2+(k * 2)] = xybuf[(k * 2)];
                xypnt[2 + (k * 2 + 1)] = xybuf[(k * 2 + 1)];
            }

            xystk[stkcnt] = xypnt;
            
            stkcnt++;
            if (stkcnt >= XYBMAX)
            {
                _drawingContext.Logger?.LogInformation($"Buffer stack limit exceeded in gxshad");
                return (1);
            }
        }

        cont:
        xycnt = 0;
        return (0);
    }

/* Calculate shortest combined path length through a col point.
   Return true if shortest path is side 1/2,3/4, else false.         */

    int spathl(double p1, double p2, double p3, double p4)
    {
        double v1, v2, v3, v4, d1, d2;

        v1 = (val - p1) / (p2 - p1);
        v2 = (val - p2) / (p3 - p2);
        v3 = (val - p4) / (p3 - p4);
        v4 = (val - p1) / (p4 - p1);
        d1 = GaUtil.hypot(1.0 - v1, v2) + GaUtil.hypot(1.0 - v4, v3);
        d2 = GaUtil.hypot(v1, v4) + GaUtil.hypot(1.0 - v2, 1.0 - v3);
        if (d2 < d1) return (0);
        return (1);
    }

/* Determine characteristics of an undefined path col */

    int undcol(int i, int j)
    {
        double[] p1, p2, p3, p4;
        byte[] p1u, p2u, p3u, p4u;

        if (i < 2 || i > imax - 1 || j < 2 || j > jmax - 1)
        {
            _drawingContext.Logger?.LogInformation($"Logic error 32 in gxshad\n");
            return (9);
        }

        int p1cnt = 0, p2cnt = 0, p3cnt = 0, p4cnt = 0;
        p1cnt = ((j - 2) * imax + i - 2);
        p2cnt = p1cnt + 2;
        p3cnt = p2cnt + imax * 2;
        p4cnt = p3cnt - 2;

        if (gru[p1cnt] == 0 && gru[p3cnt] == 0 && gru[p2cnt] != 0 && gru[p4cnt] != 0) return (1);
        if (gru[p1cnt] != 0 && gru[p3cnt] != 0 && gru[p2cnt] == 0 && gru[p4cnt] == 0) return (2);
        _drawingContext.Logger?.LogInformation($"Logic error 36 in gxshad\n");
        return (9);
    }


    int putxy(double x, double y)
    {
        if (xycnt >= grsize) return (1);
        xybuf[xycnt * 2] = x;
        xybuf[xycnt * 2 + 1] = y;
        xycnt++;
        return (0);
    }

/* Remove duplicate consecutive points from the closed contour   */

    void shdcmp()
    {
        int i, j;

        i = 0;
        for (j = 1; j < xycnt; j++)
        {
            if (xybuf[(i * 2)] != xybuf[(j * 2)] ||
                xybuf[(i * 2 + 1)] != xybuf[(j * 2 + 1)])
            {
                i++;
                if (i != j)
                {
                    xybuf[(i * 2)] = xybuf[(j * 2)];
                    xybuf[(i * 2 + 1)] = xybuf[j * 2 + 1];
                }
            }
        }

        xycnt = i + 1;
    }

/* Determine if the current closed contour (contained in xybuf) 
   is a max or a min.                                            */

    int shdmax()
    {
        double x, y, xsave, ysave = 0.0;
        double[] p1;
        int i, j;

        /* If we hit some boundry during our travels, then this one has
           to be a max (since we are doing strips 3 grid boxes wide, and
           this makes it impossible to have a "floating" undef region)  */

        if (bndflg > 0) return (1);

        /* Find the minimum x value in the contour line.  Check the
           right hand point to see if this is a max or a min.          */

        xsave = 9.9e33;
        for (i = 0; i < xycnt; i++)
        {
            x = xybuf[(i * 2)];
            y = xybuf[(i * 2 + 1)];
            if (y == Math.Floor(y) && x < xsave)
            {
                xsave = x;
                ysave = y;
            }
        }

        i = (int)xsave;
        j = (int)ysave;
        int p1cnt = ((j - 1) * imax + i);
        if (gr[p1cnt] > val) return (1);
        return (0);
    }

    void chksid()
    {
        int[] f1, f4;
        int i, j;
        double x, y;
        int f1cnt = 0;
        int f4cnt = 0;
        for (j = 1; j <= jmax; j++)
        {
            for (i = 1; i <= imax; i++)
            {
                if (i < imax)
                {
                    if (flgh[f1cnt] == 1) _drawingContext.GradsDrawingInterface.SetDrawingColor(1);
                    else if (flgh[f1cnt] == 7) _drawingContext.GradsDrawingInterface.SetDrawingColor(3);
                    else if (flgh[f1cnt] == 8) _drawingContext.GradsDrawingInterface.SetDrawingColor(8);
                    else if (flgh[f1cnt] == 9) _drawingContext.GradsDrawingInterface.SetDrawingColor(4);
                    else if (flgh[f1cnt] == 0) _drawingContext.GradsDrawingInterface.SetDrawingColor(15);
                    else _drawingContext.GradsDrawingInterface.SetDrawingColor(2);
                    _drawingContext.GradsDrawingInterface.gxconv((double)i, (double)j, out x, out y, 3);
                    _drawingContext.GradsDrawingInterface.gxplot(x, y, 3);
                    _drawingContext.GradsDrawingInterface.gxconv((double)(i + 1), (double)j, out x, out y, 3);
                    _drawingContext.GradsDrawingInterface.gxplot(x, y, 2);
                }

                if (j < jmax)
                {
                    if (flgv[f4cnt] == 1) _drawingContext.GradsDrawingInterface.SetDrawingColor(1);
                    else if (flgv[f4cnt] == 7) _drawingContext.GradsDrawingInterface.SetDrawingColor(3);
                    else if (flgv[f4cnt] == 8) _drawingContext.GradsDrawingInterface.SetDrawingColor(8);
                    else if (flgv[f4cnt] == 9) _drawingContext.GradsDrawingInterface.SetDrawingColor(4);
                    else if (flgv[f4cnt] == 0) _drawingContext.GradsDrawingInterface.SetDrawingColor(15);
                    else _drawingContext.GradsDrawingInterface.SetDrawingColor(2);
                    _drawingContext.GradsDrawingInterface.gxconv((double)i, (double)j, out x, out y, 3);
                    _drawingContext.GradsDrawingInterface.gxplot(x, y, 3);
                    _drawingContext.GradsDrawingInterface.gxconv((double)i, (double)(j + 1), out x, out y, 3);
                    _drawingContext.GradsDrawingInterface.gxplot(x, y, 2);
                }

                f1cnt++;
                f4cnt++;
            }
        }

        _drawingContext.GradsDrawingInterface.gxfrme(0);
    }
}