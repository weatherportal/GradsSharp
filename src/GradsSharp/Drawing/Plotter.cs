using GradsSharp.Builders;
using GradsSharp.Drawing.Grads;
using GradsSharp.Functions;
using GradsSharp.Models;

namespace GradsSharp.Drawing;

public class Plotter
{

    private readonly GradsEngine _engine;

    public Plotter(GradsEngine engine)
    {
        _engine = engine;
    }

    public void Draw(Plot plot)
    {

        
        if (plot.Orientation == Orientation.Landscape)
        {
            _engine.GradsCommandInterface.SetPaperSize(11.0f, 8.5f);
        }
        else
        {
            _engine.GradsCommandInterface.SetPaperSize( 8.5f,11.0f);
        }

        if (plot.AspectRatio > -990)
        {
            if (plot.AspectRatio < 1.0f)
            {
                _engine.GradsCommandInterface.SetPaperSize(11.0f * plot.AspectRatio, 11.0f);
                
            }
            else
            {
                _engine.GradsCommandInterface.SetPaperSize(11.0f, 11.0f / plot.AspectRatio);
            }
        }
        
        _engine.InitEngine();

        // _engine.GradsCommandInterface.CommonData.fnum = 0;
        // _engine.GradsCommandInterface.CommonData.undef = -9.99e8;
        // _engine.GradsCommandInterface.CommonData.fillpoly = -1;
        // _engine.GradsCommandInterface.CommonData.marktype = 3;
        // _engine.GradsCommandInterface.CommonData.marksize = 0.05;

        
        
        StartPlot(plot);

       _engine.EndDrawing();
    }

    private void StartPlot(Plot plot)
    {
        if (plot.DataFiles.Count == 0)
        {
            throw new Exception("No datafiles added");
        }

        // for now only support for 1 file
        var dataFile = plot.DataFiles.First();
        _engine.GradsCommandInterface.Open(dataFile.FileName, dataFile.Format);
        _engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        _engine.GradsCommandInterface.SetLat(40, 60);
        _engine.GradsCommandInterface.SetLon(-10,20);
        _engine.GradsCommandInterface.SetMPVals(OnOffSetting.On, -10, 20, 30, 60);
        _engine.GradsCommandInterface.SetPArea(OnOffSetting.On, 0, 11, 0, 8);
        _engine.GradsCommandInterface.SetMProjection(Projection.Latlon);
        
        //_engine.GradsCommandInterface.GaUser.Define("t", "tmp2m-273.15");

        foreach (Chart c in plot.Charts)
        {
            _engine.GradsCommandInterface.SetLat(c.LatitdeMin, c.LatitudeMax);
            _engine.GradsCommandInterface.SetLon(c.LongitudeMin, c.LongitudeMax);
            _engine.GradsCommandInterface.SetMapResolution(c.Resolution);
            _engine.GradsCommandInterface.SetGridOptions(c.GridSetting);
            _engine.GradsCommandInterface.SetAxisLabels(Axis.X, c.AxisLabelOptionX, c.AxisLabelFormatX);
            _engine.GradsCommandInterface.SetAxisLabels(Axis.Y, c.AxisLabelOptionY, c.AxisLabelFormatY);
            if (c.MapColor != null)
            {
                _engine.GradsCommandInterface.SetMapOptions(c.MapColor.Value, null, null);
            }

            

            foreach (ChartLayer cl in c.Layers)
            {
                
                if (cl.ColorShadingEnabled)
                {
                    var cb = new ColorRange(_engine.GradsCommandInterface);
                    cb.Min = cl.ColorShadingMin;
                    cb.Max = cl.ColorShadingMax;
                    cb.Interval = cl.ColorShadingInterval;
                    cb.GxOut = cl.ColorShadingGxOut;
                    cb.Kind = cl.ColorShadingKind??"";
                    cb.SetColors();
                }
                
                _engine.GradsCommandInterface.SetDataAction(cl.DataAction);
                _engine.GradsCommandInterface.SetGraphicsOut(cl.LayerType);

                if (cl.LayerType == GxOutSetting.Contour)
                {
                    _engine.GradsCommandInterface.SetCStyle(cl.LineStyle);
                    if(cl.ContourInterval!=null)
                        _engine.GradsCommandInterface.SetCInt(cl.ContourInterval.Value);
                    if(cl.ContourMin!=null)
                        _engine.GradsCommandInterface.SetCMin(cl.ContourMin.Value);
                    if(cl.ContourMax!=null)
                        _engine.GradsCommandInterface.SetCMax(cl.ContourMax.Value);
                    if(cl.ContourLabelOption!=null)
                        _engine.GradsCommandInterface.SetCLab(cl.ContourLabelOption.Value);
                    if(cl.ContourColor!=null)
                        _engine.GradsCommandInterface.SetCColor(cl.ContourColor.Value);
                    if(cl.ContourThickness!=null)
                        _engine.GradsCommandInterface.SetCThick(cl.ContourThickness.Value);
                    foreach (var cd in cl.ColorDefinitions)
                    {
                        _engine.GradsCommandInterface.SetColor(cd.ColorNumber, cd.Red, cd.Green, cd.Blue, cd.Alpha); 
                    }
                }
                else if (cl.LayerType == GxOutSetting.Shaded)
                {
                    foreach (var cd in cl.ColorDefinitions)
                    {
                        _engine.GradsCommandInterface.SetColor(cd.ColorNumber, cd.Red, cd.Green, cd.Blue, cd.Alpha); 
                    }
                    _engine.GradsCommandInterface.SetCLevs(cl.Levels);
                    _engine.GradsCommandInterface.SetCCols(cl.Colors);
                    
                }
                _engine.GradsCommandInterface.Display(cl.VariableToDisplay);
            }

            if (c.ColorBarSettings != null)
            {
                var cb = new ColorBar(c.ColorBarSettings, _engine.GradsCommandInterface, _engine.GradsDrawingInterface);
                cb.DrawColorBar();
            }

            foreach (var tb in c.TextBlocks)
            {
                _engine.GradsCommandInterface.SetStringSize(tb.Width, tb.Height);
                _engine.GradsCommandInterface.SetStringOptions(tb.FontColor, tb.Justification, tb.Thickness, tb.Rotation);
                _engine.GradsCommandInterface.DrawString(tb.LocationX, tb.LocationY, tb.Text);    
            }
           
            _engine.GradsCommandInterface.printim(plot.OutputFile, horizontalSize: plot.OutputSizeX, verticalSize: plot.OutputSizeY);

        }
        
        
        
        
    }
    
    
}