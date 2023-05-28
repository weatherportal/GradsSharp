using GradsSharp.Data;
using GradsSharp.Models;

namespace GradsSharp.Drawing;

/* Stack to evaluate the expression.  The stack consists of an
   array of structures.                                               */
internal class smem {
    internal int type;        /* Entry type: -2 stn,-1 grid,1=op,2='(',3=')'    */
    public class sobj {
        public int op;        /* Operator: 0=*, 1=/, 2=+                        */
        public gagrid pgr; /* Operand (grid or stn)                      */
        public gastn stn;
        
    }

    public sobj obj = new();
};



/* GA status structure.  Contains necessary info about the scaling
   and file structure in force.                                       */
internal class gastat {
    public List<gafile> pfi1;       /* Pointer to first gafile in chain      */
    public gafile pfid;       /* Pointer to default gafile             */
    public List<gadefn> pdf1 = new List<gadefn>();       /* Pointer to first define block         */
    //gaclct pclct;     /* Pointer to the collection pointers    */
    public gadata result = new gadata();       /* Result goes here                      */
    public dt tmin = new dt(),tmax = new dt();
    public double[] dmin = new double[5],dmax = new double[5];  /* Range of absolute dimensions          */
    public int fnum;                /* Default file number                   */
    public int type;                /* Result type (grid==1 or stn==0)       */
    public int idim,jdim;           /* Varying dimensions                    */
};

class gxcntr {
    public double labsiz;             /* Size of contour label, plotting inches */
    public int spline;                /* Spline fit flag - 0 no, 1 yes */
    public int ltype;                 /* Label type (off, on, masked, forced */
    public int mask;                  /* Label masking flag - 0 no, 1 yes */
    public int labcol;                /* Override label color, -1 uses contour color */
    public int labwid;                /* Override label width, -1 uses contour width,
                                         -999 does double plot */
    public int ccol;                  /* Contour color */
    public string label;                 /* Contour label */
    public double val;                /* Contour value */
    public int shpflg;                /* flag for shapfiles */
};

class gxclbuf {
    public int len;                     /* Number of contour points */
    public int color, style, width, sfit;  /* Output options for this line */
    public double[]? lxy;                 /* Line points, x,y number len */
    public double val;                  /* contour level value */
};

class gaindx
{
    int type; /* Indexing file type */
    int hinum; /* Number of header ints */
    int hfnum; /* Number of header floats */
    int intnum; /* Number of index ints (long) */
    int fltnum; /* Number of index floats */
    int[] hipnt; /* Pointer to header int values */
    float[] hfpnt; /* Pointer to header float values */
    int[] intpnt; /* Pointer to index int values */
    float[] fltpnt; /* Pointer to index float values */
};

class gaindxb
{
    int bignum; /* Number of off_t values */
    long[] bigpnt; /* Pointer to off_t values */
};

/* classures for GRIB2 data */
class gag2indx
{
    int version; /* Version number: 
				   1: int offsets  
				   2: off_t offsets 
				   3: new header elements, including off_t flag */

    int bigflg; /* off_t offsets in use */
    int trecs; /* Number of records (XY grids) per time step */
    int tsz, esz; /* Sizes of T and E dimensions */
    int g2intnum; /* Number of index offset values */
    int[] g2intpnt; /* Pointer to index g2ints */
    long[] g2bigpnt; /* Pointer to record offsets when off_t offsets in use */
}

class gaattr
{
    public string varname; /* Name of variable or 'global' */
    public string name; /* Name of attribute -- e.g. "units" */
    public string type; /* Type of attribute -- e.g. "String", "Float32", etc. */
    public int nctype; /* NetCDF (or HDF) data type index value */
    public int len; /* Length of this attribute */
    public int fromddf; /* Flag for attributes from descriptor file */
    public object[] value; /* Attribute value -- strings may contains blanks. */
};

/* classure for ensemble metadata */
class gaens
{
    string name; /* name of ensemble */
    int length; /* length of time axis */
    dt tinit; /* initial time */
    int gt; /* initial time in grid units */
    int[] grbcode; /* grib2 codes */
};

