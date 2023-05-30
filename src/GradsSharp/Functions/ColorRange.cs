using GradsSharp.Drawing;
using GradsSharp.Models;
using SixLabors.ImageSharp.Formats.Webp;

namespace GradsSharp.Functions;

internal class ColorRange
{
    private IGradsCommandInterface _gradsCommandInterface;
    public ColorRange(IGradsCommandInterface commandInterface)
    {
        _gradsCommandInterface = commandInterface;
    }

    public GxOutSetting GxOut { get; set; } = GxOutSetting.Shaded;
    public string Kind { get; set; } = "blue->white->red";
    public int Alpha { get; set; } = 255;
    public int Sample { get; set; } = 0;

    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? Interval { get; set; }

    public int Div { get; set; } = 10;
    public IntervalType IntervalType { get; set; } = IntervalType.Interval;
    public List<double> Levels { get; set; } = new();


    public void SetColors()
    {
        if (Min != null && Max != null && Interval == null)
        {
            Interval = (Max - Min) / Div;
        }

        Kind = SpecialColor();

        if ((Min == null || Max == null || Interval == null) && Levels.Count == 0)
        {
            throw new Exception("Cannot determine color levels");
        }

        if ((Min != null && Max != null && Interval != null) && Levels.Count > 0)
        {
            throw new Exception("Multiple definition of color levels");
        }

        if (IntervalType == IntervalType.Fac && Interval < 1)
        {
            throw new Exception("interval should be greater than 1 when inttype=\"fac\"");
        }

        int colnum = 0;

        if (Levels.Count == 0)
        {
            double val = Min ?? 0;
            while (val <= Max)
            {
                Levels.Add(val);
                if (IntervalType == IntervalType.Interval)
                {
                    val += Interval ?? 10;
                }
                else
                {
                    val = val * (Interval ?? 10);
                }
            }
        }

        colnum = Levels.Count;

        List<int> colnums = new();
        for (int c = 1; c <= colnum; c++)
        {
            colnums.Add(15 + c);
        }

        if (GxOut == GxOutSetting.GridFill || GxOut == GxOutSetting.Shaded || GxOut == GxOutSetting.Shade1 ||
            GxOut == GxOutSetting.Shade2 || GxOut == GxOutSetting.Shade2b)
        {
            colnum++;
            colnums.Add(colnum + 15);
        }

        _gradsCommandInterface.SetCLevs(Levels.ToArray());
        _gradsCommandInterface.SetCCols(colnums.ToArray());

        // define colors
        int i = 1;
        int max = -999;
        string?[] col = new string?[3600];
        int[] ncol = new int[3600];

        while (max != i - 2)
        {
            col[i] = getcol(i);
            if (col[i] == "")
            {
                max = i - 1;
            }
            else
            {
                ncol[i] = -1;
                int l = col[i].Length - 1;
                if (col[i][l] == ')')
                {
                    l--;
                    int mp = 1;
                    ncol[i] = 0;
                    while (col[i][l] != '(' && l > 0)
                    {
                        ncol[i] += Convert.ToInt32(col[i][l]) * mp;
                        mp *= 10;
                        l--;
                    }

                    if (l == 0)
                    {
                        ncol[i] = -1;
                    }
                    else
                    {
                        col[i] = col[i].Substring(1, l - 2);
                    }
                }
            }

            i++;
        }

int max_ncol = max;
        int colnum_ncol = 0;
        i = 1;
        while (i <= max - 1)
        {
            if (ncol[i] >= 0)
            {
                max_ncol--;
                colnum_ncol += ncol[i] + 1;
            }

            i++;
        }

        i = 1;
        while (i < max - 1)
        {
            if (ncol[i] < 0)
            {
                ncol[i] = (colnum - colnum_ncol - 1) / (max_ncol - 1) - 1;
            }

            i++;
        }


        int endnum = 16;
        i = 1;
        while (i <= max - 1)
        {
            int ipp = i + 1;
            string scol = col[i] ?? "";
            string ecol = col[ipp] ?? "";
            int snum = endnum;
            endnum = snum + ncol[i] + 1;
            if (i == max - 1)
            {
                endnum = 16 + colnum - 1;
            }

            defcol(snum, scol, endnum, ecol, Alpha);
            i++;
        }
    }

