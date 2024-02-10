using System.Drawing;
using System.Reflection;
using GradsSharp.Data;
using GradsSharp.Data.GridFunctions;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Models;

namespace GradsSharp.UnitTests;

public class KmlTests
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
    public void KmlTestTemperature2m()
    {
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.Open("Data//gfs.t00z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetMPVals(OnOffSetting.On, -2.9,12,47,56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLat(47,56);
        engine.GradsCommandInterface.SetLon(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        engine.GradsCommandInterface.SetGraphicsOut(GxOutSetting.Kml);
        
        SetTemp2m();
        
        var outputFile = Path.GetTempFileName();
        
        engine.GradsCommandInterface.SetKmlOutput(KmlOutputFlag.Contour, outputFile);
        engine.GradsCommandInterface.Display("t2m");

        var outputStream = File.OpenRead(outputFile);
        
        
        Helpers.CompareXmlFiles(Assembly.GetExecutingAssembly().GetManifestResourceStream("GradsSharp.UnitTests.Data.Expected.t2m.kml") ,outputStream);
        
        outputStream.Close();
        
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