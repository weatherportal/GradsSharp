using GradsSharp.Drawing.Grads;

namespace GradsSharp.Data.Grads;

internal class GradsDatabase : IGradsDatabase
{
    static int[] pdcred = {  0,255,250,  0, 30,  0,240,230,240,160,160,  0,230,  0,130,170};
    static int[] pdcgre = {  0,255, 60,220, 60,200,  0,220,130,  0,230,160,175,210,  0,170};
    static int[] pdcblu = {  0,255, 60,  0,255,200,130, 50, 40,200, 50,255, 45,140,220,170};

    static double[] pdcwid = {0.6, 0.8, 1.0, 1.25, 1.5, 1.75, 2.0, 2.2, 2.4, 2.6, 2.8, 3.0};

    private int[] reds = new int[Gx.COLORMAX], greens = new int[Gx.COLORMAX], blues = new int[Gx.COLORMAX], alphas=new int[Gx.COLORMAX];
    private int[] tilenum = new int[Gx.COLORMAX];
    
    private int[] ptypes = new int[Gx.COLORMAX], pxsz = new int[Gx.COLORMAX], pysz = new int[Gx.COLORMAX];
    private int[] pfthick = new int[Gx.COLORMAX],pfcols = new int[Gx.COLORMAX],pbcols = new int[Gx.COLORMAX];
    private string[] pnames = new string[Gx.COLORMAX];
    
    
    private double[] widths = new double[256];
    private string[] fontname = new string[100];
    private string fn_serif = "serif";
    private string fn_sans = "sans-serif";
    private bool[] fnbold = new bool[100];
    private int[] fnitalic = new int[100];
    private int hershflag;                /* For fn 1 to 6, use Hershey fonts or not */
    private int dbdevbck;                 /* Device background color */
    private int dboutbck;                 /* Ouput (image or hardcopy) background color */
    private int dbtransclr;               /* transparent color number (for hardcopy) */

    public int gxdbqhersh () {    
        return (hershflag);
    }
    public void gxdbinit()
    {
        for (int i=0; i<Gx.COLORMAX; i++) {
            reds[i]   = 150; 
            greens[i] = 150;
            blues[i]  = 150;
            alphas[i] = 255;
            tilenum[i] = -999;
            ptypes[i] = -999;
            pnames[i] = null;
            pxsz[i] = 10; 
            pysz[i] = 10;
            pfthick[i] = 3; 
            pfcols[i] = -999; 
            pbcols[i] = -999;
        }
        for (int i=0; i<16; i++) {
            reds[i]   = pdcred[i];
            greens[i] = pdcgre[i];
            blues[i]  = pdcblu[i];
            alphas[i] = 255;
        }

        /* initialize line widths (default and user-defined) */
        for (int i=0; i<256; i++) widths[i] = 1.0; 
        for (int i=0; i<12; i++) widths[i] = pdcwid[i]; 

        /* Initialize font settings */
        for (int i=0; i<100; i++) {
            fontname[i] = null;
            fnbold[i] = false;
            fnitalic[i] = 0;
        }

        /* these will be for emulations of hershey fonts 0-5, but not 3 */
        fontname[0] = fn_sans;   fnbold[0] = false;  fnitalic[0] = 0; 
        fontname[1] = fn_serif;  fnbold[1] = false;  fnitalic[1] = 0;
        fontname[2] = fn_sans;  fnbold[2] = false;  fnitalic[2] = 0;
        fontname[4] = fn_sans;   fnbold[4] = true;  fnitalic[4] = 1;
        fontname[5] = fn_serif;  fnbold[5] = true;  fnitalic[5] = 1;

        /* other flags for this and that */
        hershflag  = 0;     /* zero, use hershey fonts.  1, use emulation. */  
        dbdevbck   = 1;     /* initial device background color is black */
        dboutbck   = -1;    /* initial output background color is 'undefined' */
        dbtransclr = -1;    /* initial transparent color is 'undefined' */
    }

    public (string, bool, int) gxdbqfont (int fn) {
        if (fn<0 || fn>99)
        {
            return (fontname[0], fnbold[0], fnitalic[0]);
        } else {
            return (fontname[fn], fnbold[fn], fnitalic[fn]);
        }
    }
    
    public int gxdbkq() {    
        /* If the output background color is not set, return device background color */
        if (dboutbck != -1) 
            return (dboutbck);
        else 
            return (dbdevbck);
    }
    
    public void gxdboutbck (int clr) {    
        dboutbck = clr;
    }

    public (int, int, int, int, int) gxdbqcol(int colr)
    {
        if (colr < 0 || colr > Gx.COLORMAX)
        {
            return (150, 150, 150, 255, -999);
        }
        else
        {
            return (reds[colr], greens[colr], blues[colr], alphas[colr], tilenum[colr]); 
        }
    }
    
    public int gxdbacol (int clr, int red, int green, int blue, int alpha) {
        if (clr<0 || clr>=Gx.COLORMAX) return(1); 
        if (red == -9) {
            /* this is a pattern */
            reds[clr] = red; 
            greens[clr] = green; 
            blues[clr] = blue;  
            alphas[clr] = 255;
            tilenum[clr] = green;
        } else {
            /* this is a color */
            reds[clr] = red;
            greens[clr] = green;
            blues[clr] = blue;
            alphas[clr] = alpha;
            tilenum[clr] = -999;
        }
        return 0;
    }
    
    public void gxdbsettransclr (int clr) {   
        dbtransclr = clr; 
    }

    public int gxdbqtransclr()
    {
        return dbtransclr;
    }
    

    public double gxdbqwid(int idx)
    {
        if (idx < 0 || idx > 255)
        {
            return 1.0;
        }
        return widths[idx];
    }
}