    private string getcol(int num)
    {
        string result = "";

        int length = Kind.Length;
        int order = 1;
        int i = 0;
        while (i <= length - 1)
        {
            int sublength = 2;
            if (i == length - 1)
            {
                sublength = 1;
            }
            string next = Kind.Substring(i, sublength);
            if (next == "->")
            {
                order++;
                i++;
            }
            else
            {
                if (order == num)
                {
                    result += Kind[i];
                }
            }

            i++;
        }


        return result;
    }

    private void defcol(int snum, string scol, int endnum, string ecol, int defalpha)
    {
        int diff = endnum - snum;
        if (diff <= 0)
        {
            return;
        }

        int sr = colornum(scol, 'r');
        int sg = colornum(scol, 'g');
        int sb = colornum(scol, 'b');
        int sa = colornum(scol, 'a');
        if (sa == -1)
        {
            sa = defalpha;
            
        }

        int er = colornum(ecol, 'r');
        int eg = colornum(ecol, 'g');
        int eb = colornum(ecol, 'b');
        int ea = colornum(ecol, 'a');
        if( ea == -1 )
        {
            ea = defalpha;
        }

        int i = snum;
        if (snum != 16)
        {
            i++;
        }
        
        while( i <= endnum )
        {
            int r = (int)(sr + (er - sr) * (i - snum) / diff);
            int g = (int)(sg + (eg - sg) * (i - snum) / diff);
            int b = (int)(sb + (eb - sb) * (i - snum) / diff);
            int a = (int)(sa + (ea - sa) * (i - snum) / diff);
            //Console.WriteLine($"Define color {i} as ({r},{g},{b},{a})");
            
            _gradsCommandInterface.SetColor(i, r, g, b, a);
            i = i + 1;
        }
    }

