using GradsSharp.Enums;
using GradsSharp.Models;

namespace GradsSharp.Data;

public interface IVariableMapping
{
    object GetVariableInfo(string variable);
    string GetGrib2VarType(int disciple, int paramCategory, int paramNumber);
}