using GradsSharp.Data.GridFunctions;
using GradsSharp.Models;

public class GridMath
{
    /// <summary>
    /// Power function
    /// </summary>
    /// <param name="grid">Grid to apply power function on</param>
    /// <param name="value">Value for the power function</param>
    /// <returns></returns>
    public static IGradsGrid Pow(IGradsGrid grid, double value)
    {
        return grid.Pow(value);
    }
    
    /// <summary>
    /// Calculate the square root of all elements in a grid
    /// </summary>
    /// <param name="grid">Grid to operate on</param>
    /// <returns></returns>
    public static IGradsGrid Sqrt(IGradsGrid grid)
    {
        return grid.Sqrt();
    }
    
    /// <summary>
    /// Calculate exp of all elements in a grid
    /// </summary>
    /// <param name="grid">Grid to operate on</param>
    /// <returns>Resulting grid</returns>
    public static IGradsGrid Exp(IGradsGrid grid)
    {
        return grid.Exp();
    }
    
    /// <summary>
    /// Calculate the logarithm of all elements in a grid
    /// </summary>
    /// <param name="grid">Grid to operate on</param>
    /// <returns></returns>
    public static IGradsGrid Log(IGradsGrid grid)
    {
        return grid.Log();
    }
    
    /// <summary>
    /// Calculate the average of a set of grids
    /// </summary>
    /// <param name="grid1">First grid</param>
    /// <param name="otherGrids">Other grids</param>
    /// <returns>Average of all grids</returns>
    public static IGradsGrid Avg(IGradsGrid grid1, params IGradsGrid[] otherGrids)
    {
        return grid1.Average(otherGrids);
    }
}