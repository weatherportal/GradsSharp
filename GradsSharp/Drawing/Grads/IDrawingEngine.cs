namespace GradsSharp.Drawing.Grads;

public interface IDrawingEngine
{
    void gxpcfg();
    bool gxpckfont();
    void gxpbgn(double xsz, double ysz);
    void gxpinit(double xsz, double ysz);
    void gxpend();
    /* Render the hardcopy output. 

    fnout -- output filename
    xin,yin -- image sizes (-999 for non-image formats)
    bwin -- background color
    fmtflg -- output format 
    bgImage, fgImage -- background/foreground image filenames
    tcolor -- transparent color 
*/

    int gxprint(string fnout, int xin, int yin, int bwin, int fmtflg,
        string bgImage, string fgImage, int tcolor, double border);

    void gxpcol(int col);
    void gxpacol(int col);
    void gxpwid(int wid);
    void gxprec(double x1, double x2, double y1, double y2);
    void gxpbpoly();
    void gxpepoly(double[] xybuf, int xyc);
    void gxpmov(double xpos, double ypos);
    void gxpdrw(double xpos, double ypos);
    void gxpflush();
    void gxpsignal(int sig);
    double gxpch(char ch, int fn, double x, double y, double w, double h, double rot);
    double gxpqchl(char ch, int fn, double w);
    void gxpclip(double x1, double x2, double y1, double y2);
}