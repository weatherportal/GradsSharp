using System.Reflection;
using GradsSharp.Data;
using GradsSharp.Data.GridFunctions;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;
using GradsSharp.Models;

namespace GradsSharp.UnitTests.DrawingTests;

public class TiffTest
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

    [Test, Ignore("Tif library outputs other bytes, visual inspection needed")]
    public void TestTiffOutput()
    {
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetPolarStereoValues(OnOffSetting.On, -2.9,12,47,56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLatitude(47,56);
        engine.GradsCommandInterface.SetLongitude(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        engine.GradsCommandInterface.SetGraphicsOutputMode(GraphicsOutputMode.Kml);
        
        SetTemp2m();

        var outputFile = Path.GetTempPath() + "t2m.kml";
        
        engine.GradsCommandInterface.SetKmlOutput(KmlOutputFlag.Image, outputFile);
        engine.GradsCommandInterface.Display("t2m");
        
        var outputStream = File.OpenRead(outputFile);
        var outputStreamTif = File.OpenRead(outputFile.Replace(".kml", ".tif"));
        
        
        Helpers.CompareXmlFiles(Assembly.GetExecutingAssembly().GetManifestResourceStream("GradsSharp.UnitTests.Data.Expected.t2m_tif.kml") ,outputStream);
        Helpers.CompareBinaryFiles(Assembly.GetExecutingAssembly().GetManifestResourceStream("GradsSharp.UnitTests.Data.Expected.t2m_tif.tif") ,outputStreamTif);
        
        outputStream.Close();
        outputStreamTif.Close();
        
        File.Delete(outputFile);
        File.Delete(outputFile.Replace(".kml", ".tif"));
        
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