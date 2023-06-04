namespace GradsSharp.Models;

public class DimensionInfo
{
    public int DefaultFileNumber { get; set; }
    public DimensionType DimensionTypeX { get; set; }
    public DimensionType DimensionTypeY { get; set; }
    public DimensionType DimensionTypeZ { get; set; }
    public DimensionType DimensionTypeT { get; set; }
    public DimensionType DimensionTypeE { get; set; }
    
    public double LonMin { get; set; }
    public double LonMax { get; set; }
    public double LatMin { get; set; }
    public double LatMax { get; set; }
    
    public double Level { get; set; }
    
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