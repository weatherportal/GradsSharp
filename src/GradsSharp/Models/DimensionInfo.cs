using GradsSharp.Enums;

namespace GradsSharp.Models;

/// <summary>
/// Dimension info returned by <see cref="IGradsCommandInterface.QueryDimensionInfo"/>
/// </summary>
public class DimensionInfo
{
    /// <summary>
    /// File number currently in use for fetching data
    /// </summary>
    public int DefaultFileNumber { get; set; }
    /// <summary>
    /// Type of dimension for X-axis
    /// </summary>
    public DimensionType DimensionTypeX { get; set; }
    /// <summary>
    /// Type of dimension for Y-axis
    /// </summary>
    public DimensionType DimensionTypeY { get; set; }
    /// <summary>
    /// Type of dimension for Z-axis
    /// </summary>
    public DimensionType DimensionTypeZ { get; set; }
    /// <summary>
    /// Type of dimension for T-dimension
    /// </summary>
    public DimensionType DimensionTypeT { get; set; }
    /// <summary>
    /// Type of dimension for E-dimension
    /// </summary>
    public DimensionType DimensionTypeE { get; set; }
    
    /// <summary>
    /// Minimum longitude value
    /// </summary>
    public double LonMin { get; set; }
    /// <summary>
    /// Maximum longitude value
    /// </summary>
    public double LonMax { get; set; }
    /// <summary>
    /// Minimum latitude value
    /// </summary>
    public double LatMin { get; set; }
    /// <summary>
    /// Maximum latitude value
    /// </summary>
    public double LatMax { get; set; }
    
    /// <summary>
    /// Current level
    /// </summary>
    public double Level { get; set; }
    
    /// <summary>
    /// Current time
    /// </summary>
    public DateTime Time { get; set; }
    public int Ensemble { get; set; }
    
    
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double ZMin { get; set; }
    public double ZMax { get; set; }
    public double TMin { get; set; }
    public double TMax { get; set; }
    public double EMin { get; set; }
    public double EMax { get; set; }
}