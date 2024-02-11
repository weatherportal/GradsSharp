# GradsSharp.DrawingEngine.Cairo

This library is a drawing engine for GradsSharp. It uses the Cairo library to draw the graphics.


## Installation

```bash
dotnet add package GradsSharp.DrawingEngine.Cairo
```

## Usage

```csharp
using GradsSharp.DrawingEngine.Cairo;
using GradsSharp;

var engine = new GradsEngine();
engine.RegisterDrawingEngine(new CairoDrawingEngine(engine));
engine.InitEngine();
```

## License

This library is licensed under the MIT license.