class gachsub
{
    int t1; /* First time for this substitution */
    int t2; /* Last time.  -99 indicates open ended */
    char[] ch; /* Substitution string */
};

class gabufr_varinf
{
    int scale;
    int offset;
    int width;
    int datatype; /* flag to indicate numerical or string data */
    string description;
}

class gabufr_val
{
    int x; /* BUFR ID (F,X,Y) */
    int y; /* BUFR ID (F,X,Y) */
    int z; /* replication offset (vert. level), if present, or -1 */
    char undef; /* set to GABUFR_UNDEF if packed data was all ones */
    double val; /* data value when datatype is NUM, or DBL_MIN otherwise */
    string sval; /* data value when datatype is STR, or NULL otherwise*/
}

class gabufr_tbl_inf
{
    int bufr_edition;
    int master_tbl_num;
    int master_tbl_version;
    int local_tbl_version;
}

class gabufr_msg
{
    int year; /* base time for entire message */
    int month;
    int day;
    int hour;
    int min;
    int subcnt; /* number of subsets */

    gabufr_val[] subs; /* array of linked lists, with size nsub; 
			    one linked list per subset in message */

    int fileindex; /* index of message in file, just for reference */

    int is_new_tbl; /* if 0, message contains data, otherwise it's a
		            replacement BUFR table */

    /* remainder for use during parsing */
    byte[] section0;
    byte[] section1;
    byte[] section3;
    byte[] section4;
    byte[] end;
    gabufr_tbl_inf tbl_inf;
}

/* classure that contains the x,y pairs for bufr time values */
class bufrtimeinfo
{
    int[] yrxy;
    int[] moxy;
    int[] dyxy;
    int[] hrxy;
    int[] mnxy;
    int[] scxy;
};

/* classure that contains the x,y pairs for file-wide bufr variables */
class bufrinfo
{
    int[] lonxy;
    int[] latxy;
    int[] levxy;
    int[] stidxy;
    bufrtimeinfo s_base, s_offset; /* classures for base and offset time values */
};

class gabufr_dset
{
    gabufr_msg[] msgs; /* linked list of decoded messages 
			(some may be missing if parsing failed */

    int msgcnt; /* number of messages in file */

/* remainder for use during parsing */
    object[] buf;
    int len;
};

internal class gafile
{

    
    public IGriddedDataReader DataReader { get; set; }
    
    int fseq; /* Unique sequence number for cache detection */
    public string name; /* File name or URL                      */
    string tempname; /* File name of open file (differs with templates) */
    string dnam; /* Descriptor file name                  */

    string mnam; /* Map(index) file name */

    public Stream? infile;              /* File pointer.                         */
    public int type; /* Type of file:  1 = grid
                                               2 = simple station
                                               3 = mapped station
                                               4 = defined grid       */

    string title; /* Title -- describes the file.          */
    public double undef; /* Global undefined value for this file  */
    double ulow, uhi; /* Undefined limits for missing data test  */

    public float[] sbuf; /* Buffer for file I/O equal in length
                                to the size needed to hold
                                the largest station report            */

    public double[] rbuf; /* Buffer for file I/O equal in length
                                to one grid row in the file           */

    char[] pbuf; /* Same as rbuf, for unpacking           */
    char[] bbuf; /* Same as rbuf, for bit map I/O         */
    public byte[] ubuf; /* Same as rbuf, for undef mask          */
    int bswap; /* Byte swapping needed */
    int dhandle; /* libgadap file handle.                 */

    int[] dapinf; /* pointer to coordinate variable indices
				(first four elements are lon,lat,lev,time
				fifth is station id)
				for opendap station data only */

    int mtype; /* Stn map file type                     */

    int[] tstrt; /* Pointer to list length dnum[3] of
                                start points of times in the file     */

    int[] tcnt; /* Count of stns for assctd time         */

    int stcnt; /* Count of mapped stids when stn data
                                and map file is type stidmap.         */

    int stpos; /* Position in map file of start of
                                stid info for map file type stidmap.  */

    public Stream? mfile;               /* File pointer to stidmap file          */
    public int[] dnum; /* Dimension sizes for this file.        */
    int tlpflg; /* Circular file flag                    */
    int tlpst; /* Start time offset in circular file    */
    public int vnum; /* Number of variables.                  */

