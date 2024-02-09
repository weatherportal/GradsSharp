using GradsSharp.Data;
using GradsSharp.Drawing;

namespace GradsSharp.Models.Internal;

internal class GradsCommon
{
    
    public double[] dmin { get; private set; } = new double[5];
    public double[] dmax { get; private set; } = new double[5]; /* Current absolute coordinate limits    */

    public Func<double[], double, double> xgr2ab { get; set; }
    public Func<double[], double, double> ygr2ab { get; set; }
    public Func<double[], double, double> xab2gr { get; set; }
    public Func<double[], double, double> yab2gr { get; set; }

    public Action<IDataAdapter>? DataAction { get; set; }
    

    public double[] xgrval;
    public double[] ygrval;
    public double[] xabval;
    public double[] yabval;
    public int aaflg; /* Hardware anti-aliasing flag           */
    public int hbufsz; /* Metafile buffer size                  */
    public int g2bufsz; /* Grib2 cache buffer size               */
    public int pass; /* Number of passes since last clear     */
    public int[] gpass = new int[10]; /* Number of passes for each gx type     */
    public int loopdim; /* Looping dimension                     */
    public int loopflg; /* Looping on or off                     */
    public List<GradsFile>? pfi1;
    public GradsFile? pfid;
    public IVariableMapping VariableMapping { get; set; }
    public int fnum; /* File count                            */
    public int dfnum; /* Default file number   */

    public int fseq; /* Unique sequence num for files opened  */

    public List<gadefn>? pdf1 = new(); /* Pointer to first define block         */
    public dt tmin = new dt(), tmax = new();
    public int[] vdim = new int[5]; /* Which dimensions vary?                */
    public int x1ex, x2ex, y1ex, y2ex; /* For -ex flag on fwrite */
    public int xexflg, yexflg; /* -ex -- are dims valid? */
    public double pxsize, pysize; /* Physical page size in inches          */
    public int orient; /* Page orientation                      */
    public int vpflag; /* If 1, virtual page being used         */
    public double xsiz, xsiz1, xsiz2; /* Physical plotting size in X direction */
    public double ysiz, ysiz1, ysiz2; /* Physical plotting size in Y direction */
    public bool paflg; /* User has specified plotting area      */
    public double pxmin, pxmax; /* User specified plotting area          */
    public double pymin, pymax;
    public int clab; /* control contour labels.               */
    public int clskip; /* Contour label skipping                */
    public string? clstr; /* Contour label template                */
    public double rainmn, rainmx; /* control rainbow colors                */
    public int rbflg; /* User rainbow colors specified         */
    public int[] rbcols = new int[256]; /* User rainbow colors                   */
    public double cmin, cmax, cint; /* User specified contour limits         */
    public int cflag; /* If true, user specifies contour levels*/
    public double[] clevs = new double[256]; /* User specified contour levels         */
    public int ccflg; /* If true, user specifies contour colors*/
    public int[] ccols = new int[256]; /* User specified contour colors         */
    public int[] shdcls = new int[256]; /* Shade colors after shading            */
    public double[] shdlvs = new double[256]; /* Shade levels                          */
    public int shdcnt; /* Number of shdlvs, shdcls              */
    public int cntrcnt; /* Number of contours (after countouring)*/
    public int[] cntrcols = new int[256]; /* Contour colors (after contouring)     */
    public double[] cntrlevs = new double[256]; /* Contour levels (after contouring)     */
    public int ccolor, cstyle; /* User contour/line appearance          */
    public int cthick; /* User gx display line thickness        */
    public int cmark; /* Line marker type                      */
    public int csmth; /* Contour smoothing on or off           */
    public int cterp; /* Spline fit on or off                  */
    public double rmin, rmax, rint; /* Axis limits for 1-D plots             */
    public double rmin2, rmax2, rint2; /* Axis limits for 1-D plots             */
    public int aflag, aflag2; /* Keep 1D axis limits fixed             */
    public int grflag; /* Grid flag */
    public int grstyl, grcolr; /* Grid linestyle, color           */
    public int grthck; /* Grid thickness                        */
    public int dignum; /* grid value plot control (gxout=grid)  */
    public double digsiz;
    public bool arrflg; /* Use already set arrow scaling         */
    public double arrsiz; /* Arrow size in inches                  */
    public bool arlflg; /* Arrow label flag */
    public double arrmag; /* Vector magnitude producing arrsiz arrw*/
    public double ahdsiz; /* Arrow head size.       */
    public int hemflg; /* -1; auto  0; nhem  1; shem */
    public int miconn; /* Connect line graph accross missing    */
    public int strmden; /* Streamline density indicator  */
    public double strmarrd; /* Streamline distance between arrowheads */
    public double strmarrsz; /* Streamline arrowhead size */
    public int strmarrt; /* Streamline arrowhead type */
    public int mdlblnk, mdldig3; /* Station model plot opts */
    public string prstr = "{0:0.00}"; /* Format string for gxout print */
    public int prlnum = 8; /* Number of values per record */
    public int prbnum; /* Number of blanks to add between values */
    public int prudef; /* Undef printed as "undef" or value */
    public int[] fgvals = new int[50]; /* Values for grid fill */
    public int[] fgcols = new int[50];
    public int fgcnt;
    public int gridln; /* Line attributes for gxout grid */
    public int stidflg; /* Plot station ids with values      */
    public double axmin, axmax, axint; /* Overrides for X-axis labels           */
    public double aymin, aymax, ayint; /* Overrides for Y-axis labels           */
    public int axflg, ayflg; /* Is override in effect for the axis?   */
    public int frame; /* Display frame?  */
    public bool rotate; /* Rotate plot from default orientation  */
    public bool xflip, yflip; /* Flip X or Y axes                      */
    public bool zlog; /* Z coordinate in log scale */
    public int log1d; /* Log scaling for 1D plots              */
    public bool coslat; /* Lat coordinate scaled as cos lat */

