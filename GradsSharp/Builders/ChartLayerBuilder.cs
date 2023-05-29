using GradsSharp.Models;

namespace GradsSharp.Builders;

public class ChartLayerBuilder
{

    private ChartLayer _chartLayer = new();

    internal ChartLayer Build()
    {
        return _chartLayer;
    }

    public ChartLayerBuilder WithLayerType(GxOutSetting setting)
    {
        _chartLayer.LayerType = setting;
        return this;
    }
    
    public ChartLayerBuilder WithLevels(int[] levels)
    {
        _chartLayer.Levels = levels;
        return this;
        
    }

    public ChartLayerBuilder WithDataFunction(Action<IDataAdapter> dataFunction)
    {
        _chartLayer.DataAction = dataFunction;
        return this;
    }

    public ChartLayerBuilder WithVariableToDisplay(string variable)
    {
        _chartLayer.VariableToDisplay = variable;
        return this;
    }
}