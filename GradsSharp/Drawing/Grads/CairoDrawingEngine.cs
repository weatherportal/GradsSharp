using Cairo;

namespace GradsSharp.Drawing.Grads;

internal class CairoDrawingEngine : IDrawingEngine
{
    private const int SX = 800;

    static int lcolor = -999, lwidth; /* Current attributes */
    static int actualcolor; /* current *actual* color */
    static int lcolorsave, lwidthsave; /* for save and restore */
    static int rsav, gsav, bsav, psav; /* Avoid excess color change calls */
    static int aaflg = 0; /* Anti-Aliasing flag */
    static int filflg = 0; /* Polygon-Filling flag */
    static int brdrflg = 0; /* Vector graphic border flag */
    static double brdrwid = 0.0; /* Vector graphic border size */
    static int rotate = 0; /* rotate landscape plots for hardcopy output */
    static int width, height; /* Drawable size */
    static int Xwidth, Xheight; /* Window size */
    static int Bwidth, Bheight; /* Batch surface size */
    static int force = 0; /* force a color change */
    static double xsize, ysize; /* GrADS page size (inches) */
    static double xscl, yscl; /* Window Scaling */

    static double xxx, yyy; /* Old position */

//static FT_Library library = NULL;             /* for drawing fonts with FreeType with cairo */
    static string?[] face = new string[100]; /* for saving FreeType font faces */
    static bool faceinit = false; /* for knowing whether font faces have been initialized */
    static Surface? surface = null; /* Surface being drawn to */
    static Context? cr; /* graphics context */

    static int surftype; /* Type of current surface.
                                               1=X, 2=Image, 3=PS/EPS, 4=PDF, 5=SVG */

    static Surface? Xsurface = null, Hsurface = null, Bsurface = null;

/* X, Hardcopy, and Batch mode surfaces */
/* X surface is passed to us by gxX. */
/* Others are created as needed. */
    static Context? Xcr, Hcr, Bcr; /* X, Hardcopy, and Batch mode contexts */
    static Pattern?[] pattern = new Pattern[2048]; /* Save our patterns here */
    static ImageSurface[] pattsurf = new ImageSurface[2048];
    static Surface surfmask, surfsave; /* Stuff for masking */
    static Context? crmask, crsave;
    static SurfacePattern? patternmask;
    static int drawing; /* In the middle of line-to's? */
    static int maskflag; /* Masking going on */
    static int batch = 0; /* Batch mode */
    static double clx, cly, clw, clh; /* current clipping coordinates */
    static double clxsav, clysav, clwsav, clhsav; /* saved clipping coordinates */

    private DrawingContext _drawingContext;

