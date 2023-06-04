using GradsSharp.Data;
using GradsSharp.Models;

namespace GradsSharp;

public interface IGradsCommandInterface
{
    void stack();
    void reset(bool doReinit = false);
    void close();
    void clear(ClearAction? action);

    void printim(string path, OutputFormat format = OutputFormat.Undefined, string backgroundImage = "",
        string foregroundImage = "",
        BackgroundColor backColor = BackgroundColor.Default, int transparentColor = -1,
        int horizontalSize = -999, int verticalSize = -999, double borderWidth = -1.0);

    void swap();
    void reinit();
    void gacln(int flg);
    void SetGridLine(GridLine gridLine, int color = -1);
    void SetVpage(OnOffSetting page, int xlo = 0, int xhi = 0, int ylo = 0, int yhi = 0);
    void SetColor(int colorNr, int red, int green, int blue, int alpha = 255);
    void SetPArea(OnOffSetting onOff, int xlo = 0, int xhi = 0, int ylo = 0, int yhi = 0);
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
    void SetContourLineOptions(int size = -1, int color = -1, int thickness = -1);

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
    
    void Define(string varName, string formula);
    void Define(string varName, double[] data);
    void Open(string dataFile, IGriddedDataReader dataReader);
    int Display(string variable);
    int DrawString(double x, double y, string text);
    void SetMapOptions(int color, LineStyle? style , int? thickness);
    
    void SetDataAction(Action<IDataAdapter> dataAction);

    void SetPaperSize(double xsize, double ysize);

    DimensionInfo QueryDimensionInfo();

}