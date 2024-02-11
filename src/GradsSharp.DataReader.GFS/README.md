# GradsSharp.DataReader.GFS 

This library is a data reader for GradsSharp. It reads data from the GFS model.

## Installation

```bash
dotnet add package GradsSharp.DataReader.GFS
```

## Usage

```csharp
using GradsSharp.DataReader.GFS;
using GradsSharp;

var engine = new GradsEngine();
var reader = new GFSDataReader();
engine.Open("somefile.grb2", reader);

```

## License

This library is licensed under the MIT license.

