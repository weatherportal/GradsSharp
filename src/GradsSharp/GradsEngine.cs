using GradsSharp.Data.Grads;
using GradsSharp.Drawing;
using GradsSharp.Drawing.Grads;
using Microsoft.Extensions.Logging;

namespace GradsSharp;

public class GradsEngine
{
    private DrawingContext _drawingContext;
    private ILogger? _logger;
    private static ILogger? _staticLogger;

    public static ILogger? Logger => _staticLogger; 
    
    public GradsEngine(ILogger? logger = null)
    {
        _drawingContext = new DrawingContext();
        _logger = logger;
        //TODO for now wire it up here as a static too
        _staticLogger = logger;
    }

    public void RegisterDrawingEngine(IDrawingEngine engine)
    {
        _drawingContext.GradsDrawingInterface.DrawingEngine = engine;
    }

    public void DrawBuffersToOutput()
    {
        _drawingContext.GxMeta.gxhdrw(0, 1);
    }

    public void InitEngine()
    {
        _drawingContext.CommonData.InitData();
        int rc = _drawingContext.GaGx.gagx();
        if (rc > 0) throw new Exception("Error setting up graphics");
    }

    public void EndDrawing()
    {
        _drawingContext.GradsDrawingInterface.gxend();
    }

    public IGradsDrawingInterface GradsDrawingInterface => _drawingContext.GradsDrawingInterface;
    public IGradsCommandInterface GradsCommandInterface => _drawingContext.GradsCommandInterface;
    public IGradsDatabase GradsDatabase => _drawingContext.GradsDatabase;
}