    public int mproj; /* Map projection -- used for X,Y plot   */

    /*  only.  0 = no map.                   */
    public int mpdraw; /* Draw map outline - 0=no               */
    public double[] mpvals = new double[10]; /* Map projection option values.         */
    public int mpflg; /* Map projection option values are set. */
    public string[] mpdset = new string[8]; /* Map data set names.                   */
    public int[] mpcols = new int[256]; /* Map Color array                       */
    public int[] mpstls = new int[256]; /* Map line styles array                 */
    public int[] mpthks = new int[256]; /* Map line widths array                 */
    public int mapcol, mapstl, mapthk; /* Default map color, style, thickness   */
    public int gout0; /* Graphics output type for stat.        */
    public int gout1; /* Graphics output type for 1-D.         */
    public int gout1a; /* Graphics output type for 1-D.         */
    public int gout2a; /* Graphics output type for 2-D.         */
    public int gout2b; /* Graphics output type for 2-D.         */
    public int goutstn; /* Graphics output type for stns */
    public bool blkflg; /* Leave certain values black when shadng*/
    public double blkmin, blkmax; /* Black range */
    public int reccol, recthk; /* Draw Rectangle color, brdr thickness  */
    public int lincol, linstl, linthk; /* Draw line color, style, thickness     */
    public int mcolor; /* auto color (orange or grey)           */
    public int strcol, strthk, strjst; /* Draw string color, thckns, justifictn */
    public double strrot; /* Draw string rotation */
    public double strhsz, strvsz; /* Draw string hor. size, vert. size     */
    public int anncol, annthk; /* Draw title color, thickness           */
    public int xlcol, xlthck, ylcol, ylthck, clcol, clthck; /* color, thickness */
    public int xlside, ylside, ylpflg;
    public double xlsiz, ylsiz, clsiz, xlpos, ylpos, yllow; /* Axis lable size */
    public double[] xlevs = new double[50], ylevs = new double[50]; /* User specified x/y axis labels  */
    public int xlflg, ylflg; /* Number of user specified labels */
    public int xtick, ytick; /* Number of extra tick marks      */
    public double xlint, ylint; /* User specified label increment */
    public string? xlstr, ylstr; /* user substitution string for labels */
    public int xlab, ylab; /* Axis label options */
    public List<string>? xlabs = new(), ylabs = new(); /* User specifies all labels */
    public int ixlabs, iylabs; /* Count of user labels */
    public int tlsupp; /* Suppress year or month of time labels */
    public int lfc1, lfc2; /* Linefill colors */
    public int[] wxcols = new int[5]; /* wx symbol colors */
    public int wxopt; /* wx options */
    public int tser; /* station time series type */
    public bool barbolin; /* Wind barb pennant outline flag */
    public int bargap; /* Bar Gap in percent  */
    public int barolin; /* Bar outline flag */
    public double barbase; /* Bar Base Value      */

    public int barflg; /* Bar flag: 1, use base value  */

    /*           0, draw from plot base */
    /*          -1, draw from plot top  */
    public int btnfc, btnbc, btnoc, btnoc2; /* Current button attributes */
    public int btnftc, btnbtc, btnotc, btnotc2;
    public int btnthk;
    public int dlgfc, dlgbc, dlgoc; /* Current dialog attributes */

