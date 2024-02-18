using GradsSharp.Data;

namespace GradsSharp.DataReader.GFS;

public class GfsVariables : IVariableMapping
{
    private Dictionary<string, GribDataSetInfo> _mapping;

    public GfsVariables()
    {

        _mapping = new Dictionary<string, GribDataSetInfo>();

        _mapping["Temperature"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 0, ParameterName = "Temperature", ParameterUnit = "K" };
        _mapping["SurfaceRoughness"] = new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 1, ParameterName = "Surface roughness", ParameterUnit = "m" };
        _mapping["SoilTemperature"] = new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 2, ParameterName = "Soil temperature", ParameterUnit = "" };
        _mapping["Dewpoint"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 6, ParameterName = "Dew point temperature", ParameterUnit = "K" };
        _mapping["Pressure"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 0, ParameterName = "Pressure", ParameterUnit = "Pa" };
        _mapping["PressureMSL"] = new GribDataSetInfo { Discipline = 0,  ParameterCategory = 3, ParameterNumber = 1, ParameterName = "Pressure reduced to MSL", ParameterUnit = "Pa" };
        _mapping["CloudMixingRatio"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 22, ParameterName = "Cloud mixing ratio", ParameterUnit = "kg kg-1" };
        _mapping["IceWaterMixingRatio"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 23, ParameterName = "Ice water mixing ratio", ParameterUnit = "kg kg-1" };
        _mapping["RainMixingRatio"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 24, ParameterName = "Rain mixing ratio", ParameterUnit = "kg kg-1" };
        _mapping["SnowMixingRatio"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 25, ParameterName = "Snow mixing ratio", ParameterUnit = "kg kg-1" };
        _mapping["Graupel"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 32, ParameterName = "Graupel (snow pellets)", ParameterUnit = "kg kg-1" };
        _mapping["Visibility"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 19, ParameterNumber = 0, ParameterName = "Visibility", ParameterUnit = "m" };
        _mapping["UWind"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 2, ParameterName = "u-component of wind", ParameterUnit = "m s-1" };
        _mapping["VWind"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 3, ParameterName = "v-component of wind", ParameterUnit = "m s-1" };
        _mapping["WindGust"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 22, ParameterName = "Wind speed (gust)", ParameterUnit = "m s-1" };
        _mapping["GeopotentialHeight"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 5, ParameterName = "Geopotential height", ParameterUnit = "gpm" };
        _mapping["RelativeHumidity"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 1, ParameterName = "Relative humidity", ParameterUnit = "%" };
        _mapping["SpecificHumidity"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 0, ParameterName = "Specific humidity", ParameterUnit = "kg kg-1" };
        _mapping["VerticalVorticityPressure"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 8, ParameterName = "Vertical velocity (pressure)", ParameterUnit = "Pa s-1" };
        _mapping["VerticalVorticityGeometric"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 9, ParameterName = "Vertical velocity (geometric)", ParameterUnit = "m s-1" };
        _mapping["AbsoluteVorticity"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 10, ParameterName = "Absolute vorticity", ParameterUnit = "s-1" };
        _mapping["TotalCloudCover"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 6, ParameterNumber = 1, ParameterName = "Total cloud cover", ParameterUnit = "%" };
        _mapping["CAPE"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 7, ParameterNumber = 6, ParameterName = "Convective available potential energy", ParameterUnit = "J kg-1" };
        _mapping["CIN"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 7, ParameterNumber = 7, ParameterName = "Convective inhibition", ParameterUnit = "J kg-1" };
        _mapping["SRH"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 7, ParameterNumber = 8, ParameterName = "Storm relative helicity", ParameterUnit = "J kg-1" };
        _mapping["WaterEquivalentOfSnowDepth"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 13, ParameterName = "Water equivalent of accumulated snow depth", ParameterUnit = "kg m-2" };
        _mapping["SnowDepth"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 11, ParameterName = "Snow depth", ParameterUnit = "m" };
        _mapping["IceThickness"] = new GribDataSetInfo { Discipline = 10, ParameterCategory = 2, ParameterNumber = 1, ParameterName = "Ice thickness", ParameterUnit = "m" };
        _mapping["ApparentTemperature"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 21, ParameterName = "Apparent Temperature ", ParameterUnit = "K" };
        _mapping["IceGrowthRate"] = new GribDataSetInfo { Discipline = 10, ParameterCategory = 2, ParameterNumber = 6, ParameterName = "Ice growth rate", ParameterUnit = "m s-1" };
        _mapping["PercentFrozenPrecip"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 39, ParameterName = "Percent frozen precipitation", ParameterUnit = "%" };
        _mapping["PrecipRate"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 7, ParameterName = "Precipitation rate", ParameterUnit = "kg m-2 s-1" };
        _mapping["Vegetation"] = new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 4, ParameterName = "Vegetation", ParameterUnit = "%" };
        _mapping["SoilType"] = new GribDataSetInfo { Discipline = 2, ParameterCategory = 3, ParameterNumber = 0, ParameterName = "Soil type", ParameterUnit = "Code table (4.213)" };
        _mapping["PrecipitableWater"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 3, ParameterName = "Precipitable water", ParameterUnit = "kg m-2" };
        _mapping["CloudWater"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 6, ParameterNumber = 6, ParameterName = "Cloud water", ParameterUnit = "kg m-2" };
        _mapping["TotalOzone"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 14, ParameterNumber = 0, ParameterName = "Total ozone", ParameterUnit = "Dobson" };
        _mapping["LowCloudCover"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 6, ParameterNumber = 3, ParameterName = "Low cloud cover", ParameterUnit = "%" };
        _mapping["MediumCloudCover"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 6, ParameterNumber = 4, ParameterName = "Medium cloud cover", ParameterUnit = "%" };
        _mapping["HighCloudCover"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 6, ParameterNumber = 5, ParameterName = "High cloud cover", ParameterUnit = "%" };
        _mapping["ICAOReferenceHeight"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 3, ParameterName = "ICAO Standard Atmosphere Reference Height", ParameterUnit = "m" };
        _mapping["Potentialemperature"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 2, ParameterName = "Potential temperature", ParameterUnit = "K" };
        _mapping["LandCover"] = new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 0, ParameterName = "Land cover (1=land, 2=sea)", ParameterUnit = "Proportion" };
        _mapping["IceCover"] = new GribDataSetInfo { Discipline = 10, ParameterCategory = 2, ParameterNumber = 0, ParameterName = "Ice cover", ParameterUnit = "Proportion" };
        _mapping["MaximumTemperature"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 4, ParameterName = "Maximum temperature", ParameterUnit = "K" };
        _mapping["MinimumTemperature"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 5, ParameterName = "Minimum temperature", ParameterUnit = "K" };
        _mapping["ConvectivePrecipRate"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 37, ParameterName = "Convective precipitation", ParameterUnit = "kg m-2" };
        _mapping["TotalPrecipitation"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 8, ParameterName = "Total precipitation", ParameterUnit = "kg m-2" };
        _mapping["ConvectivePrecipitation"] = new GribDataSetInfo { ParameterCategory = 1, ParameterNumber = 10, ParameterName = "Convective precipitation", ParameterUnit = "kg m-2" };
        _mapping["WaterRunoff"] = new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 5, ParameterName = "Water runoff", ParameterUnit = "kg m-2" };
        _mapping["LatentNetHeatFlux"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 10, ParameterName = "Latent heat net flux", ParameterUnit = "W m-2" };
        _mapping["SensibleNetHeatFlux"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 0, ParameterNumber = 11, ParameterName = "Sensible heat net flux", ParameterUnit = "W m-2" };
        _mapping["MomentFluxU"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 17, ParameterName = "Momentum flux, u component", ParameterUnit = "s-1" };
        _mapping["MomentFluxV"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 18, ParameterName = "Momentum flux, v component", ParameterUnit = "s-1" };
        _mapping["Albedo"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 19, ParameterNumber = 1, ParameterName = "Albedo", ParameterUnit = "%" };
        _mapping["Reflectivity"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 16, ParameterNumber = 195, ParameterName = "Reflectivity", ParameterUnit = "dB" };
        _mapping["CompositeReflectivity"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 16, ParameterNumber = 196, ParameterName = "Composite Reflectivity", ParameterUnit = "dB" };
        _mapping["VentilationRate"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 224, ParameterName = "Ventilation Rate", ParameterUnit = "m2 s-1" };
        _mapping["CategoricalRain"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 192, ParameterName = "Categorical Rain", ParameterUnit = "" };
        _mapping["CategoricalFreezingRain"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 193, ParameterName = "Categorical Freezing Rain", ParameterUnit = "" };
        _mapping["CategoricalIcePellets"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 194, ParameterName = "Categorical Ice Pellets", ParameterUnit = "" };
        _mapping["CategoricalSnow"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 195, ParameterName = "Categorical Freezing Snow", ParameterUnit = "" };
        _mapping["PotentialEvaporationRate"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 1, ParameterNumber = 200, ParameterName = "Potential Evaporation Rate", ParameterUnit = "W m-2" };
        _mapping["FrictionalVelocity"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 197, ParameterName = "Frictional Velocity", ParameterUnit = "m s-1" };
        _mapping["PressureMSLPEta"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 192, ParameterName = "MSLP (Eta model reduction)", ParameterUnit = "Pa" };
        _mapping["LiftedIndexSurface"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 7, ParameterNumber = 192, ParameterName = "Surface Lifted Index", ParameterUnit = "K" };
        _mapping["LiftedIndex4Layer"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 7, ParameterNumber = 193, ParameterName = "Best (4 layer) Lifted Index", ParameterUnit = "K" };
        _mapping["UStormMotion"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 194, ParameterName = "U Component Storm Motion", ParameterUnit = "m s-1" };
        _mapping["VStormMotion"] = new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 195, ParameterName = "V Component Storm Motion", ParameterUnit = "m s-1" };
        
        /*
         
         new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 194, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 195, ParameterName = "Unknown", ParameterUnit = "" };

new GribDataSetInfo { Discipline = 0, ParameterCategory = 14, ParameterNumber = 192, ParameterName = "Unknown", ParameterUnit = "" };

new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 192, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 193, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 196, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 2, ParameterCategory = 0, ParameterNumber = 201, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 2, ParameterCategory = 3, ParameterNumber = 192, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 2, ParameterCategory = 3, ParameterNumber = 203, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 2, ParameterCategory = 4, ParameterNumber = 2, ParameterName = "Unknown", ParameterUnit = "" };


new GribDataSetInfo { Discipline = 0, ParameterCategory = 6, ParameterNumber = 201, ParameterName = "Unknown", ParameterUnit = "" };

new GribDataSetInfo { Discipline = 0, ParameterCategory = 6, ParameterNumber = 193, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 4, ParameterNumber = 192, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 5, ParameterNumber = 192, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 4, ParameterNumber = 193, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 5, ParameterNumber = 193, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 194, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 195, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 2, ParameterNumber = 192, ParameterName = "Unknown", ParameterUnit = "" };

new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 196, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 0, ParameterCategory = 3, ParameterNumber = 200, ParameterName = "Unknown", ParameterUnit = "" };
new GribDataSetInfo { Discipline = 10, ParameterCategory = 2, ParameterNumber = 8, ParameterName = "Unknown", ParameterUnit = "" };
         
         */
    }

    public object GetVariableInfo(string variable)
    {
        GribDataSetInfo? result = null ;
        if (_mapping.TryGetValue(variable, out result))
        {
            return result;
        }

        // should not happen
        throw new Exception($"Variable {variable} not found");
    }

    public string GetGrib2VarType(int disciple, int paramCategory, int paramNumber)
    {
        foreach (var dsi in _mapping)
        {
            if (dsi.Value.Discipline == disciple && dsi.Value.ParameterCategory == paramCategory &&
                dsi.Value.ParameterNumber == paramNumber)
                return dsi.Key;
        }

        return "";
        //throw new Exception("Variable not found");
    }
}