namespace GradsSharp.Enums;

/// <summary>
/// Map resolution options
/// </summary>
public enum MapResolution
{
    /// <summary>
    /// Defaults to low resolution
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// Low resolution map (default)
    /// </summary>
    LowResolution = 1,
    /// <summary>
    /// Medium resolution: includes state and country lines
    /// </summary>
    MediumResolution = 2,
    /// <summary>
    /// High resolution: includes state and country lines
    /// </summary>
    HighResolution = 3
}