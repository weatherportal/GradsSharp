using GradsSharp.Data;
using GradsSharp.Data.GridFunctions;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;

namespace GradsSharp.UnitTests.MathTests;

public class CDiffTests
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
    public void CDiff_FunctionTest()
    {
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetPolarStereoValues(OnOffSetting.On, -2.9, 12, 47, 56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLatitude(47, 56);
        engine.GradsCommandInterface.SetLongitude(-2.9, 12);
        engine.GradsCommandInterface.SetT(1);
        
        var tmp = engine.GradsCommandInterface.GetVariable("Temperature", FixedSurfaceType.SpecifiedHeightLevelAboveGround, 2);
        var lat = engine.GradsCommandInterface.GetVariable("lat", FixedSurfaceType.Missing, 0);
        var lon = engine.GradsCommandInterface.GetVariable("lon", FixedSurfaceType.Missing, 0);
        var dtx = tmp.CDiff(Dimension.X);
        var dty = tmp.CDiff(Dimension.Y);

        var dy = lat.CDiff(Dimension.Y) * 3.1416 / 180.0;
        var dx = lon.CDiff(Dimension.X) * 3.1416 / 180.0;

        
        
        

    }
}