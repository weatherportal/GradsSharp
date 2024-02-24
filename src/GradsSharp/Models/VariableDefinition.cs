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
    
    
    public override bool Equals(object? obj)
    {
        if (obj is VariableDefinition other)
        {
            return VariableName == other.VariableName && HeightType == other.HeightType && Math.Abs(HeightValue - other.HeightValue) < 0.0001;
        }

        return false;
    }
    
    public static bool operator ==(VariableDefinition a, VariableDefinition b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(VariableDefinition a, VariableDefinition b)
    {
        return !(a == b);
    }
}