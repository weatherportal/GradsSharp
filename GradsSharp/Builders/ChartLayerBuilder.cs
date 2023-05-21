using GradsSharp.Models;

namespace GradsSharp.Builders;

public class ChartLayerBuilder
{

    private ChartLayer _chartLayer = new();

    internal ChartLayer Build()
    {
        return null;
    }

    public ChartLayerBuilder WithLevels(int[] levels)
    {
        _chartLayer.Levels = levels;
        return this;
        
    } 
    
}