    public CairoDrawingEngine(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public void gxpcfg()
    {
    }

    public bool gxpckfont()
    {
        return true;
    }

    public void gxpbgn(double xsz, double ysz)
    {
        xsize = xsz;
        ysize = ysz;
    }

    public void gxpinit(double xsz, double ysz)
    {
        int dh, dw; /* image size in pixels */
        int i;

        batch = 1;
        if (!faceinit)
        {
            for (i = 0; i < 100; i++) face[i] = null;
            faceinit = true;
            //gxCftinit(); /* make sure FreeType library has been initialized */
        }

        /* set page sizes  in inches */
        xsize = xsz;
        ysize = ysz;

        /* set window dimensions; 100 times the page size or 1100 by 850 */
        if (xsize > ysize)
        {
            dw = 1100;
            dh = 850;
        }
        else
        {
            dw = 850;
            dh = 1100;
        }

        width = dw;
        height = dh;
        Bwidth = dw;
        Bheight = dh;

        /* scale factors (pixels per inch) */
        xscl = (double)(dw) / xsize;
        yscl = (double)(dh) / ysize;

        /* initialize clipping parameters */
        clx = 0;
        cly = 0;
        clw = width;
        clh = height;

        /* create the image surface */
        surface = new ImageSurface(Format.Argb32, dw, dh);
        surftype = 2;
        cr = new Context(surface);

        Bsurface = surface;
        Bcr = cr;
    }

    public void gxpend()
    {
        if (batch > 0)
        {
            /* batch mode */
            Bcr.Dispose();
            Bsurface.Finish();
            Bsurface.Dispose();
            Bcr = null;
            Bsurface = null;
        }
        else
        {
            /* the interactive session */
            // gxCflush(0);
            // cairo_destroy(cr);
            // cr = NULL;
        }
        /* close the FreeType library */
        //gxCftend();
    }

    public int gxprint(string fnout, int xin, int yin, int bwin, int fmtflg, string bgImage, string fgImage, int tcolor,
        double border)
    {
/* Make sure we don't try to print an unsupported format */
        if (fmtflg != 1 && fmtflg != 2 && fmtflg != 3 && fmtflg != 4 && fmtflg != 5) return (9);

        if (tcolor != -1)
            _drawingContext.GxDb.gxdbsettransclr(tcolor); /* tell graphics database about transparent color override */

        /* initialize the output for vector graphics or image */
        int rc = gxChinit(xsize, ysize, xin, yin, bwin, fnout, fmtflg, bgImage, border);
        if (rc > 0) return (rc);

        /* draw the contents of the metabuffer */
        _drawingContext.GxMeta.gxhdrw(0, 1);
        if (rc > 0) return (rc);

        /* finish up */
        rc = gxChend(fnout, fmtflg, fgImage);

        _drawingContext.GxDb.gxdbsettransclr(-1); /* unset transparent color override */
        return (rc);
    }


    int gxChend(string fname, int fmtflg, string fgImage)
    {
        int fgwidth, fgheight, len;
        Status status;
        Surface? sfc2 = null; /* Surface for fgImage */

        gxpflush(1); /* finish drawing */

        /* bgImage and fgImage only work when output format is PNG */
        if (fmtflg == 5)
        {
            if (!String.IsNullOrEmpty(fgImage))
            {
                /* Make sure fgImage is a .png  */
                if (!fgImage.ToLower().EndsWith(".png")) return 6;

                /* create a new surface from the foreground image */
                sfc2 = new ImageSurface(fgImage);
                status = sfc2.Status;
                if (status != Status.Success)
                {
                    GaGx.gaprnt(0, $"Error in gxChend: unable to import foreground image {fgImage}");
                    GaGx.gaprnt(0, $"Cairo status: {status}");
                    sfc2.Finish();
                    sfc2.Dispose();
                    return (8);
                }

                /* check to make sure foreground and output images are the same size */
                fgwidth = ((ImageSurface)sfc2).Width;
                fgheight = ((ImageSurface)sfc2).Height;
                if (fgwidth != width || fgheight != height)
                {
                    GaGx.gaprnt(0, $" foreground image size is {fgwidth} x {fgheight}");
                    GaGx.gaprnt(0, $"     output image size is {width} x {height} ");
                    sfc2.Finish();
                    sfc2.Dispose();
                    return (11);
                }

                /* draw the foreground image OVER the hardcopy surface */
                cr.Operator = Operator.Over;
                cr.SetSource(sfc2);
                cr.Paint();
                /* clean up */
                sfc2.Finish();
                sfc2.Dispose();
            }
        }

        /* Make sure surface was rendered without any problems */
        status = surface.Status;
        if (status != Status.Success)
        {
            GaGx.gaprnt(0, $"Error in gxChend: an error occured during rendering of hardcopy surface ");
            GaGx.gaprnt(0, $"Cairo status: {status}");
            surface.Finish();
            surface.Dispose();
            return (1);
        }

        /* dump everything to hardcopy output, finish and destroy the surface */
        //cairo_surface_show_page(surface);
        if (fmtflg == 5)
        {
            surface.WriteToPng(fname); /* for image (png) output only */
        }

        cr.Dispose();
        surface.Finish();
        surface.Dispose();
        rotate = 0; /* unset rotation for landscape hardcopy */
        brdrflg = 0; /* reset border flag for vector graphic output */
        brdrwid = 0.0; /* reset border width for vector graphic output */
        GxDb.gxdboutbck(-1); /* unset output background color */

        /* Reset everything back to the interactive/batch surface for drawing */
        Hsurface = null;
        if (batch > 0)
        {
            surface = Bsurface;
            cr = Bcr;
            width = Bwidth;
            height = Bheight;
        }
        else
        {
            surface = Xsurface;
            cr = Xcr;
            width = Xwidth;
            height = Xheight;
        }

        lcolor = lcolorsave;
        lwidth = lwidthsave;
        clx = clxsav;
        cly = clysav;
        clw = clwsav;
        clh = clhsav;
        xscl = (double)(width) / xsize;
        yscl = (double)(height) / ysize;

        return (0);
    }


    int gxChinit(double xsz, double ysz, int xin, int yin, int bwin,
        string fname, int fmtflg, string bgImage, double border)
    {
        Status? status;
        int dh = 0, dw = 0; /* image size in pixels */
        double psx = 0, psy = 0; /* page size in points */
        int bcol, i;
        int bgwidth, bgheight, len;

        if (!faceinit)
        {
            for (i = 0; i < 100; i++) face[i] = null;
            faceinit = true;
            //gxCftinit();         /* make sure FreeType library has been initialized */
        }

        gxpflush(1);
        drawing = 0;
        maskflag = 0;

        /* save these settings to restore when hardcopy drawing is finished */
        lcolorsave = lcolor;
        lwidthsave = lwidth;
        clxsav = clx;
        clysav = cly;
        clwsav = clw;
        clhsav = clh;

        /* set page sizes in inches */
        xsize = xsz;
        ysize = ysz;

        if (fmtflg == 5)
        {
            /* Image output */
            /*  Set the image width and height */
            if (xin < 0 || yin < 0)
            {
                /* user has not specified image size */
                if (xsize > ysize)
                {
                    /* landscape */
                    dw = SX;
                    dh = (int)((ysize / xsize) * SX);
                }
                else
                {
                    dw = (int)((xsize / ysize) * SX);
                    dh = SX;
                }
            }
            else
            {
                dw = xin;
                dh = yin;
            }

            /* image dimensions */
            width = dw;
            height = dh;
            /* scale factors (pixels per inch) */
            xscl = (double)(dw) / xsize;
            yscl = (double)(dh) / ysize;
        }

        if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3 || fmtflg == 4)
        {
            /* vector graphic output */
            /* set page size in points */
            if (ysize > xsize)
            {
                /* portrait: no rotation needed for any format */
                psx = xsize * 72.0;
                psy = ysize * 72.0;
            }
            else
            {
                /* landscape: ps, eps, and pdf must be rotated to fit on page */
                if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3)
                {
                    rotate = 1;
                    psx = ysize * 72.0;
                    psy = xsize * 72.0;
                }
                else
                {
                    /* landscape: svg doesn't need to be rotated */
                    psx = xsize * 72.0;
                    psy = ysize * 72.0;
                }
            }

            /* page dimensions */
            width = (int)psx;
            height = (int)psy;
            /* scale factors (points per inch) */
            xscl = 72.0;
            yscl = 72.0;
            /* set a border for ps, eps, and pdf formats */
            if (fmtflg <= 3)
            {
                brdrwid = border * xscl;
                brdrflg = 1;
            }
        }

