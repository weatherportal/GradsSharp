using System.Drawing;
using System.Reflection;
using GradsSharp.Data;
using GradsSharp.Data.GridFunctions;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;
using GradsSharp.Models;

namespace GradsSharp.UnitTests;

public class ContourFillTests
{
    
    private GradsEngine engine;
    private IGriddedDataReader reader;
    
    [SetUp]
    public void Setup()
    {
        engine = new GradsEngine();
        engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
        engine.InitEngine();

        reader = new GFSDataReader();
    }

    [Test]
    public void ContourFillTestTemperature2m()
    {
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.Open("Data//gfs.t00z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetPolarStereoValues(OnOffSetting.On, -2.9,12,47,56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLat(47,56);
        engine.GradsCommandInterface.SetLon(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        engine.GradsCommandInterface.SetGraphicsOutputMode(GraphicsOutputMode.Shade2b);
        SetTemp2m();
        engine.GradsCommandInterface.Display("t2m");

        var outputFile = Path.GetTempFileName();
        
        engine.GradsCommandInterface.printim(outputFile, horizontalSize:1024, verticalSize:768, format: OutputFormat.PNG);

        
        var generated = Bitmap.FromFile(outputFile) as Bitmap;
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("GradsSharp.UnitTests.Data.Expected.contourfill-test.png");
        var expected = Bitmap.FromStream(stream) as Bitmap;
        
        // compare the bitmaps and assert they are equal
        Helpers.CompareBitmaps(generated, expected);
        
        generated.Dispose();
        expected.Dispose();
        
        File.Delete(outputFile);
        

        
    }


    private void SetTemp2m()
    {
        IGradsGrid data = engine.GradsCommandInterface.GetVariable(new VariableDefinition()
        {
            HeightType = FixedSurfaceType.SpecifiedHeightLevelAboveGround,
            HeightValue = 2,
            VariableType = DataVariable.Temperature
        });

        data = data.Subtract(273.15);

        engine.GradsCommandInterface.Define("t2m", data);
    }
}