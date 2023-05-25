namespace GradsSharp.Models;

public class VariableDefinition
{
    public string VariableName { get; set; }
    public DataVariable VariableType { get; set; }
    public FixedSurfaceType HeightType { get; set; }
    public double HeightValue { get; set; }

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
            DataVariable.UWind => "u",
            DataVariable.VWind => "v",
            DataVariable.GeopotentialHeight => "hgt",
            DataVariable.Pressure => "pres",
            DataVariable.CloudMixingRatio => "cloudmix",
            DataVariable.IceWaterMixingRatio => "icewmix",
            DataVariable.RainMixingRatio => "rainmix",
            DataVariable.SnowMixingRatio => "snowmix",
            DataVariable.Visibility => "vis",
            DataVariable.WindGust => "gust",
            DataVariable.VerticalVorticity => "vertvort",
            DataVariable.AbsoluteVorticity => "absvort",
            DataVariable.VerticalVelocity => "vertvel",
            DataVariable.TotalCloudCover => "tcc",
            _ => throw new ArgumentOutOfRangeException()
        };

        if (HeightType == FixedSurfaceType.SpecifiedHeightLevelAboveGround)
        {
            result += HeightValue + "m";
        }

        return result;
    }
}