namespace GradsSharp.Enums;

/// <summary>
/// Controls the contour line labeling
/// </summary>
public enum LabelOption 
{
    /// <summary>
    /// 'fast' contour labels are plotted where the contour lines are horizontal
    /// </summary>
    On,
    /// <summary>
    /// No Contour labels are plotted
    /// </summary>
    Off,
    /// <summary>
    /// Contour lines have gaps for the labels, so rectangles for label background are not drawn; contour labels never overlap.
    /// </summary>
    Masked,
    /// <summary>
    /// An attempt is made to label all contour lines
    /// </summary>
    Forced,
    /// <summary>
    /// Reset the label format
    /// </summary>
    Auto
    
}