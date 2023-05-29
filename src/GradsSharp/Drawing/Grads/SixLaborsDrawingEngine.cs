using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;

namespace GradsSharp.Drawing.Grads;

internal class SixLaborsDrawingEngine : IDrawingEngine
{

    private const int SX = 800;

    private DrawingContext _drawingContext;
    
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
    static int faceinit = 0; /* for knowing whether font faces have been initialized */

    static int surftype; /* Type of current surface.
                                               1=X, 2=Image, 3=PS/EPS, 4=PDF, 5=SVG */

// static cairo_surface_t *Xsurface=NULL, *Hsurface=NULL, *Bsurface=NULL;
//                                             /* X, Hardcopy, and Batch mode surfaces */
//                                             /* X surface is passed to us by gxX. */
//                                             /* Others are created as needed. */
// static cairo_t *Xcr, *Hcr, *Bcr;            /* X, Hardcopy, and Batch mode contexts */  
// static cairo_pattern_t *pattern[2048];      /* Save our patterns here */
// static cairo_surface_t *pattsurf[2048];
// static cairo_surface_t *surfmask, *surfsave;  /* Stuff for masking */
// static cairo_t *crmask, *crsave;
// static cairo_pattern_t *patternmask;
    static int drawing; /* In the middle of line-to's? */
    static int maskflag; /* Masking going on */
    static int batch = 0; /* Batch mode */
    static double clx, cly, clw, clh; /* current clipping coordinates */
    static double clxsav, clysav, clwsav, clhsav; /* saved clipping coordinates */
    
    
    private static Color _currentColor;
    private static float _lineWidth;
    private PointF _moveTo;
    
    private Image<Argb32>? _image;

    public SixLaborsDrawingEngine(DrawingContext drawingContext)
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
        int dh, dw;

        xsize = xsz;
        ysize = ysz;

        if (xsz > ysz)
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

        _image = new Image<Argb32>(dw, dh);

