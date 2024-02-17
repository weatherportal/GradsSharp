using GradsSharp.Enums;

namespace GradsSharp.Models;

/// <summary>
/// This class is used to supply information about the opened file to GradsEngine.
/// </summary>
public class InputFile
{
    
    /// <summary>
    /// Path to the file
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// Type of the file
    /// </summary>
    public FileType FileType { get; set; } = FileType.Gridded;

    /// <summary>
    /// Preprojected type
    /// </summary>
    public PreprojectedType PreprojectedType { get; set; } = PreprojectedType.None;
    
    /// <summary>
    /// Horizontal size of the preprojected grid
    /// </summary>
    public int PreprojectedISize { get; set; }
    
    /// <summary>
    /// Vertical size of the preprojected grid
    /// </summary>
    public int PreprojectedJSize { get; set; }
    
    /// <summary>
    /// Number of points in X-Direction
    /// </summary>
    public int Dx { get; set; }

    /// <summary>
    /// Number of points in Y-Direction
    /// </summary>
    public int Dy { get; set; }
    
    /// <summary>
    /// Number of points in Z-direction
    /// </summary>
    public int Dz { get; set; }
    
    /// <summary>
    /// Number of points in T-Direction
    /// </summary>
    public int Dt { get; set; }

    /// <summary>
    /// Number of points in E-direction
    /// </summary>
    public int De { get; set; }

    public InputFileDimensionType XDimensionType { get; set; } = InputFileDimensionType.Linear;
    public InputFileDimensionType YDimensionType { get; set; } = InputFileDimensionType.Linear;
    public InputFileDimensionType ZDimensionType { get; set; } = InputFileDimensionType.Levels;
    public InputFileDimensionType TDimensionType { get; set; } = InputFileDimensionType.Linear;
    public InputFileDimensionType EDimensionType { get; set; } = InputFileDimensionType.Linear;
    
    public double? XMin { get; set; }
    public double? YMin { get; set; }
    public double? ZMin { get; set; }
    public double? TMin { get; set; }
    public double? EMin { get; set; }
    
    public double? XIncrement { get; set; }
    public double? YIncrement { get; set; }
    public double? ZIncrement { get; set; }
    public double? TIncrement { get; set; } = 1;
    public double? EIncrement { get; set; } = 1;
    
    public double[]? XLevels { get; set; } 
    public double[]? YLevels { get; set; } 
    public double[]? ZLevels { get; set; } 
    public double[]? TLevels { get; set; } 
    public double[]? ELevels { get; set; } 
    
    public double[]? PreProjectionValues { get; set; }
    
    /// <summary>
    /// Time step of the file
    /// </summary>
    public DateTime ReferenceTime { get; set; }

    /// <summary>
    /// List of variables in the file
    /// </summary>
    public List<InputVariable> Variables { get; set; } = new();
    
    public int TimeStepIntervalMinutes { get; set; }

}