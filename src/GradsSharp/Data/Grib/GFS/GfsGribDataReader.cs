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

        double[] dmin = pcmn.dmin;
        double[] dmax = pcmn.dmax;
        
        dmin[0] = Math.Floor(dmin[0] + 0.0001);
        dmax[0] = Math.Ceiling(dmax[0] - 0.0001);
        dmin[1] = Math.Floor(dmin[1] + 0.0001);
        dmax[1] = Math.Ceiling(dmax[1] - 0.0001);
        
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
                            if (val.Key.Latitude >= dmin[1] && val.Key.Latitude <= dmax[1] &&
                                val.Key.Longitude >= dmin[0] && val.Key.Longitude <= dmax[0])
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