namespace GradsSharp.Models;

public class InputFile
{
    public string FileName { get; set; }
    public FileType FileType { get; set; } = FileType.Gridded;

    public PreprojectedType PreprojectedType { get; set; } = PreprojectedType.None;
    
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
    
    public DateTime ReferenceTime { get; set; }

    public List<InputVariable> Variables { get; set; } = new();
    
    public int TimeStepIntervalMinutes { get; set; }

}