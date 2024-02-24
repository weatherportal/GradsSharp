using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;
using GradsSharp.Models;
using Shouldly;

namespace GradsSharp.UnitTests.CommandTests;

public class GetVariableTests
{
    [Test]
    public void TestLongVariable()
    {
        GFSDataReader reader = new GFSDataReader();
        var engine = new GradsEngine();
        engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
        engine.InitEngine();

        engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetLatitude(47,56);
        engine.GradsCommandInterface.SetLongitude(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        var data = engine.GradsCommandInterface.GetVariable("RelativeHumidity",  FixedSurfaceType.IsobaricSurface, 850);

        data.GridData.Length.ShouldBeGreaterThan(0);

    }
    
    [Test]
    public void TestFetchDefinedVariable()
    {
        GFSDataReader reader = new GFSDataReader();
        var engine = new GradsEngine();
        engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
        engine.InitEngine();

        engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetLatitude(47,56);
        engine.GradsCommandInterface.SetLongitude(-2.9,12);
        engine.GradsCommandInterface.SetT(1);
        var data = engine.GradsCommandInterface.GetVariable("RelativeHumidity",  FixedSurfaceType.IsobaricSurface, 850);

        engine.GradsCommandInterface.Define("t850", data);
        
        
        var data2 = engine.GradsCommandInterface.GetVariable(new VariableDefinition() { VariableName = "t850" });
        
        data2.GridData.Length.ShouldBe(data.GridData.Length);
        data2.GridData.ShouldBe(data.GridData);

    }
}