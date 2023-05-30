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
    
    // map settings
    public MapResolution Resolution { get; set; } = MapResolution.Undefined;
    public int? MapColor { get; set; }
    public GridOption GridSetting { get; set; } = GridOption.On;

    public AxisLabelOption AxisLabelOptionX { get; set; } = AxisLabelOption.On;
    public AxisLabelOption AxisLabelOptionY { get; set; } = AxisLabelOption.On;
    public string? AxisLabelFormatX { get; set; }
    public string? AxisLabelFormatY { get; set; }


    
    public ColorBarSettings? ColorBarSettings { get; set; }
    
    public List<ChartLayer> Layers { get; set; } = new();
    public List<TextOutput> TextBlocks { get; set; } = new();

}