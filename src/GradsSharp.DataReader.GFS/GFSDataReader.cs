using GradsSharp.Data;
using GradsSharp.Enums;
using GradsSharp.Models;
using NGrib;
using NGrib.Grib2;
using NGrib.Grib2.Templates.GridDefinitions;
using NGrib.Grib2.Templates.ProductDefinitions;

namespace GradsSharp.DataReader.GFS;

public class GFSDataReader : IGriddedDataReader
{

    private InputFile? _inputFile;
    private GfsVariables _variableMapping = new GfsVariables();
    private List<DataSet>? _dataSets = null;
    public InputFile OpenFile(Stream stream, string file)
    {
        var inputFile = new InputFile();
        inputFile.FileName = file;
        using Grib2Reader rdr = new Grib2Reader(stream);
        return GetInputFile(rdr, inputFile);
    }

    public InputFile OpenFile(string file)
    {
        var inputFile = new InputFile();
        inputFile.FileName = file;
        
        using Grib2Reader rdr = new Grib2Reader(file);
        return GetInputFile(rdr, inputFile);
    }

    private InputFile GetInputFile(Grib2Reader rdr, InputFile inputFile)
    {
        _dataSets = rdr.ReadAllDataSets().ToList();
        var dataset = _dataSets.First();

        ProductDefinition0000 pd;

        List<double> levels = new List<double>();

        foreach (var ds in _dataSets)
        {
            var pdef = ds.ProductDefinitionSection.ProductDefinition as ProductDefinition0000;
            if (pdef.FirstFixedSurfaceType == NGrib.Grib2.CodeTables.FixedSurfaceType.IsobaricSurface)
            {
                if (!levels.Contains((pdef.FirstFixedSurfaceValue ?? 0) / 100.0))
                {
                    levels.Add((pdef.FirstFixedSurfaceValue ?? 0) / 100.0);
                }
            }
        }


        levels = levels.OrderByDescending(x => x).ToList();
        
        if (dataset.GridDefinitionSection.GridDefinition is LatLonGridDefinition gd)
        {
            var lonMin = gd.Lo1;
            var lonMax = gd.Lo2;
            if (lonMin > lonMax)
            {
                lonMin -= 360;
            }


            inputFile.FileType = FileType.Gridded;
            
            
            // X-Dimension = lon
            inputFile.Dx = (int)gd.Nx;
            inputFile.XMin = lonMin;
            inputFile.XIncrement = gd.Dx;
            inputFile.XDimensionType = InputFileDimensionType.Linear;
            
            inputFile.Dy = (int)gd.Ny;
            inputFile.YMin = gd.La1;
            inputFile.YIncrement = gd.Dy;
            inputFile.YDimensionType = InputFileDimensionType.Linear;

            inputFile.Dz = levels.Count;
            inputFile.ZDimensionType = InputFileDimensionType.Levels;
            inputFile.ZLevels = levels.ToArray();

            inputFile.Dt = 1;   // for GFS this is always one
            inputFile.ReferenceTime = dataset.Message.IdentificationSection.ReferenceTime;
            inputFile.TDimensionType = InputFileDimensionType.Linear;
            inputFile.TimeStepIntervalMinutes = 60;

            inputFile.De = 1;
            inputFile.EDimensionType = InputFileDimensionType.Linear;
            inputFile.EMin = 1;
            inputFile.EIncrement = 1;
           
        }

        foreach (var ds in _dataSets)
        {
            var ps = ds.ProductDefinitionSection.ProductDefinition as ProductDefinition0000;
            FixedSurfaceType sfcType = (FixedSurfaceType)ps.FirstFixedSurfaceType;
            var heightValue = ps.FirstFixedSurfaceValue;
        
            if (sfcType == FixedSurfaceType.IsobaricSurface)
            {
                heightValue /= 100;
            }
            
            
            InputVariable gv = new();
        
            gv.Definition = new VariableDefinition
            {
                HeightType = sfcType,
                HeightValue = heightValue ?? Double.MinValue,
                VariableName = _variableMapping.GetGrib2VarType((int)ds.Message.IndicatorSection.Discipline, 
                    ds.ProductDefinitionSection.ProductDefinition.ParameterCategory,
                    ds.ProductDefinitionSection.ProductDefinition.ParameterNumber)
            };
            gv.Abbreviation = gv.Definition.VariableName;

            inputFile.Variables.Add(gv);
        }
        
        
        _inputFile = inputFile;

        return inputFile;
    }

    public void CloseFile()
    {
        
    }

    public void ReadData(IGradsGrid grid, VariableDefinition definition)
    {

        if (_inputFile == null) throw new Exception("No file open yet");
        
        GribDataSetInfo info = (GribDataSetInfo)_variableMapping.GetVariableInfo(definition.VariableName);

        double[] dmin = grid.WorldDimensionMinimum;
        double[] dmax = grid.WorldDimensionMaximum;
        
        // dmin[0] = Math.Floor(dmin[0] + 0.0001);
        // dmax[0] = Math.Ceiling(dmax[0] - 0.0001);
        // dmin[1] = Math.Floor(dmin[1] + 0.0001);
        // dmax[1] = Math.Ceiling(dmax[1] - 0.0001);

        double heightValue = Double.IsNaN(definition.HeightValue) ? dmin[2] : definition.HeightValue;

        if (definition.HeightType == FixedSurfaceType.IsobaricSurface)
        {
            heightValue *= 100;
        }
        
        using Grib2Reader rdr = new Grib2Reader(_inputFile.FileName);
        foreach (var ds in _dataSets)
        {
            if (info.Discipline == (int)ds.Message.IndicatorSection.Discipline &&
                info.ParameterCategory == ds.ProductDefinitionSection.ProductDefinition.ParameterCategory &&
                info.ParameterNumber == ds.ProductDefinitionSection.ProductDefinition.ParameterNumber)
            {

                var ps = ds.ProductDefinitionSection.ProductDefinition as ProductDefinition0000;
                if ((int)ps.FirstFixedSurfaceType == (int)definition.HeightType)
                {
                    if (ps.FirstFixedSurfaceValue == heightValue)
                    {

                        List<double> result = new();
                        var valueEnum = rdr.ReadDataSetValues(ds);
                        foreach (var val in valueEnum)
                        {
                            if (val.Key.Latitude >= dmin[1] && val.Key.Latitude <= dmax[1] &&
                                val.Key.Longitude >= dmin[0] && val.Key.Longitude <= dmax[0])
                            {
                                result.Add((double)(val.Value ?? grid.Undef));
                            }
                        }

                        grid.GridData = result.ToArray();
                        grid.UndefinedMask = new byte[grid.GridData.Length];
                        for (int j = 0; j < grid.GridData.Length; j++)
                        {
                            grid.UndefinedMask[j] = 1;
                        }
                    }
                }

            }
        }

    }
}