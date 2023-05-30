namespace GradsSharp.Models;

public class ChartLayer
{
    public Action<IDataAdapter>? DataAction { get; set; } 
    public double[]? Levels { get; set; }
    public int[]? Colors { get; set; }
    public GxOutSetting LayerType { get; set; } = GxOutSetting.Contour;
    public LineStyle LineStyle { get; set; } = LineStyle.NoContours;
    public double? ContourInterval { get; set; }
    public LabelOption? ContourLabelOption { get; set; } = LabelOption.Off;
    public double? ContourMin { get; set; }
    public double? ContourMax { get; set; }
    
    public int? ContourThickness { get; set; }
    
    public int? ContourColor { get; set; }
    public string VariableToDisplay { get; set; }
    
    // color shading settings
    public double? ColorShadingMin { get; set; }
    public double? ColorShadingMax { get; set; }
    public double? ColorShadingInterval { get; set; }
    public GxOutSetting ColorShadingGxOut { get; set; }
    public string? ColorShadingKind { get; set; }
    public bool ColorShadingEnabled { get; set; } = false;

    public List<ColorDefinition> ColorDefinitions { get; set; } = new();


}