    int ivnum; /* Number of level independent variables
                                for station data file                 */

    int lvnum; /* Number of level dependent variables
                                for station data file                 */

    public List<gavar> pvar1; /* Pointer to an array of classures.
                                Each classure in the array has info
                                about the specific variable.          */

    gaens ens1; /* pointer to array of ensemble classures */

    public long gsiz; /* Number of elements in a grid (x*y)    */

    /* This is for actual grid on disk,
       not psuedo grid (when pp in force) */
    long tsiz; /* Number of elements in an entire time
                                  group (all variables at all levels
                                  for one time).                        */

    int trecs; /* Number of records (XY grids) per time
                                  group.                                */

    long fhdr; /* Number of bytes to ignore at file head*/
    public int wrap; /* The grid globally 'wraps' in X        */
    int seqflg, yrflg, zrflg; /* Format flags */
    public int ppflag; /* Pre-projected data in use */
    int pdefgnrl; /* Keyword 'general' used instead of 'file' */
    public int ppwrot; /* Pre-projection wind rotation flag */
    public int ppisiz, ppjsiz; /* Actual size of preprojected grid */

    double[] ppvals; /* Projection constants for pre-projected
                                  grids.  Values depend on projection. */

    int[] ppi; /* Pointers to offsets for pre-projected
                                  grid interpolation */

    double[] ppf; /* Pointers to interpolation constants
                                  for pre-projected grids */

    public double[] ppw; /* Pointer to wind rotation array */

    public Func<double[], double, double>[] gr2ab; /* Addresses of routines to do conversion
                                  from grid coordinates to absolute
                                  coordinates for X, Y, Z.  All Date/time
                                  conversions handled by gr2t.          */

    public Func<double[], double, double>[] ab2gr; /* Addresses of routines to do conversion
                                  from absolute coordinates to grid
                                  coordinates for X,Y,Z.  All date/time
                                  conversions handled by t2gr.          */


    public List<double[]> grvals; /* Pointers to conversion information for
                                  grid-to-absolute conversion routines. */

    public List<double[]> abvals = new(5); /* Pointers to conversion information for
                                  absolute-to-grid conversion routines. */

    public int[] linear; /* Indicates if a dimension has a linear
                                  grid/absolute coord transformation
                                  (Time coordinate always linear).      */

    public int[] dimoff = new int[5]; /* Dimension offsets for defined grids   */
    public int climo; /* Climatological Flag (defined grids)   */
    int cysiz; /* Cycle size for climo grids            */
    public int idxflg; /* File records are indexed; 1==grib,station 2==grib2 */
    int grbgrd; /* GRIB Grid type */
    public gaindx pindx; /* Index Strucure if indexed file */
    public gaindxb pindxb; /* Index Strucure if off_t offsets are being used */
    public gag2indx g2indx; /* Index Strucure for grib2 index file */

    public int tmplat; /* File name templating:
                                   3==templating on E and T 
                                   2==templating only on E 
                                   1==templating only on T, or when 
                                      ddf has 'options template', but no % in dset 
                                   0==no templating  */

    int[] fnums; /* File number for each time */
    int fnumc; /* Current file number that is open */
    int fnume; /* Current ensemble file number that is open */
    gachsub pchsub1; /* Pointer to first %ch substitution */
    int errcnt; /* Current error count */
    int errflg; /* Current error flag */
    public int ncflg; /* 1==netcdf  2==hdfsds */
    int ncid; /* netcdf file id */
    int sdid; /* hdf-sds file id */
    int h5id; /* hdf5 file id */
    int packflg; /* Data are packed with scale and offset values */
    int undefattrflg; /* Undefined values are retrieved individually  */
    string scattr; /* scale factor attribute name for unpacking data */
    string ofattr; /* offset attribute name for unpacking data */
    string undefattr; /* undef attribute name */
    string undefattr2; /* secondary undef attribute name */
    long xyhdr; /* Number of bytes to ignore at head of xy grids*/
    long xytrlr; /* Number of bytes to ignore at end of xy grids*/
    public int calendar; /* Support for 365-day calendars */
    int pa2mb; /* convert pressure values in descriptor file from Pa -> mb */
    int bufrflg; /* 1==dtype bufr */
    bufrinfo bufrinfo; /* x,y pairs from descriptor file */
    gabufr_dset[] bufrdset; /* pointer to parsed bufr data */
    gaattr attr; /* pointer to link list of attribute metadata */
    int nsdfdims;
    int[] sdfdimids;
    int[] sdfdimsiz;
    int time_type; /* temporary flag for SDF time handling */
    char[,] sdfdimnam = new char[100, 129];
    long cachesize; /* default netcdf4/hdf5 cache size */
    

