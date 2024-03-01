using System.Drawing;
using System.Reflection;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;

namespace GradsSharp.UnitTests.DrawingTests;

public class PlotTextTest
{
    private GradsEngine engine;
    
    
    [SetUp]
    public void Setup()
    {
        engine = new GradsEngine();
        engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
        engine.InitEngine();
        
        
    }

    [Test]
    public void PlotMultipleTextTest()
    {
        
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        //engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        
        
        engine.GradsCommandInterface.SetStringOptions(1, StringJustification.Center, 3, 0);
        engine.GradsCommandInterface.DrawString(4, 5, "Some centered string");
        
        engine.GradsCommandInterface.SetStringOptions(1, StringJustification.Left, 3, 0);
        engine.GradsCommandInterface.DrawString(4, 4.7, "Some left string");
        
        engine.GradsCommandInterface.SetStringOptions(1, StringJustification.Right, 5, 0);
        engine.GradsCommandInterface.DrawString(4, 4.4, "Some right string");
        
        engine.GradsCommandInterface.SetStringOptions(1, StringJustification.Left, 3, 0);
        engine.GradsCommandInterface.DrawString(4, 4.1, "\u00603.\u00600C, higher means `bincreasing `amoisture`n \u0026 temperature");
        
        engine.GradsCommandInterface.SetStringOptions(1, StringJustification.Left, 5, 270);
        engine.GradsCommandInterface.DrawString(1, 3.4, "Some vertical string");
        
        engine.GradsCommandInterface.SetStringOptions(1, StringJustification.Left, 5, 90);
        engine.GradsCommandInterface.DrawString(1.6, 5.4, "Some vertical string");
        
        
        engine.GradsCommandInterface.SetStringOptions(1, StringJustification.Left, 3, 0);
        engine.GradsCommandInterface.DrawString(9, 5.4, "`0Font 0");
        engine.GradsCommandInterface.DrawString(9, 5.1, "`1Font 1");
        engine.GradsCommandInterface.DrawString(9, 4.8, "`2Font 2");
        engine.GradsCommandInterface.DrawString(9, 4.5, "`3Font 3");
        engine.GradsCommandInterface.DrawString(9, 4.2, "`5Font 5");
        

        var outputFile = Path.GetTempFileName();
        
        engine.GradsCommandInterface.ExportImage(outputFile, horizontalSize:1024, verticalSize:768, format: OutputFormat.PNG);

        var generated = Bitmap.FromFile(outputFile) as Bitmap;
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("GradsSharp.UnitTests.Data.Expected.plottext-test.png");
        var expected = Bitmap.FromStream(stream) as Bitmap;
        
        // compare the bitmaps and assert they are equal
        Helpers.CompareBitmaps(generated, expected);
        
        generated.Dispose();
        expected.Dispose();
        
        File.Delete(outputFile);
        
    }
}
