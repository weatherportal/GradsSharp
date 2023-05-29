using GradsSharp.Data;
using GradsSharp.Drawing;

namespace GradsSharp.Models.Internal;

internal class GradsFile
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
    

    public GradsFile()
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