        /* initialize clipping parameters */
        clx = 0;
        cly = 0;
        clw = width;
        clh = height;

        /* Create the hardcopy surface */
        if (fmtflg == 1 || fmtflg == 2)
        {
            /* PS or EPS */
            surftype = 3;
            surface = new PSSurface(fname, psx, psy);
            if (fmtflg == 1) throw new NotImplementedException();
        }
        else if (fmtflg == 3)
        {
            /* PDF */
            surftype = 4;
            surface = new PdfSurface(fname, psx, psy);
        }
        else if (fmtflg == 4)
        {
            /* SVG */
            surftype = 5;
            surface = new SvgSurface(fname, psx, psy);
        }
        else if (fmtflg == 5)
        {
            /* PNG */
            surftype = 2;

            if (!String.IsNullOrEmpty(bgImage))
            {
                /* Make sure bgImage is a .png  */
                if (!bgImage.ToLower().EndsWith(".png")) return 5;
                /* create a new surface from the background image */
                surface = new ImageSurface(bgImage);
                status = surface.Status;
                if (status != Status.Success)
                {
                    GaGx.gaprnt(0, $"Error in gxChinit: unable to import background image {bgImage}");
                    GaGx.gaprnt(0, $"Cairo status: {status}");
                    surface.Finish();
                    surface.Dispose();
                    return (7);
                }

                /* make sure background image size matches output image size */
                bgwidth = ((ImageSurface)surface).Width;
                bgheight = ((ImageSurface)surface).Height;
                if (bgwidth != width || bgheight != height)
                {
                    GaGx.gaprnt(0, $" background image size is {bgwidth} x {bgheight}");
                    GaGx.gaprnt(0, $"     output image size is {width} x {height}");
                    surface.Finish();
                    surface.Dispose();
                    return (10);
                }
            }
            else
            {
                /* no background image specified by user */
                surface = new ImageSurface(Format.Argb32, dw, dh);
            }
        }

        /* Make sure surface set up without any problems */
        status = surface.Status;
        if (status != Status.Success)
        {
            GaGx.gaprnt(0, "Error in gxChinit: failed to initialize hardcopy surface ");
            GaGx.gaprnt(0, $"Cairo status {status}");
            surface.Finish();
            surface.Dispose();
            return (1);
        }

