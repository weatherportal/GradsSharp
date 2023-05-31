namespace GradsSharp.Drawing.Grads;

/// <summary>
/// This interface can be used to implement a new drawing engine for Grads.  See all methods for more information on the different functions
/// </summary>
public interface IDrawingEngine
{
    
    void gxpcfg();
    bool gxpckfont();
    /// <summary>
    /// Initialize the drawing 
    /// </summary>
    /// <param name="xsz"></param>
    /// <param name="ysz"></param>
    void gxpinit(double xsz, double ysz);
    void gxpend();
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
    void gxpflush(int opt);
    void gxpsignal(int sig);
    double gxpch(char ch, int fn, double x, double y, double w, double h, double rot);
    double gxpqchl(char ch, int fn, double w);
    void gxpclip(double x1, double x2, double y1, double y2);
}