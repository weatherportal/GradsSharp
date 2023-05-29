using GradsSharp.Drawing;
using GradsSharp.Models;
using GradsSharp.Models.Internal;
using NGrib;
using NGrib.Grib2.Templates.ProductDefinitions;

namespace GradsSharp.Data;

internal class GfsGribDataReader : IGriddedDataReader
{
    public double[] ReadData(GradsCommon pcmn, GradsFile file, VariableDefinition definition)
    {
        Grib2Reader rdr = new Grib2Reader(file.name);
        foreach (var ds in rdr.ReadAllDataSets())
        {
            if (ds.Parameter != null)
            {

                if (ds.Parameter.Value.Name == pcmn.VariableMapping.GetVariableName(definition.VariableType))
                {

                    var ps = ds.ProductDefinitionSection.ProductDefinition as ProductDefinition0000;
                    if ((int)ps.FirstFixedSurfaceType == (int)definition.HeightType)
                    {
                        if (ps.FirstFixedSurfaceValue == definition.HeightValue)
                        {

                            List<double> result = new();
                            var valueEnum = rdr.ReadDataSetValues(ds);
                            foreach (var val in valueEnum)
                            {
                                if (val.Key.Latitude >= pcmn.dmin[1] && val.Key.Latitude <= pcmn.dmax[1] &&
                                    val.Key.Longitude >= pcmn.dmin[0] && val.Key.Longitude <= pcmn.dmax[0])
                                {
                                    result.Add((double)(val.Value??pcmn.undef));
                                }
                            }

                            return result.ToArray();
                        }
                    }

                }
                
            }
        }

        return Array.Empty<double>();
    }
}