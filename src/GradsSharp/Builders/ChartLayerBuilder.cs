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
    
    public ChartLayerBuilder WithLevels(double[] levels)
    {
        _chartLayer.Levels = levels;
        return this;
        
    }

    public ChartLayerBuilder WithColors(int[] colors)
    {
        _chartLayer.Colors = colors;
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

    public ChartLayerBuilder WithContourThickness(int thickness)
    {
        _chartLayer.ContourThickness = thickness;
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
    
    
    public ChartLayerBuilder WithColorShading(double? min, double? max, double? interval, GxOutSetting gxout, string? kind)
    {
        _chartLayer.ColorShadingMin = min;
        _chartLayer.ColorShadingMax = max;
        _chartLayer.ColorShadingInterval = interval;
        _chartLayer.ColorShadingKind = kind;
        _chartLayer.ColorShadingGxOut = gxout;
        _chartLayer.ColorShadingEnabled = true;
        return this;
    }

    public ChartLayerBuilder WithColorDefinition(int colorNumber, int red, int green, int blue, int alpha = 0)
    {
        _chartLayer.ColorDefinitions.Add((colorNumber, red, green, blue, alpha));
        return this;
    }
    
    public ChartLayerBuilder WithColorDefinition(List<ColorDefinition> colorDefinitions)
    {
        _chartLayer.ColorDefinitions.AddRange(colorDefinitions);
        return this;
    }
}