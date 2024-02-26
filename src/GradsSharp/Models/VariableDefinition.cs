using GradsSharp.Data;
using GradsSharp.Enums;

namespace GradsSharp.Models;

/// <summary>
/// This class holds information needed to fetch variabbles from the data.  It is passed on the a <see cref="IGriddedDataReader"/> to fetch the data
/// </summary>
public class VariableDefinition
{
    /// <summary>
    /// Name of the variable
    /// </summary>
    public string VariableName { get; set; }
    /// <summary>
    /// First Surface type
    /// </summary>
    public FixedSurfaceType HeightType { get; set; } = FixedSurfaceType.Missing;
    
    /// <summary>
    /// Second Surface type
    /// </summary>
    public FixedSurfaceType SecondHeightType { get; set; } = FixedSurfaceType.Missing;
    
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
        return $"{VariableName}-{HeightType}-{SecondHeightType}-{HeightValue}";
    }
    
    
    public override bool Equals(object? obj)
    {
        if (obj is VariableDefinition other)
        {
            return VariableName == other.VariableName && HeightType == other.HeightType && SecondHeightType == other.SecondHeightType && Math.Abs(HeightValue - other.HeightValue) < 0.0001;
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