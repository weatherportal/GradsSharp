namespace GradsSharp.Models;

/// <summary>
/// Information about a variable in the input file
/// </summary>
public class InputVariable
{
    /// <summary>
    /// Abrreviation of the variable
    /// </summary>
    public string Abbreviation { get; set; }
    /// <summary>
    /// Variable definition
    /// </summary>
    public VariableDefinition Definition { get; set; }
    
}