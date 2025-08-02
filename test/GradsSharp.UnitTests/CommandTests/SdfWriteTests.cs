using GradsSharp.Data;
using GradsSharp.Data.GridFunctions;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;
using GradsSharp.Models;
using Microsoft.Research.Science.Data;

namespace GradsSharp.UnitTests.CommandTests;

public class SdfWriteTests
{
    private GradsEngine engine;
    private IGriddedDataReader reader;
    private string testFilePath = "";
    
    [SetUp]
    public void Setup()
    {
        engine = new GradsEngine();
        engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
        engine.InitEngine();

        reader = new GFSDataReader();
    }

    [TearDown]
    public void TearDown()
    {
        if(File.Exists(testFilePath))
            File.Delete(testFilePath);
    }

    [Test]
    [Ignore("only on mac and linux")]
    public void TestWriteNetCDF()
    {
        testFilePath = Path.GetTempFileName() + ".nc";
        engine.GradsCommandInterface.reset();
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetPolarStereoValues(OnOffSetting.On, -2.9,12,47,56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLatitude(47,56);
        engine.GradsCommandInterface.SetLongitude(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        engine.GradsCommandInterface.SetGraphicsOutputMode(GraphicsOutputMode.Grid);
        SetTemp2m();
        engine.GradsCommandInterface.SetSdfWrite(testFilePath);
        engine.GradsCommandInterface.SdfWrite("t2m");
        
        engine.GradsCommandInterface.reset();


        var dataSet = DataSet.Open(testFilePath, ResourceOpenMode.ReadOnly);
        Assert.That(dataSet.Variables.Count, Is.EqualTo(3));

        var lonValues = dataSet.Variables["lon"].GetData();
        var latValues = dataSet.Variables["lat"].GetData();
        var dataValues = dataSet.Variables["t2m"].GetData();
        
        Assert.That(lonValues.Length, Is.EqualTo(61));
        Assert.That(latValues.Length, Is.EqualTo(37));
        Assert.That(dataValues.GetLength(0), Is.EqualTo(37));
        Assert.That(dataValues.GetLength(1), Is.EqualTo(61));

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