    private int colornum(string color, char rgb)
    {
        int r = -1, g = -1, b = -1, a = -1;
        if (color == "black")
        {
            r = 0;
            g = 0;
            b = 0;
        }

        if (color == "navy")
        {
            r = 0;
            g = 0;
            b = 128;
        }

        if (color == "darkblue")
        {
            r = 0;
            g = 0;
            b = 139;
        }

        if (color == "mediumblue")
        {
            r = 0;
            g = 0;
            b = 205;
        }

        if (color == "blue")
        {
            r = 0;
            g = 0;
            b = 255;
        }

        if (color == "darkgreen")
        {
            r = 0;
            g = 100;
            b = 0;
        }

        if (color == "green")
        {
            r = 0;
            g = 128;
            b = 0;
        }

        if (color == "teal")
        {
            r = 0;
            g = 128;
            b = 128;
        }

        if (color == "darkcyan")
        {
            r = 0;
            g = 139;
            b = 139;
        }

        if (color == "deepskyblue")
        {
            r = 0;
            g = 191;
            b = 255;
        }

        if (color == "darkturquoise")
        {
            r = 0;
            g = 206;
            b = 209;
        }

        if (color == "mediumspringgreen")
        {
            r = 0;
            g = 250;
            b = 154;
        }

        if (color == "lime")
        {
            r = 0;
            g = 255;
            b = 0;
        }

        if (color == "springgreen")
        {
            r = 0;
            g = 255;
            b = 127;
        }

        if (color == "aqua")
        {
            r = 0;
            g = 255;
            b = 255;
        }

        if (color == "cyan")
        {
            r = 0;
            g = 255;
            b = 255;
        }

        if (color == "midnightblue")
        {
            r = 25;
            g = 25;
            b = 112;
        }

        if (color == "dodgerblue")
        {
            r = 30;
            g = 144;
            b = 255;
        }

        if (color == "lightseagreen")
        {
            r = 32;
            g = 178;
            b = 170;
        }

        if (color == "forestgreen")
        {
            r = 34;
            g = 139;
            b = 34;
        }

        if (color == "seagreen")
        {
            r = 46;
            g = 139;
            b = 87;
        }

        if (color == "darkslategray")
        {
            r = 47;
            g = 79;
            b = 79;
        }

        if (color == "limegreen")
        {
            r = 50;
            g = 205;
            b = 50;
        }

        if (color == "mediumseagreen")
        {
            r = 60;
            g = 179;
            b = 113;
        }

        if (color == "turquoise")
        {
            r = 64;
            g = 224;
            b = 208;
        }

        if (color == "royalblue")
        {
            r = 65;
            g = 105;
            b = 225;
        }

        if (color == "steelblue")
        {
            r = 70;
            g = 130;
            b = 180;
        }

        if (color == "darkslateblue")
        {
            r = 72;
            g = 61;
            b = 139;
        }

        if (color == "mediumturquoise")
        {
            r = 72;
            g = 209;
            b = 204;
        }

        if (color == "indigo")
        {
            r = 75;
            g = 0;
            b = 130;
        }

        if (color == "darkolivegreen")
        {
            r = 85;
            g = 107;
            b = 47;
        }

        if (color == "cadetblue")
        {
            r = 95;
            g = 158;
            b = 160;
        }

        if (color == "cornflowerblue")
        {
            r = 100;
            g = 149;
            b = 237;
        }

        if (color == "mediumaquamarine")
        {
            r = 102;
            g = 205;
            b = 170;
        }

        if (color == "dimgray")
        {
            r = 105;
            g = 105;
            b = 105;
        }

        if (color == "slateblue")
        {
            r = 106;
            g = 90;
            b = 205;
        }

        if (color == "olivedrab")
        {
            r = 107;
            g = 142;
            b = 35;
        }

        if (color == "slategray")
        {
            r = 112;
            g = 128;
            b = 144;
        }

        if (color == "lightslategray")
        {
            r = 119;
            g = 136;
            b = 153;
        }

        if (color == "mediumslateblue")
        {
            r = 123;
            g = 104;
            b = 238;
        }

        if (color == "lawngreen")
        {
            r = 124;
            g = 252;
            b = 0;
        }

        if (color == "chartreuse")
        {
            r = 127;
            g = 255;
            b = 0;
        }

        if (color == "aquamarine")
        {
            r = 127;
            g = 255;
            b = 212;
        }

        if (color == "maroon")
        {
            r = 128;
            g = 0;
            b = 0;
        }

        if (color == "purple")
        {
            r = 128;
            g = 0;
            b = 128;
        }

        if (color == "olive")
        {
            r = 128;
            g = 128;
            b = 0;
        }

        if (color == "gray")
        {
            r = 128;
            g = 128;
            b = 128;
        }

        if (color == "skyblue")
        {
            r = 135;
            g = 206;
            b = 235;
        }

        if (color == "lightskyblue")
        {
            r = 135;
            g = 206;
            b = 250;
        }

        if (color == "blueviolet")
        {
            r = 138;
            g = 43;
            b = 226;
        }

        if (color == "darkred")
        {
            r = 139;
            g = 0;
            b = 0;
        }

        if (color == "darkmagenta")
        {
            r = 139;
            g = 0;
            b = 139;
        }

        if (color == "saddlebrown")
        {
            r = 139;
            g = 69;
            b = 19;
        }

        if (color == "darkseagreen")
        {
            r = 143;
            g = 188;
            b = 143;
        }

        if (color == "lightgreen")
        {
            r = 144;
            g = 238;
            b = 144;
        }

        if (color == "mediumpurple")
        {
            r = 147;
            g = 112;
            b = 219;
        }

        if (color == "darkviolet")
        {
            r = 148;
            g = 0;
            b = 211;
        }

        if (color == "palegreen")
        {
            r = 152;
            g = 251;
            b = 152;
        }

        if (color == "darkorchid")
        {
            r = 153;
            g = 50;
            b = 204;
        }

        if (color == "yellowgreen")
        {
            r = 154;
            g = 205;
            b = 50;
        }

        if (color == "sienna")
        {
            r = 160;
            g = 82;
            b = 45;
        }

        if (color == "brown")
        {
            r = 165;
            g = 42;
            b = 42;
        }

        if (color == "darkgray")
        {
            r = 169;
            g = 169;
            b = 169;
        }

        if (color == "lightblue")
        {
            r = 173;
            g = 216;
            b = 230;
        }

        if (color == "greenyellow")
        {
            r = 173;
            g = 255;
            b = 47;
        }

        if (color == "paleturquoise")
        {
            r = 175;
            g = 238;
            b = 238;
        }

        if (color == "lightsteelblue")
        {
            r = 176;
            g = 196;
            b = 222;
        }

        if (color == "powderblue")
        {
            r = 176;
            g = 224;
            b = 230;
        }

        if (color == "firebrick")
        {
            r = 178;
            g = 34;
            b = 34;
        }

        if (color == "darkgoldenrod")
        {
            r = 184;
            g = 134;
            b = 11;
        }

        if (color == "mediumorchid")
        {
            r = 186;
            g = 85;
            b = 211;
        }

        if (color == "rosybrown")
        {
            r = 188;
            g = 143;
            b = 143;
        }

        if (color == "darkkhaki")
        {
            r = 189;
            g = 183;
            b = 107;
        }

        if (color == "silver")
        {
            r = 192;
            g = 192;
            b = 192;
        }

        if (color == "mediumvioletred")
        {
            r = 199;
            g = 21;
            b = 133;
        }

        if (color == "indianred")
        {
            r = 205;
            g = 92;
            b = 92;
        }

        if (color == "peru")
        {
            r = 205;
            g = 133;
            b = 63;
        }

        if (color == "chocolate")
        {
            r = 210;
            g = 105;
            b = 30;
        }

        if (color == "tan")
        {
            r = 210;
            g = 180;
            b = 140;
        }

        if (color == "lightgray")
        {
            r = 211;
            g = 211;
            b = 211;
        }

        if (color == "thistle")
        {
            r = 216;
            g = 191;
            b = 216;
        }

        if (color == "orchid")
        {
            r = 218;
            g = 112;
            b = 214;
        }

        if (color == "goldenrod")
        {
            r = 218;
            g = 165;
            b = 32;
        }

        if (color == "palevioletred")
        {
            r = 219;
            g = 112;
            b = 147;
        }

        if (color == "crimson")
        {
            r = 220;
            g = 20;
            b = 60;
        }

        if (color == "gainsboro")
        {
            r = 220;
            g = 220;
            b = 220;
        }

        if (color == "plum")
        {
            r = 221;
            g = 160;
            b = 221;
        }

        if (color == "burlywood")
        {
            r = 222;
            g = 184;
            b = 135;
        }

        if (color == "lightcyan")
        {
            r = 224;
            g = 255;
            b = 255;
        }

        if (color == "lavender")
        {
            r = 230;
            g = 230;
            b = 250;
        }

        if (color == "darksalmon")
        {
            r = 233;
            g = 150;
            b = 122;
        }

        if (color == "violet")
        {
            r = 238;
            g = 130;
            b = 238;
        }

        if (color == "palegoldenrod")
        {
            r = 238;
            g = 232;
            b = 170;
        }

        if (color == "lightcoral")
        {
            r = 240;
            g = 128;
            b = 128;
        }

        if (color == "khaki")
        {
            r = 240;
            g = 230;
            b = 140;
        }

        if (color == "aliceblue")
        {
            r = 240;
            g = 248;
            b = 255;
        }

        if (color == "honeydew")
        {
            r = 240;
            g = 255;
            b = 240;
        }

        if (color == "azure")
        {
            r = 240;
            g = 255;
            b = 255;
        }

        if (color == "sandybrown")
        {
            r = 244;
            g = 164;
            b = 96;
        }

        if (color == "wheat")
        {
            r = 245;
            g = 222;
            b = 179;
        }

        if (color == "beige")
        {
            r = 245;
            g = 245;
            b = 220;
        }

        if (color == "whitesmoke")
        {
            r = 245;
            g = 245;
            b = 245;
        }

        if (color == "mintcream")
        {
            r = 245;
            g = 255;
            b = 250;
        }

        if (color == "ghostwhite")
        {
            r = 248;
            g = 248;
            b = 255;
        }

        if (color == "salmon")
        {
            r = 250;
            g = 128;
            b = 114;
        }

        if (color == "antiquewhite")
        {
            r = 250;
            g = 235;
            b = 215;
        }

        if (color == "linen")
        {
            r = 250;
            g = 240;
            b = 230;
        }

        if (color == "lightgoldenrodyellow")
        {
            r = 250;
            g = 250;
            b = 210;
        }

        if (color == "oldlace")
        {
            r = 253;
            g = 245;
            b = 230;
        }

        if (color == "red")
        {
            r = 255;
            g = 0;
            b = 0;
        }

        if (color == "fuchsia")
        {
            r = 255;
            g = 0;
            b = 255;
        }

        if (color == "magenta")
        {
            r = 255;
            g = 0;
            b = 255;
        }

        if (color == "deeppink")
        {
            r = 255;
            g = 20;
            b = 147;
        }

        if (color == "orangered")
        {
            r = 255;
            g = 69;
            b = 0;
        }

        if (color == "tomato")
        {
            r = 255;
            g = 99;
            b = 71;
        }

        if (color == "hotpink")
        {
            r = 255;
            g = 105;
            b = 180;
        }

        if (color == "coral")
        {
            r = 255;
            g = 127;
            b = 80;
        }

        if (color == "darkorange")
        {
            r = 255;
            g = 140;
            b = 0;
        }

        if (color == "lightsalmon")
        {
            r = 255;
            g = 160;
            b = 122;
        }

        if (color == "orange")
        {
            r = 255;
            g = 165;
            b = 0;
        }

        if (color == "lightpink")
        {
            r = 255;
            g = 182;
            b = 193;
        }

        if (color == "pink")
        {
            r = 255;
            g = 192;
            b = 203;
        }

        if (color == "gold")
        {
            r = 255;
            g = 215;
            b = 0;
        }

        if (color == "peachpuff")
        {
            r = 255;
            g = 218;
            b = 185;
        }

        if (color == "navajowhite")
        {
            r = 255;
            g = 222;
            b = 173;
        }

        if (color == "moccasin")
        {
            r = 255;
            g = 228;
            b = 181;
        }

        if (color == "bisque")
        {
            r = 255;
            g = 228;
            b = 196;
        }

        if (color == "mistyrose")
        {
            r = 255;
            g = 228;
            b = 225;
        }

        if (color == "blanchedalmond")
        {
            r = 255;
            g = 235;
            b = 205;
        }

        if (color == "papayawhip")
        {
            r = 255;
            g = 239;
            b = 213;
        }

        if (color == "lavenderblush")
        {
            r = 255;
            g = 240;
            b = 245;
        }

        if (color == "seashell")
        {
            r = 255;
            g = 245;
            b = 238;
        }

        if (color == "cornsilk")
        {
            r = 255;
            g = 248;
            b = 220;
        }

        if (color == "lemonchiffon")
        {
            r = 255;
            g = 250;
            b = 205;
        }

        if (color == "floralwhite")
        {
            r = 255;
            g = 250;
            b = 240;
        }

        if (color == "snow")
        {
            r = 255;
            g = 250;
            b = 250;
        }

        if (color == "yellow")
        {
            r = 255;
            g = 255;
            b = 0;
        }

        if (color == "lightyellow")
        {
            r = 255;
            g = 255;
            b = 224;
        }

        if (color == "ivory")
        {
            r = 255;
            g = 255;
            b = 240;
        }

        if (color == "white")
        {
            r = 255;
            g = 255;
            b = 255;
        }

        int length = color.Length;
        char first = color[0];
        if (first == '(')
        {

            string[] parts = color.Substring(1, length - 2).Split(',');
            r = Convert.ToInt32(parts[0]);
            g = Convert.ToInt32(parts[1]);
            b = Convert.ToInt32(parts[2]);
            if (parts.Length == 4)
            {
                a = Convert.ToInt32(parts[3]);
            }
        }

        if( rgb == 'r' ) return( r );
        if( rgb == 'g' ) return( g );
        if( rgb == 'b' ) return( b );
        if( rgb == 'a' ) return( a );

        return -1;
    }


