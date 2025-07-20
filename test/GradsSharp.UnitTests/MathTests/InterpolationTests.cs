using GradsSharp.Data;
using GradsSharp.Data.GridFunctions;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;
using Shouldly;

namespace GradsSharp.UnitTests.MathTests;

public class InterpolationTests
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
    public void Regrid_FunctionTest()
    {
        engine.GradsCommandInterface.SetGrads(OnOffSetting.Off);
        engine.GradsCommandInterface.Open("Data//gfs.t06z.pgrb2.0p25.f001", reader);
        engine.GradsCommandInterface.SetPolarStereoValues(OnOffSetting.On, -2.9, 12, 47, 56);
        engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
        engine.GradsCommandInterface.SetGridOptions(GridOption.On);
        engine.GradsCommandInterface.SetLatitude(53, 55);
        engine.GradsCommandInterface.SetLongitude(1, 4);
        engine.GradsCommandInterface.SetT(1);
        
        var tmp = engine.GradsCommandInterface.GetVariable("Temperature", FixedSurfaceType.SpecifiedHeightLevelAboveGround, 2);

        var result = GridInterpolationFunctions.Regrid(tmp,  0.5 , engine.GradsCommandInterface, InterpolationMode.Bilinear);
        
        result.DimensionMaximum.ShouldBe(tmp.DimensionMaximum);
        result.DimensionMinimum.ShouldBe(tmp.DimensionMinimum);

        result.ISize.ShouldBe(7);
        result.JSize.ShouldBe(5);
        result.GridData.Length.ShouldBe(35);

    }
}