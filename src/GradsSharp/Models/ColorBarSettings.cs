using GradsSharp.Enums;

namespace GradsSharp.Models;

public class ColorBarSettings
{
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double XOffset { get; set; }
    public double YOffset { get; set; }
    public double FontWidth { get; set; } = 0.12;
    public double FontHeight { get; set; } = 0.13;
    public double FontThickness { get; set; } = 0.13 * 40;
    public double LabelInterval { get; set; } = 1;
    public string LabelOffset { get; set; } = "0";
    public double LabelXOffset { get; set; }
    public double LabelYOffset { get; set; }
    public int LabelColorNumber { get; set; } = 1;
    public string LabelFormat { get; set; }
    public ColorBarDirection Direction { get; set; } = ColorBarDirection.Auto;
    public OnOffSetting Lines { get; set; } = OnOffSetting.On;
    public int LineColor { get; set; } = 1;
    
    public string Caption { get; set; }

}