﻿using System.Drawing;
using System.Xml;
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

    public static void CompareXmlFiles(Stream expected, Stream generated)
    {
        StreamReader expectedReader = new StreamReader(expected);
        StreamReader generatedReader = new StreamReader(generated);

        var expectedData = expectedReader.ReadToEnd();
        var generatedData = generatedReader.ReadToEnd();
        
        XmlDocument expectedXml = new XmlDocument();
        expectedXml.LoadXml(expectedData);
        
        XmlDocument generatedXml = new XmlDocument();
        generatedXml.LoadXml(generatedData);
        
        
        
        expectedXml.ToString().ShouldBe(generatedXml.ToString());
    }
}