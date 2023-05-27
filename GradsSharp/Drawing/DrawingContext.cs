using GradsSharp.Drawing.Grads;
using GradsSharp.Models;

namespace GradsSharp.Drawing;

internal class DrawingContext
{

    public gacmn CommonData { get; set; } = new gacmn();
    

    public GaSubs GaSubs { get; set; }
    public GaIO GaIO { get; set; }
    public GaUser GaUser { get; set; }
    public GaExpr GaExpr { get; set; }
    public GxMeta GxMeta { get; set; }
    public GxShad GxShad { get; set; }
    public GxShad2 GxShad2 { get; set; }
    public GaGx GaGx { get; set; }
    public GxStrm GxStrm { get; set; }
    public GxChpl GxChpl { get; set; }
    public GxContour GxContour { get; set; }

    public GxDb GxDb { get; set; } = new GxDb();
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
    
    public List<Color> Colors { get; set; } = new();

    public int[] MapColors { get; set; } = new int[256];
    public int[] MapStyles { get; set; } = new int[256];
    public int[] MapLineWidths { get; set; } = new int[256];

    public int CurrentColor { get; set; } = 1;      //
    public int AutoColor { get; set; } = 8;         // mcolor
    public int DefaultColor { get; set; } = 1;
    public int DefaultStyle { get; set; } = 1;
    public int DefaultThickness { get; set; } = 3;
    
    
    public Plot Plot { get; set; }
    public DrawingContext()
    {
        GaSubs = new GaSubs(this);
        GaUser = new GaUser(this);
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
        
        Colors.Add(Color.FromRgba(0,0,0,255));
        Colors.Add(Color.FromRgba(255,255,255,255));
        Colors.Add(Color.FromRgba(250,60,60,255));
        Colors.Add(Color.FromRgba(0,220,0,255));
        Colors.Add(Color.FromRgba(30,60,255,255));
        Colors.Add(Color.FromRgba(0,200,200,255));
        Colors.Add(Color.FromRgba(240,0,130,255));
        Colors.Add(Color.FromRgba(230,220,50,255));
        Colors.Add(Color.FromRgba(240,130,40,255));
        Colors.Add(Color.FromRgba(160,230,50,255));
        Colors.Add(Color.FromRgba(0,160,255,255));
        Colors.Add(Color.FromRgba(230,175,45,255));
        Colors.Add(Color.FromRgba(0,210,140,255));
        Colors.Add(Color.FromRgba(130,0,220,255));
        Colors.Add(Color.FromRgba(170,170,170,255));

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

    public Color GetColor(int color)
    {
        if (color >= Colors.Count)
        {
            return Color.FromRgba(150, 150, 150, 255);
        }
        return Colors[color - 1];
    }
}