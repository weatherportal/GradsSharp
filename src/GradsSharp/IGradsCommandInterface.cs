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
    /// The ExportImage command will produce a PNG, GIF, or JPG formatted image file based on the current contents of the metabuffer.
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
    void ExportImage(string path, OutputFormat format = OutputFormat.Undefined, string backgroundImage = "",
        string foregroundImage = "",
        BackgroundColor backColor = BackgroundColor.Default, int transparentColor = -1,
        int horizontalSize = -999, int verticalSize = -999, double borderWidth = -1.0);

    void swap();
    void reinit();
    void gacln(int flg);
    
    /// <summary>Used with <see cref="SetGraphicsOutputMode"/> with <sGraphicsOutputModeutSetting.Grid"/> option to control the presence and appearance of the grid lines.</summary>
    /// <param name="gridLine">Controls the gridline setting</param>
    /// <param name="color">The color of the grid lines. If <see cref="GridLine.Color"/> is used</param>
    void SetGridLine(GridLine gridLine, int color = -1);
    
    /// <summary>
    /// This command defines a "virtual page" that fits within the specified limits of the real page. All graphics output will be drawn into this "virtual page" until another set SetVpage command is entered. A clear command clears the physical page (and any virtual pages drawn on it).
    /// When GradsEngine is first initialized, it prompts for landscape or portrait mode. This defines the size of the real page (11x8.5 or 8.5x11), and the dimensions for the virtual page must fit within this real page.
    /// The SetVpage command will define virtual page limits in terms of inches (virtual page inches), which are the coordinates that will be used in the various commands that require inches to be used. The new page limits are printed when the SetVpage command completes.
    /// To return to the default state where the real page equals the virtual page, enter:
    /// <code>
    /// SetVpage(OnOffSetting.Off);
    /// </code>
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
    /// <remarks>
    /// <list type="number">
    /// <item><description>Contour levels are reset by <see cref="Clear">Clear</see> or <see cref="GradsSharp.IGradsCommandInterface.Display">Display</see>.</description></item>
    /// <item><description>Often used in conjunction with <see cref="GradsSharp.IGradsCommandInterface.SetContourColors">SetContourColors</see> to override the default settings and specify exact contour levels and the colors that go with them.</description></item>
    /// </list>
    /// </remarks>
    /// </summary>
    
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
    
    
    /// <summary>
    /// Set the style of the marker for line plots, or for the gxout option 'stnmark'
    /// </summary>
    /// <param name="cmark"></param>
    void SetMarkerType(double cmark);
    
    
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
    
    /// <summary>
    /// Set the options for the contour labels.
    /// </summary>
    /// <param name="option">Contour label mode</param>
    /// <param name="format">Format of the labels.  Formatting is done with <see href="https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-string-format#get-started-with-the-stringformat-method">standard c# string formatting</see></param>
    /// <example>
    /// Example format: {0:0.0} will format the label to one decimal place
    /// </example>
    void SetContourLabelOptions(LabelOption option, string format = "");
    
    /// <summary>
    /// Set label options for the x-axis labels
    /// </summary>
    /// <param name="option">Label option</param>
    /// <param name="format">Format of the labels.  Formatting is done with <see href="https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-string-format#get-started-with-the-stringformat-method">standard c# string formatting</see></param>
    void SetXAxisLabelOptions(LabelOption option, string format = "");
    
    /// <summary>
    /// Set label options for the y-axis labels
    /// </summary>
    /// <param name="option">Label option</param>
    /// <param name="format">Format of the labels.  Formatting is done with <see href="https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-string-format#get-started-with-the-stringformat-method">standard c# string formatting</see></param>
    void SetYAxisLabelOptions(LabelOption option, string format = "");
    
    
    /// <summary>
    /// Set axis labels for the x-axis or y-axis
    /// </summary>
    /// <param name="axis">X or Y axis</param>
    /// <param name="option">Axis label option</param>
    /// <param name="format">Axis label format</param>
    void SetAxisLabels(Axis axis, AxisLabelOption option, string? format);
    
    /// <summary>
    /// Control the grid options.
    /// </summary>
    /// <param name="option">Grid option</param>
    /// <param name="style">
    /// Line style for the grid<br/>
    /// Options for line style are:<br/>
    /// 1 = solid<br/>
    /// 2 = long dash<br/>
    /// 3 = short dash<br/>
    /// 4 = long dash short dash<br/>
    /// 5 = dotted<br/>
    /// 6 = dot dash<br/>
    /// 7 = dot dot dash<br/>
    /// </param>
    /// <param name="color">Line color for the grid.  This may be one of the default colors or a new color defined with <see cref="SetColor"/></param>
    /// <param name="thickness">Thickness for the grid lines.  Must be an integer between 1 and 256</param>
    void SetGridOptions(GridOption option, int style = -1, int color = -1, int thickness = -1);

    /// <summary>
    /// Used with <see cref="SetGraphicsOutputMode"/> with option <see cref="GraphicsOutputMode.Grid"/> to control the presence and appearance of the grid lines. The options are as follows:
    /// </summary>
    /// <param name="option">Control the presence</param>
    /// <param name="color">Color number if selecting the color mode</param>
    void SetGridLineOptions(GridLineOption option, int? color = 0);
    
    /// <summary>
    /// Set the style of the contour labels
    /// </summary>
    /// <param name="color">Color number of the labels.  Default of -1 indicates the same color as the contour lines</param>
    /// <param name="thickness">Thickness for the contour labels.  Default of -1 indicates the same thickness as the contour lines</param>
    /// <param name="size">Size of the labels.  Default is 0.09</param>
    void SetContourLabelStyle(int color = -1, int thickness = -1, double size = 0.09);

    /// <summary>
    /// Set the options for drawing strings
    /// </summary>
    /// <param name="color">Color of the string to be printed.  This may be one of the default colors or a new color defined with <see cref="SetColor"/></param>
    /// <param name="justification">Justification</param>
    /// <param name="thickness">Integer between 1 and 12</param>
    /// <param name="rotation">Desired string rotation in degrees. The center of rotation is the justification point. Rotation is counter-clockwise. Defaut rotation is 0.</param>
    void SetStringOptions(int color, StringJustification justification = StringJustification.Undefined,
        int thickness = -1, double rotation = -1);

    /// <summary>
    /// Draw logo and timestamp on the plot
    /// </summary>
    /// <param name="onOffSetting">Turn the logo and timestamp on or off.  Default is on</param>
    void SetGrads(OnOffSetting onOffSetting);
    /// <summary>
    /// Set the size of the string
    /// </summary>
    /// <param name="hsize">width of the characters in virtual page inches</param>
    /// <param name="vsize">height of the characters in virtual page inches. If vsize is not defined it will be set the same as the hsize parameter</param>
    void SetStringSize(double hsize, double vsize = -1);
    
    /// <summary>
    /// Set thickness for the contour lines
    /// </summary>
    /// <param name="cthck">Value between 1 and 256</param>
    void SetContourLineThickness(int cthck);
    /// <summary>
    /// Set style of contour lines
    /// </summary>
    /// <param name="style">
    /// 0 = no contours<br/>
    /// 1 = solid<br/>
    /// 2 = long dash<br/>
    /// 3 = short dash<br/>
    /// 4 = long dash short dash<br/>
    /// 5 = dotted<br/>
    /// 6 = dot dash<br/>
    /// 7 = dot dot dash<br/>
    /// </param>
    void SetContourLineStyle(LineStyle style);
    /// <summary>
    /// Set the color of the contour lines
    /// </summary>
    /// <param name="color">Color of the contour line.  This may be one of the default colors or a new color defined with <see cref="SetColor"/></param>
    void SetContourLineColor(int color);
    /// <summary>
    /// If on, the grid is interpolated to a finer grid using cubic interpolation before contouring. "Sticks".
    /// <remarks>
    /// This option will result in contour values below and above the min and max of the un-interpolated grid. This may result in physically invalid values such as in the case of negative rainfall
    /// </remarks>
    /// </summary>
    /// <param name="option">Smoothing option</param>
    void SetContourLineSmoothing(SmoothOption option);
    /// <summary>
    /// This set command sets the longitude dimension of the dimension environment using world coordinates.
    /// <remarks>
    /// A file needs to be open before this command can be used
    /// </remarks>
    /// </summary>
    /// <param name="min">Low value</param>
    /// <param name="max">High value (optional)</param>
    void SetLongitude(double min, double max = Double.MaxValue);
    /// <summary>
    /// This set command sets the latitude dimension of the dimension environment using world coordinates.
    /// <remarks>
    /// A file needs to be open before this command can be used
    /// </remarks>
    /// </summary>
    /// <param name="min">Low value</param>
    /// <param name="max">High value (optional)</param>
    void SetLatitude(double min, double max = Double.MaxValue);
    /// <summary>
    /// This set command sets the level dimension of the dimension environment using world coordinates.
    /// <remarks>
    /// A file needs to be open before this command can be used
    /// </remarks>
    /// </summary>
    /// <param name="level">Level value</param>
    void SetLevel(double lev);
    /// <summary>
    /// This set command sets the x dimension of the dimension environment using grid coordinates. 
    /// </summary>
    /// <param name="xMin">Low value</param>
    /// <param name="xMax">High value</param>
    void SetX(double xMin, double xMax);
    /// <summary>
    /// This set command sets the y dimension of the dimension environment using grid coordinates. 
    /// </summary>
    /// <param name="yMin">Low value</param>
    /// <param name="yMax">High value</param>
    void SetY(double yMin, double yMax);
    /// <summary>
    /// This set command sets the z dimension of the dimension environment using grid coordinates. 
    /// </summary>
    /// <param name="zMin">Low value</param>
    void SetZ(double zMin);
    /// <summary>
    /// This set command sets the z dimension of the dimension environment using grid coordinates. 
    /// </summary>
    /// <param name="zMin">Low value</param>
    /// <param name="zMax">High value</param>
    void SetZ(double zMin, double zMax);
    /// <summary>
    /// This set command sets the time dimension of the dimension environment using grid coordinates. 
    /// </summary>
    /// <param name="tMin">Low value</param>
    void SetT(double tMin);
    void SetE(double eMin);
    
    /// <summary>
    /// This command controls the appearance of the streamlines
    /// </summary>
    /// <param name="density">An integer between -10 and 10. Negative values are for high-res grids. The default value is 5.</param>
    void SetStreamDensity(int density);

    /// <summary>
    /// Flips the order of the horizontal axis
    /// </summary>
    /// <param name="flip"></param>
    void SetXFlip(bool flip);
    /// <summary>
    /// Flips the order of the vertical axis
    /// </summary>
    /// <param name="flip"></param>
    void SetYFlip(bool flip);

    
    /// <summary>
    /// Define a new variable
    /// </summary>
    /// <param name="varName">Name of the variable.  This name can be passed to the <see cref="Display">Display</see> command to execute the plotting</param>
    /// <param name="data">Data for the variable</param>
    /// <seealso cref="GetVariable"/> 
    void Define(string varName, IGradsGrid data);
    
    /// <summary>
    /// Open a new file for reading
    /// </summary>
    /// <param name="dataFile">Path to the file</param>
    /// <param name="dataReader">Data reader implementation to read the file</param>
    void Open(string dataFile, IGriddedDataReader dataReader);
    
    /// <summary>
    /// Draw data for a variable
    /// </summary>
    /// <param name="variable">Name of a <see cref="Define">defined</see> variable.</param>
    /// <returns>0 if everything went fine. 1 when an error occurred</returns>
    int Display(string variable);
    /// <summary>
    /// Draw a string on the plot
    /// </summary>
    /// <param name="x">X-coordinate in virtual page units</param>
    /// <param name="y">Y-coordinate in virtual page units</param>
    /// <param name="text">Text to plot</param>
    /// <returns>0 if everything went fine. 1 when an error occurred</returns>
    /// <seealso cref="SetStringOptions"/>
    /// <seealso cref="SetStringSize"/>
    int DrawString(double x, double y, string text);
    
    /// <summary>
    /// This command controls the appearance of the map lines.
    /// </summary>
    /// <param name="color">Color of the map lines.  This may be one of the default colors or a new color defined with <see cref="SetColor"/></param>
    /// <param name="style">
    /// Line style for the map<br/>
    /// Options for line style are:<br/>
    /// 1 = solid<br/>
    /// 2 = long dash<br/>
    /// 3 = short dash<br/>
    /// 4 = long dash short dash<br/>
    /// 5 = dotted<br/>
    /// 6 = dot dash<br/>
    /// 7 = dot dot dash<br/>
    /// </param>
    /// <param name="thickness">Thickness for the map lines.  Must be an integer between 1 and 256</param>
    void SetMapOptions(int color, LineStyle? style, int? thickness);

    /// <summary>
    /// Override the default paper size.  The default paper size is 8.5x11 inches
    /// </summary>
    /// <param name="xsize">X-size in inches</param>
    /// <param name="ysize">Y-size in inches</param>
    void SetPaperSize(double xsize, double ysize);

    /// <summary>
    /// Set KML output options
    /// </summary>
    /// <param name="flag">KML output format</param>
    /// <param name="filename">Filename to write the KML data to</param>
    void SetKmlOutput(KmlOutputFlag flag, string filename);

    /// <summary>
    /// Get information about the current dimensions in use
    /// </summary>
    /// <returns>Information about the dimensions in use</returns>
    DimensionInfo QueryDimensionInfo();

    /// <summary>
    /// Get information about the current open file
    /// </summary>
    /// <returns>Information about the current file</returns>
    FileInfo QueryFileInfo();

    /// <summary>
    /// Return the data for a variable stored in a file
    /// </summary>
    /// <param name="definition">Definition of the variable</param>
    /// <param name="file">File number to read the info from</param>
    /// <returns><see cref="IGradsGrid">Grid</see> with meta information and the actual data</returns>
    IGradsGrid GetVariable(VariableDefinition definition, int file = 1);

    

    /// <summary>
    /// Return the data for a variable stored in a file
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="surfaceType">Surface type</param>
    /// <param name="heightValue">Height value</param>
    /// <param name="file">File number to read the info from</param>
    /// <returns><see cref="IGradsGrid">Grid</see> with meta information and the actual data</returns>
    IGradsGrid GetVariable(string name, FixedSurfaceType surfaceType, double heightValue = 0, int file = 1);
    
    /// <summary>
    ///  Returns data from multiple levels for a specific variable
    /// </summary>
    /// <param name="definition">Definition of the variable</param>
    /// <param name="startLevel">Start level to fetch data</param>
    /// <param name="endLevel">End level to fetch data</param>
    /// <param name="function">Function to apply on the data</param>
    /// <returns><see cref="IGradsGrid">Grid</see> with meta information and the actual data</returns>
    IGradsGrid? GetMultiLevelData(VariableDefinition definition, double startLevel, double endLevel, MultiLevelFunction function); 

    
    /// void Define(string varName, string formula);
}