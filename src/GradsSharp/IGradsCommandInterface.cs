using GradsSharp.Data;
using GradsSharp.Exceptions;
using GradsSharp.Models;
using FileInfo = GradsSharp.Models.FileInfo;

namespace GradsSharp;

public interface IGradsCommandInterface
{
    void stack();
    void reset(bool doReinit = false);
    
    /// <summary>
    /// Close the the last opened file
    /// </summary>
    void close();
    
    /// <summary>
    /// Issued without parameters, the clear command does pretty heavy duty clearing of many of the GradsSharp internal settings. Parameters can be added to limit what is cleared when using more advanced features.
    /// </summary>
    void clear(ClearAction? action);

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
    
    /// <summary>Used with <see cref="GradsSharp.IGradsCommandInterface.SetGraphicsOut"/> with <see cref="GradsSharp.Models.GxOutSetting.Grid"/> option to control the presence and appearance of the grid lines.</summary>
    /// <param name="gridLine">Controls the gridline setting</param>
    /// <param name="color">The color of the grid lines. If <see cref="GradsSharp.Models.GridLine.Color"/> is used</param>
    void SetGridLine(GridLine gridLine, int color = -1);
    
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
    void SetPArea(OnOffSetting onOff, int xlo = 0, int xhi = 0, int ylo = 0, int yhi = 0);
    
    /// <summary>
    /// Sets the contour interval to the specified value. Reset by <see cref="GradsSharp.IGradsCommandInterface.clear">clear</see> or <see cref="GradsSharp.IGradsCommandInterface.Display">Display</see>.
    /// </summary>
    /// <param name="val">Contour interval value</param>
    void SetCInt(double val);

    void SetMPVals(OnOffSetting onOff, double lonmin = 0.0, double lonmax = 0.0, double latmin = 0.0,
        double latmax = 0.0);

    void SetCLevs(double[] levels);
    void SetCCols(int[] cols);
    void SetCMin(double cmin);
    void SetCMax(double cmax);
    void SetCMark(double cmark);
    void SetMProjection(Projection projection);
    void SetMapResolution(MapResolution mapResolution);
    void SetGraphicsOut(GxOutSetting setting);
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