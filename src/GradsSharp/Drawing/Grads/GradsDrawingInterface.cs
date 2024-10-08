﻿using System.Text;
using GradsSharp.Data.Grads;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Drawing.Grads;

/*
 * Old gasubs class containing drawing functions
 */


internal class GradsDrawingInterface : IGradsDrawingInterface
{
    private Func<double, double, GradsPoint>? fconv;
    private Func<double, double, GradsPoint>? gconv;
    private Func<double, double, GradsPoint>? bconv;

    private double xsize, ysize; /* Virtual page size  */
    private double rxsize, rysize; /* Real page size     */
    private int lwflg; /* Reduce lw due vpage*/
    private double clminx, clmaxx, clminy, clmaxy; /* Clipping region    */
    private bool cflag; /* Clipping flag      */
    private int mflag; /* mask flag          */
    private double[] dash = new double[8]; /* Linestyle pattern  */
    public int dnum, lstyle; /* Current linestyle  */
    public int CurrentLineColor { get; set; } /* Current color      */
    public int CurrentLineWidth { get; set; } /* Current linewidth  */
    private double oldx, oldy; /* Previous position  */
    private int bufmod; /* Buffering mode     */
    private double xsave, ysave, alen, slen; /* Linestyle constants*/
    private int jpen, dpnt;

    private int intflg; /* Batch/Interactive flag    */

    //private void (*fconv)(double, double, double *, double *); /* for proj rnt */
    //private void (*gconv)(double, double, double *, double *); /* for grid rnt */
    //private void (*bconv)(double, double, double *, double *); /* for back transform rnt */
    private int bcol; /* background color */
    private int savcol; /* for color save/restore */
    private char[]? mask; /* pointer to mask array */
    private int maskx; /* Size of a row in the array */
    private int masksize; /* Size of mask array */

    private int maskflg; /* mask flag; -999 no mask yet,
                                                0 no mask used, 1 mask values set, -888 error  */

