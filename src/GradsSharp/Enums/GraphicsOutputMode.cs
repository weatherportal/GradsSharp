namespace GradsSharp.Enums;

/// <summary>
/// Graphics output mode
/// </summary>
public enum GraphicsOutputMode
{
    /// <summary>
    /// Contour plot
    /// </summary>
    Contour = 1,
    /// <summary>
    /// Shaded/filled contour plot, alias for original algorithm (shade1)
    /// </summary>
    Shaded = 2,
    /// <summary>
    /// Shaded/filled contour plot, original algorithm
    /// </summary>
    Shade1 = 3,
    /// <summary>
    /// Shaded/filled contour plot, new algorithm, polygons merged to be larger and fewer in number (faster to render)
    /// </summary>
    Shade2 = 4,
    /// <summary>
    /// Shaded contour plot, new algorithm, polygons all on sub-grid scale (slower to render)
    /// </summary>
    Shade2b = 5,
    /// <summary>
    /// Grid boxes with printed values
    /// </summary>
    Grid = 6,
    /// <summary>
    /// Wind vector arrows
    /// </summary>
    Vector = 7,
    /// <summary>
    /// Generates a scatter diagram. Requires two grid expressions as arguments, separated by a semicolon
    /// </summary>
    Scatter = 8,
    /// <summary>
    /// Shaded grid boxes
    /// </summary>
    FGrid = 9,
    /// <summary>
    /// Writes data to file instead of drawing a plot
    /// \remark currently not implemented
    /// </summary>
    FWrite = 10,
    /// <summary>
    /// Wind streamlines
    /// </summary>
    Stream = 11,
    /// <summary>
    /// Shaded grid boxes
    /// </summary>
    GridFill = 12,
    /// <summary>
    /// Generates a GeoTIFF format data file
    /// \remark currently not implemented
    /// </summary>
    GeoTiff = 13,
    /// <summary>
    /// Generates a KML file
    /// </summary>
    Kml = 14,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    Imap = 15,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    Shape = 16,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    StationValues=17,
    /// <summary>
    /// Wind barbs
    /// </summary>
    Barb = 18,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    FindStation = 19,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    Model = 20,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    WXSymbol = 21,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    StationMark = 22,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    StationWrt = 23,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    Line = 24,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    Stat = 25,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    Print = 26,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    TimeSeriesWeatherSymbols = 27,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    TimeSeriesBarb = 28,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    Bar = 29,
    /// <summary>
    /// \remark currently not implemented
    /// </summary>
    ErrorBar = 30,
    /// <summary>
    /// Color fill between two lines
    /// \remark currently not implemented
    /// </summary>
    LineFill = 31,
}