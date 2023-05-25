using GradsSharp.Drawing;
using GradsSharp.Models;
using NGrib;
using NGrib.Grib2.Templates.ProductDefinitions;

namespace GradsSharp.Data;

internal class GfsGribDataReader : IGriddedDataReader
{
    public double[] ReadData(gafile file, VariableDefinition definition)
    {
        Grib2Reader rdr = new Grib2Reader(file.name);
        foreach (var ds in rdr.ReadAllDataSets())
        {
            if (ds.Parameter != null)
            {

                if (ds.Parameter.Value.Name == definition.VariableType.ToString())
                {

                    var ps = ds.ProductDefinitionSection.ProductDefinition as ProductDefinition0000;
                    if ((int)ps.FirstFixedSurfaceType == (int)definition.HeightType)
                    {
                        if (ps.FirstFixedSurfaceValue == definition.HeightValue)
                        {
                            float?[] values = rdr.ReadDataSetRawData(ds).ToArray();
                            return Array.ConvertAll(values, x => (double)(x ?? Double.MinValue));
                        }
                    }

                }
                
            }
        }

        return Array.Empty<double>();
    }
}