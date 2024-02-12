using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Enums;
using GradsSharp.Exceptions;
using Shouldly;

namespace GradsSharp.UnitTests.CommandTests;

public class CommandTests
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
    public void TestSetColor()
    {
       
        engine.GradsCommandInterface.SetColor(17, 2, 3, 4);
        var result = engine.GradsDatabase.gxdbqcol(17);
        result.Item1.ShouldBe(2);
        result.Item2.ShouldBe(3);
        result.Item3.ShouldBe(4);
        result.Item4.ShouldBe(255);
        result.Item5.ShouldBe(-999);
    }

    [Test]
    public void TestSetInvalidColor()
    {
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(15, 2, 3, 4));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(2049, 2, 3, 4));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, -1, 3, 4));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, 256, 3, 4));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, 2, -1, 4));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, 2, 256, 4));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, 2, 3, -1));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, 2, 3, 256));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, 2, 3, 4, -256));
        Should.Throw<InvalidColorException>(() => engine.GradsCommandInterface.SetColor(17, 2, 3, 4, 256));
    }

    [Test]
    public void TestSetGraphicsOut()
    {
        engine.GradsCommandInterface.SetGraphicsOutputMode(GraphicsOutputMode.Contour);
        
        engine.DrawingContext.CommonData.gout2a.ShouldBe(1);
    }
}