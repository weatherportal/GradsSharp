namespace GradsSharp.Models;

public class ChartLayer
{
    public Action<IDataAdapter>? DataAction { get; set; } 
    public int[]? Levels { get; set; }
    
    public GxOutSetting LayerType { get; set; }
    
    public string VariableToDisplay { get; set; }
}