using GradsSharp.Models;

namespace GradsSharp.Builders;

public class ChartBuilder
{

    private Chart _chart = new();

    public ChartBuilder AddLayer(Func<ChartLayerBuilder, ChartLayerBuilder> chartLayerBuilder)
    {
        _chart.Layers.Add(chartLayerBuilder(new ChartLayerBuilder()).Build());
        return this;
    }
    
    internal Chart Build()
    {
        return _chart;
    }

    public ChartBuilder WithTextBlock(TextOutput block)
    {
        _chart.TextBlocks.Add(block);
        return this;
    }
    public ChartBuilder WithMapResolution(MapResolution resolution)
    {
        _chart.Resolution = resolution;
        return this;
    }
    
    public ChartBuilder WithMapColor(int color)
    {
        _chart.MapColor = color;
        return this;
    }

    public ChartBuilder WithAxisLabels(Axis axis, AxisLabelOption labelOption, string? format)
    {
        if (axis == Axis.X)
        {
            _chart.AxisLabelFormatX = format;
            _chart.AxisLabelOptionX = labelOption;
        }
        if (axis == Axis.Y)
        {
            _chart.AxisLabelFormatY = format;
            _chart.AxisLabelOptionY = labelOption;
        }
        return this;
    }
    
    public ChartBuilder WithGridSetting(GridOption option)
    {
        _chart.GridSetting = option;
        return this;
    }

    public ChartBuilder WithLatitudeRange(double latMin, double latMax)
    {
        _chart.LatitdeMin = latMin;
        _chart.LatitudeMax = latMax;
        return this;
    }

    public ChartBuilder WithLongitudeRange(double lonMin, double lonMax)
    {
        _chart.LongitudeMin = lonMin;
        _chart.LongitudeMax = lonMax;
        return this;
    }


    public ChartBuilder WithColorBarSettings(ColorBarSettings settings)
    {
        _chart.ColorBarSettings = settings;
        return this;
    }


    
}