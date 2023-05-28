using System.Reflection;
using GradsSharp.Drawing.Grads;
using GradsSharp.Models;
using SixLabors.ImageSharp.Drawing;

namespace GradsSharp.Drawing;

public class Plotter
{
    DrawingContext context = new DrawingContext();
    Image<Argb32> image;
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

        var dataFile = context.Plot.DataFiles.First();
        context.GaUser.Open(dataFile.FileName, dataFile.Format);
        context.GaUser.SetGrads(OnOffSetting.Off);
        //context.CommonData.pfid = new gafile();
        context.GaUser.SetLat(40, 60);
        context.GaUser.SetLon(-10,20);
        context.GaUser.SetMPVals(OnOffSetting.On, -10, 20, 30, 60);
        context.GaUser.SetPArea(OnOffSetting.On, 0, 11, 0, 8);
        context.GaUser.SetMProjection(Projection.Latlon);
        context.GaUser.Define("t", "tmp2m-273.15");
        
        foreach (Chart c in context.Plot.Charts)
        {
            context.GaUser.SetLat(c.LatitdeMin, c.LatitudeMax);
            context.GaUser.SetLon(c.LongitudeMin, c.LongitudeMax);
            context.GaUser.SetMapResolution(c.Resolution);
            context.GaUser.SetCCols(new []{ 1,2,3,4,5,6,7,8,9,10,11,12,13,14});
            context.GaUser.SetGraphicsOut(GxOutSetting.Shaded);
            context.GaUser.Display("t");
            context.GaUser.SetGraphicsOut(GxOutSetting.Contour);
            context.GaUser.SetCStyle(LineStyle.ShortDash);
            context.GaUser.SetCInt(5);
            context.GaUser.SetCLab(LabelOption.Off);
            context.GaUser.SetCThick(0);
            context.GaUser.Display("t");
            
            context.GaUser.SetStringSize(0.18);
            context.GaUser.SetStringOptions(1, StringJustification.Right, 12, 0);
            context.GaUser.DrawString(10.95, 8.3, "2m Temperature (C) & Mean sea layer pressure");
            
            context.GaUser.SetStringSize(0.10);
            context.GaUser.SetStringOptions(4, StringJustification.Right, 4, 0);
            context.GaUser.DrawString(10.95, 8.1, "https://www.weatherportal.eu - Run: 06Z2023MAY27 - Valid: Sat 27/05 06:00Z");
            
            context.GaUser.printim(@"tst.png", horizontalSize: 1024, verticalSize: 768);
            // context.GaUser.SetGraphicsOut(GxOutSetting.Print);
            // context.GaUser.Display("tmp2m");
        }
        
        
        
        
    }
    
    
}