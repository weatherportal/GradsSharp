# Quick start

GradsSharp allows you to draw gridded data to any output.

## Installing

To start using GradsSharp in your project first add the dependency to your project:

```dotnet add package GradsSharp```

Next install a drawing engine, e.g. Cairo:

```dotnet add package GradsSharp.DrawingEngine.Cairo```

## Sending commands to the engine:

```csharp

var engine = new GradsEngine();
engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
engine.InitEngine();

engine.GradsCommandInterface.Open("somegrib.grb2", DataFormat.GFS_GRIB2);

// setup the environment with all available methods in the command interface
// engine.GradsCommandInterface.SetLat(30,50);
// ....
// 

// now output the image to output.png
engine.GradsCommandInterface.printim("output.png", OutputFormat.PNG, horizontalSize: 1024, verticalSize: 768);

engine.EndDrawing();
 
```
