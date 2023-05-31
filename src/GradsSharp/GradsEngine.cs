using GradsSharp.Data.Grads;
using GradsSharp.Drawing;
using GradsSharp.Drawing.Grads;
using Microsoft.Extensions.Logging;

namespace GradsSharp;

/// <summary>
/// This class is the basis to start working with the library.  It gives you an interface to send commands to the
/// drawing backend.  It requires registration of a <see cref="IDrawingEngine">IDrawingEngine</see> to be able to output the drawings to an outputtype
/// of choice
/// </summary>
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

    /// <summary>
    /// Register a drawing engine to output the drawing instructions to
    /// </summary>
    /// <param name="engine">Instance of the drawing engine</param>
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