        /* Set the Cairo context */
        cr = new Context(surface);
        Hsurface = surface;
        Hcr = cr;

        /* default drawing settings */
        cr.LineCap = LineCap.Round;
        cr.LineJoin = LineJoin.Round;
        gxCaa(1); /* initialize anti-aliasing to be ON */

        force = 1; /* force a color change */
        bcol = GxDb.gxdbkq();
        if (bcol > 1)
        {
            /* User has set the background to be a color other than black/white.
               The background rectangle is therefore in the metabuffer and will
               cover whatever we draw here to initalize the output */
            if (bwin > -900)
            {
                GaGx.gaprnt(0, $"Warning: Background color cannot be changed at print time  ");
                GaGx.gaprnt(0, $" if it has been set to a color other than black or white. ");
                GaGx.gaprnt(0, $" The current background color is {bcol}. ");
                if (bwin == 1)
                    GaGx.gaprnt(0, " The option \"white\" will be ignored. ");
                else
                    GaGx.gaprnt(0, " The option \"black\" will be ignored. ");
            }
        }
        else
        {
            if (bwin > -900)
                GxDb.gxdboutbck(bwin); /* change the background color if user provided an override */
            gxpcol(0); /* 0 here means 'background' */
            cr.Paint(); /* paint it */
        }

