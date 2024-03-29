﻿using GradsSharp.Data.Grads;
using GradsSharp.Drawing.Grads;
using GradsSharp.Models.Internal;
using Microsoft.Extensions.Logging;

namespace GradsSharp.Drawing;

internal class DrawingContext
{

    public GradsCommon CommonData { get; set; } = new GradsCommon();
    

    public GradsDrawingInterface GradsDrawingInterface { get; set; }
    public GaIO GaIO { get; set; }
    public GradsCommandInterface GradsCommandInterface { get; set; }
    public GaExpr GaExpr { get; set; }
    public GxMeta GxMeta { get; set; }
    public GxShad GxShad { get; set; }
    public GxShad2 GxShad2 { get; set; }
    public GaGx GaGx { get; set; }
    public GxStrm GxStrm { get; set; }
    public GxChpl GxChpl { get; set; }
    public GxContour GxContour { get; set; }

    public GradsDatabase GradsDatabase { get; set; } = new GradsDatabase();
    public GxWmap GxWmap { get; set; }
    
    public float XSize { get; set; }
    public float YSize { get; set; }

    public int DrawWidth { get; set; }
    public int DrawHeight { get; set; }
    public double XScale { get; set; }
    public double YScale { get; set; }
    public int ClipX { get; set; }
    public int ClipY { get; set; }
    public int ClipWidth { get; set; }
    public int ClipHeight { get; set; }

    public double MinLat { get; set; }
    public double MaxLat { get; set; }
    public double MinLon { get; set; }
    public double MaxLon { get; set; }
    
    public int[] MapColors { get; set; } = new int[256];
    public int[] MapStyles { get; set; } = new int[256];
    public int[] MapLineWidths { get; set; } = new int[256];

    public int CurrentColor { get; set; } = 1;      //
    public int AutoColor { get; set; } = 8;         // mcolor
    public int DefaultColor { get; set; } = 1;
    public int DefaultStyle { get; set; } = 1;
    public int DefaultThickness { get; set; } = 3;


    private ILogger? _logger;
    
    public ILogger? Logger => _logger;
    
    public DrawingContext(ILogger? logger = null)
    {
        _logger = logger;
        
        GradsDrawingInterface = new GradsDrawingInterface(this);
        GradsCommandInterface = new GradsCommandInterface(this);
        GaGx = new GaGx(this);
        GxWmap = new GxWmap(this);
        GxStrm = new GxStrm(this);
        GxShad = new GxShad(this);
        GxShad2 = new GxShad2(this);
        GxChpl = new GxChpl(this);
        GxContour = new GxContour(this);
        GxMeta = new GxMeta(this);
        GaIO = new GaIO(this);
        GaExpr = new GaExpr(this);
        
        for (int j = 0; j < 256; j++)
        {
            MapColors[j] = -9;
            MapStyles[j] = 1;
            MapLineWidths[j] = 3;
        }

        MapColors[0] = -1;
        MapColors[1] = -1;
        MapColors[2] = -1;
    }
}