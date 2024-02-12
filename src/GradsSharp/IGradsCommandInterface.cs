using GradsSharp.Data;
using GradsSharp.Enums;
using GradsSharp.Exceptions;
using GradsSharp.Models;
using FileInfo = GradsSharp.Models.FileInfo;

namespace GradsSharp;

/// <summary>
/// Interface for sending commands to the Grads engine
/// </summary>
public interface IGradsCommandInterface
{
    void stack();
    void reset(bool doReinit = false);
    
    /// <summary>
    /// Close the the last opened file
    /// </summary>
    void Close();
    
    /// <summary>
    /// Issued without parameters, the clear command does pretty heavy duty clearing of many of the GradsSharp internal settings. Parameters can be added to limit what is cleared when using more advanced features.
    /// </summary>
    void Clear(ClearAction? action);

    /// <summary>
    /// The printim command will produce a PNG, GIF, or JPG formatted image file based on the current contents of the metabuffer.
    /// </summary>
    /// <param name="path">The name of the output file. If this file exists, it will be replaced.<br/>
    ///     If the filename ends with ".png" or ".PNG" then GradsSharp will automatically create the image in PNG format<br/>
    ///     If the filename ends with ".gif" or ".GIF" then GradsSharp will automatically create the image in GIF format<br/>
    ///     If the filename ends with ".jpg" or ".JPG" then GradsSharp will automatically create the image in JPEG format
    /// </param>
    /// <param name="format">The format of the output file. If not specified, the format will be determined by the file extension of the path parameter.</param>
    /// <param name="backgroundImage">The name of the background image file.</param>
    /// <param name="foregroundImage">The name of the foreground image file.</param>
    /// <param name="backColor">The background color of the image. If not specified, the default background color will be used.</param>
    /// <param name="transparentColor">The color number to be treated as transparent.</param>
    /// <param name="horizontalSize">The horizontal size of the image in pixels. If not specified, the default size will be calculated.</param>
    /// <param name="verticalSize">The vertical size of the image in pixels. If not specified, the default size will be calculated.</param>
    /// <param name="borderWidth">The width of the border.</param>  
    void printim(string path, OutputFormat format = OutputFormat.Undefined, string backgroundImage = "",
        string foregroundImage = "",
        BackgroundColor backColor = BackgroundColor.Default, int transparentColor = -1,
        int horizontalSize = -999, int verticalSize = -999, double borderWidth = -1.0);

    void swap();
    void reinit();
    void gacln(int flg);
    
    /// <summary>Used with <see cref="SetGraphicsOutputMode"/> with <sGraphicsOutputModeutSetting.Grid"/> option to control the presence and appearance of the grid lines.</summary>
    /// <param name="gridLine">Controls the gridline setting</param>
    /// <param name="color">The color of the grid lines. If <see cref="GradsSharp.Models.GridLine.Color"/> is used</param>
    void SetGridLine(GridLine gridLine, int color = -1);
    
    /// <summary>
    /// This command defines a "virtual page" that fits within the specified limits of the real page. All graphics output will be drawn into this "virtual page" until another set SetVpage command is entered. A clear command clears the physical page (and any virtual pages drawn on it).
    /// When GradsEngine is first initialized, it prompts for landscape or portrait mode. This defines the size of the real page (11x8.5 or 8.5x11), and the dimensions for the virtual page must fit within this real page.
    /// The SetVpage command will define virtual page limits in terms of inches (virtual page inches), which are the coordinates that will be used in the various commands that require inches to be used. The new page limits are printed when the SetVpage command completes.
    /// To return to the default state where the real page equals the virtual page, enter:
    /// 
    /// </summary>
    /// <param name="page">VPage on or off</param>
    /// <param name="xlo"></param>
    /// <param name="xhi"></param>
    /// <param name="ylo"></param>
    /// <param name="yhi"></param>
    void SetVpage(OnOffSetting page, int xlo = 0, int xhi = 0, int ylo = 0, int yhi = 0);
    
    /// <summary>
    /// Define a new color or update an existing color
    /// </summary>
    /// <param name="colorNr">Integer number between 16 and 2048</param>
    /// <param name="red">Red value. Number between 0 and 255</param>
    /// <param name="green">Green value. Number between 0 and 255</param>
    /// <param name="blue">Blue value. Number between 0 and 255</param>
    /// <param name="alpha">Transparency value. Number between -255 and 255</param>
    /// <exception cref="InvalidColorException">Throws an exception when trying to define an invalid color</exception>
    void SetColor(int colorNr, int red, int green, int blue, int alpha = 255);
    void SetPrintingArea(OnOffSetting onOff, int xlo = 0, int xhi = 0, int ylo = 0, int yhi = 0);
    
