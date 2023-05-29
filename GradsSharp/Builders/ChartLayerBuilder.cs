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

    public ChartLayerBuilder WithContourStyle(LineStyle style)
    {
        _chartLayer.LineStyle = style;
        return this;
    }

    public ChartLayerBuilder WithContourInterval(double interval)
    {
        _chartLayer.ContourInterval = interval;
        return this;
    }

    public ChartLayerBuilder WithLabelOption(LabelOption option)
    {
        _chartLayer.ContourLabelOption = option;
        return this;
    }

    public ChartLayerBuilder WithContourMin(double min)
    {
        _chartLayer.ContourMin = min;
        return this;
    }
    public ChartLayerBuilder WithContourMax(double max)
    {
        _chartLayer.ContourMax = max;
        return this;
    }

    public ChartLayerBuilder WithContourColor(int clr)
    {
        _chartLayer.ContourColor = clr;
        return this;
    }
}