    public gafile()
    {
        fseq = 0;
        name = "";
        tempname = null;
        dnam = null;
        mnam = null;
        type = 0;
        title = null;
        undef = 0;
        ulow = 0;
        uhi = 0;
        sbuf = new float[] { };
        rbuf = new double[] { };
        pbuf = new char[] { };
        bbuf = new char[] { };
        ubuf = new byte[] { };
        bswap = 0;
        dhandle = 0;
        dapinf = new int[5];
        mtype = 0;
        tstrt = new int[] { };
        tcnt = new int[] { };
        stcnt = 0;
        stpos = 0;
        dnum = new int[5];
        tlpflg = 0;
        tlpst = 0;
        vnum = 0;
        ivnum = 0;
        lvnum = 0;
        pvar1 = new List<gavar>();
        ens1 = default;
        gsiz = 0;
        tsiz = 0;
        trecs = 0;
        fhdr = 0;
        wrap = 0;
        seqflg = 0;
        yrflg = 0;
        zrflg = 0;
        ppflag = 0;
        pdefgnrl = 0;
        ppwrot = 0;
        ppisiz = 0;
        ppjsiz = 0;
        ppvals = new double[] { };
        ppi = new int[] { };
        ppf = new double[] { };
        ppw = new double[] { };
        gr2ab = new Func<double[], double, double>[5];
        ab2gr = new Func<double[], double, double>[5];
        grvals = new List<double[]>(5);
        grvals.Add(new double[6]);
        grvals.Add(new double[6]);
        grvals.Add(new double[6]);
        grvals.Add(new double[6]);
        grvals.Add(new double[6]);
        abvals = new List<double[]>(5);
        abvals.Add(new double[6]);
        abvals.Add(new double[6]);
        abvals.Add(new double[6]);
        abvals.Add(new double[7]);
        abvals.Add(new double[6]);
        linear = new int[5];
        dimoff = new int[5];
        climo = 0;
        cysiz = 0;
        idxflg = 0;
        grbgrd = 0;
        pindx = default;
        pindxb = default;
        g2indx = default;
        tmplat = 0;
        fnums = new int[] { };
        fnumc = 0;
        fnume = 0;
        pchsub1 = default;
        errcnt = 0;
        errflg = 0;
        ncflg = 0;
        ncid = 0;
        sdid = 0;
        h5id = 0;
        packflg = 0;
        undefattrflg = 0;
        scattr = null;
        ofattr = null;
        undefattr = null;
        undefattr2 = null;
        xyhdr = 0;
        xytrlr = 0;
        calendar = 0;
        pa2mb = 0;
        bufrflg = 0;
        bufrinfo = default;
        bufrdset = new gabufr_dset[] { };
        attr = default;
        nsdfdims = 0;
        sdfdimids = new int[] { };
        sdfdimsiz = new int[] { };
        time_type = 0;
        cachesize = 0;
    }
};

class garpt
{
    string stid; /* Station id                           */
    double lat, lon, lev, tim; /* Location of station                  */
    int work; /* Work area                            */
    double val; /* Value of variable                    */
    char umask; /* Undef mask                           */
};

class gavar
{
    string varnm; /* Variable description.                */
    public string abbrv; /* Variable abbreviation.               */
    string longnm; /* netcdf/hdf var name if different     */

    double[] units; /* Units indicator.                     
				  Vals 0-7 are for variable codes:
				  grib, non-float data, and nc/hdf dims;
				  Vals 8-15 are for grib level codes;
			          Vals 16-48 are for extra grib2 codes */

    int g2aflg; /* var requires additional grib2 codes  */