    /// <summary>
    /// Sets the contour interval to the specified value. Reset by <see cref="Clear">Clear</see> or <see cref="GradsSharp.IGradsCommandInterface.Display">Display</see>.
    /// </summary>
    /// <param name="val">Contour interval value</param>
    void SetContourInterval(double val);

    
    /*
     * set mpvals
     */
    /// <summary>
    /// Sets reference longitudes and latitudes for polar stereographic plots. By default, these are set to the current dimension environment limits. This command overrides that, and allows the data-reference to be decoupled with the map display. The polar plot will be drawn such that the region bounded by these longitudes and latitudes will be entirely included in the plot.
    /// <br/><br/>
    /// GradsSharp will plot lat/lon lines on polar plots with no labels as yet. To turn this off, set grid off.
    /// </summary>
    /// <param name="onOff">Set polar stereographic on or off</param>
    /// <param name="lonmin">Minimum longitude</param>
    /// <param name="lonmax">Maximum longitude</param>
    /// <param name="latmin">Minimum latitude</param>
    /// <param name="latmax">Maximum latitude</param>
    void SetPolarStereoValues(OnOffSetting onOff, double lonmin = 0.0, double lonmax = 0.0, double latmin = 0.0,
        double latmax = 0.0);

    /// <summary>
    /// Sets specific contour levels.<br/>
    ///
    /// Usage notes:
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item><description>Contour levels are reset by <see cref="Clear">Clear</see> or <see cref="GradsSharp.IGradsCommandInterface.Display">Display</see>.</description></item>
    /// <item><description>Often used in conjunction with <see cref="GradsSharp.IGradsCommandInterface.SetContourColors">SetContourColors</see> to override the default settings and specify exact contour levels and the colors that go with them.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="levels">Array of levels</param>
    void SetContourLevels(double[] levels);
    
    /// <summary>
    /// Sets specific color numbers for contour levels specified by <see cref="GradsSharp.IGradsCommandInterface.SetContourLevels">SetContourLevels</see>.
    /// </summary>
    /// <param name="cols">Array of color numbers</param>
    void SetContourColors(int[] cols);
    
    
    /// <summary>
    /// Contours not drawn below this value. reset by <see cref="Clear">Clear</see> or <see cref="GradsSharp.IGradsCommandInterface.Display">Display</see>.
    /// </summary>
    /// <param name="cmin">Minimum contour level</param>
    void SetContourMinimum(double cmin);
    
    /// <summary>
    /// Contours not drawn above this value. reset by <see cref="Clear">Clear</see> or <see cref="GradsSharp.IGradsCommandInterface.Display">Display</see>.
    /// </summary>
    /// <param name="cmax">Maximum contour level</param>
    void SetContourMaximum(double cmax);
    
    
    void SetCMark(double cmark);
    
    
    /// <summary>
    /// Sets current map projection. See <see cref="Projection"/> for a list of available projections.
    /// </summary>
    /// <param name="projection">Projection mode</param>
    void SetMapProjection(Projection projection);
    
    /// <summary>
    /// Set map resolution. See <see cref="MapResolution"/> for a list of available resolutions.
    /// </summary>
    /// <param name="mapResolution">Map resolution</param>
    void SetMapResolution(MapResolution mapResolution);
    
    /// <summary>
    /// Set output mode for graphics. See <see cref="GraphicsOutputMode"/> for a list of available output modes.
    /// </summary>
    /// <param name="mode">Output mode</param>
    void SetGraphicsOutputMode(GraphicsOutputMode mode);
    
    
    void SetCLab(LabelOption option, string format = "");
    void SetXLab(LabelOption option, string format = "");
    void SetYLab(LabelOption option, string format = "");
    void SetAxisLabels(Axis axis, AxisLabelOption option, string? format);
    void SetGridOptions(GridOption option, int style = -1, int color = -1, int thickness = -1);
    void SetContourLabelOptions(int color = -1, int thickness = -1, int size = -1);

    void SetStringOptions(int color, StringJustification justification = StringJustification.Undefined,
        int thickness = -1, double rotation = -1);

    void SetGrads(OnOffSetting onOffSetting);
    void SetStringSize(double hsize, double vsize = -1);
    void SetCThick(int cthck);
    void SetCStyle(LineStyle style);
    void SetCColor(int color);
    void SetCSmooth(SmoothOption option);
    void SetLon(double min, double max = Double.MaxValue);
    void SetLat(double min, double max = Double.MaxValue);
    void SetLev(double lev);
    void SetX(double xMin, double xMax);
    void SetY(double yMin, double yMax);
    void SetZ(double zMin);
    void SetZ(double zMin, double zMax);
    void SetT(double tMin);
    void SetE(double eMin);
    void SetStreamDensity(int density);

    void SetXFlip(bool flip);
    void SetYFlip(bool flip);

    void Define(string varName, string formula);
    void Define(string varName, IGradsGrid data);
    void Open(string dataFile, IGriddedDataReader dataReader);
    int Display(string variable);
    int DrawString(double x, double y, string text);
    void SetMapOptions(int color, LineStyle? style, int? thickness);

    void SetDataAction(Action<IDataAdapter> dataAction);

    void SetPaperSize(double xsize, double ysize);

    void SetKmlOutput(KmlOutputFlag flag, string filename);

    DimensionInfo QueryDimensionInfo();

    FileInfo QueryFileInfo();

IGradsGrid GetVariable(VariableDefinition definition, int file = 1);

    IGradsGrid? GetMultiLevelData(VariableDefinition definition, double startLevel, double endLevel, MultiLevelFunction function); 

}