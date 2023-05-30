using GradsSharp.Models;

namespace GradsSharp.Data;

public interface IVariableMapping
{
    object GetVariableInfo(DataVariable variable);
    DataVariable GetGrib2VarType(int disciple, int paramCategory, int paramNumber);
}