    public long offset; /* Offset in grid elements of the start
                                  of this variable within a time group
                                  within this file.                    */

    int recoff; /* Record (XY grid) offset of the start
                                  of this variable within a time group */

    int ncvid; /* netcdf vid for this variable         */
    int sdvid; /* hdf vid for this variable            */
    int h5vid; /* hdf5 dataset id for this variable    */

    public int levels; /* Number of levels for this variable.
                                  0 is special and indiates one grid is
                                  available for the surface only.      */

    int dfrm; /* format  type indicator
  				  1 - unsigned char
				  4 - int  			       */

    int var_t; /* variable t transform                 */
    double scale; /* scale factor for unpacking data      */
    double add; /* offset value for unpacking data      */
    double undef; /* undefined value                      */
    double undef2; /* secondary undefined value            */
    public int vecpair; /* Variable has a vector pair           */
    public int isu; /* Variable is the u-component of a vector pair */
    int isdvar; /* Variable is a valid data variable (for SDF files) */
    int nvardims; /* Number of variable dimensions        */
    int nh5vardims; /* Number of variable dimensions for hdf5 */
    int[] vardimids = new int[100]; /* Variable dimension IDs. 	       */
    
    public VariableDefinition? variableDefinition { get; set; }

    public gavar()
    {
        varnm = null;
        abbrv = null;
        longnm = null;
        units = new double[48] ;
        g2aflg = 0;
        offset = 0;
        recoff = 0;
        ncvid = -999;
        sdvid = -999;
        h5vid = -999;
        levels = 0;
        dfrm = 0;
        var_t = 0;
        scale = 0;
        add = 0;
        undef = -9.99e8;
        undef2 = 0;
        vecpair = -999;
        isu = 0;
        isdvar = 0;
        nvardims = 0;
        nh5vardims = 0;
    }
    // #if USEHDF5==1
//   hid_t h5varflg;              /* hdf5 variable has been opened */
//   hid_t dataspace;             /* dataspace allocated for hdf5 variable */
// #endif
};

class gastn
{
    garpt rpt; /* Address of start of link list        */
    int rnum; /* Number of reports.                   */
    garpt[] blks; /* ptrs to memory holding rpts      */

    gafile pfi; /* Address of the associated gafile
                                  classure to get the data from
                                  (requestor block only)               */

    double undef; /* Undefined value for this data.       */
    double smin, smax; /* Min and Max values for this data     */

    int idim, jdim; /* Varying dimensions for this data
                                 -1 = This dimension does not vary
                                  1 = X dimension (longitude)
                                  2 = Y dimension (lattitude)
                                  3 = Z dimension (pressure)
                                  4 = Time                           */

    double[] dmin, dmax; /* Dimension limits for each dimension
                                  (X,Y,Z) in world coords.
                                  Non-varying dimensions can have
                                  limits in this classure.           */

    int rflag; /* Get stations within specified radius in
                                  degrees of fixed lat and lon         */

    double radius; /* Radius */
    int sflag; /* Get specific station  */
    char[] stid; /* Station id to get */
    int tmin, tmax; /* Grid limits of time */

    double ftmin, ftmax; /* Float-valued grid limits of time, 
			  	  equivalent to dmin[3],dmax[3]         */

    double[] tvals; /* Pointer to conversion info for the
                                  time conversion routines.            */

    gavar pvar; /* Pointer to the classure with info
                                  on this particular variable.  If
                                  NULL, this grid is the result of
                                  an expression evaluation where the
                                  variable type is unkown.             */

    garpt prev; /* Used for allocating rpt classures   */
    garpt crpt;
    int rptcnt, blkcnt;
};

class gadata
{
    public gagrid? pgr;
    public gastn stn;
};

internal class mapopt
{
    public double lnmin, lnmax, ltmin, ltmax; /* Plot bounds */
    public int dcol, dstl, dthk; /* Default color, style, thickness */
    public int[] mcol, mstl, mthk; /* Arrays of map line attributes */
    public string mpdset; /* Map data set name */
}

class gadefn {
    public gafile pfi;          /* File Structure containing the data   */
    public string abbrv;              /* Abbreviation assigned to this        */
};

