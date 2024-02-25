namespace GradsSharp.Enums;

/// <summary>
/// Describes the options for drawing grid lines when using the grid output mode
/// </summary>
public enum GridLineOption
{
    /// <summary>
    /// The grid lines are drawn and are the same color as the text.
    /// </summary>
    Auto = 0,
    /// <summary>
    /// The grid lines are not drawn.
    /// </summary>
    Off = 1,
    /// <summary>
    /// The grid lines are drawn in the specified color
    /// </summary>
    Color = 2
}