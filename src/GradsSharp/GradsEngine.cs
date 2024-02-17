using GradsSharp.Data.Grads;
using GradsSharp.Drawing;
using GradsSharp.Drawing.Grads;
using GradsSharp.Enums;
using GradsSharp.Models;
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
    
    /// <summary>
    /// Constructor for the GradsEngine.  The constructor will initialize the paper size based on orientation and aspectratio.
    ///
    /// Default values for the papersizes:
    ///
    /// - Landscape:  11.0 x 8.5
    /// - Portrait:   8.5 x 11.0
    /// </summary>
    /// <param name="logger">optional logger to output information about the drawing</param>
    /// <param name="orientation">Orientation of the output</param>
    /// <param name="aspectRatio">Aspect ratio for the output size</param>
    public GradsEngine(ILogger? logger = null, Orientation orientation = Orientation.Landscape, double aspectRatio = -999)
    {
        _drawingContext = new DrawingContext(logger);
        
        if (orientation == Orientation.Landscape)
        {
            GradsCommandInterface.SetPaperSize(11.0f, 8.5f);
        }
        else
        {
            GradsCommandInterface.SetPaperSize( 8.5f,11.0f);
        }

        if (aspectRatio > -990)
        {
            if (aspectRatio < 1.0f)
            {
                GradsCommandInterface.SetPaperSize(11.0f * aspectRatio, 11.0f);
                
            }
            else
            {
                GradsCommandInterface.SetPaperSize(11.0f, 11.0f / aspectRatio);
            }
        }
        
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

    /// <summary>
    /// Called from a drawing engine to draw all instructions in the buffer to the output
    /// </summary>
    public void DrawBuffersToOutput()
    {
        _drawingContext.GxMeta.gxhdrw(0, 1);
    }

    /// <summary>
    /// Initializes the engine and graphics output.  This should only be called once
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void InitEngine()
    {
        _drawingContext.CommonData.InitData();
        int rc = _drawingContext.GaGx.gagx();
        if (rc > 0) throw new Exception("Error setting up graphics");
    }

    /// <summary>
    /// Ends the drawing operations
    /// </summary>
    public void EndDrawing()
    {
        _drawingContext.GradsDrawingInterface.gxend();
    }

    /// <summary>
    /// Entry to direct drawing some basic shapes and text
    /// </summary>
    public IGradsDrawingInterface GradsDrawingInterface => _drawingContext.GradsDrawingInterface;
    /// <summary>
    /// Entry to send commands to the engine
    /// </summary>
    public IGradsCommandInterface GradsCommandInterface => _drawingContext.GradsCommandInterface;
    /// <summary>
    /// Entry to the in memory database of colors, line widths, ...
    /// </summary>
    public IGradsDatabase GradsDatabase => _drawingContext.GradsDatabase;
    
    internal DrawingContext DrawingContext => _drawingContext;
}