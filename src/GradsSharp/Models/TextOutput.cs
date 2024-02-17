

using GradsSharp.Enums;

namespace GradsSharp.Models;

public class TextOutput
{
    private double _width = 0.12;
    public string Text { get; set; }

    public double Width
    {
        get => _width;
        set
        {
            _width = value;
            Height = value;
        }
    }

    public double Height { get; set; } = 0.12;
    public int FontColor { get; set; } = 0;
    public double Rotation { get; set; } = 0;
    public StringJustification Justification { get; set; }
    public int Thickness { get; set; } = 1;
    public double LocationX { get; set; }
    public double LocationY { get; set; }
}