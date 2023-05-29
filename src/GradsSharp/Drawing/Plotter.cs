using GradsSharp.Builders;
using GradsSharp.Functions;
using GradsSharp.Models;

namespace GradsSharp.Drawing;

public class Plotter
{
    DrawingContext context = new DrawingContext();
    public void Draw(Plot plot)
    {

        context.Plot = plot;
        
        if (context.Plot.Orientation == Orientation.Landscape)
        {
            context.CommonData.xsiz = 11.0f;
            context.CommonData.ysiz = 8.5f;
        }
        else
        {
            context.CommonData.xsiz = 8.5f;
            context.CommonData.ysiz = 11.0f;
        }

        if (context.Plot.AspectRatio > -990)
        {
            if (context.Plot.AspectRatio < 1.0f)
            {
                context.CommonData.xsiz = 11.0f * plot.AspectRatio;
                context.CommonData.ysiz = 11.0f;
            }
            else
            {
                context.CommonData.xsiz = 11.0f;
                context.CommonData.ysiz = 11.0f / plot.AspectRatio;
            }
        }
        
        context.CommonData.InitData();

        context.CommonData.fnum = 0;
        context.CommonData.undef = -9.99e8;
        context.CommonData.fillpoly = -1;
        context.CommonData.marktype = 3;
        context.CommonData.marksize = 0.05;

        int rc = context.GaGx.gagx();
        if (rc > 0) throw new Exception("Error setting up graphics");
        
        StartPlot();

        context.GaSubs.gxend();
    }

    private void StartPlot()
    {
        if (context.Plot.DataFiles.Count == 0)
        {
            throw new Exception("No datafiles added");
        }

        // for now only support for 1 file
        var dataFile = context.Plot.DataFiles.First();
        context.GaUser.Open(dataFile.FileName, dataFile.Format);
        context.GaUser.SetGrads(OnOffSetting.Off);
        context.GaUser.SetLat(40, 60);
        context.GaUser.SetLon(-10,20);
        context.GaUser.SetMPVals(OnOffSetting.On, -10, 20, 30, 60);
        context.GaUser.SetPArea(OnOffSetting.On, 0, 11, 0, 8);
        context.GaUser.SetMProjection(Projection.Latlon);
        
        //context.GaUser.Define("t", "tmp2m-273.15");

        foreach (Chart c in context.Plot.Charts)
        {
            context.GaUser.SetLat(c.LatitdeMin, c.LatitudeMax);
            context.GaUser.SetLon(c.LongitudeMin, c.LongitudeMax);
            context.GaUser.SetMapResolution(c.Resolution);
            context.GaUser.SetGridOptions(c.GridSetting);
            context.GaUser.SetAxisLabels(Axis.X, c.AxisLabelOptionX, c.AxisLabelFormatX);
            context.GaUser.SetAxisLabels(Axis.Y, c.AxisLabelOptionY, c.AxisLabelFormatY);
            if (c.MapColor != null)
            {
                context.GaUser.SetMapOptions(c.MapColor.Value, null, null);
            }

            if (c.ColorBarEnabled)
            {
                var cb = new ColorRange(context);
                cb.Min = c.ColorBarMin;
                cb.Max = c.ColorBarMax;
                cb.Interval = c.ColorBarInterval;
                cb.GxOut = c.ColorBarGxOut;
                cb.Kind = c.ColorBarKind??"";
                cb.SetColors();
            }

            foreach (ChartLayer cl in c.Layers)
            {
                context.CommonData.DataAction = cl.DataAction;
                context.GaUser.SetGraphicsOut(cl.LayerType);

                if (cl.LayerType == GxOutSetting.Contour)
                {
                    context.GaUser.SetCStyle(cl.LineStyle);
                    if(cl.ContourInterval!=null)
                        context.GaUser.SetCInt(cl.ContourInterval.Value);
                    if(cl.ContourMin!=null)
                        context.GaUser.SetCMin(cl.ContourMin.Value);
                    if(cl.ContourMax!=null)
                        context.GaUser.SetCMax(cl.ContourMax.Value);
                    if(cl.ContourLabelOption!=null)
                        context.GaUser.SetCLab(cl.ContourLabelOption.Value);
                    if(cl.ContourColor!=null)
                        context.GaUser.SetCColor(cl.ContourColor.Value);
                }
                
                context.GaUser.Display(cl.VariableToDisplay);
            }

            if (c.ColorBarEnabled && c.ColorBarSettings != null)
            {
                var cb = new ColorBar(c.ColorBarSettings, context);
                cb.DrawColorBar();
            }

            foreach (var tb in c.TextBlocks)
            {
                context.GaUser.SetStringSize(tb.Width, tb.Height);
                context.GaUser.SetStringOptions(tb.FontColor, tb.Justification, tb.Thickness, tb.Rotation);
                context.GaUser.DrawString(tb.LocationX, tb.LocationY, tb.Text);    
            }
           
            context.GaUser.printim(context.Plot.OutputFile, horizontalSize: context.Plot.OutputSizeX, verticalSize: context.Plot.OutputSizeY);

        }
        
        
        
        
    }
    
    
}