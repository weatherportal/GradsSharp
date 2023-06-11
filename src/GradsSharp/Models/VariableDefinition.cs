namespace GradsSharp.Models;

public class VariableDefinition
{
    public string VariableName { get; set; }
    public DataVariable VariableType { get; set; } = DataVariable.NotDefined;
    public FixedSurfaceType HeightType { get; set; } = FixedSurfaceType.Missing;
    public double HeightValue { get; set; } = Double.NaN;

    public int File { get; set; } = 1;
    
    public string GetVarName()
    {
        var result = VariableType switch
        {
            DataVariable.Dewpoint => "dpt",
            DataVariable.Graupel => "graup",
            DataVariable.NotDefined => "undef",
            DataVariable.Temperature => "tmp",
            DataVariable.RelativeHumidity => "rh",
            DataVariable.SpecificHumidity => "sh",
            DataVariable.CAPE => "cape",
            DataVariable.CIN => "cin",
            DataVariable.SRH => "srh",
            DataVariable.UWind => "uwind",
            DataVariable.VWind => "vwind",
            DataVariable.GeopotentialHeight => "hgt",
            DataVariable.Pressure => "pres",
            DataVariable.PressureMSL => "presmsl",
            DataVariable.CloudMixingRatio => "cloudmix",
            DataVariable.IceWaterMixingRatio => "icewmix",
            DataVariable.RainMixingRatio => "rainmix",
            DataVariable.SnowMixingRatio => "snowmix",
            DataVariable.Visibility => "vis",
            DataVariable.WindGust => "wgust",
            DataVariable.VerticalVorticityGeometric => "vertvortg",
            DataVariable.VerticalVorticityPressure => "vertvortp",
            DataVariable.AbsoluteVorticity => "absvort",
            DataVariable.VerticalVelocity => "vertvel",
            DataVariable.TotalCloudCover => "tcc",
            DataVariable.SoilTemperature => "soiltmp",
            DataVariable.Potentialemperature => "pottmp",
            DataVariable.WaterEquivalentOfSnowDepth => "wesd",
            DataVariable.SnowDepth => "snowdep",
            DataVariable.IceThickness => "icethick",
            DataVariable.ApparentTemperature => "apptmp",
            DataVariable.IceGrowthRate=> "icegrow",
            DataVariable.PercentFrozenPrecip => "percfrzprecip",
            DataVariable.PrecipRate => "preciprate",
            DataVariable.SurfaceRoughness =>"surfrough",
            DataVariable.Vegetation => "veg",
            DataVariable.SoilType => "soiltyp",
            DataVariable.PrecipitableWater => "precipwat",
            DataVariable.CloudWater => "cloudwat",
            DataVariable.TotalOzone => "totaloz",
            DataVariable.LowCloudCover => "lcc",
            DataVariable.MediumCloudCover => "mcc",
            DataVariable.HighCloudCover => "hcc",
            DataVariable.ICAOReferenceHeight => "icaorefh",
            DataVariable.LandCover => "landcov",
            DataVariable.IceCover => "icecov",
            DataVariable.MaximumTemperature => "tmpmax",
            DataVariable.MinimumTemperature => "tmpmin",
            DataVariable.ConvectivePrecipRate => "cprecrate",
            DataVariable.TotalPrecipitation => "prectot",
            DataVariable.ConvectivePrecipitation => "precconv",
            DataVariable.WaterRunoff => "waterrun",
            DataVariable.LatentNetHeatFlux => "lnhf",
            DataVariable.SensibleNetHeatFlux => "snhf",
            DataVariable.MomentFluxU => "mfu",
            DataVariable.MomentFluxV => "mfv",
            DataVariable.Albedo => "alb",
            DataVariable.CategoricalSnow => "catsn",
            DataVariable.LiftedIndexSurface => "lftxsft",
            DataVariable.LiftedIndex4Layer => "lftidx4l",
            DataVariable.UStormMotion => "urmstrm",
            DataVariable.VStormMotion => "vrmstrm",
            _ => throw new ArgumentOutOfRangeException($"{VariableType}")
        };

        if (HeightType == FixedSurfaceType.SpecifiedHeightLevelAboveGround)
        {
            result += HeightValue + "m";
        }
        
        else if (HeightType == FixedSurfaceType.IsobaricSurface)
        {
            result += "prs";
        }
        else if (HeightType == FixedSurfaceType.PotentialVorticitySurface)
        {
            result += "pvu";
        }
        return result;
    }

    public override string ToString()
    {
        return $"{VariableType}-{HeightType}-{HeightValue}";
    }
}