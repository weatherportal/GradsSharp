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
        //context.CommonData.pfid = new gafile();
        context.GaUser.SetLat(48, 52);
        context.GaUser.SetLon(-3,4);
        context.GaUser.SetMProjection(Projection.Ortographic);
        
        foreach (Chart c in context.Plot.Charts)
        {
            context.GaUser.SetMapResolution(c.Resolution);
            context.GaGx.gawmap(true);
        }
        
        context.GaUser.Display("tmp2m");
        
        context.GaUser.printim(@"tst.png", horizontalSize: 1024, verticalSize: 768);
    }
    
    
}