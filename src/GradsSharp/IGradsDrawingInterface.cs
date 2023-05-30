namespace GradsSharp;

public interface IGradsDrawingInterface
{
    void gxcolr(int clr);
    void gxmove(double x, double y);
    void gxdraw(double x, double y);
    void gxrecf(double xlo, double xhi, double ylo, double yhi);
    
    int ShadeCount { get; }
    double[] ShadeLevels { get; }
    int[] ShadeColors { get; }
}