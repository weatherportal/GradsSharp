namespace GradsSharp.Enums;

public enum InterpolationMode
{
    Undefined = -1,
    PiecewiseLinear = 0,
    Spline = 1,
    Polynomnial = 2
}

public enum ConstMode
{
    Undefined,
    Missing,
    All
}

public enum InputFileDimensionType
{
    Linear,
    Levels
}

public enum ColorBarDirection
{
    Auto,
    Vertical,
    Horizontal
}

public enum IntervalType
{
    Interval,
    Fac
}

public enum Orientation
{
    Landscape = 1,
    Portrait = 2
}

public enum BackgroundColor
{
    Default = -999,
    Black = 0,
    White = 1
}

public enum GridLine
{
    Auto,
    Off,
    Color
}

public enum KmlOutputFlag
{
    Image = 1,
    Contour = 2,
    Polygon = 3
}

public enum LineStyle
{
    NoContours = 0,
    Solid = 1,
    LongDash = 2,
    ShortDash = 3,
    LongDashShortDash = 4,
    Dotted = 5,
    DotDash = 6,
    DotDotDash = 7
}

public enum StringJustification
{
    Undefined = 0,
    TopLeft = 6,
    TopCenter = 7,
    TopRight = 8,
    Left = 3,
    Center = 4, 
    Right = 5,
    BottomLeft = 0,
    BottomCenter = 1,
    BottomRight = 2
}

public enum AxisLabelOption
{
    On = 1,
    Off = 0,
    Format = 2
}


public enum Axis
{
    X,
    Y
}