using System.Drawing;
using Shouldly;

namespace GradsSharp.UnitTests;

public class Helpers
{
    public static void CompareBitmaps(Bitmap bitmap1, Bitmap bitmap2)
    {
        bitmap1.Width.ShouldBe(bitmap2.Width);
        bitmap1.Height.ShouldBe(bitmap2.Height);

        for (int y = 0; y < bitmap1.Height; y++)
        {
            for (int x = 0; x < bitmap1.Width; x++)
            {
                bitmap1.GetPixel(x, y).ShouldBe(bitmap2.GetPixel(x, y));
            }
        }
    }
}