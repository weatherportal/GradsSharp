using GradsSharp.Models;

namespace GradsSharp.Data;

public class GfsVariables : IVariableMapping
{
    private Dictionary<DataVariable, string> _mapping;

    public GfsVariables()
    {

        _mapping = new Dictionary<DataVariable, string>();

        _mapping[DataVariable.Temperature] = "Temperature";
        _mapping[DataVariable.Pressure] = "Pressure";
        _mapping[DataVariable.PressureMSL] = "Pressure reduced to MSL";
        _mapping[DataVariable.CloudMixingRatio] = "Cloud mixing ratio";
        _mapping[DataVariable.IceWaterMixingRatio] = "Ice water mixing ratio";
        _mapping[DataVariable.RainMixingRatio] = "Rain mixing ratio";
        _mapping[DataVariable.SnowMixingRatio] = "Snow mixing ratio";
        _mapping[DataVariable.Graupel] = "Graupel (snow pellets)";
        _mapping[DataVariable.Visibility] = "Visibility";
        _mapping[DataVariable.UWind] = "u-component of wind";
        _mapping[DataVariable.VWind] = "v-component of wind";
        _mapping[DataVariable.WindGust] = "Wind speed (gust)";
        _mapping[DataVariable.GeopotentialHeight] = "Geopotential height";
        _mapping[DataVariable.RelativeHumidity] = "Relative humidity";
        _mapping[DataVariable.SpecificHumidity] = "Specific humidity";
        _mapping[DataVariable.VerticalVorticityPressure] = "Vertical velocity (pressure)";
        _mapping[DataVariable.VerticalVorticityGeometric] = "Vertical velocity (geometric)";
        _mapping[DataVariable.AbsoluteVorticity] = "Absolute vorticity";
        _mapping[DataVariable.TotalCloudCover] = "Total cloud cover";
        _mapping[DataVariable.Dewpoint] = "Dew point temperature";
        _mapping[DataVariable.CAPE] = "Convective available potential energy";
        _mapping[DataVariable.CIN] = "Convective inhibition";
        _mapping[DataVariable.SRH] = "Storm relative helicity";
        _mapping[DataVariable.SoilTemperature] = "Soil temperature";
        _mapping[DataVariable.WaterEquivalentOfSnowDepth] = "Water equivalent of accumulated snow depth";
        _mapping[DataVariable.SnowDepth] = "Snow depth";
        _mapping[DataVariable.IceThickness] = "Ice thickness";
        _mapping[DataVariable.ApparentTemperature] = "Apparent Temperature ";
        _mapping[DataVariable.IceGrowthRate] = "Ice growth rate";
        _mapping[DataVariable.PercentFrozenPrecip] = "Percent frozen precipitation";
        _mapping[DataVariable.PrecipRate] = "Precipitation rate";
        _mapping[DataVariable.SurfaceRoughness] = "Surface roughness";
        _mapping[DataVariable.Vegetation] = "Vegetation";
        _mapping[DataVariable.SoilType] = "Soil type";
        _mapping[DataVariable.PrecipitableWater] = "Precipitable water";
        _mapping[DataVariable.CloudWater] = "Cloud water";
        _mapping[DataVariable.TotalOzone] = "Total ozone";
        _mapping[DataVariable.LowCloudCover] = "Low cloud cover";
        _mapping[DataVariable.MediumCloudCover] = "Medium cloud cover";
        _mapping[DataVariable.HighCloudCover] = "High cloud cover";
        _mapping[DataVariable.ICAOReferenceHeight] = "ICAO Standard Atmosphere Reference Height";
        _mapping[DataVariable.Potentialemperature] = "Potential temperature";
        _mapping[DataVariable.LandCover] = "Land cover (1=land, 2=sea)";
        _mapping[DataVariable.IceCover] = "Ice cover";
        _mapping[DataVariable.MaximumTemperature] = "Maximum temperature";
        _mapping[DataVariable.MinimumTemperature] = "Minimum temperature";
        _mapping[DataVariable.ConvectivePrecipRate] = "Convective Precipitation Rate";
        _mapping[DataVariable.TotalPrecipitation] = "Total precipitation";
        _mapping[DataVariable.ConvectivePrecipitation] = "Convective precipitation";
        _mapping[DataVariable.WaterRunoff] = "Water runoff";
        _mapping[DataVariable.LatentNetHeatFlux] = "Latent heat net flux";
        _mapping[DataVariable.SensibleNetHeatFlux] = "Sensible heat net flux";
        _mapping[DataVariable.MomentFluxU] = "Momentum flux, u component";
        _mapping[DataVariable.MomentFluxV] = "Momentum flux, v component";
        _mapping[DataVariable.Albedo] = "Albedo";

    }

    public string GetVariableName(DataVariable variable)
    {
        string result = "";
        if (_mapping.TryGetValue(variable, out result))
        {
            return result;
        }

        // should not happen
        throw new Exception($"Variable {variable} not found");
    }
    
    
    public DataVariable GetVarType(string name)
    {




        foreach (var map in _mapping)
        {
            if (map.Value == name) return map.Key;
        }

        throw new Exception($"Variable name {name} not mapped");
    }
}