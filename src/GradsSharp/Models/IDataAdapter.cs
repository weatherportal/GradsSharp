namespace GradsSharp.Models;

public interface IDataAdapter
{
    double[] GetVariable(VariableDefinition definition);

    void DefineVariable(string name, double[] data);

    double GetLevMin();
    double GetLevMax();
}