namespace GradsSharp.Models;

public class Chart
{
    // region to plot
    public double LatitdeMin { get; set; }
    public double LatitudeMax { get; set; }
    public double LongitudeMin { get; set; }
    public double LongitudeMax { get; set; }
    
    // plot area
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public bool PlotAreaDefined { get; set; } = false;

    public MapResolution Resolution { get; set; } = MapResolution.Undefined;

    public double? ColorBarMin { get; set; }
    public double? ColorBarMax { get; set; }
    public double? ColorBarInterval { get; set; }
    public GxOutSetting ColorBarGxOut { get; set; }
    public string? ColorBarKind { get; set; }

    public bool ColorBarEnabled { get; set; } = false;
    
    public string Title { get; set; }
    
    public List<ChartLayer> Layers { get; set; } = new List<ChartLayer>();

}