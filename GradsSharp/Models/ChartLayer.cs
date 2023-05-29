namespace GradsSharp.Models;

public class ChartLayer
{
    public Action<IDataAdapter>? DataAction { get; set; } 
    public int[]? Levels { get; set; }

    public GxOutSetting LayerType { get; set; } = GxOutSetting.Contour;
    public LineStyle LineStyle { get; set; } = LineStyle.NoContours;
    public double? ContourInterval { get; set; }
    public LabelOption? ContourLabelOption { get; set; } = LabelOption.Off;
    public double? ContourMin { get; set; }
    public double? ContourMax { get; set; }
    
    public int? ContourColor { get; set; }
    public string VariableToDisplay { get; set; }
}