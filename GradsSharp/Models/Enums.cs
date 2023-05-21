namespace GradsSharp.Models;

public enum MapResolution
{
    Undefined = 0,
    LowResolution = 1,
    MediumResolution = 2,
    HighResolution = 3
    
}

public enum Orientation
{
    Landscape = 1,
    Portrait = 2
}

public enum ClearAction
{
    NoReset = 1,
    Events = 2,
    Graphics = 3,
    HBuff = 4,
    Rband = 6,
    SdfWrite = 8,
    Mask = 9,
    Shp = 10
}

public enum OutputFormat
{
    Undefined = 0,
    EPS = 1,
    PS = 2,
    PDF = 3,
    SVG = 4,
    PNG = 5,
    GIF = 6,
    JPEG = 7
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

public enum OnOffSetting
{
    On,
    Off
}

public enum Projection
{
    Off = 0,
    Scaled = 1,
    Latlon = 2,
    Nps = 3,
    Sps = 4,
    Robinson = 5,
    Mollweide = 6,
    Ortographic = 7,
    Lambert = 13
}

public enum GxOutSetting
{
    Contour = 1,
    Shaded = 2,
    Shade1 = 3,
    Shade2 = 4,
    Shade2b = 5,
    Grid = 6,
    Vector = 7,
    Scatter = 8,
    FGrid = 9,
    FWrite = 10,
    Stream = 11,
    GridFill = 12,
    GeoTiff = 13,
    Kml = 14,
    Imap = 15,
    Shape = 16,
    StationValues=17,
    Barb = 18,
    FindStation = 19,
    Model = 20,
    WXSymbol = 21,
    StationMark = 22,
    StationWrt = 23,
    Line = 24,
    Stat = 25,
    Print = 26,
    TimeSeriesWeatherSymbols = 27,
    TimeSeriesBarb = 28,
    Bar = 29,
    ErrorBar = 30,
    LineFill = 31,
}

public enum LabelOption {

    On,
    Off,
    Masked,
    Forced,
    Auto
    
}

public enum GridOption
{
    On = 1,
    Off = 0,
    Horizontal = 2,
    Vertical = 3
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

public enum SmoothOption
{
    On = 1,
    Off = 0,
    Linear = 2
}

public enum DataFormat
{
    GRIB2
}