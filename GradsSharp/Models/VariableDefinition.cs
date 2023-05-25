namespace GradsSharp.Models;

public class VariableDefinition
{
    public string VariableName { get; set; }
    public DataVariable VariableType { get; set; }
    public FixedSurfaceType HeightType { get; set; }
    public double HeightValue { get; set; }
}