        surftype = 2;
    }

    public void gxpend()
    {
        if (_image == null) return;
        _image.Dispose();
        _image = null;
    }

    private int gxhcinit(double xsz, double ysz, int xin, int yin, int bwin,
        string fname, int fmtflg, string bgImage, double border)
    {
        int status;
    int dh = 0, dw = 0;          /* image size in pixels */
    double psx = 0, psy = 0;     /* page size in points */
    int bcol, i;
    int bgwidth, bgheight, len;

    // if (!faceinit) {
    //     for (i = 0; i < 100; i++) face[i] = NULL;
    //     faceinit = 1;
    //     gxCftinit();         /* make sure FreeType library has been initialized */
    // }

    //gxCflush(1);
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

    if (fmtflg == 5) {  /* Image output */
        /*  Set the image width and height */
        if (xin < 0 || yin < 0) {     /* user has not specified image size */
            if (xsize > ysize) {    /* landscape */
                dw = SX;
                dh = (int) ((ysize / xsize) * SX);
            } else {
                dw = (int) ((xsize / ysize) * SX);
                dh = SX;
            }
        } else {
            dw = xin;
            dh = yin;
        }
        /* image dimensions */
        width = dw;
        height = dh;
        /* scale factors (pixels per inch) */
        xscl = (double) (dw) / xsize;
        yscl = (double) (dh) / ysize;
    }

    if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3 || fmtflg == 4) {  /* vector graphic output */
        /* set page size in points */
        if (ysize > xsize) {
            /* portrait: no rotation needed for any format */
            psx = xsize * 72.0;
            psy = ysize * 72.0;
        } else {
            /* landscape: ps, eps, and pdf must be rotated to fit on page */
            if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3) {
                rotate = 1;
                psx = ysize * 72.0;
                psy = xsize * 72.0;
            } else {
                /* landscape: svg doesn't need to be rotated */
                psx = xsize * 72.0;
                psy = ysize * 72.0;
            }
        }
        /* page dimensions */
        width = (int) psx;
        height = (int) psy;
        /* scale factors (points per inch) */
        xscl = 72.0;
        yscl = 72.0;
        /* set a border for ps, eps, and pdf formats */
        if (fmtflg <= 3) {
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
    if (fmtflg == 1 || fmtflg == 2) {            /* PS or EPS */
        surftype = 3;
        throw new Exception("PS And EPS are not supported in SixLabors Drawing Engine");
    } else if (fmtflg == 3) {                    /* PDF */
        surftype = 4;
        throw new Exception("PDF is not supported in SixLabors Drawing Engine");
    } else if (fmtflg == 4) {                    /* SVG */
        surftype = 5;
        throw new Exception("SVG is not supported in SixLabors Drawing Engine");
    } else if (fmtflg == 5) {                    /* PNG */
        surftype = 2;

        if (bgImage!="") {
            /* Make sure bgImage is a .png  */
            if (bgImage.EndsWith(".png")) return 5;
            /* create a new surface from the background image */
            
            
            
            var bg = Image.Load(bgImage);
            /* make sure background image size matches output image size */
            bgwidth = bg.Width;
            bgheight = bg.Height;
            if (bgwidth != width || bgheight != height) {
                // printf(" background image size is %d x %d \n", bgwidth, bgheight);
                // printf("     output image size is %d x %d \n", width, height);
                // cairo_surface_finish(surface);
                // cairo_surface_destroy(surface);
                
                return (10);
            }

            _image.Mutate<Argb32>(x => x.DrawImage(bg, new Rectangle(0,0, width, height), PixelColorBlendingMode.Normal, 0));
        } else {
            /* no background image specified by user */
            _image = new Image<Argb32>(dw, dh);
        }
    }

    /* Make sure surface set up without any problems */
    // status = cairo_surface_status(surface);
    // if (status) {
    //     printf("Error in gxChinit: failed to initialize hardcopy surface \n");
    //     printf("Cairo status: %s\n", cairo_status_to_string(status));
    //     cairo_surface_finish(surface);
    //     cairo_surface_destroy(surface);
    //     return (1);
    // }

    /* Set the Cairo context */
    // cr = cairo_create(surface);
    // Hsurface = surface;
    // Hcr = cr;

    /* default drawing settings */
    // cairo_set_line_cap(cr, CAIRO_LINE_CAP_ROUND);
    // cairo_set_line_join(cr, CAIRO_LINE_JOIN_ROUND);
    // gxCaa(1);                   /* initialize anti-aliasing to be ON */

    force = 1;             /* force a color change */
    bcol = GxDb.gxdbkq();
    if (bcol > 1) {
        /* User has set the background to be a color other than black/white.
           The background rectangle is therefore in the metabuffer and will
           cover whatever we draw here to initalize the output */
        if (bwin > -900) {
            // printf("Warning: Background color cannot be changed at print time    \n");
            // printf(" if it has been set to a color other than black or white. \n");
            // printf(" The current background color is %d. \n", bcol);
            // if (bwin == 1)
            //     printf(" The option \"white\" will be ignored. \n");
            // else
            //     printf(" The option \"black\" will be ignored. \n");
        }
    } else {
        if (bwin > -900)
            GxDb.gxdboutbck(bwin);  /* change the background color if user provided an override */
        gxpcol(0);           /* 0 here means 'background' */
        //cairo_paint(cr);     /* paint it */
    }

    gxpcol(1);             /* set the initial color to 'foreground' */
    force = 0;             /* back to unforced color changes */
    return (0);
    }
    
    public int gxprint(string fnout, int xin, int yin, int bwin, int fmtflg, string bgImage, string fgImage, int tcolor,
        double border)
    {
        if (fmtflg!=1 && fmtflg!=2 && fmtflg!=3 && fmtflg!=4 && fmtflg!=5) return (9);

        if (tcolor != -1) _drawingContext.GxDb.gxdbsettransclr (tcolor);   /* tell graphics database about transparent color override */

        int rc = 0;
        /* initialize the output for vector graphics or image */
        rc = gxhcinit (xsize,ysize,xin,yin,bwin,fnout,fmtflg,bgImage,border);
        if (rc>0) return (rc); 

        /* draw the contents of the metabuffer */
        _drawingContext.GxMeta.gxhdrw (0,1);
        if (rc>0) return (rc); 

        /* finish up */
        rc = gxChend (fnout,fmtflg,fgImage);

        _drawingContext.GxDb.gxdbsettransclr (-1);    /* unset transparent color override */
        return (rc);
        
    }

    int gxChend(string fname, int fmtflg, string fgImage) {
    int fgwidth, fgheight, len, status;
    Image sfc2;     /* Surface for fgImage */

    //gxCflush(1);         /* finish drawing */

    /* bgImage and fgImage only work when output format is PNG */
    if (fmtflg == 5) {
        if (fgImage!="") {
            /* Make sure fgImage is a .png  */
            if (!fgImage.EndsWith(".png"))
            {
                return 6;
            }
            /* create a new surface from the foreground image */
            sfc2 = Image.Load(fgImage);
            /* check to make sure foreground and output images are the same size */
            fgwidth = sfc2.Width;
            fgheight = sfc2.Height;
            if (fgwidth != width || fgheight != height) {
                // printf(" foreground image size is %d x %d \n", fgwidth, fgheight);
                // printf("     output image size is %d x %d \n", width, height);
                // cairo_surface_finish(sfc2);
                // cairo_surface_destroy(sfc2);
                return (11);
            }
            /* draw the foreground image OVER the hardcopy surface */
            _image.Mutate<Argb32>(x => x.DrawImage(sfc2, new Rectangle(0,0, width, height), PixelColorBlendingMode.Overlay, 0));
            // cairo_set_operator(cr, CAIRO_OPERATOR_OVER);
            // cairo_set_source_surface(cr, sfc2, 0, 0);
            // cairo_paint(cr);
            // /* clean up */
            // cairo_surface_finish(sfc2);
            // cairo_surface_destroy(sfc2);
        }
    }

    /* Make sure surface was rendered without any problems */
    // status = cairo_surface_status(surface);
    // if (status) {
    //     printf("Error in gxChend: an error occured during rendering of hardcopy surface \n");
    //     printf("Cairo status: %s\n", cairo_status_to_string(status));
    //     cairo_surface_finish(surface);
    //     cairo_surface_destroy(surface);
    //     return (1);
    // }

    /* dump everything to hardcopy output, finish and destroy the surface */
    //cairo_surface_show_page(surface);
    if (fmtflg == 5) {
        //cairo_surface_write_to_png(surface, fname);  /* for image (png) output only */
        _image.SaveAsPng(fname);
    }
    // cairo_destroy(cr);
    // cairo_surface_finish(surface);
    // cairo_surface_destroy(surface);
    rotate = 0;             /* unset rotation for landscape hardcopy */
    brdrflg = 0;            /* reset border flag for vector graphic output */
    brdrwid = 0.0;          /* reset border width for vector graphic output */
    GxDb.gxdboutbck(-1);       /* unset output background color */

    /* Reset everything back to the interactive/batch surface for drawing */
    _image = null;
    if (batch>0) {
        width = Bwidth;
        height = Bheight;
    } else {
        width = Xwidth;
        height = Xheight;
    }
    lcolor = lcolorsave;
    lwidth = lwidthsave;
    clx = clxsav;
    cly = clysav;
    clw = clwsav;
    clh = clhsav;
    xscl = (double) (width) / xsize;
    yscl = (double) (height) / ysize;

    return (0);
}
    
    public void gxpcol(int col)
    {
        var c = _drawingContext.GxDb.gxdbqcol(col);
        _currentColor = Color.FromRgba( (byte)c.Item1, (byte)c.Item2, (byte)c.Item3, (byte)c.Item4);
    }

    public void gxpacol(int col)
    {
    }

    public void gxpwid(int wid)
    {
        if (drawing == 1)
        {
            //TODO cairo_stroke ?
        }

        drawing = 0;
        lwidth = wid;
        _lineWidth = (float)_drawingContext.GxDb.gxdbqwid(wid - 1);
        
    }

    public void gxprec(double x1, double x2, double y1, double y2)
    {
        float di1, di2, dj1, dj2;
        if (drawing==1)
        {
            //cairo_stroke(cr);
        }
        drawing = 0;
        gxCxycnv(x1, y1, out di1, out dj1);
        gxCxycnv(x2, y2, out di2, out dj2);
        if (di1 != di2 && dj1 != dj2)
        {
           
            _image.Mutate(x => x.Fill(_currentColor, new RectangleF(di1, dj2, di2-di1, dj1-dj2)));
            // cairo_rectangle(cr, di1, dj2, di2 - di1, dj1 - dj2);
            // /* disable antialiasing, otherwise faint lines appear around the edges */
            // if (aaflg) {
            //     cairo_set_antialias(cr, CAIRO_ANTIALIAS_NONE);
            //     cairo_fill(cr);
            //     cairo_set_antialias(cr, CAIRO_ANTIALIAS_DEFAULT);
            // } else {
            //     cairo_fill(cr);
            // }
        } else {
            _image.Mutate(x => x.DrawLines(_currentColor, _lineWidth, new PointF(di1, dj1), new PointF(di2, dj2)));
            // cairo_move_to(cr, di1, dj1);
            // cairo_line_to(cr, di2, dj2);
            // cairo_stroke(cr);
        }
    }

    public void gxpbpoly()
    {
        // no-op
    }

    public void gxpepoly(double[] xybuf, int n)
    {
        float x, y;
        int i;
        int pt = 0;

        if (drawing>0)
        {
            //cairo_stroke(cr);
        }
        drawing = 0;
        pt = 0;

        List<PointF> points = new List<PointF>();
        
        

        
        for (i = 0; i < n; i++) {
            gxCxycnv( xybuf[pt], xybuf[pt+1], out x, out y);
            points.Add(new PointF(x, y));
            pt += 2;
            
        }
        /* disable antialiasing, otherwise faint lines appear around the edges */
        // if (aaflg) {
        //     cairo_set_antialias(cr, CAIRO_ANTIALIAS_NONE);
        //     cairo_fill(cr);
        //     cairo_set_antialias(cr, CAIRO_ANTIALIAS_DEFAULT);
        // } else {
        //     cairo_fill(cr);
        // }
        _image.Mutate(x => x.FillPolygon(_currentColor, points.ToArray()));
        
        /* turn off polygon-filling flag */
        filflg = 0;
        
    }

    public void gxpmov(double xpos, double ypos)
    {
        float x, y;
        gxCxycnv( xpos, xpos, out x, out y);
        _moveTo = new PointF(x, y);
    }

    public void gxpdrw(double xpos, double ypos)
    {
        float x, y;
        gxCxycnv( xpos, ypos, out x, out y);

        double dist = Math.Sqrt(Math.Pow(x - _moveTo.X, 2) + Math.Pow(y - _moveTo.Y, 2));

        
        if (dist > 15)
        {
            Console.WriteLine($"Draw line from ({_moveTo.X},{_moveTo.Y}) to ({x},{y})=({xpos},{ypos})");    
        }

        _image.Mutate(d => d.DrawLines(Color.Azure, _lineWidth, _moveTo, new PointF(x, y)));
        _moveTo = new PointF(x, y);
    }

    public void gxpflush(int opt)
    {
    }

    public void gxpsignal(int sig)
    {
    }

    public double gxpch(char ch, int fn, double x, double y, double w, double h, double rot)
    {
        return 0;
    }

    public double gxpqchl(char ch, int fn, double w)
    {
        return 0;
    }

    public void gxpclip(double x1, double x2, double y1, double y2)
    {
        float di1, di2, dj1, dj2;
        gxCxycnv(x1, y1, out di1, out dj1);
        gxCxycnv(x2, y2, out di2, out dj2);
        /* set the clipping variables -- these get used when drawing characters */
        clx = di1;
        cly = dj2;
        clw = di2 - di1;
        clh = dj1 - dj2;
    }
    
    void gxCxycnv(double x, double y, out float nx, out float ny) {
        if (surftype > 2 && rotate>0) {
            ny = (float) (height - x * xscl);
            nx = (float) (width - (y * yscl));
        } else {
            nx = (float)((float)x * xscl);
            ny = (float)((float) height - (y * yscl));
        }
        if (brdrflg>0) {
            nx = (float)(brdrwid + nx * (((double) width - brdrwid * 2.0) / (double) width));
            ny = (float)(brdrwid + ny * (((double) height - brdrwid * 2.0) / (double) height));
        }
    }
}