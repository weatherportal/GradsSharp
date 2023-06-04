using Microsoft.VisualBasic;

namespace GradsSharp.Models;

public interface IDataAdapter
{
    double[] GetVariable(VariableDefinition definition);

    void DefineVariable(string name, double[] data);

    double GetLevMin();
    double GetLevMax();

    double[] GetMultiLevelData(VariableDefinition definition, double startlevel, double endLevel, MultiLevelFunction function); 
}