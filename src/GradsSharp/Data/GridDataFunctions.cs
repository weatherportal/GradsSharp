using GradsSharp.Models;

namespace GradsSharp.Data;

public static class GridDataFunctions
{

    public static IGradsGrid Average(this IGradsGrid grid1, params IGradsGrid[] otherGrids)
    {
        IGradsGrid result = grid1.CloneGrid();
        int nr = 1 + otherGrids.Length;

        for (int j = 0; j < result.GridData.Length; j++)
        {
            double sum = grid1.GridData[j];
            foreach (var g in otherGrids)
            {
                sum += g.GridData[j];
            }

            result.GridData[j] = sum / nr;
        }

        return result;
    }
    
    public static IGradsGrid Sqrt(this IGradsGrid grid)
    {
        return UpdateAllElementsInArray(grid, d => Math.Sqrt(d));
    }

    public static IGradsGrid Multiply(this IGradsGrid grid, double value)
    {
        return UpdateAllElementsInArray(grid, d => d * value);
    }
    
    public static IGradsGrid Subtract(this IGradsGrid grid, double value)
    {
        return UpdateAllElementsInArray(grid, d => d - value);
    }
    
    public static IGradsGrid Add(this IGradsGrid grid, double value)
    {
        return UpdateAllElementsInArray(grid, d => d + value);
    }
    
    public static IGradsGrid Divide(this IGradsGrid grid, double value)
    {
        return UpdateAllElementsInArray(grid, d => d / value);
    }
    
    public static IGradsGrid Multiply(this IGradsGrid grid1, IGradsGrid grid2)
    {
        return TwoGridOperation(grid1, grid2, (a, b) => a * b);
    }
    public static IGradsGrid Subtract(this IGradsGrid grid1, IGradsGrid grid2)
    {
        return TwoGridOperation(grid1, grid2, (a, b) => a - b);
    }

    public static IGradsGrid Add(this IGradsGrid grid1, IGradsGrid grid2)
    {
        return TwoGridOperation(grid1, grid2, (a, b) => a + b);
    }
    public static IGradsGrid Divide(this IGradsGrid grid1, IGradsGrid grid2)
    {
        return TwoGridOperation(grid1, grid2, (a, b) => a / b);
    }

    private static IGradsGrid TwoGridOperation(IGradsGrid grid1, IGradsGrid grid2, Func<double, double, double> func)
    {
        IGradsGrid result = grid1.CloneGrid();
        for (int j = 0; j < result.GridData.Length; j++)
        {
            result.GridData[j] = func(grid1.GridData[j], grid2.GridData[j]);
        }

        return result;
    }


    private static IGradsGrid UpdateAllElementsInArray(IGradsGrid grid, Func<double, double> func)
    {   
        IGradsGrid result = grid.CloneGrid();
        
        for (int j = 0; j < grid.GridData.Length; j++)
        {
            result.GridData[j] = func(grid.GridData[j]);
        }

        return result;
    }
}