    internal GradsDrawingInterface(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public IDrawingEngine DrawingEngine { get; set; }

    private DrawingContext _drawingContext;


    public int gxstrt(double xmx, double ymx, int batch, int hbufsz, string gxdopt, string gxpopt, string xgeom)
    {
        _drawingContext.GradsDatabase.gxdbinit();
        // if (gxpopt == "SixLabors")
        // {
        //     DrawingEngine = new SixLaborsDrawingEngine(_drawingContext);
        // }
        // else if (gxpopt == "Cairo")
        // {
        //     DrawingEngine = new CairoDrawingEngine(_drawingContext);
        // }

        DrawingEngine.gxpinit(xmx, ymx);
        //DrawingEngine.gxpbgn(xmx, ymx);

        rxsize = xmx; /* Set local variables with real page size  */
        rysize = ymx;
        clminx = 0;
        clmaxx = xmx; /* Set clipping area       */
        clminy = 0;
        clmaxy = ymx;
        xsave = 0.0;
        ysave = 0.0;
        lstyle = 0;
        CurrentLineWidth = 3;
        oldx = 0.0;
        oldy = 0.0;
        fconv = null; /* No projection set up    */
        gconv = null; /* No grid scaling set up  */
        bconv = null; /* No back transform       */
        _drawingContext.GxChpl.gxchii(); /* Init character plotting */
        bufmod = 0; /* double buffering is OFF */
        _drawingContext.GxMeta.gxhnew(rxsize, rysize, hbufsz); /* Init hardcopy buffering */
        gxscal(0.0, xmx, 0.0, ymx, 0.0, xmx, 0.0, ymx); /* Linear scaling=inches   */
        gxvpag(xmx, ymx, 0.0, xmx, 0.0, ymx); /* Virtual page scaling    */
        mask = null;
        maskflg = -999; /* Don't allocate mask until first use */
        SetDrawingColor(1);
        return 0;
    }


    /* Terminate graphics output */
    public void gxend()
    {
        DrawingEngine.gxpend(); /* Tell printing layer to destroy batch mode surface */
        //printf("GX Package Terminated \n");
    }

/* Send a signal to the rendering engine and the metabuffer.
   Signal values are:
     1 == Done with draw/display so finish rendering
     2 == Disable anti-aliasing
     3 == Enable anti-aliasing
     4 == Cairo push
     5 == Cairo pop and then paint
*/
    public void gxsignal(int sig)
    {
        _drawingContext.GxMeta.hout1c(-22, sig); /* put the signal in the metafile buffer */
    }


/* Query the width of a character */
    public double gxqchl(char ch, int fn, double w)
    {
        
        double wid;

        /* cases where we want to use Hershey fonts */
        if (fn == 3) return (-999.9); /* symbol font */
        if (fn > 5 && fn < 10) return (-999.9); /* user-defined font file */
        if (fn < 6 && _drawingContext.GradsDatabase.gxdbqhersh() == 0)
            return (-999.9); /* font 0-5 and hershflag=0 in gxmeta.c */


        wid = DrawingEngine.gxpqchl(ch, fn, w); /* get the character width (batch mode) */
        return (wid);
    }

/* Draw a character */
    public double gxdrawch(char ch, int fn, double x, double y, double w, double h, double rot)
    {
        double wid;

        /* cases where we want to use Hershey fonts */
        if (fn == 3) return (-999.9); /* symbol font */
        if (fn > 5 && fn < 10) return (-999.9); /* user-defined font file */
        if (fn < 6 && _drawingContext.GradsDatabase.gxdbqhersh() == 0)
            return (-999.9); /* font 0-5 and hershflag=0 in gxmeta.c */

        /* from here on we're using Cairo fonts */
        gxvcon(x, y, out x, out y); /* scale the position and size for the virual page */
        gxvcon2(w, h, out w, out h);
        if (w > 0 && h > 0)
        {
            /* make sure the width and height are non-zero */
            wid = DrawingEngine.gxpqchl(ch, fn, w); /* get the character width (batch mode) */
            _drawingContext.GxMeta.houtch(ch, fn, x, y, w, h, rot); /* put the character in the metabuffer */
            gxppvp2(wid, out wid); /* rescale the character width back to real page size */
            return (wid); /* return character width */
        }
        else return (0);
    }


/* Frame action.  Values for action are:
      0 -- new frame (clear display), wait before clearing.
      1 -- new frame, no wait.
      2 -- New frame in double buffer mode.  If not supported
           has same result as action=1.  Usage involves multiple
           calls with action=2 to obtain an animation effect.
      7 -- new frame, but just clear graphics.  Do not clear
           event queue; redraw buttons.
      8 -- clear only the event queue.
      9 -- clear only the X request buffer */

    public void gxfrme(int action)
    {
        if (action > 7)
        {
            return;
        }

        gxmaskclear();
        // if (intflg) {
        //     if (action == 0) getchar();        /* Wait if requested */
        //     if (action != 2 && bufmod) {
        //         dsubs.gxdsgl();               /* tell hardware to turn off double buffer mode */
        //         bufmod = 0;
        //     }
        //     if (action == 2 && (!bufmod)) {
        //         dsubs.gxddbl();               /* tell hardware to turn on double buffer mode */
        //         bufmod = 1;
        //     }
        //     if (bufmod) dsubs.gxdswp();     /* swap */
        //     dsubs.gxdfrm(action);           /* tell hardware layer about frame action */
        //     dsubs.gxdfrm(9);                /* clear the X request buffer */
        // }
        _drawingContext.GxMeta.gxhfrm(action); /* reset metabuffer */
        bcol = _drawingContext.GradsDatabase.gxdbkq();
        if (bcol > 1)
        {
            /* If background is not black/white, draw a full page rectangle and populate the metabuffer */
            savcol = CurrentLineColor;
            SetDrawingColor(bcol);
            DrawFilledRectangle(0.0, rxsize, 0.0, rysize);
            SetDrawingColor(savcol);
        }
    }


/* Set color.  Colors are: 0 - black;    1 - white
                           2 - red;      3 - green     4 - blue
                           5 - cyan;     6 - magenta   7 - yellow
                           8 - orange;   9 - purple   10 - lt. green
                          11 - m.blue   12 - yellow   13 - aqua
			  14 - d.purple 15 - gray
   Other colors may be available but are defined by the device driver */

    public void SetDrawingColor(int clr)
    {
        // old method name: gxcolr
        /* Set color     */
        if (clr < 0) clr = 0;
        if (clr >= Gx.COLORMAX) clr = Gx.COLORMAX - 1;

        _drawingContext.GxMeta.hout1(-3, clr);
        //if (intflg) dsubs.gxdcol(clr);
        CurrentLineColor = clr;
    }

/* define a new color */

    public int gxacol(int clr, int red, int green, int blue, int alpha)
    {
        int rc = 0;
        _drawingContext.GradsDatabase.gxdbacol(clr, red, green, blue, alpha); /* update the database */
        _drawingContext.GxMeta.hout5i(-5, clr, red, green, blue, alpha); /* tell the metabuffer */
        //if (intflg) rc = dsubs.gxdacol(clr, red, green, blue, alpha);  /* tell hardware */
        return (rc);
    }


/* Set line weight */

    public void gxwide(int wid)
    {
        /* Set width     */
        int hwid;
        hwid = wid;
        _drawingContext.GxMeta.hout2i(-4, hwid, wid);
        //if (intflg) dsubs.gxdwid(hwid);
        CurrentLineWidth = hwid;
    }

/* Move to x, y with 'clipping'.  Clipping is implmented
   coarsely, where any move or draw point that is outside the
   clip region is not plotted.                          */

    public void MoveToPoint(double x, double y)
    {
        // old method name:  gxmove
        /* Move to x,y   */
        mflag = 0;
        oldx = x;
        oldy = y;
        if (x < clminx || x > clmaxx || y < clminy || y > clmaxy)
        {
            cflag = true;
            return;
        }

        cflag = false;
        gxvcon(x, y, out x, out y);
        _drawingContext.GxMeta.hout2(-10, x, y);
        //if (intflg) dsubs.gxdmov(x, y);
    }

/* Draw to x, y with clipping */

    public void DrawLineToPoint(double x, double y)
    {
        // old method name: gxdraw
        /* Draw to x,y   */
        double xnew, ynew;
        int pos = 0;
        if (x < clminx || x > clmaxx || y < clminy || y > clmaxy)
        {
            if (!cflag)
            {
                bdterp(oldx, oldy, x, y, out xnew, out ynew);
                gxvcon(xnew, ynew, out xnew, out ynew);
                _drawingContext.GxMeta.hout2(-11, xnew, ynew);
                //if (intflg) dsubs.gxddrw(xnew, ynew);
                cflag = true;
            }

            oldx = x;
            oldy = y;
            return;
        }

        if (cflag)
        {
            bdterp(oldx, oldy, x, y, out xnew, out ynew);
            cflag = false;
            gxvcon(xnew, ynew, out xnew, out ynew);
            _drawingContext.GxMeta.hout2(-10, xnew, ynew);
            //if (intflg) dsubs.gxdmov(xnew, ynew);
        }

        oldx = x;
        oldy = y;
        gxvcon(x, y, out x, out y);
        if (maskflg > 0) pos = ((int)(y * 100.0)) * maskx + (int)(x * 100.0);
        if (maskflg > 0 && pos > 0 && pos < masksize && mask[pos] == '1')
        {
            _drawingContext.GxMeta.hout2(-10, x, y);
            //if (intflg) dsubs.gxdmov(x, y);
            mflag = 1;
            return;
        }

        if (mflag > 0)
        {
            _drawingContext.GxMeta.hout2(-10, x, y);
            //if (intflg) dsubs.gxdmov(x, y);
            mflag = 0;
            return;
        }

        _drawingContext.GxMeta.hout2(-11, x, y);
        //if (intflg) dsubs.gxddrw(x, y);
    }

/* Draw lines in small segments, sometimes needed when masking is in use
   (eg, grid lines)  */

    void gxsdrw(double x, double y)
    {
        double xdif, ydif, xx, yy, slope, incr;
        int xnum, ynum, i;

        if (maskflg > 0)
        {
            ydif = Math.Abs(oldy - y);
            xdif = Math.Abs(oldx - x);
            if (ydif < 0.03 && xdif < 0.03) DrawLineToPoint(x, y);
            else
            {
                if (xdif > ydif)
                {
                    incr = 0.03;
                    if (ydif / xdif < 0.3) incr = 0.02;
                    xnum = (int)(xdif / incr);
                    slope = (y - oldy) / (x - oldx);
                    xx = oldx;
                    yy = oldy;
                    if (x < oldx) incr = -1.0 * incr;
                    for (i = 0; i < xnum; i++)
                    {
                        xx = xx + incr;
                        yy = yy + incr * slope;
                        DrawLineToPoint(xx, yy);
                    }

                    DrawLineToPoint(x, y);
                }
                else
                {
                    incr = 0.03;
                    if (xdif / ydif < 0.3) incr = 0.02;
                    ynum = (int)(ydif / incr);
                    slope = (x - oldx) / (y - oldy);
                    xx = oldx;
                    yy = oldy;
                    if (y < oldy) incr = -1.0 * incr;
                    for (i = 0; i < ynum; i++)
                    {
                        xx = xx + incr * slope;
                        yy = yy + incr;
                        DrawLineToPoint(xx, yy);
                    }

                    DrawLineToPoint(x, y);
                }
            }
        }
        else
        {
            DrawLineToPoint(x, y);
        }
    }

/* Set software linestyle */

    public void gxstyl(int style)
    {
        /* Set line style  */
        if (style == -9) style = 1;
        lstyle = style;
        if (style == 2)
        {
            dnum = 1;
            dash[0] = 0.25;
            dash[1] = 0.1;
        }
        else if (style == 3)
        {
            dnum = 1;
            dash[0] = 0.03;
            dash[1] = 0.03;
        }
        else if (style == 4)
        {
            dnum = 3;
            dash[0] = 0.25;
            dash[1] = 0.1;
            dash[2] = 0.1;
            dash[3] = 0.1;
        }
        else if (style == 5)
        {
            dnum = 1;
            dash[0] = 0.01;
            dash[1] = 0.08;
        }
        else if (style == 6)
        {
            dnum = 3;
            dash[0] = 0.15;
            dash[1] = 0.08;
            dash[2] = 0.01;
            ;
            dash[3] = 0.08;
        }
        else if (style == 7)
        {
            dnum = 5;
            dash[0] = 0.15;
            dash[1] = 0.08;
            dash[2] = 0.01;
            dash[3] = 0.08;
            dash[4] = 0.01;
            dash[5] = 0.08;
        }
        else lstyle = 0;

        slen = dash[0];
        jpen = 2;
        dpnt = 0;
    }


/* Move and draw with linestyles and clipping */

    public void gxplot(double x, double y, int ipen)
    {
        /* Move or draw  */
        double x1, y1;

        if (lstyle < 2)
        {
            if (ipen == 2) DrawLineToPoint(x, y);
            else MoveToPoint(x, y);
            xsave = x;
            ysave = y;
            return;
        }

        if (ipen == 3)
        {
            slen = dash[0];
            dpnt = 0;
            jpen = 2;
            xsave = x;
            ysave = y;
            MoveToPoint(x, y);
            return;
        }

        alen = GaUtil.hypot((x - xsave), (y - ysave));
        if (alen < 0.001) return;
        while (alen > slen)
        {
            x1 = xsave + (x - xsave) * (slen / alen);
            y1 = ysave + (y - ysave) * (slen / alen);
            if (jpen == 2) DrawLineToPoint(x1, y1);
            else MoveToPoint(x1, y1);
            dpnt += 1;
            if (dpnt > dnum) dpnt = 0;
            slen = slen + dash[dpnt];
            jpen += 1;
            if (jpen > 3) jpen = 2;
        }

        slen = slen - alen;
        xsave = x;
        ysave = y;
        if (jpen == 2) DrawLineToPoint(x, y);
        else MoveToPoint(x, y);
        if (slen < 0.001)
        {
            dpnt += 1;
            if (dpnt > dnum) dpnt = 0;
            slen = dash[dpnt];
            jpen += 1;
            if (jpen > 3) jpen = 2;
        }
    }

/* Specify software clip region.  */

    public void gxclip(double xmin, double xmax, double ymin, double ymax)
    {
        double clxmin, clxmax, clymin, clymax;

        /* for software clipping */
        clminx = xmin;
        clmaxx = xmax;
        clminy = ymin;
        clmaxy = ymax;
        if (clminx < 0.0) clminx = 0.0;
        if (clmaxx > xsize) clmaxx = xsize;
        if (clminy < 0.0) clminy = 0.0;
        if (clmaxy > ysize) clmaxy = ysize;

        /* specify the hardware clip region, and put it in the metabuffer as well */
        gxvcon(clminx, clminy, out clxmin, out clymin);
        gxvcon(clmaxx, clmaxy, out clxmax, out clymax);
        //if (intflg) dsubs.gxdclip(clxmin, clxmax, clymin, clymax);
        _drawingContext.GxMeta.hout4(-23, clxmin, clxmax, clymin, clymax);
    }


    private double xm, xb, ym, yb;

    public void gxscal(double xmin, double xmax, double ymin, double ymax,
        double smin, double smax, double tmin, double tmax)
    {
        xm = (xmax - xmin) / (smax - smin);
        xb = xmin - (xm * smin);
        ym = (ymax - ymin) / (tmax - tmin);
        yb = ymin - (ym * tmin);
    }

    private double vxm, vxb, vym, vyb;

    /* Specify virtual page scaling.
   Input args are as follows:
     xmax,ymax == virtual page sizes
     smin,smax == real page X-coordinates of virtual page
     tmin,tmax == real page Y-coordinates of virtual page
*/

    public void gxvpag(double xmax, double ymax,
        double smin, double smax, double tmin, double tmax)
    {
        double xmin, ymin;
        /* set virtual page size */
        xmin = 0.0;
        ymin = 0.0;
        xsize = xmax;
        ysize = ymax;
        /* check if virtual page coordinates extend beyond the real page size */
        if (smin < 0.0) smin = 0.0;
        if (smax > rxsize) smax = rxsize;
        if (tmin < 0.0) tmin = 0.0;
        if (tmax > rysize) tmax = rysize;
        /* set clipping area to virtual page */
        clminx = 0.0;
        clmaxx = xmax;
        clminy = 0.0;
        clmaxy = ymax;
        /* if virtual page is small, set a flag to reduce line thickness */
        if ((smax - smin) / rxsize < 0.49 || (tmax - tmin) / rysize < 0.49) lwflg = 1;
        else lwflg = 0;
        /* set up constants for virtual page scaling */
        vxm = (smax - smin) / (xmax - xmin);
        vxb = smin - (vxm * xmin);
        vym = (tmax - tmin) / (ymax - ymin);
        vyb = tmin - (vym * ymin);
        /* For non-software clipping ... put coordinates in the metabuffer and tell the hardware */
        gxclip(clminx, clmaxx, clminy, clmaxy);
    }

    void gxvcon(double s, double t, out double x, out double y)
    {
        /* positions, real->virtual */
        x = s * vxm + vxb;
        y = t * vym + vyb;
    }

    void gxvcon2(double s, double t, out double x, out double y)
    {
        /* characters, real->virtual */
        x = s * vxm;
        y = t * vym;
    }

    void gxppvp(double x, double y, out double s, out double t)
    {
        /* positions, virtual->real */
        s = (x - vxb) / vxm;
        t = (y - vyb) / vym;
    }

    void gxppvp2(double x, out double s)
    {
        /* character width, virtual->real */
        s = (x) / vxm;
    }


/* Convert coordinates at a particular level to level 0 coordinates
   (hardware coords, 'inches').  The level of the input coordinates
   is provided.  User projection and grid scaling routines are called
   as needed.  */

    public void gxconv(double s, double t, out double x, out double y, int level)
    {
        GradsPoint tu = new GradsPoint(s, t);

        if (level > 2 && gconv != null) tu = gconv(s, t);
        if (level > 1 && fconv != null) tu = fconv(tu.x, tu.y);
        if (level > 0)
        {
            s = tu.x * xm + xb;
            t = tu.y * ym + yb;
        }

        x = s;
        y = t;
    }

/* Convert from level 0 coordinates (inches) to level 2 world
   coordinates.  The back transform is done via conversion
   linearly from level 0 to level 1, then calling the back
   transform map routine, if available, to do level 1 to level
   2 transform.  */

    public void gxxy2w(double x, double y, out double s, out double t)
    {
        /* Do level 0 to level 1 */
        if (xm == 0.0 || ym == 0.0)
        {
            s = -999.9;
            t = -999.9;
            return;
        }

        s = (x - xb) / xm;
        t = (y - yb) / ym;

        /* Do level 1 to level 2 */
        GradsPoint tu = new GradsPoint(s, t);
        if (bconv != null) tu = bconv(s, t);
        s = tu.x;
        t = tu.y;
    }

/* Allow caller to specify a routine to do the back transform from
   level 1 to level 2 coordinates. */
    public void gxback(Func<double, double, GradsPoint> fproj)
    {
        bconv = fproj;
    }

/* Specify projection-level scaling, typically used for map
   projections.  The address of the routine to perform the scaling
   is provided.  This is scaling level 2, and is the level that
   mapping is done. */

    public void gxproj(Func<double, double, GradsPoint>? fproj)
    {
        fconv = fproj;
    }

/* Specify grid level scaling, typically used to convert a grid
   to lat-lon values that can be input to the projection or linear
   level scaling.  The address of a routine is provided to perform
   the possibly non-linear scaling.  This is scaling level 3, and
   is the level that contouring is done.  */

    public void gxgrid(Func<double, double, GradsPoint> fproj)
    {
        gconv = fproj;
    }


/* Convert from grid coordinates to map coordinates (level 3 to level 2) */
    public void gxgrmp(double s, double t, out double x, out double y)
    {
        GradsPoint result = new GradsPoint(0, 0);
        if (gconv != null) result = gconv(s, t);
        x = result.x;
        y = result.y;
    }

/* Convert an array of higher level coordinates to level 0 coordinates.
   The conversion is done 'in place' and the input coordinates are
   lost.  This routine performs the same function as coord except is
   somewhat more efficient for many coordinate transforms.         */

    public double[] gxcord(double[] coords, int num, int level)
    {
        int i;
        int xy;
        double[] result = coords;

        if (level > 2 && gconv != null)
        {
            xy = 0;
            for (i = 0; i < num; i++)
            {
                var tuple = gconv(coords[xy], coords[xy + 1]);
                result[xy] = tuple.x;
                result[xy + 1] = tuple.y;
                xy += 2;
            }
        }

        if (level > 1 && fconv != null)
        {
            xy = 0;
            for (i = 0; i < num; i++)
            {
                var tuple = fconv(result[xy], result[xy + 1]);
                result[xy] = tuple.x;
                result[xy + 1] = tuple.y;
                xy += 2;
            }
        }

        if (level > 0)
        {
            xy = 0;
            for (i = 0; i < num; i++)
            {
                result[xy] = result[xy] * xm + xb;
                xy++;
                result[xy] = result[xy] * ym + yb;
                xy++;
            }
        }

        return result;
    }

/* Delete level 3 or level 2 and level 3 scaling.
   Level 1 scaling cannot be deleted.  */

    public void gxrset(int level)
    {
        if (level > 2) gconv = null;
        if (level > 1)
        {
            fconv = null;
            bconv = null;
        }
    }

/* Plot a color filled rectangle.  */

    public void DrawFilledRectangle(double xlo, double xhi, double ylo, double yhi)
    {
        // old method name: gxrecf
        double x;

        if (xlo > xhi)
        {
            x = xlo;
            xlo = xhi;
            xhi = x;
        }

        if (ylo > yhi)
        {
            x = ylo;
            ylo = yhi;
            yhi = x;
        }

        if (xhi <= clminx || xlo >= clmaxx || yhi <= clminy || ylo >= clmaxy) return;
        if (xlo < clminx) xlo = clminx;
        if (xhi > clmaxx) xhi = clmaxx;
        if (ylo < clminy) ylo = clminy;
        if (yhi > clmaxy) yhi = clmaxy;
        gxvcon(xlo, ylo, out xlo, out ylo);
        gxvcon(xhi, yhi, out xhi, out yhi);
        _drawingContext.GxMeta.hout4(-6, xlo, xhi, ylo, yhi);
        // if (intflg) {
        //     dsubs.gxdrec(xlo, xhi, ylo, yhi);
        // }
    }

    public int ShadeCount => _drawingContext.CommonData.shdcnt;
    public double[] ShadeLevels => _drawingContext.CommonData.shdlvs;
    public int[] ShadeColors => _drawingContext.CommonData.shdcls;

    /* Define fill pattern for rectangles and polygons. */

    void gxptrn(int typ, int den, int ang)
    {
        _drawingContext.GxMeta.hout3i(-12, typ, den, ang);
        //if (intflg) dsubs.gxdptn(typ, den, ang);
    }

/* query line width */

    int gxqwid()
    {
        return (CurrentLineWidth);
    }

/* query color */

    int gxqclr()
    {
        return (CurrentLineColor);
    }

/* query style */

    int gxqstl()
    {
        return (lstyle);
    }

/* Draw markers 1-5. */

    public void gxmark(int mtype, double x, double y, double siz)
    {
        double[] xy = new double[80];
        double siz2;
        int i, ii, cnt;

        siz2 = siz / 2.0;
        if (mtype == 1)
        {
            /* cross hair */
            MoveToPoint(x, y - siz2);
            DrawLineToPoint(x, y + siz2);
            MoveToPoint(x - siz2, y);
            DrawLineToPoint(x + siz2, y);
            return;
        }

        if (mtype == 2 || mtype == 3 || mtype == 10 || mtype == 11)
        {
            /* circles */
            if (siz < 0.1) ii = 30;
            else if (siz < 0.3) ii = 15;
            else ii = 10;
            if (mtype > 3) ii = 15;
            cnt = 0;
            for (i = 60; i < 415; i += ii)
            {
                xy[cnt * 2] = x + siz2 * Math.Cos((double)(i) * Math.PI / 180.0);
                xy[cnt * 2 + 1] = y + siz2 * Math.Sin((double)(i) * Math.PI / 180.0);
                cnt++;
            }

            xy[cnt * 2] = xy[0];
            xy[cnt * 2 + 1] = xy[1];
            cnt++;
            if (mtype == 2)
            {
                /* Open circle */
                MoveToPoint(xy[0], xy[1]);
                for (i = 1; i < cnt; i++) DrawLineToPoint(xy[i * 2], xy[i * 2 + 1]);
            }
            else if (mtype == 3)
            {
                /* Filled circle */
                gxfill(xy, cnt);
            }
            else if (mtype == 10)
            {
                /* Scattered fill */
                MoveToPoint(xy[6], xy[7]);
                for (i = 4; i < 14; i++) DrawLineToPoint(xy[i * 2], xy[i * 2 + 1]);
                MoveToPoint(xy[30], xy[31]);
                for (i = 16; i < 25; i++) DrawLineToPoint(xy[i * 2], xy[i * 2 + 1]);
                DrawLineToPoint(xy[0], xy[1]);
                for (i = 8; i < 14; i++) xy[i] = xy[i + 18];
                xy[14] = xy[2];
                xy[15] = xy[3];
                gxfill(xy[2..], 7);
            }
            else if (mtype == 11)
            {
                /* Broken fill */
                xy[0] = x + siz2 * Math.Cos(68.0 * Math.PI / 180.0);
                xy[1] = y + siz2 * Math.Sin(68.0 * Math.PI / 180.0);
                xy[8] = x + siz2 * Math.Cos(112.0 * Math.PI / 180.0);
                xy[9] = y + siz2 * Math.Sin(112.0 * Math.PI / 180.0);
                xy[24] = x + siz2 * Math.Cos(248.0 * Math.PI / 180.0);
                xy[25] = y + siz2 * Math.Sin(248.0 * Math.PI / 180.0);
                xy[32] = x + siz2 * Math.Cos(292.0 * Math.PI / 180.0);
                xy[33] = y + siz2 * Math.Sin(292.0 * Math.PI / 180.0);
                MoveToPoint(xy[0], xy[1]);
                for (i = 1; i < 5; i++) DrawLineToPoint(xy[i * 2], xy[i * 2 + 1]);
                MoveToPoint(xy[24], xy[25]);
                for (i = 13; i < 17; i++) DrawLineToPoint(xy[i * 2], xy[i * 2 + 1]);
                xy[26] = xy[8];
                xy[27] = xy[9];
                gxfill(xy[8..], 10);
                xy[50] = xy[0];
                xy[51] = xy[1];
                gxfill(xy[32..], 10);
            }

            return;
        }

        if (mtype == 4 || mtype == 5)
        {
            /* Draw sqaures */
            xy[0] = x - siz2;
            xy[1] = y + siz2;
            xy[2] = x + siz2;
            xy[3] = y + siz2;
            xy[4] = x + siz2;
            xy[5] = y - siz2;
            xy[6] = x - siz2;
            xy[7] = y - siz2;
            xy[8] = xy[0];
            xy[9] = xy[1];
            if (mtype == 4)
            {
                MoveToPoint(xy[0], xy[1]);
                for (i = 1; i < 5; i++) DrawLineToPoint(xy[i * 2], xy[i * 2 + 1]);
            }
            else
            {
                gxfill(xy, 5);
            }

            return;
        }

        if (mtype == 6)
        {
            /* ex marks the spot */
            MoveToPoint(x - siz2 * 0.71, y - siz2 * 0.71);
            DrawLineToPoint(x + siz2 * 0.71, y + siz2 * 0.71);
            MoveToPoint(x - siz2 * 0.71, y + siz2 * 0.71);
            DrawLineToPoint(x + siz2 * 0.71, y - siz2 * 0.71);
            return;
        }

        if (mtype == 7 || mtype == 12)
        {
            /* Open or closed diamond */
            MoveToPoint(x - siz2 * 0.75, y);
            DrawLineToPoint(x, y + siz2 * 1.1);
            DrawLineToPoint(x + siz2 * 0.75, y);
            DrawLineToPoint(x, y - siz2 * 1.1);
            DrawLineToPoint(x - siz2 * 0.75, y);
            if (mtype == 12)
            {
                xy[0] = x - siz2 * 0.75;
                xy[1] = y;
                xy[2] = x;
                xy[3] = y + siz2 * 1.1;
                xy[4] = x + siz2 * 0.75;
                xy[5] = y;
                xy[6] = x;
                xy[7] = y - siz2 * 1.1;
                xy[8] = x - siz2 * 0.75;
                xy[9] = y;
                gxfill(xy, 4);
            }

            return;
        }

        if (mtype == 8 || mtype == 9)
        {
            /* Triangles */
            xy[0] = x;
            xy[1] = y + siz2;
            xy[2] = x + siz2 * 0.88;
            xy[3] = y - siz2 * 0.6;
            xy[4] = x - siz2 * 0.88;
            xy[5] = y - siz2 * 0.6;
            xy[6] = x;
            xy[7] = y + siz2;
            if (mtype == 8)
            {
                MoveToPoint(xy[0], xy[1]);
                for (i = 1; i < 4; i++) DrawLineToPoint(xy[i * 2], xy[i * 2 + 1]);
            }
            else
            {
                gxfill(xy, 4);
            }

            return;
        }
    }

/* Plot centered title.  Only supports angle of 0 and 90 */

    void gxtitl(string chrs, double x, double y, double height,
        double width, double angle)
    {
        double xx, yy;
        int len, i;

        i = 0;
        len = 0;
        while (i < chrs.Length)
        {
            if (chrs[i] != ' ') len = i + 1;
            i++;
        }

        if (len == 0) return;

        xx = x;
        yy = y;
        if (angle > 45.0)
        {
            yy = y - 0.5 * width * (double)len;
        }
        else
        {
            xx = x - 0.5 * width * (double)len;
        }

        gxchpl(chrs, len, xx, yy, height, width, angle);
    }

/* Plot character string */

    public void gxchpl(string chrs, int len, double x, double y, double height, double width, double angle)
    {

        _drawingContext.GxChpl.gxchpl(chrs, len, x, y, height, width, angle);
    }


/* Do polygon fill.  It is assumed the bulk of the work will be done
   in hardware.  We do perform clipping at this level, and
   actually do the work to clip at the clipping boundry.       */

    public void gxfill(double[] xy, int num)
    {
        double[] r, outxy, buff, xybuff = new double[40];
        double x, y;
        int i, onum;
        bool flag = false, aflag = false;
        if (num < 3) return;
        /* Do clipping.    */

        aflag = false;
        if (num < 10) buff = xybuff;
        else
        {
            buff = new double[num * 4];
            aflag = true;
        }

        r = xy;
        outxy = buff;
        onum = 0;
        int cnt = 0, outcnt = 0;
        if (r[cnt] < clminx || r[cnt] > clmaxx || r[cnt + 1] < clminy || r[cnt + 1] > clmaxy) flag = true;
        for (i = 0; i < num; i++)
        {
            if (r[cnt] < clminx || r[cnt] > clmaxx || r[cnt + 1] < clminy || r[cnt + 1] > clmaxy)
            {
                if (!flag)
                {
                    bdterp(r[cnt - 2], r[cnt - 1], r[cnt], r[cnt + 1], out x, out y);
                    outxy[outcnt] = x;
                    outxy[outcnt + 1] = y;
                    onum++;
                    outcnt += 2;
                }

                outxy[outcnt] = r[cnt];
                outxy[outcnt + 1] = r[cnt + 1];
                if (r[cnt] < clminx) outxy[outcnt] = clminx;
                if (r[cnt] > clmaxx) outxy[outcnt] = clmaxx;
                if (r[cnt + 1] < clminy) outxy[outcnt + 1] = clminy;
                if (r[cnt + 1] > clmaxy) outxy[outcnt + 1] = clmaxy;
                onum++;
                outcnt += 2;
                flag = true;
            }
            else
            {
                if (flag)
                {
                    bdterp(r[cnt - 2], r[cnt - 1], r[cnt], r[cnt + 1], out x, out y);
                    outxy[outcnt] = x;
                    outxy[outcnt + 1] = y;
                    onum++;
                    outcnt += 2;
                }

                outxy[outcnt] = r[cnt];
                outxy[outcnt + 1] = r[cnt + 1];
                onum++;
                outcnt += 2;
                flag = false;
            }

            cnt += 2;
        }

        r = buff;
        cnt = 0;
        for (i = 0; i < onum; i++)
        {
            double k, l;
            gxvcon(r[cnt], r[cnt + 1], out k, out l);
            r[cnt] = k;
            r[cnt + 1] = l;
            cnt += 2;
        }

        /* Output to metabuffer */

        _drawingContext.GxMeta.hout1(-7, onum); /* start a polygon fill */
        r = buff;
        cnt = 0;
        _drawingContext.GxMeta.hout2(-10, r[cnt], r[cnt + 1]); /* move to first point in polygon */
        cnt += 2;
        for (i = 1; i < onum; i++)
        {
            _drawingContext.GxMeta.hout2(-11, r[cnt], r[cnt + 1]); /* draw to next point in polygon */
            cnt += 2;
        }

        _drawingContext.GxMeta.hout0(-8); /* terminate polygon */

        /* Output to hardware */

        //if (intflg) dsubs.gxdfil(buff, onum);
        //if (aflag) free(buff);
    }

/* Perform edge interpolation for clipping  */

    void bdterp(double x1, double y1, double x2, double y2,
        out double x, out double y)
    {
        x = 0;
        y = 0;
        if (x1 < clminx || x2 < clminx || x1 > clmaxx || x2 > clmaxx)
        {
            x = clminx;
            if (x1 > clmaxx || x2 > clmaxx) x = clmaxx;
            y = y1 - ((y1 - y2) * (x1 - x) / (x1 - x2));
            if (y < clminy || y > clmaxy) goto sideh;
            return;
        }

        sideh:

        if (y1 < clminy || y2 < clminy || y1 > clmaxy || y2 > clmaxy)
        {
            y = clminy;
            if (y1 > clmaxy || y2 > clmaxy) y = clmaxy;
            x = x1 - ((x1 - x2) * (y1 - y) / (y1 - y2));
            return;
        }
    }

// void gxbutn(int bnum, struct gbtn *pbn) {
//     _drawingContext.GxMeta.hout1(-20, bnum);
//     dsubs.gxdpbn(bnum, pbn, 0, 0, -1);
// }

/* Set mask for a rectangular area */

    public void gxmaskrec(double xlo, double xhi, double ylo, double yhi)
    {
        int siz, i, j, pos, ilo, ihi, jlo, jhi, jj;

        if (maskflg == -888) return;

        if (mask == null)
        {
            /* If not allocated yet, now's the time */
            siz = (int)(rxsize * rysize * 10000.0);
            mask = new char[siz];
            masksize = siz;
            maskx = (int)(rxsize * 100.0);
            gxmaskclear();
        }

        maskflg = 1;

        /* do clipping for the mask */
        if (xlo < clminx && xhi < clminx) return;
        if (xlo > clmaxx && xhi > clmaxx) return;
        if (ylo < clminy && yhi < clminy) return;
        if (ylo > clmaxy && yhi > clmaxy) return;

        if (xlo < clminx) xlo = clminx;
        if (xhi > clmaxx) xhi = clmaxx;
        if (ylo < clminy) ylo = clminy;
        if (yhi > clmaxy) yhi = clmaxy;

        /* convert to virtual page coordinates */
        gxvcon(xlo, ylo, out xlo, out ylo);
        gxvcon(xhi, yhi, out xhi, out yhi);

        ilo = (int)(xlo * 100.0);
        ihi = (int)(xhi * 100.0);
        jlo = (int)(ylo * 100.0);
        jhi = (int)(yhi * 100.0);
        if (ilo < 0) ilo = 0;
        if (ihi < 0) ihi = 0;
        if (ilo >= maskx) ilo = maskx - 1;
        if (ihi >= maskx) ihi = maskx - 1;
        for (j = jlo; j <= jhi; j++)
        {
            jj = j * maskx;
            for (i = ilo; i <= ihi; i++)
            {
                pos = jj + i;
                if (pos >= 0 && pos < masksize) mask[pos] = '1';
            }
        }
    }

/* Given a rectangular area, check to see if it overlaps with any existing
   mask.  This is used to avoid overlaying contour labels. */

    public int gxmaskrq(double xlo, double xhi, double ylo, double yhi)
    {
        int i, j, ilo, ihi, jlo, jhi, jj, pos;

        if (maskflg == -888) return (0);
        if (mask == null) return (0);
        if (maskflg == 0) return (0);

        /* If query region is partially or completely outside of clip area, indicate an overlap */

        if (xlo < clminx || xhi > clmaxx || ylo < clminy || yhi > clmaxy) return (1);

        /* convert to virtual page coordinates */
        gxvcon(xlo, ylo, out xlo, out ylo);
        gxvcon(xhi, yhi, out xhi, out yhi);

        ilo = (int)(xlo * 100.0);
        ihi = (int)(xhi * 100.0);
        jlo = (int)(ylo * 100.0);
        jhi = (int)(yhi * 100.0);
        if (ilo < 0) ilo = 0;
        if (ihi < 0) ihi = 0;
        if (ilo > maskx) ilo = maskx;
        if (ihi > maskx) ihi = maskx;
        for (j = jlo; j <= jhi; j++)
        {
            jj = j * maskx;
            for (i = ilo; i <= ihi; i++)
            {
                pos = jj + i;
                if (pos >= 0 && pos < masksize)
                {
                    if (mask[pos] == '1') return (1);
                }
            }
        }

        return (0);
    }

/* Set mask to unset state */

    public void gxmaskclear()
    {
        int i;
        if (maskflg > 0)
        {
            for (i = 0; i < masksize; i++) mask[i] = '0';
            maskflg = 0;
        }
    }


/* Query env symbol */

    public string? gxgsym(string ch)
    {
        return (System.Environment.GetEnvironmentVariable(ch));
    }

/* Construct full file path name from env symbol or default */

// string gxgnam(string ch) {
//     string? fname = null, ddir;
//     int len, i, j;
//     size_t sz;
//
//     /* calc partial length of output string */
//     len = ch.Length;
//     i = ch.Length;
//     
//     /* Query the env symbol */
//     ddir = gxgsym("GADDIR");
//
//     /* calc the total length of the output string */
//     if (ddir == null) {
//         i = 0;
//         while (*(datad + i)) {
//             i++;
//             len++;
//         }
//     } else {
//         i = 0;
//         while (*(ddir + i)) {
//             i++;
//             len++;
//         }
//     }
//
//     /* Allocate memory for the output */
//     sz = len + 15;
//     fname = (char *) malloc(sz);
//     if (fname == NULL) {
//         printf("Memory allocation error in data set open\n");
//         return (NULL);
//     }
//
//     /* fill in the directory depending on the value of the env var */
//     if (ddir == NULL) {
//         i = 0;
//         while (*(datad + i)) {
//             *(fname + i) = *(datad + i);
//             i++;
//         }
//     } else if (*ddir == '.') {
//         i = 0;
//     } else {
//         i = 0;
//         while (*(ddir + i)) {
//             *(fname + i) = *(ddir + i);
//             i++;
//         }
//     }
//
//     /* Insure a slash between dir name and file name */
//     if (i != 0 && *(fname + i - 1) != '/') {
//         *(fname + i) = '/';
//         i++;
//     }
//
//     /* fill in the file name */
//     j = 0;
//     while (*(ch + j)) {
//         *(fname + i) = *(ch + j);
//         i++;
//         j++;
//     }
//     *(fname + i) = '\0';
//
//     return (fname);
// }
}