    public int dlgpc, dlgth, dlgnu;

    //int[] drvals = new int[15];          /* Attributes for drop menus */
//   char *shpfname;            /* shapefile write file name */
//   int shptype;             /* shapefile output type: 1=point, 2=line */
//   int gtifflg;             /* geotiff data type: 1=float 2=double */
//   char *gtifname;            /* geotiff write file name */
//   char *tifname;             /* kml image  file name */
    public string kmlname = "";             /* kml text file name */
    
    public int kmlflg = 0;              /* kml output: 1==img, 2==contours */
//   char *sdfwname;            /* netcdf/hdf write file name */
//   int sdfwtype;            /* type of sdf output: 1=classic, 2=nc4 */
//   int sdfwpad;             /* pad the sdf output with extra dims: 1=4D, 2=5D */
//   int sdfprec;             /* precision (8==double, 4==float, etc.) */
//   int sdfchunk;            /* flag to indicate whether or not to chunk */
//   int sdfzip;              /* flag to indicate whether or not to compress */
//   int sdfrecdim;           /* flag to indicate record dimensions */
//   int ncwid;               /* netcdf write file id  */
//   int xchunk;              /* size of sdfoutput file chunk in X dimension */
//   int ychunk;              /* size of sdfoutput file chunk in Y dimension */
//   int zchunk;              /* size of sdfoutput file chunk in Z dimension */
//   int tchunk;              /* size of sdfoutput file chunk in T dimension */
//   int echunk;              /* size of sdfoutput file chunk in E dimension */
//   class gaattr *attr;       /* pointer to link list of user-specified SDF attributes */
// #if USESHP==1
//   class dbfld *dbfld;       /* pointer to link list of user-specified data base fields */
// #endif
//   int dblen;               /* total number of digits for formatting data base fields */
//   int dbprec;              /* precision digits for formatting data base fields: %len.prec */
//   FILE *ffile;               /* grads.fwrite file handle */
//   FILE *sfile;               /* grads.stnwrt file handle */
//   char *fwname;              /* fwrite file name */
//   int fwenflg;             /* fwrite byte order control */
//   int fwsqflg;             /* fwrite stream vs fortran seq */
//   int fwappend;            /* write mode (1): append */
//   int fwexflg;             /* fwrite exact grid dims */
    public bool grdsflg; /* Indicate whether to put grads atrib.  */
    public bool timelabflg; /* Indicate whether to put cur time atrib.  */
    public int stnprintflg; /* Indicate whether to put cur time atrib.  */
    public int dbflg; /* Double buffer mode flag     */
    public int batflg; /* Batch mode */
    public int numgrd, relnum; /* Number of data objects held           */
    public int[] type = new int[16]; /* Data type of each data object         */

    public gadata[] result = new gadata[16]; /* Pointers to held data objects         */

    //class gaclct *clct[32];   /* Anchor for collection */
    //int clctnm[32];          /* Number of items collected */
    //int clcttp[32];          /* Varying dimension of collection */
    public int lastgx; /* Last gx plotted */
    public int xdim, ydim; /* Which dimensions on X and Y axis */
    public bool statflg; /* stat txt output on all displays */
    public int impflg; /* Implied run flag */
    public string impnam; /* Implided run script name */
    public int impcmd; /* Implicit run */
    public int sig; /* User has signalled */
    public int ptflg; /* Pattern fill flag */

    public int ptopt; /* Pattern option: */

    /*		0, open  */
    /*		1, solid */
    /*		2, dot */
    /*		3, line  */
    public int ptden; /* Dot or line pattern density */
    public int ptang; /* Line pattern angle */
    public bool dwrnflg; /* Issue, or not, warnings about missing or constant data */
    public double undef; /* default or user-defined undef value for print and file output */
    public long cachesf; /* global scale factor for netcdf4/hdf5 cache size */
    public int fillpoly; /* color to fill shapfile polygons, -1 for no fill */
    public int marktype; /* type of mark for shapefile points */
    public double marksize; /* size of mark for shapefile points */
    public string xgeom; /* geometry string for size of X window on startup */
    public string gxdopt; /* Name of graphics display back end     */
    public string gxpopt; /* Name of graphics printing back end    */


