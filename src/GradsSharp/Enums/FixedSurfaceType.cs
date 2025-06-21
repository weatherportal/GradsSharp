using System.ComponentModel;

namespace GradsSharp.Enums;

/// <summary>
/// Surface type for data sources
/// </summary>
public enum FixedSurfaceType
{
    [Description("Ground or water surface")] GroundOrWaterSurface = 1,
    [Description("Cloud base level")] CloudBaseLevel = 2,
    [Description("Level of cloud tops")] LevelOfCloudTops = 3,
    [Description("Level of 0°C isotherm")] LevelOf0CIsotherm = 4,
    [Description("Level of adiabatic condensation lifted from the surface")] LevelOfAdiabaticCondensationLiftedFromTheSurface = 5,
    [Description("Maximum wind level")] MaximumWindLevel = 6,
    [Description("Tropopause")] Tropopause = 7,
    [Description("Nominal top of the atmosphere")] NominalTopOfTheAtmosphere = 8,
    [Description("Sea bottom")] SeaBottom = 9,
    [Description("Departure level of the most unstable parcel of air (MUDL)")] MostUnstableParcelLevel = 17, // 0x00000017
    [Description("Isothermal level")] IsothermalLevel = 20, // 0x00000014
    [Description("Isobaric surface")] IsobaricSurface = 100, // 0x00000064
    [Description("Mean sea level")] MeanSeaLevel = 101, // 0x00000065
    [Description("Specific altitude above mean sea level")] SpecificAltitudeAboveMeanSeaLevel = 102, // 0x00000066
    [Description("Specified height level above ground")] SpecifiedHeightLevelAboveGround = 103, // 0x00000067
    [Description("Sigma level")] SigmaLevel = 104, // 0x00000068
    [Description("Hybrid level")] HybridLevel = 105, // 0x00000069
    [Description("Depth below land surface")] DepthBelowLandSurface = 106, // 0x0000006A
    [Description("Isentropic (theta) level")] IsentropicLevel = 107, // 0x0000006B
    [Description("Level at specified pressure difference from ground to level")] LevelAtSpecifiedPressureDifferenceFromGroundToLevel = 108, // 0x0000006C
    [Description("Potential vorticity surface")] PotentialVorticitySurface = 109, // 0x0000006D
    [Description("Eta* level")] EtaLevel = 111, // 0x0000006F
    [Description("Mixed layer depth")] MixedLayerDepth = 117, // 0x00000075
    [Description("Soil Level numeric")] SoilLevelNumeric = 151, 
    [Description("Depth below sea  level")] DepthBelowSeaLevel = 160, // 0x000000A0
    [Description("Missing ")] Missing = 255, // 0x000000FF
}