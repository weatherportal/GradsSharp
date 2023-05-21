using GradsSharp.Models;

namespace GradsSharp.Builders;

public class PlotBuilder
{

    private Plot _plot = new Plot();
    
    public PlotBuilder AddChart(Func<ChartBuilder, ChartBuilder> chartBuilder)
    {
        _plot.AddChart(chartBuilder(new ChartBuilder()).Build());
        return this;
    }


    public Plot Build()
    {
        return _plot;
    }

    
    public PlotBuilder WithOrientation(Orientation orientation)
    {
        _plot.Orientation = orientation;
        return this;
    }

    public PlotBuilder WithAspectRatio(float aspect)
    {
        if (aspect <= 0.2 || aspect >= 5.0)
        {
            throw new ArgumentException("Aspect ratio must be between 0.2 and 5.0");
        }

        _plot.AspectRatio = aspect;
        return this;
    }

    public PlotBuilder WithDataFile(string fileName, DataFormat format)
    {
        _plot.AddDataFile(new DataFile { FileName = fileName, Format = format});
        return this;
    }
}