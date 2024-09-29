namespace GradsSharp.Models.Internal;

internal struct GradsPoint
{
    public GradsPoint(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public double x { get; }
    public double y { get; }
}