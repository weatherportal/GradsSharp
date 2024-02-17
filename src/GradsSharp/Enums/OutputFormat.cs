namespace GradsSharp.Enums;

/// <summary>
/// Output image format
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Not defined. The output format will be determined by the file extension of the path parameter.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// EPS Output format
    /// </summary>
    EPS = 1,
    /// <summary>
    /// PS Output format
    /// </summary>
    PS = 2,
    /// <summary>
    /// PDF output format
    /// </summary>
    PDF = 3,
    /// <summary>
    /// SVG output format
    /// </summary>
    SVG = 4,
    /// <summary>
    /// PNG output format
    /// </summary>
    PNG = 5,
    /// <summary>
    /// GIF output format
    /// </summary>
    GIF = 6,
    /// <summary>
    /// JPEG output format
    /// </summary>
    JPEG = 7
}