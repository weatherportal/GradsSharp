using GradsSharp.Models;
using GradsSharp.Models.Internal;
using NGrib;
using NGrib.Grib2.Templates.ProductDefinitions;

namespace GradsSharp.Data.Grib.GFS;

internal class GfsGribDataReader : IGriddedDataReader
{
    public double[] ReadData(GradsCommon pcmn, GradsFile file, VariableDefinition definition)
    {
        GribDataSetInfo info = (GribDataSetInfo)pcmn.VariableMapping.GetVariableInfo(definition.VariableType);
       
        
        Grib2Reader rdr = new Grib2Reader(file.name);
        foreach (var ds in rdr.ReadAllDataSets())
        {
            if (info.Discipline == (int)ds.Message.IndicatorSection.Discipline &&
                info.ParameterCategory == ds.ProductDefinitionSection.ProductDefinition.ParameterCategory &&
                info.ParameterNumber == ds.ProductDefinitionSection.ProductDefinition.ParameterNumber)
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
                                result.Add((double)(val.Value ?? pcmn.undef));
                            }
                        }

                        return result.ToArray();
                    }
                }

            }


        }

        return Array.Empty<double>();
    }
}