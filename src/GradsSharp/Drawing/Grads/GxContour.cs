using System.Globalization;
using GradsSharp.Data.Grads;
using GradsSharp.Models.Internal;
using GradsSharp.Utils;
using Microsoft.Extensions.Logging;

namespace GradsSharp.Drawing.Grads;

internal class GxContour
{
    private DrawingContext _drawingContext;

    private const int LABMAX = 200;

    private List<gxclbuf>? clbufanch = null; /* Anchor for contour line buffering for masking */
    private string pout;

    private double[] gxlabx = new double[LABMAX];
    private double[] gxlaby = new double[LABMAX];
    private double[] gxlabs = new double[LABMAX];
    private string[] gxlabv = new string[LABMAX];
    private int[] gxlabc = new int[LABMAX];
    private int gxlabn = 0;
    private double ldmin = 2.5; /* Minimum distance between labels */

/* Common values for the contouring routines.                        */

    private short[]? lwk; /* Pntr to flag work area     */
    private int lwksiz = 0; /* Size of flag work area     */
    private double[]? fwk; /* Pntr to X,Y coord buffer   */
    private int fwksiz = 0; /* Size of coord buffer       */
    private int fwkmid; /* fwk midpoint               */
    private int xystrt, xyend; /* Pntrs into the fwk buffer  */
    private int iss, iww, jww; /* Grid row lengths           */
    private double vv; /* Value being contoured      */
    private double[] rr; /* Start of grid              */

    private string clabel; /* Label for current contour  */

