namespace GradsSharp.Models;

public class ColorDefinition
{
    public ColorDefinition(int colorNumber)
    {
        ColorNumber = colorNumber;
        Red = 0;
        Green = 0;
        Blue = 0;
        Alpha = 255;

    }

    public ColorDefinition(int colorNumber, int red, int green, int blue)
    {
        ColorNumber = colorNumber;
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = 255;
    }

    public ColorDefinition(int colorNumber, int red, int green, int blue, int alpha)
    {
        ColorNumber = colorNumber;
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    public int ColorNumber { get; set; }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public int Alpha { get; set; }

    public static implicit operator ColorDefinition((int c, int r, int g, int b, int a) colorDef) =>
        new ColorDefinition(colorDef.c, colorDef.r, colorDef.g, colorDef.b, colorDef.a);
    
    public static implicit operator ColorDefinition((int c, int r, int g, int b) colorDef) =>
        new ColorDefinition(colorDef.c, colorDef.r, colorDef.g, colorDef.b);
}