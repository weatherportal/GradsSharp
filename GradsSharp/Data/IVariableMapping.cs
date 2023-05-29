using GradsSharp.Models;

namespace GradsSharp.Data;

public interface IVariableMapping
{
    string GetVariableName(DataVariable variable);
    DataVariable GetVarType(string name);
}