internal class gacmn
{
    public double[] dmin = new double[5];
    public double[] dmax = new double[5]; /* Current absolute coordinate limits    */

    public Func<double[], double, double> xgr2ab;
    public Func<double[], double, double> ygr2ab;
    public Func<double[], double, double> xab2gr;
    public Func<double[], double, double> yab2gr;

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
    public List<gafile>? pfi1;
    public gafile? pfid;
    public IVariableMapping _variableMapping;
    public int fnum; /* File count                            */
    public int dfnum; /* Default file number   */

    public int fseq; /* Unique sequence num for files opened  */

    public List<gadefn>? pdf1 = new();         /* Pointer to first define block         */
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
    public int prlnum; /* Number of values per record */
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
//   char *kmlname;             /* kml text file name */
//   int kmlflg;              /* kml output: 1==img, 2==contours */
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

internal class dt
{
    public long yr;
    public long mo;
    public long dy;
    public long hr;
    public long mn;
};

internal class gagrid : ICloneable
{
    public gafile pfile;
    /* Address of the associated gafile
                                    classure to get the data from
                                    (requestor block only)               */
    public double[] grid; /* Address of the grid.                 */

    public int mnum; /* Number of grids when a multiple
                                  grid result.  Note in this case, *grid
                                  points to more than one grid, with the
                                  "default" result being the 1st grid  */

    public int mtype; /* Type of multiple result grid         */
    public List<int> mnums; /* See mvals  */

    public List<double> mvals; /* Metadata associated with a multiple
                                  grid result.  What is here depends on
                                  the value of mtype.                  */

    public double undef; /* Undefined value for this grid.       */

    public double rmin, rmax; /* Minimum/Maximum grid value
                                  (rmin is set to the grid value when
                                  isiz=jsiz=1.  *grid points to here.) */

    public byte[] umask; /* Mask for undefined values in the grid */

    public byte umin, umax; /* Min/max undefined mask values. 
                                  (when isiz=jsiz=1, umin is set to the 
                                  mask value and *umask points to umin) */

    public int isiz, jsiz; /* isiz = number of elements per row.
                                  jsiz = number of rows.               */

    public int idim, jdim; /* Dimension of rows and columns.
                                  -1 = This dimension does not vary
                                   0 = X dimension (usually longitude)
                                   1 = Y dimension (usually lattitude)
                                   2 = Z dimension (usually pressure)
                                   3 = Time
                                   4 = Ensemble
                                  If both dimensions are -1, then the
                                  grid has one value, which will be
                                  placed in rmin.                      */

    public int iwrld, jwrld; /* World coordinates valid?             */

    public int[] dimmin = new int[5], dimmax = new int[5]; /* Dimension limits for each dimension
                                  (X,Y,Z,T,E) in grid units.           */

    public gavar pvar; /* Pointer to the classure with info
                                  on this particular variable.  If
                                  NULL, this grid is the result of
                                  an expression evaluation where the
                                  variable type is unkown.             */
    public string exprsn; /* If grid is a 'final' result, this
                                  will point to a character string that
                                  contains the original expression.    */
    public int alocf; /* Scaling info allocated for us only  */

    
    public Func<double[], double, double>? igrab;
    public Func<double[], double, double>? jgrab;
    /* Addresses of routines to perform
   grid-to-absolute coordinate
   transforms for this grid's i and j
   dimensions (unless i or j = 3).      */
    public Func<double[], double, double> iabgr;
    public Func<double[], double, double> jabgr;
    /* Absolute to grid conversion routines */
    
    public double[] ivals, jvals;  /* Conversion info for grid to abs      */
    public double[] iavals, javals;  /* Conversion info for abs to grid      */
    public int ilinr, jlinr; /* Indicates if linear transformation   */
    public int toff; /* Indicates if T dim values are forecast offsets */

    public IGriddedDataReader DataReader { get; set; }