    public void InitData()
    {
        hbufsz = 1000000;
        g2bufsz = 10000000;
        loopdim = 3;
        csmth = 0;
        cterp = 1;
        cint = 0;
        cflag = 0;
        ccflg = 0;
        cmin = -9.99e33;
        cmax = 9.99e33;
        arrflg = false;
        arlflg = true;
        ahdsiz = 0.05;
        hemflg = -1;
        aflag = 0;
        axflg = 0;
        ayflg = 0;
        rotate = false;
        xflip = false;
        yflip = false;
        gridln = -9;
        zlog = false;
        log1d = 0;
        coslat = false;
        numgrd = 0;
        gout0 = 0;
        gout1 = 1;
        gout1a = 0;
        gout2a = 1;
        gout2b = 4;
        goutstn = 1;
        cmark = -9;
        grflag = 1;
        grstyl = 5;
        grthck = 4;
        grcolr = 15;
        blkflg = false;
        dignum = 0;
        digsiz = 0.07;
        reccol = 1;
        recthk = 3;
        lincol = 1;
        linstl = 1;
        linthk = 3;
        mproj = 2;
        mpdraw = 1;
        mpflg = 0;
        mapcol = -9;
        mapstl = 1;
        mapthk = 1;
        for (int i = 0; i < 256; i++)
        {
            mpcols[i] = -9;
            mpstls[i] = 1;
            mpthks[i] = 3;
        }

        mpcols[0] = -1;
        mpcols[1] = -1;
        mpcols[2] = -1;
        for (int i = 0; i < 8; i++)
        {
            //if (mpdset[i]) gree(mpdset[i], "g1");
            mpdset[i] = null;
        }

        mpdset[0] = "lowres";
        for (int i = 1; i < 8; i++) mpdset[i] = null;

        strcol = 1;
        strthk = 3;
        strjst = 0;
        strrot = 0.0;
        strhsz = 0.1;
        strvsz = 0.12;
        anncol = 1;
        annthk = 5;
        tlsupp = 0;
        xlcol = 1;
        ylcol = 1;
        xlthck = 4;
        ylthck = 4;
        xlsiz = 0.11;
        ylsiz = 0.11;
        xlflg = 0;
        ylflg = 0;
        xtick = 1;
        ytick = 1;
        xlint = 0.0;
        ylint = 0.0;
        xlpos = 0.0;
        ylpos = 0.0;
        ylpflg = 0;
        yllow = 0.0;
        xlside = 0;
        ylside = 0;
        clsiz = 0.09;
        clcol = -1;
        clthck = -1;
        stidflg = 0;
        grdsflg = true;
        timelabflg = true;
        stnprintflg = 0;
        fgcnt = 0;
        barbolin = false;
        barflg = 0;
        bargap = 0;
        barolin = 0;
        clab = 1;
        clskip = 1;
        xlab = 1;
        ylab = 1;
        clstr = null;
        xlstr = null;
        ylstr = null;
        xlabs = null;
        ylabs = null;
        dbflg = 0;
        rainmn = 0.0;
        rainmx = 0.0;
        rbflg = 0;
        miconn = 0;
        impflg = 0;
        impcmd = 1;
        strmden = 5;
        strmarrd = 0.4;
        strmarrsz = 0.05;
        strmarrt = 1;
        frame = 1;
        pxsize = xsiz;
        pysize = ysiz;
        vpflag = 0;
        xsiz1 = 0.0;
        xsiz2 = xsiz;
        ysiz1 = 0.0;
        ysiz2 = ysiz;
        paflg = false;
        for (int i = 0; i < 10; i++) gpass[i] = 0;
        btnfc = 1;
        btnbc = 0;
        btnoc = 1;
        btnoc2 = 1;
        btnftc = 1;
        btnbtc = 0;
        btnotc = 1;
        btnotc2 = 1;
        btnthk = 3;
        dlgpc = -1;
        dlgfc = -1;
        dlgbc = -1;
        dlgoc = -1;
        dlgth = 3;
        dlgnu = 0;
        // for (int i = 0; i < 15; i++) drvals[i] = 1;
        // drvals[1] = 0;
        // drvals[5] = 0;
        // drvals[9] = 0;
        // drvals[14] = 1;
        sig = 0;
        lfc1 = 2;
        lfc2 = 3;
        wxcols[0] = 2;
        wxcols[1] = 10;
        wxcols[2] = 11;
        wxcols[3] = 7;
        wxcols[4] = 15;
        wxopt = 1;
        ptflg = 0;
        ptopt = 1;
        ptden = 5;
        ptang = 0;
        statflg = false;
        prstr = null;
        prlnum = 8;
        prbnum = 1;
        prudef = 0;
        dwrnflg = true;
        xexflg = 0;
        yexflg = 0;
        cachesf = 1;
        // dblen = 12;
        // dbprec = 6;
        loopflg = 0;
        aaflg = 1;
        //xgeom[0] = '\0';
        gxdopt = "";
        gxpopt = "Cairo";
    }
}