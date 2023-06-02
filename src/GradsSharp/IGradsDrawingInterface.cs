namespace GradsSharp;

/// <summary>
/// Interface that gives direct access to drawing commands
/// </summary>
public interface IGradsDrawingInterface
{
    /// <summary>
    /// Set the current drawing color
    /// </summary>
    /// <param name="clr">Color number</param>
    void SetDrawingColor(int clr);
    /// <summary>
    /// Move the drawing pen to a new point with clipping.
    /// Clipping is implmented coarsely, where any move or draw point that is outside the
    /// clip region is not plotted.     
    /// </summary>
    /// <param name="x">X-Coordinate in inches</param>
    /// <param name="y">Y-Coordinate in inches</param>
    void MoveToPoint(double x, double y);
    /// <summary>
    /// Draw line from current point to x,y with clipping
    /// </summary>
    /// <param name="x">X-Coordinate in inches</param>
    /// <param name="y">Y-Coordinate in inches</param>
    void DrawLineToPoint(double x, double y);
    
    /// <summary>
    /// Plot a color filled rectangle.
    /// </summary>
    /// <param name="xlo">Low x</param>
    /// <param name="xhi">High x</param>
    /// <param name="ylo">Low y</param>
    /// <param name="yhi">High y</param>
    void DrawFilledRectangle(double xlo, double xhi, double ylo, double yhi);
    
    /// <summary>
    /// Get the number of shades after doing a contour fill.  Can be used to draw a colorbar
    /// </summary>
    int ShadeCount { get; }
    /// <summary>
    /// Get the labels for each shade color used during the contour fill
    /// </summary>
    double[] ShadeLevels { get; }
    /// <summary>
    /// Get the color numbers for each color used during the contour fille
    /// </summary>
    int[] ShadeColors { get; }
}