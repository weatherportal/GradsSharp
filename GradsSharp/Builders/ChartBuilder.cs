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

    public ChartBuilder WithMapResolution(MapResolution resolution)
    {
        _chart.Resolution = resolution;
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
}