namespace GradsSharp.Enums;

/// <summary>
/// Options for drawing grid lines
/// </summary>
public enum GridOption
{
    /// <summary>
    /// Default option.  Grid lines are drawn
    /// </summary>
    On = 1,
    /// <summary>
    /// No grid lines are drawn
    /// </summary>
    Off = 0,
    /// <summary>
    /// Only the latitude lines are drawn
    /// </summary>
    Horizontal = 2,
    /// <summary>
    /// Only the longitude lines are drawn
    /// </summary>
    Vertical = 3
}