    public GxContour(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    void gxclmn(double dis)
    {
        if (dis > 0.1) ldmin = dis;
    }

/* GXCLEV draws contours at a specified contour level and locates
   labels for that contour level.  The labels are buffered into
   the label buffer for later output.

   Contours are drawn through the grid pointed at by r, row length is,
   number of rows js, row start and end ib and ie, column start
   and end jb and je.  The contour is drawn at value V, and undefined
   values u are ignored.  Note that min value for ib or jb is 1.     */

/* Label types:  0 - none, 1 - at slope=0, 2 - forced  */

    public void gxclev(double[] r, int iis, int jjs, int ib, int ie, int jb,
        int je, double v, byte[] u, gxcntr pcntr)
    {
        int p1, p2, p4;
        int i, j, iw, jw, rc;
        int l1, l2;
        int p1u, p2u, p4u;
        long sz;


/* Figure out in advance what grid sides the contour passes
   through for this grid box.  We will actually draw the contours
   later.  Doing the tests in advance saves some calculations.
   Note that a border of flags is left unset all around the grid
   boundry.  This avoids having to test for the grid boundries
   when we are following a contour.                                  */

        clabel = pcntr.label; /* Label for this contour     */

        iw = iis + 2;
        jw = jjs + 2; /* Dimensions of flag grid    */
        iww = iw;
        jww = jw;
        iss = iis; /* Set shared values          */
        vv = v;
        rr = r; /* Set more shared values     */

/* Obtain storage for flag work area and coord buffer -- if we don't
   already have enough storage allocated for these.                  */

        if (lwksiz < iw * jw * 2)
        {
            /* If storage inadaquate then */
            lwksiz = iw * jw * 2; /* Size of lwk flag area      */
            lwk = new short[lwksiz]; /* Allocate flag work area */
            fwksiz = (iw + 1) * (jw + 1); /* Size of coord pair buffer */
            if (fwksiz < 500) fwksiz = 500; /* Insure big enough for small grids */
            fwk = new double[fwksiz]; /* Allocate fwk   */
            fwkmid = fwksiz / 2; /* fwk midpoint              */
        }

/* Set up the flags */

        for (l1 = 0; l1 < iw * jw * 2; l1++) lwk[l1] = 0; /* Clear flags            */

        for (j = jb; j < je; j++)
        {
            /* Loop through the rows      */
            p1 = iis * (j - 1) + ib - 1; /* Set grid pointers to corner ...  */
            p2 = p1 + 1;
            p4 = p1 + iis; /*   values at start of row         */
            p1u = iis * (j - 1) + ib - 1; /* Set undef pointers to corner ... */
            p2u = p1u + 1;
            p4u = p1u + iis; /*   values at start of row         */
            l1 = iw * j + ib;
            l2 = l1 + iw * jw; /* Pointers to flags          */
            for (i = ib; i < ie; i++)
            {
                /* Loop through a row         */
                /* Cntr pass through bottom?  */
                if (((r[p1] <= v && r[p2] > v) || (r[p1] > v && r[p2] <= v)) && u[p1u] != 0 && u[p2u] != 0) lwk[l1] = 1;
                /* Cntr pass through left?    */
                if (((r[p1] <= v && r[p4] > v) || (r[p1] > v && r[p4] <= v)) && u[p1u] != 0 && u[p4u] != 0) lwk[l2] = 1;
                p1++;
                p2++;
                p4++;
                l1++;
                l2++; /* Bump pntrs through the row */
                p1u++;
                p2u++;
                p4u++; /* Bump undef pntrs through the row */
            }

            if (((r[p1] <= v && r[p4] > v) || (r[p1] > v && r[p4] <= v)) && u[p1u] != 0 && u[p4u] != 0) lwk[l2] = 1;
        }

        p1 = iis * (je - 1) + ib - 1; /* Set grid pntrs to corner values  */
        p2 = p1 + 1;
        p4 = p1 + iis; /*   at start of last row           */
        p1u = iis * (je - 1) + ib - 1; /* Set undef pntrs to corner values */
        p2u = p1u + 1;
        p4u = p1u + iis; /*   at start of last row           */
        l1 = iw * je + ib; /* Flag pointers              */
        for (i = ib; i < ie; i++)
        {
            /* Loop through the last row  */
            /* Check top of grid          */
            if (((r[p1] <= v && r[p2] > v) || (r[p1] > v && r[p2] <= v)) && u[p1u] != 0 && u[p2u] != 0) lwk[l1] = 1;
            p1++;
            p2++;
            p4++;
            l1++; /* Incr pntrs through the row */
            p1u++;
            p2u++;
            p4u++; /* Incr undef pntrs through the row */
        }

/* Look for a grid side that has a contour pasMath.Sing through it that
   has not yet been drawn.  Follow it in both directions.
   The X,Y coord pairs will be put in the floating point buffer,
   starting from the middle of the buffer.                           */

        for (j = jb; j < je; j++)
        {
            /* Loop through the rows      */
            l1 = iw * j + ib;
            l2 = l1 + iw * jw; /* Pointers to flags          */
            for (i = ib; i < ie; i++)
            {
                /* Loop through a row         */
                if (lwk[l1] > 0)
                {
                    /* Do we got one?             */
                    gxcflw(i, j, 1, 2); /* Follow it                  */
                    gxcflw(i, j - 1, 3, -2); /* Follow it the other way    */
                    rc = gxcspl(false, pcntr); /* Output it                  */
                    if (rc > 0) goto merr;
                }

                if (lwk[l2] > 0)
                {
                    /* Do we got one?             */
                    gxcflw(i, j, 4, 2); /* Follow it                  */
                    gxcflw(i - 1, j, 2, -2); /* Follow it the other way    */
                    rc = gxcspl(false, pcntr); /* Output it                  */
                    if (rc > 0) goto merr;
                }

                l1++;
                l2++;
            }
        }

/* Short (two point) lines in  upper right corner may hae been missed */

        l1 = iw * je + ie - 1; /* Upper right corner flags   */
        l2 = iw * jw + iw * (je - 1) + ie;
        if (lwk[l1] > 0 && lwk[l2] > 0)
        {
            gxcflw(ie, je - 1, 4, 2); /* Follow it one way */
            xystrt = fwkmid; /* Terminate other direction */
            rc = gxcspl(false, pcntr); /* Output it */
            if (rc > 0) goto merr;
        }

        return;

        merr:
        _drawingContext.Logger?.LogInformation("Error in line contouring: Memory allocation for Label Buffering");
    }

    void gxcflw(int i, int j, int iside, int dr)
    {
/* Follow a contour to the end.  i and j are the grid location
   of the start of the contour.  is is the side on which we are
   entering the grid box.  dr is the direction in which we should
   fill the output buffer.  Other needed values are external.        */

/* The grid box:

     p4      side 3     p3
        x ------------ x
        |              |
 side 4 |              | side 2
        |              |
        |              |
        x ------------ x
      p1    side 1      p2
                                                                     */

        int l1, l2, l3, l4; /* Flags for each box side    */
        int p1, p2, p3, p4; /* Grid points for a box      */
        int isave, jsave; /* Save initial grid postn    */
        int xy, xyllim, xyulim; /* Buffer position and limits */


        isave = i;
        jsave = j;
        p1 = iss * (j - 1) + i - 1; /* Set pntrs to corner values */
        p2 = p1 + 1;
        p4 = p1 + iss;
        p3 = p4 + 1;
        l1 = iww * j + i;
        l4 = l1 + iww * jww; /* Pointers to flags          */
        l2 = l4 + 1;
        l3 = l1 + iww;
        xy = fwkmid; /* Start in middle of buffer  */
        xyllim = 6; /* Buffer limits              */
        xyulim = (fwksiz - 6);

        if (iside == 1) goto side1; /* Jump in based on side      */
        if (iside == 2) goto side2;
        if (iside == 3) goto side3;
        goto side4;

/* Calculate exit point in the current grid box, then move to the
   next grid box based on the exit side.                             */

        side1: /* Exit side 1; Enter side 3  */
        fwk[xy] = i + (vv - rr[p1]) / (rr[p2] - rr[p1]); /* Calculate exit point       */
        fwk[xy + 1] = j;
        lwk[l1] = 0; /* Indicate we were here      */
        xy += dr; /* Move buffer pntr           */
        if ((xy < xyllim) || (xy > xyulim)) goto done; /* Don't exceed buffer    */
        l3 = l1;
        l1 -= iww;
        l4 -= iww;
        l2 = l4 + 1; /* Move pntrs to lower box    */
        p4 = p1;
        p3 = p2;
        p1 -= iss;
        p2 -= iss;
        j--;
        if (lwk[l1] > 0 && lwk[l2] > 0 && lwk[l4] > 0)
        {
            /* Handle col point           */
            if (pathln(rr[p1], rr[p2], rr[p3], rr[p4]) > 0) goto side4;
            else goto side2;
        }

        if (lwk[l4] > 0) goto side4; /* Find exit point            */
        if (lwk[l1] > 0) goto side1;
        if (lwk[l2] > 0) goto side2;
        goto done; /* If no exit point, then done*/

        side2: /* Exit side 2; Enter side 4  */
        fwk[xy] = i + 1.0; /* Calculate exit point       */
        fwk[xy + 1] = j + (vv - rr[p2]) / (rr[p3] - rr[p2]);
        lwk[l2] = 0; /* Indicate we were here      */
        xy += dr; /* Move buffer pntr           */
        if ((xy < xyllim) || (xy > xyulim)) goto done; /* Don't exceed buffer    */
        l4 = l2;
        l1++;
        l2++;
        l3++; /* Move pntrs to right box    */
        p1 = p2;
        p4 = p3;
        p2++;
        p3++;
        i++;
        if (lwk[l1] > 0 && lwk[l2] > 0 && lwk[l3] > 0)
        {
            /* Handle col point           */
            if (pathln(rr[p1], rr[p2], rr[p3], rr[p4]) > 0) goto side3;
            else goto side1;
        }

        if (lwk[l1] > 0) goto side1; /* Find exit point            */
        if (lwk[l2] > 0) goto side2;
        if (lwk[l3] > 0) goto side3;
        goto done; /* If no exit point, then done*/

        side3: /* Exit side 3; Enter side 1  */
        fwk[xy] = i + (vv - rr[p4]) / (rr[p3] - rr[p4]); /* Calculate exit point       */
        fwk[xy + 1] = j + 1.0;
        lwk[l3] = 0; /* Indicate we were here      */
        xy += dr; /* Move buffer pntr           */
        if ((xy < xyllim) || (xy > xyulim)) goto done; /* Don't exceed buffer    */
        l1 = l3;
        l4 += iww;
        l3 += iww;
        l2 = l4 + 1; /* Move pntrs to upper box    */
        p1 = p4;
        p2 = p3;
        p3 += iss;
        p4 += iss;
        j++;
        if (lwk[l2] > 0 && lwk[l3] > 0 && lwk[l4] > 0)
        {
            /* Handle col point           */
            if (pathln(rr[p1], rr[p2], rr[p3], rr[p4]) > 0) goto side2;
            else goto side4;
        }

        if (lwk[l2] > 0) goto side2; /* Find exit point            */
        if (lwk[l3] > 0) goto side3;
        if (lwk[l4] > 0) goto side4;
        goto done; /* If no exit point, then done*/

        side4: /* Exit side 4; Enter side 2  */
        fwk[xy] = i; /* Calculate exit point       */
        fwk[xy + 1] = j + (vv - rr[p1]) / (rr[p4] - rr[p1]);
        lwk[l4] = 0; /* Indicate we were here      */
        xy += dr; /* Move buffer pntr           */
        if ((xy < xyllim) || (xy > xyulim)) goto done; /* Don't exceed buffer    */
        l2 = l4;
        l1--;
        l3--;
        l4--; /* Move pntrs to upper box    */
        p2 = p1;
        p3 = p4;
        p1--;
        p4--;
        i--;
        if (lwk[l3] > 0 && lwk[l4] > 0 && lwk[l1] > 0)
        {
            /* Handle col point           */
            if (pathln(rr[p1], rr[p2], rr[p3], rr[p4]) > 0) goto side1;
            else goto side3;
        }

        if (lwk[l3] > 0) goto side3; /* Find exit point            */
        if (lwk[l4] > 0) goto side4;
        if (lwk[l1] > 0) goto side1;

        done:

        if ((i == isave) && (j == jsave))
        {
            /* Closed contour?            */
            fwk[xy] = fwk[fwkmid]; /* Close it off               */
            fwk[xy + 1] = fwk[fwkmid + 1];
            xy += dr;
        }

        if (dr < 0)
            xystrt = xy + 2;
        else
            xyend = xy - 2; /* Set final buffer pntrs   */
        return;
    }

/* Calculate shortest combined path length through a col point.
   Return true if shortest path is side 1/2,3/4, else false.         */

    int pathln(double p1, double p2, double p3, double p4)
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

    int gxcspl(bool frombuf, gxcntr pcntr)
    {
/* This subroutine does the curve fitting for the GX contouring
   package.  The X,Y point pairs have been placed in a floating
   point buffer.   fwk[xystrt] points to the start of the points in
   the buffer (namely, the first X,Y pair in the buffer).   fwk[xyend]
   points to end of the buffer -- the last X in the line (ie, the
   start of the last X,Y pair).

   To handle the end point conditions more easily, there must be
   enough room left in the buffer to hold one X,Y pair before
   the start of the line, and one X,Y pair after the end.

   The points are fitted with a psuedo cubic spline curve.
   (The slopes are arbitrarily chosen).  An intermediate point is
   output every del grid units.  */


        gxclbuf pclbuf = new gxclbuf();
        int x0, x1, x2, x3, y0, y1, y2, y3;
        double sx1, sx2, sy1, sy2, d0, d1, d2, xt1, xt2, yt1, yt2;
        double t, t2, t3, tint, x, y, ax, bx, ay, by;
        double del, kurv, cmax, dacum, dcmin, c1, c2;
        double mslope, tslope, xlb = 0.0, ylb = 0.0;
        int i, icls, nump = 0, labflg;
        int xy;
        long sz;

        if (pcntr.ltype == 2) dacum = 2.0;
        else dacum = 1.0; /* Accumulated length between labels */
        dcmin = ldmin; /* Minimum distance between labels                */
        labflg = 0; /* Contour not labelled yet   */
        mslope = 1000.0; /* Minimum slope of contour line */

        del = 0.05; /* Iteration distance         */
        if (frombuf) del = 0.02;
        kurv = 0.5; /* Curviness (0 to 1)         */
        cmax = 0.7; /* Limit curviness            */

        icls = 0; /* Is it a closed contour?    */
        if (fwk[xystrt] == fwk[xyend] && fwk[xystrt + 1] == fwk[xyend + 1]) icls = 1;

/* Convert contour coordinates to plotting inches.  We will do
   our spline fit and assign labels in this lower level coordinate
   space to insure readability.    */

        if (!frombuf)
        {
            nump = xyend - xystrt;
            nump = (nump + 2) / 2;
            var result = _drawingContext.GradsDrawingInterface.gxcord(fwk.Skip(xystrt).ToArray(), nump, 3);
            for (int k = 0; k < result.Length; k++)
            {
                fwk[xystrt + k] = result[k];
            }
        }


/* If uMath.Sing label masking, buffer the lines, and output the labels. */

        if (pcntr.mask > 0 && !frombuf)
        {
            /* Allocate and chain a clbuf */

            pclbuf = new gxclbuf();
            if (clbufanch == null)
                clbufanch = new List<gxclbuf>();
            clbufanch.Add(pclbuf);

            /* Allocate space for the line points */

            pclbuf.len = nump;
            sz = nump * 2;
            pclbuf.lxy = new double[sz];

            /* Copy the line points and line info */

            for (i = 0; i < nump * 2; i++) pclbuf.lxy[i] = fwk[xystrt + i];
            pclbuf.color = _drawingContext.GradsDrawingInterface.CurrentLineColor;
            pclbuf.style = _drawingContext.GradsDrawingInterface.lstyle;
            pclbuf.width = _drawingContext.GradsDrawingInterface.CurrentLineWidth;
            pclbuf.sfit = pcntr.spline;
            pclbuf.val = pcntr.val;

            /* Plot labels and set mask */

            if (pcntr.ltype > 0 && !String.IsNullOrEmpty(clabel))
            {
                for (xy = xystrt + 2; xy < xyend - 1; xy += 2)
                {
                    dacum += GaUtil.hypot(fwk[xy] - fwk[xy - 2], fwk[xy + 1] - fwk[xy - 1]);
                    /* c1 = (thisY - prevY) * (nextY - thisY) */
                    if (GaUtil.dequal(fwk[xy + 1], fwk[xy - 1], 1e-12) == 0 ||
                        GaUtil.dequal(fwk[xy + 3], fwk[xy + 1], 1e-12) == 0)
                        c1 = 0.0;
                    else
                        c1 = (fwk[xy + 1] - fwk[xy - 1]) * (fwk[xy + 3] - fwk[xy + 1]);
                    /* c2 = abs(thisY - prevY) + abs(nextY - thisY) */
                    if (GaUtil.dequal(fwk[xy + 1], fwk[xy - 1], 1e-12) == 0)
                    {
                        if (GaUtil.dequal(fwk[xy + 3], fwk[xy + 1], 1e-12) == 0)
                        {
                            /* thisY = prevY = nextY. Check if true for X coords too */
                            if (GaUtil.dequal(fwk[xy], fwk[xy - 2], 1e-12) == 0 &&
                                GaUtil.dequal(fwk[xy + 2], fwk[xy], 1e-12) == 0)
                            {
                                /* Duplicate point. Set c2 artificially high so label is not drawn */
                                c2 = 99;
                            }
                            else
                                c2 = 0.0;
                        }
                        else
                            c2 = Math.Abs(fwk[xy + 3] - fwk[xy + 1]);
                    }
                    else
                    {
                        if (GaUtil.dequal(fwk[xy + 3], fwk[xy + 1], 1e-12) == 0)
                            c2 = Math.Abs(fwk[xy + 1] - fwk[xy - 1]);
                        else
                            c2 = Math.Abs(fwk[xy + 1] - fwk[xy - 1]) + Math.Abs(fwk[xy + 3] - fwk[xy + 1]);
                    }

                    /* Plot the label...
                   if slope is zero or has changed sign,
                   if contour doesn't bend too much,
                   and if not too close to another label */
                    if (c1 <= 0.0 && c2 < 0.02 && dacum > dcmin)
                    {
                        if (gxqclab(fwk[xy], fwk[xy + 1], pcntr.labsiz) == 0)
                        {
                            if (pcntr.shpflg == 0) gxpclab(fwk[xy], fwk[xy + 1], 0.0, _drawingContext.GradsDrawingInterface.CurrentLineColor, pcntr);
                            dacum = 0.0;
                        }
                    }
                }

                dacum += GaUtil.hypot(fwk[xyend] - fwk[xyend - 2], fwk[xyend + 1] - fwk[xyend - 1]);
                /* for closed contours, check the joining point */
                if (icls > 0)
                {
                    /* c1 = (endY - secondY) * (prevY - endY) */
                    if (GaUtil.dequal(fwk[xyend + 1], fwk[xystrt + 3], 1e-12) == 0 ||
                        GaUtil.dequal(fwk[xyend - 1], fwk[xyend + 1], 1e-12) == 0)
                        c1 = 0.0;
                    else
                        c1 = (fwk[xyend + 1] - fwk[xystrt + 3]) * (fwk[xyend - 1] - fwk[xyend + 1]);
                    /* c2 = abs(endY - prevY) + abs(secondY - endY) */
                    if (GaUtil.dequal(fwk[xyend + 1], fwk[xyend - 1], 1e-12) == 0)
                    {
                        if (GaUtil.dequal(fwk[xystrt + 3], fwk[xyend + 1], 1e-12) == 0)
                        {
                            /* thisY = prevY = nextY
                               Duplicate point. Set c2 artificially high so label is not drawn */
                            c2 = 99;
                        }
                        else
                            c2 = Math.Abs(fwk[xystrt + 3] - fwk[xyend + 1]);
                    }
                    else
                    {
                        if (GaUtil.dequal(fwk[xystrt + 3], fwk[xyend + 1], 1e-12) == 0)
                            c2 = Math.Abs(fwk[xyend + 1] - fwk[xyend - 1]);
                        else
                            c2 = Math.Abs(fwk[xyend + 1] - fwk[xyend - 1]) + Math.Abs(fwk[xystrt + 3] - fwk[xyend + 1]);
                    }

                    /* same criteria apply as for non-closed contours */
                    if (c1 <= 0.0 && c2 < 0.02 && dacum > dcmin)
                    {
                        if (gxqclab(fwk[xy], fwk[xy + 1], pcntr.labsiz) == 0)
                        {
                            if (pcntr.shpflg == 0) gxpclab(fwk[xyend], fwk[xyend + 1], 0.0, _drawingContext.GradsDrawingInterface.CurrentLineColor, pcntr);
                        }
                    }
                }
            }

            return (0);
        }

/* If specified, do not do the cubic spline fit, just output the
   contour sides, determine label locations, and return.        */

        if (pcntr.spline == 0)
        {
            if (pcntr.mask == 1) _drawingContext.GradsDrawingInterface.gxplot(fwk[xystrt], fwk[xystrt + 1], 3);
            else _drawingContext.GradsDrawingInterface.gxplot(fwk[xystrt], fwk[xystrt + 1], 3);
            for (xy = xystrt + 2; xy < xyend - 1; xy += 2)
            {
                _drawingContext.GradsDrawingInterface.gxplot(fwk[xy], fwk[xy + 1], 2);
                if (!frombuf)
                {
                    dacum += GaUtil.hypot(fwk[xy] - fwk[xy - 2], fwk[xy + 1] - fwk[xy - 1]);
                    if ((fwk[xy + 1] - fwk[xy - 1]) * (fwk[xy + 3] - fwk[xy + 1]) < 0.0 &&
                        gxlabn < LABMAX && dacum > dcmin)
                    {
                        if (!String.IsNullOrEmpty(clabel))
                        {
                            gxlabx[gxlabn] = (fwk[xy]);
                            gxlaby[gxlabn] = (fwk[xy + 1]);
                            gxlabs[gxlabn] = 0.0;
                            gxlabv[gxlabn] = clabel;
                            gxlabc[gxlabn] = _drawingContext.GradsDrawingInterface.CurrentLineColor;
                            gxlabn++;
                        }

                        dacum = 0.0;
                    }
                }
            }

            _drawingContext.GradsDrawingInterface.gxplot(fwk[xyend], fwk[xyend + 1], 2);
            if (!frombuf)
            {
                dacum += GaUtil.hypot(fwk[xyend] - fwk[xyend - 2], fwk[xyend + 1] - fwk[xyend - 1]);
                if (icls > 0)
                {
                    if ((fwk[xyend + 1] - fwk[xystrt + 3]) * (fwk[xyend - 1] - fwk[xyend + 1]) < 0.0 &&
                        gxlabn < LABMAX && dacum > dcmin)
                    {
                        if (!String.IsNullOrEmpty(clabel))
                        {
                            gxlabx[gxlabn] = (fwk[xyend]);
                            gxlaby[gxlabn] = (fwk[xyend + 1]);
                            gxlabs[gxlabn] = 0.0;
                            gxlabv[gxlabn] = clabel;
                            gxlabc[gxlabn] = _drawingContext.GradsDrawingInterface.CurrentLineColor;
                            gxlabn++;
                        }
                    }
                }
            }

            return (0);
        }

/*  We handle end points by assigning a shadow point just beyond
    the start and end of the line.  This is a bit tricky Math.Since we
    have to make sure we ignore any points that are too close
    together.  If the contour is open, we extend the line straigth
    out one more increment.  If the contour is closed, we extend the
    line by wrapping it to the other end.  This ensures that a
    closed contour will have a smooth curve fit at our artificial
    boundry.                                                         */

        x3 = xyend;
        y3 = xyend + 1; /* Point to last X,Y          */
        x2 = x3;
        y2 = y3;
        do
        {
            /* Loop to find prior point   */
            x2 -= 2;
            y2 -= 2; /* Check next prior point     */
            if (x2 < xystrt) goto exit; /* No valid line to draw      */
            d2 = GaUtil.hypot((fwk[x3] - fwk[x2]), (fwk[y3] - fwk[y2])); /* Get distance               */
        } while (d2 < 0.01); /* Loop til distance is big   */

        x0 = xystrt;
        y0 = xystrt + 1; /* Point to first X,Y         */
        x1 = x0;
        y1 = y0;
        do
        {
            /* Loop to find next point    */
            x1 += 2;
            y1 += 2; /* Point to next X,Y          */
            if (x1 > xyend) goto exit; /* Exit if no valid line      */
            d1 = GaUtil.hypot((fwk[x1] - fwk[x0]), (fwk[y1] - fwk[y0])); /* Distance to next point     */
        } while (d1 < 0.01); /* Keep looping til d1 is big */

        if (icls > 0)
        {
            /* Select shadow points       */
            fwk[xystrt - 2] = fwk[x2]; /* Wrap for closed contour    */
            fwk[xystrt - 1] = fwk[y2];
            fwk[xyend + 2] = fwk[x1];
            fwk[xyend + 3] = fwk[y1];
        }
        else
        {
            fwk[xystrt - 2] = fwk[x0] + (fwk[x0] - fwk[x1]); /* Linear for open contour    */
            fwk[xystrt - 1] = fwk[y0] + (fwk[y0] - fwk[y1]);
            fwk[xyend + 2] = fwk[x3] + (fwk[x3] - fwk[x2]);
            fwk[xyend + 3] = fwk[y3] + (fwk[y3] - fwk[y2]);
        }
/* We have extended the line on either end.  We can now loop through
   the points in the line.  First set up the loop.                   */

        x2 = x1;
        x1 = x0;
        x0 = xystrt - 2;
        x3 = x2; /* Init pointers to coords    */
        y2 = y1;
        y1 = y0;
        y0 = xystrt - 1;
        y3 = y2;

        d0 = GaUtil.hypot((fwk[x1] - fwk[x0]), (fwk[y1] - fwk[y0])); /* Init distances             */
        d1 = GaUtil.hypot((fwk[x2] - fwk[x1]), (fwk[y2] - fwk[y1]));

        xt1 = (fwk[x1] - fwk[x0]) / d0 + (fwk[x2] - fwk[x1]) / d1; /* Partial slope calculation  */
        yt1 = (fwk[y1] - fwk[y0]) / d0 + (fwk[y2] - fwk[y1]) / d1;
        xt1 *= kurv; /* Curviness factor           */
        yt1 *= kurv;

        _drawingContext.GradsDrawingInterface.gxplot(fwk[x1], fwk[y1], 3); /* Start output with pen up   */

/* Loop through the various points in the line                       */

        x3 += 2;
        y3 += 2;
        while (x3 < xyend + 3)
        {
            /* Loop to end of the line    */

            d2 = GaUtil.hypot((fwk[x3] - fwk[x2]), (fwk[y3] - fwk[y2])); /* Distance to next point     */
            while (d2 < 0.01 && x3 < xyend + 3)
            {
                /* Skip points too close      */
                x3 += 2;
                y3 += 2; /* Check next point           */
                d2 = GaUtil.hypot((fwk[x3] - fwk[x2]), (fwk[y3] - fwk[y2])); /* Distance to next point     */
            }

            if (x3 >= xyend + 3) break; /* Went too far?              */

            if (!frombuf)
            {
                dacum += d1; /* Total dist. from last labl */

                if (pcntr.ltype > 0 && ((fwk[y2] - fwk[y1]) * (fwk[y3] - fwk[y2]) < 0.0) && gxlabn < LABMAX &&
                    dacum > dcmin)
                {
                    if (String.IsNullOrEmpty(clabel))
                    {
                        gxlabx[gxlabn] = (fwk[x2]);
                        gxlaby[gxlabn] = (fwk[y2]);
                        gxlabs[gxlabn] = 0.0;
                        gxlabv[gxlabn] = clabel;
                        gxlabc[gxlabn] = _drawingContext.GradsDrawingInterface.CurrentLineColor;
                        gxlabn++;
                        labflg = 1;
                    }

                    dacum = 0.0;
                }

                if (pcntr.ltype == 2 && labflg == 0 && gxlabn < LABMAX && x2 != xyend)
                {
                    if (fwk[x1] < fwk[x2] && fwk[x2] < fwk[x3])
                    {
                        tslope = Math.Atan2(fwk[y3] - fwk[y1], fwk[x3] - fwk[x1]);
                        if (Math.Abs(mslope) > Math.Abs(tslope))
                        {
                            mslope = tslope;
                            xlb = fwk[x2];
                            ylb = fwk[y2];
                        }
                    }
                    else if (fwk[x1] > fwk[x2] && fwk[x2] > fwk[x3])
                    {
                        tslope = Math.Atan2(fwk[y1] - fwk[y3], fwk[x1] - fwk[x3]);
                        if (Math.Abs(mslope) > Math.Abs(tslope))
                        {
                            mslope = tslope;
                            xlb = fwk[x2];
                            ylb = fwk[y2];
                        }
                    }
                }
            }

            xt2 = (fwk[x2] - fwk[x1]) / d1 + (fwk[x3] - fwk[x2]) / d2; /* Partial slope calculation  */
            yt2 = (fwk[y2] - fwk[y1]) / d1 + (fwk[y3] - fwk[y2]) / d2;
            xt2 *= kurv; /* Curviness factor           */
            yt2 *= kurv;

            if (d1 > cmax) t = cmax;
            else t = d1; /* Limit curviness            */
            sx1 = xt1 * t; /* Calculate slopes           */
            sx2 = xt2 * t;
            sy1 = yt1 * t;
            sy2 = yt2 * t;

            /* Calculate Cubic Coeffic.   */
            ax = sx1 + sx2 + 2.0 * (fwk[x1]) - 2.0 * (fwk[x2]);
            bx = 3.0 * (fwk[x2]) - sx2 - 2.0 * sx1 - 3.0 * (fwk[x1]);
            ay = sy1 + sy2 + 2.0 * (fwk[y1]) - 2.0 * (fwk[y2]);
            by = 3.0 * (fwk[y2]) - sy2 - 2.0 * sy1 - 3.0 * (fwk[y1]);

            tint = del / d1; /* How much to increment      */

            for (t = 0.0; t < 1.0; t += tint)
            {
                /* Increment this segment     */
                t2 = t * t;
                t3 = t2 * t; /* Get square and cube        */
                x = ax * t3 + bx * t2 + sx1 * t + fwk[x1]; /* Get x value on curve       */
                y = ay * t3 + by * t2 + sy1 * t + fwk[y1]; /* Get y value on curve       */
                _drawingContext.GradsDrawingInterface.gxplot(x, y, 2); /* Output the point           */
            }

            d0 = d1;
            d1 = d2;
            xt1 = xt2;
            yt1 = yt2; /* Carry calcs forward        */
            x0 = x1;
            x1 = x2;
            x2 = x3; /* Update pointers            */
            y0 = y1;
            y1 = y2;
            y2 = y3;
            x3 += 2;
            y3 += 2;
        }

        _drawingContext.GradsDrawingInterface.gxplot(fwk[xyend], fwk[xyend + 1], 2); /* Last point                 */

        if (!frombuf && pcntr.ltype == 2 && labflg == 0 && gxlabn < LABMAX && Math.Abs(mslope) < 2.0)
        {
            if (!String.IsNullOrEmpty(clabel))
            {
                gxlabx[gxlabn] = xlb;
                gxlaby[gxlabn] = ylb;
                gxlabs[gxlabn] = mslope;
                gxlabv[gxlabn] = clabel;
                gxlabc[gxlabn] = _drawingContext.GradsDrawingInterface.CurrentLineColor;
                gxlabn++;
            }
        }

        exit: /* No line here, just exit    */
        return (0);
    }

/* When label masking is not in use, this routine gets called after the 
   contour lines are drawn to plot the labels.  A rectangle with the background
   color is drawn before the label to blank beneath the label.  If label
   masking is in use, this routine should not be called.  */

    public void gxclab(double csize, bool flag, int colflg)
    {
        double x, y, xd1, xd2, yd1, yd2, w, h, buff;
        double[] xy = new double[10];
        int i, lablen, colr, bcol, fcol;

        if (!flag)
        {
            gxlabn = 0;
            return;
        }

        colr = _drawingContext.GradsDrawingInterface.CurrentLineColor;
        for (i = 0; i < gxlabn; i++)
        {
            lablen = gxlabv[i].Length;
            bcol = _drawingContext.GradsDatabase.gxdbkq();
            if (bcol < 2)
                bcol = 0; /* If bcol is neither black nor white, leave it alone. Otherwise, set to 0 for 'background' */
            h = csize * 1.2; /* set label height */
            w = 0.2;
            _drawingContext.GxChpl.gxchln(gxlabv[i], lablen, csize, out w); /* get label width */
            if (gxlabs[i] == 0.0)
            {
                /* contour label is not rotated */
                x = gxlabx[i] - (w / 2.0); /* adjust reference point */
                y = gxlaby[i] - (h / 2.0);
                _drawingContext.GradsDrawingInterface.SetDrawingColor(bcol);
                buff = h * 0.2; /* add a buffer above and below the string, already padded in X */
                _drawingContext.GradsDrawingInterface.DrawFilledRectangle(x, x + w, y - buff, y + h + buff); /* draw the background rectangle,  */
                if (colflg > -1) fcol = colflg;
                else fcol = gxlabc[i];
                if (fcol == bcol)
                    _drawingContext.GradsDrawingInterface.SetDrawingColor(1); /* if label color is same as background, use foreground */
                else _drawingContext.GradsDrawingInterface.SetDrawingColor(fcol);
                _drawingContext.GradsDrawingInterface.gxchpl(gxlabv[i], lablen, x, y, h, csize, 0.0); /* draw the label */
            }
            else
            {
                /* contour label is rotated */
                xd1 = (h / 2.0) * Math.Sin(gxlabs[i]);
                xd2 = (w / 2.0) * Math.Cos(gxlabs[i]);
                yd1 = (h / 2.0) * Math.Cos(gxlabs[i]);
                yd2 = (w / 2.0) * Math.Sin(gxlabs[i]);
                x = gxlabx[i] - xd2 + xd1; /* adjust reference point */
                y = gxlaby[i] - yd2 - yd1;
                xd1 = (h / 2.0 * 1.6) * Math.Sin(gxlabs[i]);
                xd2 = 1.1 * (w / 2.0) * Math.Cos(gxlabs[i]);
                yd1 = (h / 2.0 * 1.6) * Math.Cos(gxlabs[i]);
                yd2 = 1.1 * (w / 2.0) * Math.Sin(gxlabs[i]);
                xy[0] = gxlabx[i] - xd2 + xd1; /* rotated background rectangle => polygon */
                xy[1] = gxlaby[i] - yd2 - yd1;
                xy[2] = gxlabx[i] - xd2 - xd1;
                xy[3] = gxlaby[i] - yd2 + yd1;
                xy[4] = gxlabx[i] + xd2 - xd1;
                xy[5] = gxlaby[i] + yd2 + yd1;
                xy[6] = gxlabx[i] + xd2 + xd1;
                xy[7] = gxlaby[i] + yd2 - yd1;
                xy[8] = xy[0];
                xy[9] = xy[1];
                _drawingContext.GradsDrawingInterface.SetDrawingColor(bcol);
                _drawingContext.GradsDrawingInterface.gxfill(xy, 5);
                if (colflg > -1) fcol = colflg;
                else fcol = gxlabc[i];
                if (fcol == bcol)
                    _drawingContext.GradsDrawingInterface.SetDrawingColor(1); /* if label color is same as background, use foreground */
                else _drawingContext.GradsDrawingInterface.SetDrawingColor(fcol);
                _drawingContext.GradsDrawingInterface.gxchpl(gxlabv[i], lablen, x, y, h, csize,
                    gxlabs[i] * 180 / Math.PI); /* draw the label */
            }
        }

        _drawingContext.GradsDrawingInterface.SetDrawingColor(colr);
        gxlabn = 0;
    }

/* When label masking is in use, this routine is called to plot all the 
   contour lines after the contour labels have been plotted and their masking
   regions set.  Thus the contour lines drawn will not overlay the labels.  */

    public void gxpclin()
    {
        int i, rc;
        gxclbuf p2;
        gxcntr lcntr = new();

        /* Set up gxcntr struct appropriately -- most values are dummy */
        lcntr.labsiz = 0.5;
        lcntr.ltype = 0;
        lcntr.mask = 1;
        lcntr.labcol = 1;
        lcntr.ccol = 1;
        lcntr.label = "";

        /* Copy the lines into fwk, dump the lines,
           release storage, return.  fwk should be guaranteed big enough for the
           largest line we have, and shouldn't have been release via gxcrel at
           this point. */
        if (clbufanch != null)
        {
            foreach (gxclbuf pclbuf in clbufanch)
            {
                if (pclbuf.lxy != null)
                {
                    xystrt = 2;
                    xyend = xystrt + 2 * (pclbuf.len - 1);
                    for (i = 0; i < 2 * pclbuf.len; i++) fwk[xystrt + i] = pclbuf.lxy[i];
                    _drawingContext.GradsDrawingInterface.SetDrawingColor(pclbuf.color);
                    _drawingContext.GradsDrawingInterface.gxstyl(pclbuf.style);
                    _drawingContext.GradsDrawingInterface.gxwide(pclbuf.width);
                    lcntr.spline = pclbuf.sfit;
                    rc = gxcspl(true, lcntr);
                }
            }
        }
        

        clbufanch = null;
        return;
    }

/* When gxout shape is in use, this routine is called to dump all the 
   contour vertices to the shapefile
   For each contour in the buffer: 
     get the vertex x/y coordinates, 
     convert them to lon/lat, 
     write out the coordinates to the shapefile,
     release storage and return. 
   Returns -1 on error, otherwise returns number of shapes written to file.
*/
// #if USESHP == 1
// int gxshplin (SHPHandle sfid, DBFHandle dbfid, struct dbfld *dbanch) {
// int i,rc,ival;
// struct dbfld *fld;
// struct gxclbuf *pclbuf=NULL, rr[p2];
// int shpid,*pstart=NULL,nParts,nFields;
// SHPObject *shp;
// double x,y,*lons=NULL,*lats=NULL,*vals=NULL,lon,lat,val,dval;
//  
//  nParts = 1;
//  nFields = 1;
//  pstart = (int*)galloc(nParts*sizeof(int),"pstart");
//  *pstart = 0;
//  shpid=0;
//  pclbuf = clbufanch;
//  if (pclbuf==NULL) {
//    printf("Error in gxshplin: contour buffer is empty\n");
//    rc = -1; 
//    goto cleanup;
//  }
//  while (pclbuf) { 
//    if (pclbuf.lxy) {
//      /* allocate memory for lons and lats of the vertices in contour line */
//      if ((lons = (double*)galloc (pclbuf.len*sizeof(double),"shplons"))==NULL) {
//        printf("Error in gxshplin: unable to allocate memory for lon array\n");
//        rc = -1;
//        goto cleanup;
//      }
//      if ((lats = (double*)galloc (pclbuf.len*sizeof(double),"shplats"))==NULL) {
//        printf("Error in gxshplin: unable to allocate memory for lat array\n");
//        rc = -1;
//        goto cleanup;
//      }
//      if ((vals = (double*)galloc (pclbuf.len*sizeof(double),"shpvals"))==NULL) {
//        printf("Error in gxshplin: unable to allocate memory for val array\n");
//        rc = -1;
//        goto cleanup;
//      }
//      /* get x,y values and convert them to lon,lat */
//      for (i=0; i<pclbuf.len; i++) {
//        x = *(pclbuf.lxy+(2*i)); 
//        y = *(pclbuf.lxy+(2*i+1)); 
//        gxxy2w (x,y,&lon,&lat);
//        *(lons+i) = lon;
//        *(lats+i) = lat;
//        *(vals+i) = pclbuf.val;
//      }
//      /* create the shape, write it out, then release it */
//      shp = SHPCreateObject (SHPT_ARCM,shpid,nParts,pstart,NULL,pclbuf.len,lons,lats,NULL,vals);
//      i = SHPWriteObject(sfid,-1,shp);
//      SHPDestroyObject(shp);
//      if (i!=shpid) {
//        printf("Error in gxshplin: SHPWriteObject returned %d, shpid=%d\n",i,shpid);
//        rc = -1;
//        goto cleanup;
//      }
//      gree(lons,"c10"); lons=NULL;
//      gree(lats,"c11"); lats=NULL;
//      gree(vals,"c12"); vals=NULL;
//      /* write out the attribute fields for this shape */
//      fld = dbanch;           /* point to the first one */
//      while (fld != NULL) {
//        if (fld.flag==0) {   /* static fields */
//      if (fld.type==FTString) {
//        DBFWriteStringAttribute (dbfid,shpid,fld.index,(const char *)fld.value);
//      } else if (fld.type==FTInteger) {
//        intprs(fld.value,&ival);
//        DBFWriteIntegerAttribute (dbfid,shpid,fld.index,ival);
//      } else if (fld.type==FTDouble) {
//        getdbl(fld.value,&dval);
//        DBFWriteDoubleAttribute (dbfid,shpid,fld.index,dval);
//      }
//        }
//        else {                /* dynamic fields */
//      if (strcmp(fld.name,"CNTR_VALUE")==0) {
//        val = pclbuf.val;
//        DBFWriteDoubleAttribute (dbfid,shpid,fld.index,val);
//      }
//        }
//        fld = fld.next;      /* advance to next field */
//      }
//      shpid++;
//    }
//    pclbuf = pclbuf.fpclbuf;
//  }
//  /* if no errors, return the number of contour lines written to the file */
//  rc = shpid;
//  
//  cleanup:
//  if (lons) gree (lons,"c7");
//  if (lats) gree (lats,"c8");
//  if (vals) gree (vals,"c8");
//  if (pstart) gree (pstart,"c9");
//  /* release the memory in the contour buffer */
//  pclbuf = clbufanch;
//  while (pclbuf) {
//    p2 = pclbuf.fpclbuf;
//    if (pclbuf.lxy) gree (pclbuf.lxy,"c5");
//    gree (pclbuf,"c6");
//    pclbuf = p2;
//  }
//  clbufanch = NULL;
//  clbuflast = NULL;
//  return (rc);
// }
// #endif

/* Routine to write out contour line vertices to a KML file. 
   For each contour in the buffer: 
     get the vertex x/y coordinates, 
     convert them to lon/lat, 
     write out the coordinates to the kmlfile,
     release storage and return. 
   Returns -1 on error, otherwise the number of contours written. 
*/
    public int gxclvert(FileStream kmlfp)
    {
        gxclbuf p2;
        double lon = 0, lat = 0, x, y;
        int i, j, c, err;
        err = 0;
        c = 0;
        foreach(var pclbuf in clbufanch) 
        {
            if (pclbuf.lxy != null) {
                kmlfp.WriteLine("    <Placemark>");
                kmlfp.WriteLine($"      <styleUrl>#{pclbuf.color}</styleUrl>");
                kmlfp.WriteLine($"      <name>{pclbuf.val}</name>");
                kmlfp.WriteLine("      <LineString>");
                kmlfp.WriteLine("        <altitudeMode>clampToGround</altitudeMode>");
                kmlfp.WriteLine("        <tessellate>1</tessellate>");
                kmlfp.WriteLine("        <coordinates>");

                j = 1;
                for (i = 0; i < pclbuf.len; i++)
                {
                    x = pclbuf.lxy[2 * i];
                    y = pclbuf.lxy[2 * i + 1];
                    _drawingContext.GradsDrawingInterface.gxxy2w(x, y, out lon, out lat);
                    if (lat > 90) lat = 90;
                    if (lat < -90) lat = -90;
                    var coords = String.Format(CultureInfo.InvariantCulture, "{0},{1},0 ", lon, lat);
                    
                    kmlfp.Write(coords);

                    if (j == 6 || i == (pclbuf.len - 1))
                    {
                        if (j == 6) kmlfp.Write("\n          ");
                        else kmlfp.Write("\n");
                        j = 0;
                    }
                    j++;
                }

                kmlfp.WriteLine("        </coordinates>");
                kmlfp.WriteLine("      </LineString>");
                kmlfp.WriteLine("    </Placemark>");
                c++;
            }
            
        }
        cleanup:
        /* release the memory in the contour buffer */
        
        clbufanch = null;
        if (err>0) return (-1);
        else return (c);
    }


/* Plot contour labels when label masking is in use.
   Currently, rot is assumed to be zero.  */

    void gxpclab(double xpos, double ypos, double rot, int ccol, gxcntr pcntr)
    {
        double x, y, w, h, csize, buff;
        int lablen, bcol, fcol, scol, swid;

        csize = pcntr.labsiz;
        lablen = clabel.Length;
        bcol = _drawingContext.GradsDatabase.gxdbkq();
        if (bcol < 2)
            bcol = 0; /* If bcol is neither black nor white, leave it alone. Otherwise, set to 0 for 'background' */
        h = csize * 1.2; /* set label height */
        w = 0.2;
        _drawingContext.GxChpl.gxchln(clabel, lablen, csize, out w); /* get label width */
        buff = h * 0.05; /* set a small buffer around the label */
        x = xpos - (w / 2.0); /* adjust reference point */
        y = ypos - (h / 2.0);
        scol = _drawingContext.GradsDrawingInterface.CurrentLineColor;
        if (pcntr.labcol > -1) fcol = pcntr.labcol;
        else fcol = ccol;
        _drawingContext.GradsDrawingInterface.SetDrawingColor(fcol);
        swid = _drawingContext.GradsDrawingInterface.CurrentLineWidth;
        /* if contour label thickness is set to -999, then we draw a fat version of the label
           in the background color and then overlay a thin version of the label in desired color.
           This will only work with hershey fonts, Math.Since the boldness of cairo fonts is not
           controlled by the thickness setting for contour labels. */
        if (pcntr.labwid > -1) _drawingContext.GradsDrawingInterface.gxwide(pcntr.labwid);
        if (pcntr.labwid == -999)
        {
            /* invoke settings for fat background label */
            _drawingContext.GradsDrawingInterface.SetDrawingColor(12);
            _drawingContext.GradsDrawingInterface.SetDrawingColor(bcol);
        }

        /* draw the label */
        _drawingContext.GradsDrawingInterface.gxchpl(clabel, lablen, x, y, h, csize, 0.0);
        if (pcntr.labwid == -999)
        {
            /* overlay a thin label in foreground color */
            _drawingContext.GradsDrawingInterface.SetDrawingColor(1);
            _drawingContext.GradsDrawingInterface.SetDrawingColor(fcol);
            _drawingContext.GradsDrawingInterface.gxchpl(clabel, lablen, x, y, h, csize, 0.0);
        }

        /* update the mask where this label is positioned */
        _drawingContext.GradsDrawingInterface.gxmaskrec(x - buff, x + w + buff, y - buff, y + h + buff);

        _drawingContext.GradsDrawingInterface.SetDrawingColor(scol);
        _drawingContext.GradsDrawingInterface.gxwide(swid);
    }

/* query if the contour label will overlay another, if uMath.Sing masking */

    int gxqclab(double xpos, double ypos, double csize)
    {
        double lablen, x, y, w, h, buff;
        int rc;
        lablen = clabel.Length;
        w = 0.2;
        h = csize * 1.2; /* height scaled by 1.2 for consistency with other labels */
        _drawingContext.GxChpl.gxchln(clabel, (int)lablen, csize, out w);
        x = xpos - (w / 2.0);
        y = ypos - (h / 2.0);
        buff = h * 0.2; /* set a buffer around the label */
        /* area to check is a bit (0.02) smaller than the actual mask */
        rc = _drawingContext.GradsDrawingInterface.gxmaskrq(x - buff + 0.02, x + w + buff - 0.02, y - buff + 0.02,
            y + h + buff - 0.02);
        return (rc);
    }

/*  Release storage used by the contouring package  */

    public void gxcrel()
    {
        lwk = null;
        fwk = null;
        lwksiz = 0;
        fwksiz = 0;
    }
}