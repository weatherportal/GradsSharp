using GradsSharp.Enums;

namespace GradsSharp.Models;

public class VariableDefinition
{
    /// <summary>
    /// Name of the variable
    /// </summary>
    public string VariableName { get; set; }
    /// <summary>
    /// Surface type
    /// </summary>
    public FixedSurfaceType HeightType { get; set; } = FixedSurfaceType.Missing;
    /// <summary>
    /// Height
    /// </summary>
    public double HeightValue { get; set; } = Double.NaN;

    /// <summary>
    /// Current file
    /// </summary>
    public int File { get; set; } = 1;
    
    public override string ToString()
    {
        return $"{VariableName}-{HeightType}-{HeightValue}";
    }
}