    public object Clone()
    {
        gagrid clone = new gagrid
        {
            alocf = alocf,
            dimmax = dimmax,
            dimmin = dimmin,
            grid = grid,
            iabgr = iabgr,
            iavals = iavals,
            javals = javals,
            idim = idim,
            jdim = jdim,
            igrab = igrab,
            ilinr = ilinr,
            jlinr = jlinr,
            isiz = isiz,
            jsiz = jsiz,
            ivals = ivals,
            jvals = jvals,
            iwrld = iwrld,
            jwrld = jwrld,
            jabgr = jabgr,
            jgrab = jgrab,
            mnum = mnum,
            mnums = mnums,
            mtype = mtype,
            mvals = mvals,
            rmax = rmax,
            rmin = rmin,
            toff = toff,
            umask = umask,
            umax = umax,
            umin = umin,
            undef = undef
        };

        return clone;

    }
};

internal class mapprj
{
    public double lnmn, lnmx, ltmn, ltmx; /* Lat,lon limits for projections */
    public double lnref; /* Reference longitude            */
    public double ltref1, ltref2; /* Reference latitudes            */
    public double xmn, xmx, ymn, ymx; /* Put map in this page area      */
    public double axmn, axmx, aymn, aymx; /* Actual page area used by proj. */
}

class cxclock
{
    public int year;
    public int month;
    public int date;
    public int hour;
    public int minute;
    public int second;
    public string timezone;
    public string clockenv;
    public int julian_day;
    public int epoch_time_in_sec;
}


/* One of these gets allocated for each record in the file */

/* Record types:
                  1 - statement
                  2 - assignment
                  3 - while
                  4 - endwhile
                  5 - continue
                  6 - break
                  7 - if
                  8 - else
                  9 - endif
                  10 - return
                  11 - function  */

internal class gsrecd {
    
    public gsrecd refer; /* Position of end of code block */
    public gsfdef pfdf;  /* Pointer to file def for this record */
    public int pos;            /* Start of record */
    public int epos;           /* Position of start of expression, if any */
    public int num;              /* Record number in file */
    public int type;             /* Record type */
};

/* Following structure hold information on open files
   accessed via the read/write/close user callable functions */

internal class gsiob
{
    public gsiob? forw;
   public Stream file;              /* File pointer     */
   public  string name;              /* File name        */
   public int flag;                /* Status flag: 1-read 2-write  */
};

/* Following structure describes a file that has been read in
   to become part of the running script.  */

internal class gsfdef {
    public gsrecd precd; /* Record descriptor for start of this file */
    public string? name;           /* Text name of the file  */
    public string? file;           /* The contents of the file */
};

internal class gsfnc {
    
    public List<gsrecd> recd;     /* Record block for function   */
    public string name;           /* Name of function            */
};


/* Following structure is a member of a link list providing the
   current value of a variable.    */

internal class gsvar
{
    public gsvar? forw;
    public string? name;           /* Variable name               */
    public string? strng;             /* Value of variable           */
};

/* Following structure holds global pointers needed by all the
   gs routines, and anchors most global memory allocations */


internal class gscmn {
    public List<gsfdef>? ffdef;    /* Head of input file link list */
    public gsfdef? lfdef;    /* Last in chain of input files */
    public List<gsrecd>? frecd;    /* Head of record descriptor link list */   
    public gsrecd? lrecd;    /* Last in record list list */
    public gsvar? fvar;      /* Head of variable linklist   */
    public List<gsfnc>? ffnc;      /* Head of function list       */
    public gsiob? iob;       /* Head of file I/O list       */
    public gsvar? gvar;      /* Head of global var list     */
    public gsvar? farg;      /* Pointer to function arglist */
    public string? fname;             /* Pointer to user-entered file name   */
    public string? fprefix;           /* File name prefix for loading functions */
    public string ppath;             /* Private path for gsf loads */
    public string? rres;              /* Pointer to function result  */
    public string gsfnm;             /* Most recent file name read in */
    public int gsfflg;              /* Dynamic load script functions from files */
    public int rc;                  /* Exit value                  */
};


/* Stack to evaluate the expression.  The stack consists of an
   doubly linked list of structures.                              */

internal class stck {
    public stck? pforw;               /* Forward Pointer  */
    public stck? pback;               /* Backwards Pointer */
    public int type;        /* Entry type: 0=oprnd,1=oprtr,2='(',3=')'        */
    internal class tobj {
        int op;                         /* Operator */
        string strng;                    /* Operand  */
    }
    public tobj obj;
};