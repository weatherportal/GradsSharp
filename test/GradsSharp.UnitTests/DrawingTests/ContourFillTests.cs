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
        engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetPolarStereoValues(OnOffSetting.On, -2.9,12,47,56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLatitude(47,56);
        engine.GradsCommandInterface.SetLongitude(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        engine.GradsCommandInterface.SetGraphicsOutputMode(GraphicsOutputMode.Shade2b);
        SetTemp2m();
        engine.GradsCommandInterface.Display("t2m");

        var outputFile = Path.GetTempFileName();
        
        engine.GradsCommandInterface.ExportImage(outputFile, horizontalSize:1024, verticalSize:768, format: OutputFormat.PNG);

        
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
    
    [Test]
    public void ContourFillTestTemperature850hpa()
    {
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.Open("Data//gfs.iso.t00z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetPolarStereoValues(OnOffSetting.On, -2.9,12,47,56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLatitude(47,56);
        engine.GradsCommandInterface.SetLongitude(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        engine.GradsCommandInterface.SetGraphicsOutputMode(GraphicsOutputMode.Shade2b);
        SetTemp850hpa();
        engine.GradsCommandInterface.Display("t850");

        var outputFile = Path.GetTempFileName();
        
        engine.GradsCommandInterface.ExportImage(outputFile, horizontalSize:1024, verticalSize:768, format: OutputFormat.PNG);

        
        var generated = Bitmap.FromFile(outputFile) as Bitmap;
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("GradsSharp.UnitTests.Data.Expected.t850-contour-fill.png");
        var expected = Bitmap.FromStream(stream) as Bitmap;
        
        // compare the bitmaps and assert they are equal
        Helpers.CompareBitmaps(generated, expected);
        
        generated.Dispose();
        expected.Dispose();
        
        File.Delete(outputFile);

        engine.GradsCommandInterface.reset();
        SetTemp500hpa();
        SetTemp850hpa();
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLatitude(47,56);
        engine.GradsCommandInterface.SetLongitude(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.SetGraphicsOutputMode(GraphicsOutputMode.Shade2b);
        engine.GradsCommandInterface.Display("t850");

        outputFile = Path.GetTempFileName();
        
        engine.GradsCommandInterface.ExportImage(outputFile, horizontalSize:1024, verticalSize:768, format: OutputFormat.PNG);

        
        generated = Bitmap.FromFile(outputFile) as Bitmap;
        stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("GradsSharp.UnitTests.Data.Expected.t850-contour-fill.png");
        expected = Bitmap.FromStream(stream) as Bitmap;
        
        // compare the bitmaps and assert they are equal
        Helpers.CompareBitmaps(generated, expected);
        
        generated.Dispose();
        expected.Dispose();
        
        File.Delete(outputFile);
        
    }
    
    private void SetTemp500hpa()
    {
        IGradsGrid data = engine.GradsCommandInterface.GetVariable(new VariableDefinition()
        {
            HeightType = FixedSurfaceType.IsobaricSurface,
            HeightValue = 500,
            VariableName = "Temperature"
        });

        data = data.Subtract(273.15);

        engine.GradsCommandInterface.Define("t500", data);
    }
    private void SetTemp850hpa()
    {
        IGradsGrid data = engine.GradsCommandInterface.GetVariable(new VariableDefinition()
        {
            HeightType = FixedSurfaceType.IsobaricSurface,
            HeightValue = 850,
            VariableName = "Temperature"
        });

        data = data.Subtract(273.15);

        engine.GradsCommandInterface.Define("t850", data);
    }

    private void SetTemp2m()
    {
        IGradsGrid data = engine.GradsCommandInterface.GetVariable(new VariableDefinition()
        {
            HeightType = FixedSurfaceType.SpecifiedHeightLevelAboveGround,
            HeightValue = 2,
            VariableName = "Temperature"
        });

        data = data.Subtract(273.15);

        engine.GradsCommandInterface.Define("t2m", data);
    }
}