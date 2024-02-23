namespace GradsSharp.Models.Internal;

/* Stack to evaluate the expression.  The stack consists of an
   array of structures.                                               */
internal class smem {
    internal int type;        /* Entry type: -2 stn,-1 grid,1=op,2='(',3=')'    */
    public class sobj {
        public int op;        /* Operator: 0=*, 1=/, 2=+                        */
        public GradsGrid pgr; /* Operand (grid or stn)                      */
        public gastn stn;
        
    }

    public sobj obj = new();
};



/* GA status structure.  Contains necessary info about the scaling
   and file structure in force.                                       */
internal class gastat {
    public List<GradsFile> pfi1;       /* Pointer to first gafile in chain      */
    public GradsFile pfid;       /* Pointer to default gafile             */
    public List<gadefn> pdf1 = new List<gadefn>();       /* Pointer to first define block         */
    //gaclct pclct;     /* Pointer to the collection pointers    */
    public gadata result = new gadata();       /* Result goes here                      */
    public dt tmin = new dt(),tmax = new dt();
    public double[] dmin = new double[5],dmax = new double[5];  /* Range of absolute dimensions          */
    public int fnum;                /* Default file number                   */
    public int type;                /* Result type (grid==1 or stn==0)       */
    public int idim,jdim;           /* Varying dimensions                    */
};

internal class gxcntr {
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

internal class gxclbuf {
    public int len;                     /* Number of contour points */
    public int color, style, width, sfit;  /* Output options for this line */
    public double[]? lxy;                 /* Line points, x,y number len */
    public double val;                  /* contour level value */
};

internal class gaindx
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

internal class gaindxb
{
    int bignum; /* Number of off_t values */
    long[] bigpnt; /* Pointer to off_t values */
};

/* classures for GRIB2 data */
internal class gag2indx
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

internal class gaattr
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
internal class gaens
{
    string name; /* name of ensemble */
    int length; /* length of time axis */
    dt tinit; /* initial time */
    int gt; /* initial time in grid units */
    int[] grbcode; /* grib2 codes */
};

internal class gachsub
{
    int t1; /* First time for this substitution */
    int t2; /* Last time.  -99 indicates open ended */
    char[] ch; /* Substitution string */
};

internal class gabufr_varinf
{
    int scale;
    int offset;
    int width;
    int datatype; /* flag to indicate numerical or string data */
    string description;
}

internal class gabufr_val
{
    int x; /* BUFR ID (F,X,Y) */
    int y; /* BUFR ID (F,X,Y) */
    int z; /* replication offset (vert. level), if present, or -1 */
    char undef; /* set to GABUFR_UNDEF if packed data was all ones */
    double val; /* data value when datatype is NUM, or DBL_MIN otherwise */
    string sval; /* data value when datatype is STR, or NULL otherwise*/
}

internal class gabufr_tbl_inf
{
    int bufr_edition;
    int master_tbl_num;
    int master_tbl_version;
    int local_tbl_version;
}

internal class gabufr_msg
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
internal class bufrtimeinfo
{
    int[] yrxy;
    int[] moxy;
    int[] dyxy;
    int[] hrxy;
    int[] mnxy;
    int[] scxy;
};

/* classure that contains the x,y pairs for file-wide bufr variables */
internal class bufrinfo
{
    int[] lonxy;
    int[] latxy;
    int[] levxy;
    int[] stidxy;
    bufrtimeinfo s_base, s_offset; /* classures for base and offset time values */
};

internal class gabufr_dset
{
    gabufr_msg[] msgs; /* linked list of decoded messages 
			(some may be missing if parsing failed */

    int msgcnt; /* number of messages in file */

/* remainder for use during parsing */
    object[] buf;
    int len;
};

internal class garpt
{
    string stid; /* Station id                           */
    double lat, lon, lev, tim; /* Location of station                  */
    int work; /* Work area                            */
    double val; /* Value of variable                    */
    char umask; /* Undef mask                           */
};

internal class gavar
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
    
    public VariableDefinition? VariableDefinition { get; set; }

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

internal class gastn
{
    garpt rpt; /* Address of start of link list        */
    int rnum; /* Number of reports.                   */
    garpt[] blks; /* ptrs to memory holding rpts      */

    GradsFile pfi; /* Address of the associated gafile
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

internal class gadata
{
    public GradsGrid? pgr;
    public gastn stn;
};

internal class mapopt
{
    public double lnmin, lnmax, ltmin, ltmax; /* Plot bounds */
    public int dcol, dstl, dthk; /* Default color, style, thickness */
    public int[] mcol, mstl, mthk; /* Arrays of map line attributes */
    public string mpdset; /* Map data set name */
}

internal class gadefn {
    public GradsFile pfi;          /* File Structure containing the data   */
    public string abbrv;              /* Abbreviation assigned to this        */
};

internal class dt
{
    public long yr;
    public long mo;
    public long dy;
    public long hr;
    public long mn;
};

internal class mapprj
{
    public double lnmn, lnmx, ltmn, ltmx; /* Lat,lon limits for projections */
    public double lnref; /* Reference longitude            */
    public double ltref1, ltref2; /* Reference latitudes            */
    public double xmn, xmx, ymn, ymx; /* Put map in this page area      */
    public double axmn, axmx, aymn, aymx; /* Actual page area used by proj. */
}

internal class cxclock
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