namespace GradsSharp.Enums;

/// <summary>
/// Projection modes available
/// </summary>
public enum Projection
{
    /// <summary>
    /// No map is drawn
    /// </summary>
    Off = 0,
    /// <summary>
    /// Lat/lon aspect ratio is not maintained; plot fills entire plotting area
    /// </summary>
    Scaled = 1,
    /// <summary>
    /// Lat/lon projection with aspect ratio maintained (default)
    /// </summary>
    Latlon = 2,
    /// <summary>
    /// North polar stereographic
    /// </summary>
    Nps = 3,
    /// <summary>
    /// South polar stereographic
    /// </summary>
    Sps = 4,
    /// <summary>
    /// Robinson projection, requires <see cref="GradsSharp.IGradsCommandInterface.SetLon"/> -180 180, <see cref="GradsSharp.IGradsCommandInterface.SetLat"/> -90 90
    /// </summary>
    Robinson = 5,
    /// <summary>
    /// Mollweide projection
    /// </summary>
    Mollweide = 6,
    /// <summary>
    ///  Orthographic projection
    /// </summary>
    Ortographic = 7,
    /// <summary>
    /// Lambert conformal conic projection
    /// </summary>
    Lambert = 13
}