using System.Reflection;
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
        context.CommonData.pfid = new gafile();
        context.GaUser.SetLat(48, 52);
        context.GaUser.SetLon(-3,4);
        context.GaUser.SetMProjection(Projection.Ortographic);
        
        foreach (Chart c in context.Plot.Charts)
        {
            context.GaUser.SetMapResolution(c.Resolution);
            context.GaGx.gawmap(true);
        }
        
        
        context.GaUser.printim(@"F:\Temp\tst.png", horizontalSize: 1024, verticalSize: 768);
    }
    
    private void InitImage()
    {

        
    }

    // private void DrawGeoMap(Chart c)
    // {
    //     string mapFile = "GradsSharp.Data.lowres";
    //     
    //     if (c.Resolution == MapResolution.HighResolution)
    //     {
    //         mapFile = "GradsSharp.Data.hires";
    //     }
    //     else if (c.Resolution == MapResolution.MediumResolution)
    //     {
    //         mapFile = "GradsSharp.Data.mres";
    //     }
    //     
    //     
    //     double[] lon = new double[255];
    //     double[] lat = new double[255];
    //     double xx, yy, lnmin, lnmax, ltmin, ltmax, lnfact;
    //     int num, i, ipen, rc, type, ilon, ilat, rnum, flag, st1, st2, spos;
    //     float sln1, sln2, slt1, slt2;
    //     float lnsav, ltsav, lndif, ltdif, lntmp, lttmp, llinc, llsum, lldist;
    //     byte[] hdr = new byte[3];
    //     byte[] rec = new byte[1530];
    //
    //     llinc = (float)Math.Sqrt(Math.Pow(context.MaxLon - context.MinLon, 2) + Math.Pow(context.MaxLat - context.MinLat, 2));
    //     llinc = llinc / 200;
    //     if (llinc < 0.0001) llinc = 0.0001f;
    //
    //     using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(mapFile))
    //     using (BinaryReader reader = new BinaryReader(stream))
    //     {
    //         rnum = 0;
    //         while (true)
    //         {
    //             hdr = reader.ReadBytes(3);
    //             if (hdr.Length != 3)
    //                 break;
    //
    //             rnum++;
    //             i = ConvertToInt(hdr, 0, 1);
    //             if (i < 1 || i > 3)
    //             {
    //                 throw new Exception($"Map file format error: invalid rec type {i} rec num {rnum}");
    //             }
    //
    //             if (i == 2)
    //             {
    //                 st1 = ConvertToInt(hdr, 1, 1);
    //                 st2 = ConvertToInt(hdr, 2, 1);
    //                 rec = reader.ReadBytes(16);
    //                 spos = ConvertToInt(rec, 0, 4);
    //                 ilon = ConvertToInt(rec, 4, 3);
    //                 sln1 = (float)( ilon / 1e4);
    //                 ilon = ConvertToInt(rec, 7, 3);
    //                 sln2 = (float)( ilon / 1e4);
    //                 ilat = ConvertToInt(rec, 10, 3);
    //                 slt1 = (float)( ilat / 1e4 - 90.0);
    //                 ilat = ConvertToInt(rec, 13, 3);
    //                 slt2 = (float)( ilat / 1e4 - 90.0);
    //                 flag = 0;
    //                 //TODO
    //                 for (i = 0; i < 256; i++) {
    //                     if (context.MapColors[i] != -9 && i >= st1 && i <= st2) flag = 1;
    //                 }
    //                 if (flag == 0)
    //                 {
    //                     if (spos == 0)
    //                     {
    //                         return;
    //                     }
    //                     reader.BaseStream.Seek(spos, SeekOrigin.Begin);
    //                     continue;
    //                 }
    //
    //                 flag = 0;
    //                 if (sln1 > 360.0) flag = 1;
    //                 else
    //                 {
    //                     if (slt2 <= context.MinLat || slt1 >= context.MaxLat) flag = 0;
    //                     else {
    //                         lnfact = 0.0;
    //                         while (sln2 + lnfact > context.MinLon) lnfact -= 360.0;
    //                         lnfact += 360.0;
    //                         if (sln1 + lnfact >= context.MaxLon) flag = 0;
    //                         else flag = 1;
    //                     }
    //                 }
    //
    //                 if (flag == 0)
    //                 {
    //                     if (spos == 0) {
    //                         return;
    //                     }
    //                     reader.BaseStream.Seek(spos, SeekOrigin.Begin);
    //                 }
    //
    //                 continue;
    //
    //             }
    //             type = ConvertToInt(hdr, 1, 1);
    //             num = ConvertToInt(hdr, 2, 1);
    //
    //             rec = reader.ReadBytes(num * 6);
    //             if(context.MapColors[type] == -9) continue;
    //             if (context.MapColors[type] == -1)
    //             {
    //                 
    //             }
    //             else
    //             {
    //                 
    //             }
    //         }
    //         
    //     }
    // }

    private int ConvertToInt(byte[] data, int offset, int len)
    {
        if (len == 1)
        {
            return data[offset];
        }
        if (len == 2)
        {
            return data[offset] << 8 + data[offset + 1];
        }

        if (len == 3)
        {
            return data[offset] << 16 + data[offset + 1] << 8 + data[offset + 2];
        }

        return data[offset] << 24 + data[offset + 1] << 16 + data[offset + 2] << 8 + data[offset + 3];
    }
}