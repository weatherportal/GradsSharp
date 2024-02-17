using System.Runtime.CompilerServices;
using GradsSharp.Data.GridFunctions;

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
    
    /// <summary>
    /// Operator to add a value to all elements in the grid
    /// </summary>
    /// <param name="grid">Grid to add a value to</param>
    /// <param name="value">Value to add</param>
    /// <returns></returns>
    public static IGradsGrid operator +(IGradsGrid grid, double value)
    {
        return grid.Add(value);
    }

    /// <summary>
    /// Operator to sum all elements in two grids
    /// </summary>
    /// <param name="grid">Grid to add a value to</param>
    /// <param name="otherGrid">Other grid to add</param>
    /// <returns></returns>
    public static IGradsGrid operator +(IGradsGrid grid, IGradsGrid otherGrid)
    {
        return grid.Add(otherGrid);
    }

    /// <summary>
    /// Operator to subtract a value from all elements in the grid
    /// </summary>
    /// <param name="grid">Grid to add a value to</param>
    /// <param name="value">Value to add</param>
    /// <returns></returns>
    public static IGradsGrid operator -(IGradsGrid grid, double value)
    {
        return grid.Subtract(value);
    }

    /// <summary>
    /// Operator to subtract all elements in two grids
    /// </summary>
    /// <param name="grid">Grid to add a value to</param>
    /// <param name="otherGrid">Other grid to add</param>
    /// <returns></returns>
    public static IGradsGrid operator -(IGradsGrid grid, IGradsGrid otherGrid)
    {
        return grid.Subtract(otherGrid);
    }

    /// <summary>
    /// Operator to multiply all elements in the grid with a value
    /// </summary>
    /// <param name="grid">Grid to multiply</param>
    /// <param name="value">Value to multiply with</param>
    /// <returns></returns>
    public static IGradsGrid operator *(IGradsGrid grid, double value)
    {
        return grid.Multiply(value);
    }

    /// <summary>
    /// Operator to multiply all elements in two grids
    /// </summary>
    /// <param name="grid">Grid to multiply</param>
    /// <param name="otherGrid">Other grid to multiply</param>
    /// <returns></returns>
    public static IGradsGrid operator *(IGradsGrid grid, IGradsGrid otherGrid)
    {
        return grid.Multiply(otherGrid);
    }
    
    /// <summary>
    /// Operator to divide all elements in the grid with a value
    /// </summary>
    /// <param name="grid">Grid to multiply</param>
    /// <param name="value">Value to multiply with</param>
    /// <returns></returns>
    public static IGradsGrid operator /(IGradsGrid grid, double value)
    {
        return grid.Divide(value);
    }

    /// <summary>
    /// Operator to divide all elements in two grids
    /// </summary>
    /// <param name="grid">Grid to multiply</param>
    /// <param name="otherGrid">Other grid to multiply</param>
    /// <returns></returns>
    public static IGradsGrid operator /(IGradsGrid grid, IGradsGrid otherGrid)
    {
        return grid.Divide(otherGrid);
    }
    
    /// <summary>
    /// Operator to get the square root of all elements in the grid
    /// </summary>
    public static IGradsGrid operator ^(IGradsGrid grid, double value)
    {
        return grid.Pow(value);
    }
}