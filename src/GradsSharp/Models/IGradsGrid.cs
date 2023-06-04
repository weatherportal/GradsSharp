namespace GradsSharp.Models;

/// <summary>
/// Contains data and meta information about gridded data
/// </summary>
public interface IGradsGrid : ICloneable
{
    double[] GridData { get; set; } /* Grid data                 */
    double Undef { get; set; } /* Undefined value for this grid.       */
    byte[] UndefinedMask { get; set; } /* Mask for undefined values in the grid */
    int ISize { get; } /* isiz = number of elements per row. */
    int JSize { get; } /* jsiz = number of rows.               */
    int IDimension { get; set; }
    int JDimension { get; set; }
    int[] DimensionMinimum { get; }
    int[] DimensionMaximum { get; }
    
    public double[] WorldDimensionMinimum { get; }
    public double[] WorldDimensionMaximum { get; }

    public IGradsGrid CloneGrid();
}