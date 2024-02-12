using System.ComponentModel;

namespace GradsSharp.Models;


public enum PreprojectedType
{
    None = 0,
    NPS = 1,
    SPS = 2,
    LCC = 3,
    LCCR = 4,
    ETAU = 5,
    PSE = 6,
    OPS = 7,
    BILIN = 8,
    FILE = 9,
    GENERAL = 10,
    ROTLL = 11,
    ROTLLR = 12
}

public enum InterpolationMode
{
    Undefined = -1,
    PiecewiseLinear = 0,
    Spline = 1,
    Polynomnial = 2
}

public enum ConstMode
{
    Undefined,
    Missing,
    All
}

public enum InputFileDimensionType
{
    Linear,
    Levels
}

/* Type of file:  1 = grid
                  2 = simple station
                  3 = mapped station
                  4 = defined grid       */
public enum FileType
{
    Gridded = 1,
    SimpleStation =2,
    MappedStation = 3,
    DefinedGrid = 4
}

public enum MultiLevelFunction
{
    Average = 1,
    Mean = 2,
    Sum = 3,
    SumNonWeighted = 4,
    Minimum = 5,
    Maximum = 6
    
}

public enum DimensionType{
    Fixed,
    Varying
}

public enum ColorBarDirection
{
    Auto,
    Vertical,
    Horizontal
}

public enum IntervalType
{
    Interval,
    Fac
}

public enum DataVariable
{
    NotDefined,
    Temperature,
    MaximumTemperature,
    MinimumTemperature,
    SoilTemperature,
    Potentialemperature,
    ApparentTemperature,
    RelativeHumidity,
    Dewpoint,
    SpecificHumidity,
    ConvectivePrecipRate,
    ConvectivePrecipitation,
    TotalPrecipitation,
    WaterRunoff,
    LatentNetHeatFlux,
    SensibleNetHeatFlux,
    MomentFluxU,
    MomentFluxV,
    Albedo,
    CAPE,
    CIN,
    SRH,
    UWind,
    VWind,
    Geopotential,
    GeopotentialHeight,
    Pressure,
    PressureMSL,
    CloudMixingRatio,
    IceWaterMixingRatio,
    RainMixingRatio,
    SnowMixingRatio,
    Graupel,
    Visibility,
    WindGust,
    VerticalVorticityGeometric,
    VerticalVorticityPressure,
    AbsoluteVorticity,
    VerticalVelocity,
    TotalCloudCover,
    LowCloudCover,
    MediumCloudCover,
    HighCloudCover,
    ICAOReferenceHeight,
    WaterEquivalentOfSnowDepth,
    SnowDepth,
    IceThickness,
    IceGrowthRate,
    PercentFrozenPrecip,
    PrecipRate,
    SurfaceRoughness,
    Vegetation,
    SoilType,
    PrecipitableWater,
    CloudWater,
    TotalOzone,
    LandCover,
    IceCover,
    Reflectivity,
    CompositeReflectivity,
    VentilationRate,
    CategoricalRain,
    CategoricalFreezingRain,
    CategoricalIcePellets,
    CategoricalSnow,
    PotentialEvaporationRate,
    FrictionalVelocity,
    PressureMSLPEta,
    LiftedIndexSurface,
    LiftedIndex4Layer,
    UStormMotion,
    VStormMotion
}


/// <summary>
/// Map resolution options
/// </summary>
public enum MapResolution
{
    /// <summary>
    /// Defaults to low resolution
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// Low resolution map (default)
    /// </summary>
    LowResolution = 1,
    /// <summary>
    /// Medium resolution: includes state and country lines
    /// </summary>
    MediumResolution = 2,
    /// <summary>
    /// High resolution: includes state and country lines
    /// </summary>
    HighResolution = 3
}

public enum Orientation
{
    Landscape = 1,
    Portrait = 2
}

public enum BackgroundColor
{
    Default = -999,
    Black = 0,
    White = 1
}

public enum GridLine
{
    Auto,
    Off,
    Color
}

public enum KmlOutputFlag
{
    Image = 1,
    Contour = 2,
    Polygon = 3
}

public enum LabelOption {

    On,
    Off,
    Masked,
    Forced,
    Auto
    
}

public enum GridOption
{
    On = 1,
    Off = 0,
    Horizontal = 2,
    Vertical = 3
}

public enum LineStyle
{
    NoContours = 0,
    Solid = 1,
    LongDash = 2,
    ShortDash = 3,
    LongDashShortDash = 4,
    Dotted = 5,
    DotDash = 6,
    DotDotDash = 7
}

public enum StringJustification
{
    Undefined = 0,
    TopLeft = 6,
    TopCenter = 7,
    TopRight = 8,
    Left = 3,
    Center = 4, 
    Right = 5,
    BottomLeft = 0,
    BottomCenter = 1,
    BottomRight = 2
}

public enum SmoothOption
{
    On = 1,
    Off = 0,
    Linear = 2
}

public enum AxisLabelOption
{
    On = 1,
    Off = 0,
    Format = 2
}


public enum Axis
{
    X,
    Y
}
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
    [Description("Depth below sea  level")] DepthBelowSeaLevel = 160, // 0x000000A0
    [Description("Missing ")] Missing = 255, // 0x000000FF
}