        gxpcol(1); /* set the initial color to 'foreground' */
        force = 0; /* back to unforced color changes */
        return (0);
    }


    /* Turn anti-aliasing on (flag=1) or off (flag=0) 
   Anti-aliasing is also automatically disabled in gxCrec and gxCfil so that
   faint lines do not appear around the edges of filled rectangles and polygons.
*/

    void gxCaa(int flag)
    {
        if (cr != null)
        {
            if (flag > 0)
            {
                if (aaflg == 0)
                {
                    cr.Antialias = Antialias.Default;
                    aaflg = 1;
                }
            }
            else
            {
                if (aaflg > 0)
                {
                    cr.Antialias = Antialias.None;
                    aaflg = 0;
                }
            }
        }
    }

    public void gxpcol(int clr)
    {
        int bcol;
        double wid;

        if (drawing > 0) cr.Stroke();
        drawing = 0;
        lcolor = clr; /* outside this routine lcolor 0/1 still means background/foreground */
        bcol = GxDb.gxdbkq(); /* get background color */
        if (bcol == 1)
        {
            /* if background is white ...  */
            if (clr == 0) clr = 1; /*  ...change color 0 to white (1) */
            else if (clr == 1) clr = 0; /*  ...change color 1 to black (0) */
        }

        var dbq = _drawingContext.GxDb.gxdbqcol(clr);
        if (clr == _drawingContext.GxDb.gxdbqtransclr())
        {
            /* If this is the transparent color, override values  */
            dbq.Item1 = 0;
            dbq.Item2 = 0;
            dbq.Item3 = 0;
            dbq.Item4 = 0;
            dbq.Item5 = -999;
        }

        if (force > 0 || clr != actualcolor || dbq.Item1 != rsav || dbq.Item2 != gsav || dbq.Item3 != bsav ||
            dbq.Item4 != psav)
        {
            /* change to new color */
            if (maskflag > 0) gxpflush(1);
            actualcolor = clr; /* inside this routine actualcolor 0/1 means black or white */
            if (dbq.Item5 > -900)
            {
                /* new color is a pattern */
                if (pattern[dbq.Item5] == null) gxCpattc(dbq.Item5);
                cr.SetSource(pattern[dbq.Item5]);
                pattern[dbq.Item5].Extend = Extend.Repeat;
            }
            else
            {
                if (dbq.Item4 == 255)
                {
                    cr.SetSourceRGB((double)(dbq.Item1) / 255.0, (double)(dbq.Item2) / 255.0,
                        (double)(dbq.Item3) / 255.0);
                }
                else
                {
                    /* set up color masking if alpha value is negative */
                    if (dbq.Item4 < 0)
                    {
                        maskflag = 1;
                        surfmask = new ImageSurface(Format.Argb32, width, height);
                        crmask = new Context(surfmask); /* create a new graphics context */
                        crmask.SetSourceRGBA(1.0, 1.0, 1.0, 1.0); /* set mask color */
                        wid = _drawingContext.GxDb.gxdbqwid(lwidth - 1); /* set current line width */
                        crmask.LineWidth = wid;
                        crmask.LineCap = LineCap.Round; /* set line drawing specs */
                        crmask.LineJoin = LineJoin.Round;
                        /* save the current surface/cr, set the new surfmask/crmask to be the current one */
                        crsave = cr;
                        surfsave = surface;
                        cr = crmask;
                        surface = surfmask;
                    }
                    else
                    {
                        cr.SetSourceRGBA((double)(dbq.Item1) / 255.0, (double)(dbq.Item2) / 255.0,
                            (double)(dbq.Item3) / 255.0, (double)(dbq.Item4) / 255.0);
                    }
                }
            }

            //Console.WriteLine($"Changed color to {dbq}");
            /* save these color values */
            rsav = dbq.Item1;
            gsav = dbq.Item2;
            bsav = dbq.Item3;
            psav = dbq.Item4;
        }
    }


    /* Create a tile pattern on the fly.  Get all the info on the 
   pattern from the backend db.   */

    void gxCpattc(int pnum)
    {
        Surface? surfacep1;
        Pattern? crpatt;
        double xx, yy, x1, y1;
        int pt, alph, status;

        if (pnum < 0 || pnum >= Gx.COLORMAX) return;

        //
        //
        // gxdbqpatt(pnum, &dbq);
        // gxdbqwid(dbq.pthick, &dbq);
        //
        // if (dbq.ptype == 0) {
        //     /* create tile from user-provided filename */
        //     surfacep1 = cairo_image_surface_create_from_png(dbq.fname);
        //     status = cairo_surface_status(surfacep1);
        //     if (status) {
        //         GaGx.gaprnt(0, ("Error in gxCpattc: unable to import tile image %s \n", dbq.fname);
        //         GaGx.gaprnt(0, ("Cairo status: %s\n", cairo_status_to_string(status));
        //         cairo_surface_finish(surfacep1);
        //         cairo_surface_destroy(surfacep1);
        //         return;
        //     }
        //     crpatt = cairo_create(surfacep1);
        // } else {
        //     /* create the tile on-the-fly */
        //     surfacep1 = cairo_image_surface_create(CAIRO_FORMAT_ARGB32, dbq.pxs, dbq.pys);
        //     crpatt = cairo_create(surfacep1);
        //
        //     if (dbq.ptype == 1) {
        //         /* Type is a solid -- paint entire surface with foreground color */
        //         if (dbq.pfcol < 0) {
        //             /* use default foreground color, opaque white */
        //             cairo_set_source_rgba(crpatt, 1.0, 1.0, 1.0, 1.0);
        //         } else {
        //             gxdbqcol(dbq.pfcol, &dbq);
        //             if (dbq.Item1 < 0) {
        //                 /* this color cannot be assigned to a tile, use default */
        //                 cairo_set_source_rgba(crpatt, 1.0, 1.0, 1.0, 1.0);
        //             } else {
        //                 alph = dbq.Item4;
        //                 if (alph < 0) alph = -1 * alph;
        //                 cairo_set_source_rgba(crpatt, (double) (dbq.Item1) / 255.0, (double) (dbq.Item2) / 255.0,
        //                                       (double) (dbq.Item3) / 255.0, (double) (alph) / 255.0);
        //             }
        //         }
        //     } else {
        //         /* Type is not a solid -- paint entire surface with background color */
        //         if (dbq.pbcol < 0) {
        //             /* use default background color, which is completely transparent */
        //             cairo_set_source_rgba(crpatt, 0.0, 0.0, 0.0, 0.0);
        //         } else {
        //             gxdbqcol(dbq.pbcol, &dbq);
        //             if (dbq.Item1 < 0) {
        //                 /* this color cannot be assigned to a tile, use default */
        //                 cairo_set_source_rgba(crpatt, 0.0, 0.0, 0.0, 0.0);
        //             } else {
        //                 alph = dbq.Item4;
        //                 if (alph < 0) alph = -1 * alph;
        //                 cairo_set_source_rgba(crpatt, (double) (dbq.Item1) / 255.0, (double) (dbq.Item2) / 255.0,
        //                                       (double) (dbq.Item3) / 255.0, (double) (alph) / 255.0);
        //             }
        //         }
        //     }
        //     cairo_paint(crpatt);
        //
        //     /* now paint the dots or lines */
        //     if (dbq.ptype > 1) {     /* Not solid */
        //         if (dbq.pfcol < 0) {
        //             /* use default foreground color, opaque white */
        //             cairo_set_source_rgba(crpatt, 1.0, 1.0, 1.0, 1.0);
        //         } else {
        //             gxdbqcol(dbq.pfcol, &dbq);
        //             if (dbq.Item1 < 0) {
        //                 /* this color cannot be assigned to a tile, use default */
        //                 cairo_set_source_rgba(crpatt, 1.0, 1.0, 1.0, 1.0);
        //             } else {
        //                 alph = dbq.Item4;
        //                 if (alph < 0) alph = -1 * alph;
        //                 cairo_set_source_rgba(crpatt, (double) (dbq.Item1) / 255.0, (double) (dbq.Item2) / 255.0,
        //                                       (double) (dbq.Item3) / 255.0, (double) (alph) / 255.0);
        //             }
        //         }
        //         cairo_set_line_width(crpatt, dbq.wid);
        //         xx = (double) dbq.pxs;
        //         yy = (double) dbq.pys;
        //         pt = dbq.ptype;
        //         if (pt == 2) {         /* dot */
        //             x1 = (xx / 2.0) - dbq.wid / 2.0;
        //             y1 = (yy / 2.0);
        //             cairo_arc(crpatt, x1, y1, dbq.wid, 0.0, 2.0 * 3.1416);
        //             cairo_fill(crpatt);
        //         } else if (pt >= 3 && pt <= 5) {   /* diagonals */
        //             if (pt == 3 || pt == 5) {
        //                 cairo_move_to(crpatt, 0.0, 0.0);
        //                 cairo_line_to(crpatt, xx, yy);
        //                 cairo_stroke(crpatt);
        //                 cairo_move_to(crpatt, 0.0, -1.0 * yy);
        //                 cairo_line_to(crpatt, 2.0 * xx, yy);
        //                 cairo_stroke(crpatt);
        //                 cairo_move_to(crpatt, -1.0 * xx, 0.0);
        //                 cairo_line_to(crpatt, xx, 2.0 * yy);
        //                 cairo_stroke(crpatt);
        //             }
        //             if (pt == 4 || pt == 5) {
        //                 cairo_move_to(crpatt, 0.0, yy);
        //                 cairo_line_to(crpatt, xx, 0.0);
        //                 cairo_stroke(crpatt);
        //                 cairo_move_to(crpatt, 0.0, 2.0 * yy);
        //                 cairo_line_to(crpatt, 2.0 * xx, 0.0);
        //                 cairo_stroke(crpatt);
        //                 cairo_move_to(crpatt, -1.0 * xx, yy);
        //                 cairo_line_to(crpatt, xx, -1.0 * yy);
        //                 cairo_stroke(crpatt);
        //             }
        //         } else if (pt >= 6 && pt <= 8) {   /* up/down accross */
        //             x1 = xx / 2.0;
        //             y1 = yy / 2.0;
        //             if (pt == 6 || pt == 8) {
        //                 cairo_move_to(crpatt, x1, 0.0);
        //                 cairo_line_to(crpatt, x1, yy);
        //                 cairo_stroke(crpatt);
        //             }
        //             if (pt == 7 || pt == 8) {
        //                 cairo_move_to(crpatt, 0.0, y1);
        //                 cairo_line_to(crpatt, xx, y1);
        //                 cairo_stroke(crpatt);
        //             }
        //         }
        //     }
        // }
        //
        // cairo_surface_show_page(surfacep1);
        // pattern[pnum] = cairo_pattern_create_for_surface(surfacep1);
        // pattsurf[pnum] = surfacep1;
        // cairo_destroy(crpatt);
    }

    public void gxpacol(int col)
    {
        // no op
    }

    public void gxpwid(int wid)
    {
        if (drawing > 0) cr.Stroke();
        drawing = 0;
        lwidth = wid;
        double
            w = _drawingContext.GxDb
                .gxdbqwid(wid - 1); /* at this point wid still starts at 1, so subtract to get index right */
        cr.LineWidth = w;
    }

    public void gxprec(double x1, double x2, double y1, double y2)
    {
        double di1, di2, dj1, dj2;
        if (drawing > 0) cr.Stroke();
        drawing = 0;
        gxCxycnv(x1, y1, out di1, out dj1);
        gxCxycnv(x2, y2, out di2, out dj2);
        if (di1 != di2 && dj1 != dj2)
        {
            cr.Rectangle(di1, dj2, di2 - di1, dj1 - dj2);
            /* disable antialiasing, otherwise faint lines appear around the edges */
            if (aaflg > 0)
            {
                cr.Antialias = Antialias.None;
                cr.Fill();
                cr.Antialias = Antialias.Default;
            }
            else
            {
                cr.Fill();
            }
        }
        else
        {
            cr.MoveTo(di1, dj1);
            cr.LineTo(di2, dj2);
            cr.Stroke();
        }
    }

    /* Convert x,y coordinates from inches on the page into coordinates for current drawing surface */

    void gxCxycnv(double x, double y, out double nx, out double ny)
    {
        if (surftype > 2 && rotate > 0)
        {
            ny = (double)height - x * xscl;
            nx = (double)width - (y * yscl);
        }
        else
        {
            nx = x * xscl;
            ny = (double)height - (y * yscl);
        }

        if (brdrflg > 0)
        {
            nx = brdrwid + nx * (((double)width - brdrwid * 2.0) / (double)width);
            ny = brdrwid + ny * (((double)height - brdrwid * 2.0) / (double)height);
        }
    }


    public void gxpbpoly()
    {
        filflg = 1;
    }

    public void gxpepoly(double[] xy, int n)
    {
        int pt;
        double x, y;
        int i;

        if (drawing > 0) cr.Stroke();
        drawing = 0;
        pt = 0;
        for (i = 0; i < n; i++)
        {
            gxCxycnv(xy[pt], xy[pt + 1], out x, out y);
            if (i == 0) cr.MoveTo(x, y);
            else cr.LineTo(x, y);
            pt += 2;
        }

        /* disable antialiasing, otherwise faint lines appear around the edges */
        if (aaflg > 0)
        {
            cr.Antialias = Antialias.None;
            cr.Fill();
            cr.Antialias = Antialias.Default;
        }
        else
        {
            cr.Fill();
        }

        /* turn off polygon-filling flag */
        filflg = 0;
        return;
    }

    public void gxpmov(double x, double y)
    {
        if (drawing > 0) cr.Stroke();
        drawing = 0;
        gxCxycnv(x, y, out xxx, out yyy);
        //Console.WriteLine($"Moved to {xxx},{yyy}");
    }

    public void gxpdrw(double x, double y)
    {
        double di, dj;
        gxCxycnv(x, y, out di, out dj);
        if (filflg == 0)
        {
            /* we're not filling a polygon */
            if (drawing == 0)
            {
                /* this is the start of a line */
                drawing = 1;
                cr.MoveTo(xxx, yyy);
                cr.LineTo(di, dj);
                //Console.WriteLine($"Drawing line from ({xxx},{yyy}) to ({di},{dj})");
            }
            else
            {
                cr.LineTo(di, dj);
                //Console.WriteLine($"Continue line to ({di},{dj})");
            }
        }

        xxx = di;
        yyy = dj;
    }

    public void gxpflush(int opt)
    {
        if (drawing > 0) cr.Stroke();
        drawing = 0;
        if (maskflag > 0)
        {
            cr = crsave;
            surface = surfsave;
            if (opt > 0)
            {
                // surfmask.Show();
                // cairo_surface_show_page(surfmask);
                patternmask = new SurfacePattern(surfmask);
                cr.SetSourceRGBA((double)(rsav) / 255.0, (double)(gsav) / 255.0,
                    (double)(bsav) / 255.0, (double)(-1 * psav) / 255.0);
                cr.Mask(patternmask);
            }

            crmask.Dispose();
            surfmask.Finish();
            surfmask.Dispose();
            maskflag = 0;
        }

        //if (surftype == 1 && opt) gxdXflush(); /* force completed graphics to display */
    }

    public void gxpsignal(int sig)
    {
        if (sig == 1) gxpflush(1); /* finish drawing */
        if (sig == 2) gxCaa(0); /* disable anti-aliasing */
        if (sig == 3) gxCaa(1); /* enable anti-aliasing */
    }


    /* Set the Cairo font based on the font number and the settings 
   obtained from the backend database settings */

    void gxCselfont(int fn)
    {
        FontFace? cf_face = null;
        int dflt, rc;
        FontWeight cbold;
        FontSlant citalic;

        /* get font info from graphics database */
        (string, bool, int) fontInfo = _drawingContext.GxDb.gxdbqfont(fn); 
        
        /* font 0-5 (but not 3) and hershflag=1 in gxmeta.c, so we use cairo to draw something hershey-like */
        if (fn < 6) {
            cbold = FontWeight.Normal;
            if (fontInfo.Item2) cbold = FontWeight.Bold;
            citalic = FontSlant.Normal;
            if (fontInfo.Item3 == 1) citalic = FontSlant.Italic;
            if (fontInfo.Item3 == 2) citalic = FontSlant.Oblique;
        
            if (String.IsNullOrEmpty(fontInfo.Item1)) {
                /* we should never have fn<6 and fname==NULL, but just in case... */
                cr.SelectFontFace("sans-serif",citalic,  cbold );
            } else {
                cr.SelectFontFace(fontInfo.Item1, citalic, cbold);
            }
        } else {
            /* font>=10 */
            // dflt = 0;
            // if (library == NULL) dflt = 1;      /* use default fonts */
            // if (dbq.fname == NULL) dflt = 1;    /* make sure we have a font filename */
            //
            // if (!dflt) {
            //     if (face[fn] == NULL) {
            //         /* try to open user-provided font file */
            //         rc = FT_New_Face(library, dbq.fname, 0, &newface);
            //         if (rc) {
            //             printf("Error: Unable to open font file \"%s\"\n", dbq.fname);
            //             printf(" Will use a default \"sans-serif\" font instead\n");
            //             dflt = 1;
            //             /* update the data base so this error message only appears once */
            //             gxdbsetfn(fn, NULL);
            //         } else {
            //             /* we succeeded, so save the face and update the font status */
            //             face[fn] = newface;
            //         }
            //     } else {
            //         /* this font has already been opened, so we use the saved face */
            //         newface = face[fn];
            //     }
            // }
            //
            // if (!dflt) {
            //     /* create a new font face  */
            //     cf_face = cairo_ft_font_face_create_for_ft_face(newface, 0);
            //     cairo_set_font_face(cr, cf_face);
            // } else {
            //     /* set up a default font with the Cairo "Toy" interface */
            //     cairo_select_font_face(cr, "sans-serif", CAIRO_FONT_SLANT_NORMAL, CAIRO_FONT_WEIGHT_NORMAL);
            // }
        }
        
    }

    public double gxpch(char ch, int fn, double x, double y, double w, double h, double rot)
    {
        TextExtents te;
        double xpage, ypage, usize, vsize;
        double aheight, awidth, swidth, fontsize = 100.0;
        string str;
        string astr = "A";

        if (drawing > 0) cr.Stroke();
        drawing = 0;
        gxCselfont(fn);

        /* get the scale factor based on size of "A" */
        cr.SetFontSize(fontsize);
        te = cr.TextExtents(astr);
        awidth = fontsize / te.Width;
        aheight = fontsize / te.Height;
        usize = w * xscl * awidth;
        vsize = h * yscl * aheight;
        if (brdrflg > 0)
        {
            usize = usize * (((double)width - brdrwid * 2.0) / (double)width);
            vsize = vsize * (((double)height - brdrwid * 2.0) / (double)height);
        }

        /* Convert the position coordinates and adjust the rotation if necessary */
        gxCxycnv(x, y, out xpage, out ypage);
        if (rotate > 0) rot = rot + Math.PI / 2;

        /* get the text extents of the character we want to draw */
        te = cr.TextExtents(ch.ToString());

        /* draw a character */
        cr.Save(); /* save the untranslated, unrotated, unclipped context */
        cr.Rectangle(clx, cly, clw, clh); /* set the clipping area */
        cr.Clip(); /* clip it */
        cr.Translate(xpage, ypage); /* translate to location of character */
        cr.Rotate(-1.0 * rot); /* rotate if necessary */
        cr.MoveTo(0.0, 0.0); /* move to (translated) origin */
        cr.Scale(usize / fontsize, vsize / fontsize); /* apply the scale factor right before drawing */
        cr.ShowText(ch.ToString()); /* finally, draw the darned thing */
        cr.Restore(); /* restore the saved graphics context */

        /* return the scaled width of the character */
        swidth = te.XAdvance * usize / (fontsize * xscl);
        return (swidth);

        return 0;
    }

    public double gxpqchl(char ch, int fn, double w)
    {
        TextExtents te;
        double usize, awidth, swidth, fontsize = 100.0;
        
        string astr = "A";

        gxCselfont(fn);

        /* get the scale factor (for width only) based on the size of "A" */
        cr.SetFontSize(fontsize);
        te = cr.TextExtents(astr);
        awidth = fontsize / te.Width;
        usize = w * awidth * xscl;
        if (brdrflg>0) usize = usize * (((double) width - brdrwid * 2.0) / (double) width);

        /* get the text extents of the character */
        te = cr.TextExtents(ch.ToString());

        /* return the scaled width of the character */
        swidth = te.XAdvance * usize / (fontsize * xscl);
        return (swidth);
    }

    public void gxpclip(double x1, double x2, double y1, double y2)
    {
        double di1, di2, dj1, dj2;
        gxCxycnv(x1, y1, out di1, out dj1);
        gxCxycnv(x2, y2, out di2, out dj2);
        /* set the clipping variables -- these get used when drawing characters */
        clx = di1;
        cly = dj2;
        clw = di2 - di1;
        clh = dj1 - dj2;
    }
}