    private string SpecialColor()
    {
        string[] parts = Kind.Split(new string[] { "->" }, StringSplitOptions.None);
        List<string> result = new List<string>();
        foreach (string p in parts)
        {
            if (p == "bluered")
            {
                result.AddRange(new string[] { "blue", "white", "red" });
            }
            else if (p == "rainbow")
            {
                result.AddRange(new string[] { "blue", "aqua", "lime", "yellow", "yellow" });
            }
            else if (p == "redblue")
            {
                result.AddRange(new string[] { "red", "white", "blue" });
            }
            else if (p == "grainbow")
            {
                result.AddRange(new string[]
                {
                    "(160,0,200)", "(110,0,220)", "(30,60,255)", "(0,160,255)", "(0,200,200)", "(0,210,140)",
                    "(0,220,0)", "(160,230,50)", "(230,220,50)", "(230,175,45)", "(240,130,40)", "(250,60,60)",
                    "(240,0,130)"
                });
            }
            else if (p == "revgrainbow")
            {
                result.AddRange(new string[]
                {
                    "(240,0,130)", "(250,60,60)", "(240,130,40)", "(230,175,45)", "(230,220,50)", "(160,230,50)",
                    "(0,220,0)", "(0,210,140)", "(0,200,200)", "(0,160,255)", "(30,60,255)", "(110,0,220)",
                    "(160,0,200)"
                });
            }
            else
            {
                result.Add(p);
            }
        }

        return string.Join("->", result);
    }
}