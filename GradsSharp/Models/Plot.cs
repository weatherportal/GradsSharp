using System.Collections.ObjectModel;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace GradsSharp.Models;

public class Plot
{

    private List<Chart> _charts = new ();

    internal void AddChart(Chart chart)
    {
        _charts.Add(chart);
    }
    public IReadOnlyCollection<Chart> Charts => _charts.AsReadOnly();

    public Orientation Orientation { get; internal set; } = Orientation.Landscape;
    public float AspectRatio { get; internal set; } = -999;
    
    
}