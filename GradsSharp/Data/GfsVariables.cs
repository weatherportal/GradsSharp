using GradsSharp.Models;

namespace GradsSharp.Data;

public class GfsVariables : IVariableMapping
{
    private Dictionary<DataVariable, string> _mapping;

    public GfsVariables()
    {

        _mapping = new Dictionary<DataVariable, string>();

        _mapping[DataVariable.Temperature] = "Temperature";
        _mapping[DataVariable.RelativeHumidity] = "RelativeHumidity";

    }

    public string GetVariableName(DataVariable variable)
    {
        string result = "";
        if (_mapping.TryGetValue(variable, out result))
        {
            return result;
        }

        // should not happen
        throw new Exception($"Variable {variable} not found");
    }
}