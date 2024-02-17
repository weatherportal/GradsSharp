namespace GradsSharp.Enums;

/// <summary>
/// Type of preprojected grid
/// </summary>
public enum PreprojectedType
{
    /// <summary>
    /// Not preprojected
    /// </summary>
    None = 0,
    /// <summary>
    /// North Polar Stereographic
    /// </summary>
    NPS = 1,
    /// <summary>
    /// South Polar Stereographic
    /// </summary>
    SPS = 2,
    LCC = 3,
    LCCR = 4,
    ETAU = 5,
    PSE = 6,
    OPS = 7,
    BILIN = 8,
    FILE = 9,
    GENERAL = 10,
    /// <summary>
    /// Rotated lat lon grid
    /// </summary>
    ROTLL = 11,
    /// <summary>
    /// Rotated lat lon grid with pole at lower right
    /// </summary>
    ROTLLR = 12
}