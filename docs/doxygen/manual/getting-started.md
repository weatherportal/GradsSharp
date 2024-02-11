\mainpage
# Getting started

Following instructions show how to get started with GradsSharp. The sample outputs GFS weather data to a png file. 

## Installing

To start using GradsSharp in your project first add the dependency to your project:

```
dotnet new console -n MyChartPlotter
cd MyChartPlotter
dotnet add package GradsSharp
```

Next install a drawing engine, e.g. Cairo:

```dotnet add package GradsSharp.DrawingEngine.Cairo```

Next install the GFS data reader:

```dotnet add package GradsSharp.DataReader.GFS```

## Plotting a weather chart

The following sample assumes you download a grib file from the [NOAA website](https://nomads.ncep.noaa.gov/gribfilter.php?ds=gfs_0p25)

```csharp

using GradsSharp;
using GradsSharp.Data.GridFunctions;
using GradsSharp.DataReader.GFS;
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp.Models;


var engine = new GradsEngine();
engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
engine.InitEngine();

engine.GradsCommandInterface.Open("gfs.t00z.pgrb2.0p25.f001", new GFSDataReader());

// setup the environment with the available methods in the command interface
engine.GradsCommandInterface.SetPArea(OnOffSetting.On, 0, 11, 0, 8);
engine.GradsCommandInterface.SetMPVals(OnOffSetting.On, -3, 12, 42, 55);
engine.GradsCommandInterface.SetLat(42,55);
engine.GradsCommandInterface.SetLon(-3,12);
engine.GradsCommandInterface.SetGraphicsOut(GxOutSetting.Shaded);
engine.GradsCommandInterface.SetT(1);
engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);

IGradsGrid data = engine.GradsCommandInterface.GetVariable(new VariableDefinition()
{
    HeightType = FixedSurfaceType.SpecifiedHeightLevelAboveGround,
    HeightValue = 2,
    VariableType = DataVariable.Temperature
});

data = data.Subtract(273.15);

engine.GradsCommandInterface.Define("t2m", data);

engine.GradsCommandInterface.Display("t2m");

// now output the image to output.png
engine.GradsCommandInterface.printim("t2m.png", OutputFormat.PNG, horizontalSize: 1024, verticalSize: 768);

engine.EndDrawing();
 
```



After building the project now run the executable.  On Windows do not use dotnet run, but execute the exe directly from the Debug or Release directory.

\note Make sure to have a grib file with the name gfs.t00z.pgrb2.0p25.f001 in the same directory of your executable.

The result should be an image like this:

![Temparature 2m](t2m.png)