using GradsSharp.Drawing.Grads;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data.GridFunctions;

/// <summary>
/// Math functions for grids
/// </summary>
public static class GridMathFunctions
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
    
    public static IGradsGrid Log(this IGradsGrid grid)
    {
        return UpdateAllElementsInArray(grid, d => Math.Log(d));
    }
    public static IGradsGrid Exp(this IGradsGrid grid)
    {
        return UpdateAllElementsInArray(grid, d => Math.Exp(d));
    }
    public static IGradsGrid Pow(this IGradsGrid grid, double value)
    {
        return UpdateAllElementsInArray(grid, d => Math.Pow(d, value));
    }
    public static IGradsGrid Multiply(this IGradsGrid grid1, IGradsGrid grid2)
    {
        if(grid2.JSize == 1 && grid2.ISize == 1) {
            return Multiply(grid1, grid2.GridData[0]);
        }
        return TwoGridOperation(grid1, grid2, (a, b) => a * b);
    }
    public static IGradsGrid Subtract(this IGradsGrid grid1, IGradsGrid grid2)
    {
        if(grid2.JSize == 1 && grid2.ISize == 1) {
            return Subtract(grid1, grid2.GridData[0]);
        }
        
        
        return TwoGridOperation(grid1, grid2, (a, b) => a - b);
    }

    public static IGradsGrid Add(this IGradsGrid grid1, IGradsGrid grid2)
    {
        if(grid2.JSize == 1 && grid2.ISize == 1) {
            return Add(grid1, grid2.GridData[0]);
        }
        return TwoGridOperation(grid1, grid2, (a, b) => a + b);
    }
    public static IGradsGrid Divide(this IGradsGrid grid1, IGradsGrid grid2)
    {
        if(grid2.JSize == 1 && grid2.ISize == 1) {
            return Divide(grid1, grid2.GridData[0]);
        }
        return TwoGridOperation(grid1, grid2, (a, b) => a / b);
    }


    public static IGradsGrid HCurl(this IGradsGrid grid1, IGradsGrid grid2)
    {
        GradsGrid pgr1 = grid1 as GradsGrid;
        GradsGrid pgr2 = grid2 as GradsGrid;

        IGradsGrid result = pgr1.CloneGrid();
        
        if (pgr1.IDimension != 0 && pgr2.JDimension != 1)
        {
            throw new Exception("Invalid dimension environment: Horizontal environment (X, Y Varying) is required");
        }
        
        if (GaExpr.gagchk(pgr1, pgr2, 0)>0 ||
            GaExpr.gagchk(pgr1, pgr2, 1)>0) {
            throw new Exception("Error from HCURL:  Incompatable grids - Dimension ranges unequal");
        }
        
        
        int size = pgr1.ISize * pgr1.JSize;
        for (int i = 0; i < size; i++)
        {
            result.GridData[i] = 0;
            result.UndefinedMask[i] = 0;
        }
        
        var lnvals = pgr1.ivals;
        var ltvals = pgr1.jvals;
        var lnconv = pgr1.igrab;
        var ltconv = pgr1.jgrab;

        /*             p4
                     |
                 p1--p--p3
                     |
                     p2                           */

        int p = pgr1.ISize + 1;
        int p1 = pgr2.ISize;
        int p2 = 1;
        int p3 = p1 + 2;
        int p4 = p2 + (2 * pgr1.ISize);
        double d2r = Math.PI / 180;

        for (int j = (pgr1.DimensionMinimum[1] + 1); j < pgr1.DimensionMaximum[1]; j++) {
            double rj = (double) j;
            double lat = ltconv(ltvals, rj) * d2r;
            double lat2 = ltconv(ltvals, rj - 1.0) * d2r;
            double lat4 = ltconv(ltvals, rj + 1.0) * d2r;
            for (int i = (pgr1.DimensionMinimum[0] + 1); i < pgr1.DimensionMaximum[0]; i++) {
                if (pgr2.UndefinedMask[p1] != 0 &&
                    pgr1.UndefinedMask[p2] != 0 &&
                    pgr2.UndefinedMask[p3] != 0 &&
                    pgr1.UndefinedMask[p4] != 0) {
                    double ri = (double) i;
                    double lon1 = lnconv(lnvals, ri - 1.0) * d2r;
                    double lon3 = lnconv(lnvals, ri + 1.0) * d2r;
                    result.GridData[p] = (pgr2.GridData[p3] - pgr2.GridData[p1]) / (lon3 - lon1);
                    result.GridData[p] = result.GridData[p] - (pgr1.GridData[p4] * Math.Cos(lat4) - pgr1.GridData[p2] * Math.Cos(lat2)) / (lat4 - lat2);
                    double temp = 6.37E6 * Math.Cos(lat);
                    if (temp > 1E-10) {
                        result.GridData[p] = result.GridData[p] / temp;
                        result.UndefinedMask[p] = 1;
                    } else {
                        result.UndefinedMask[p] = 0;
                    }
                }
                p++;
                p1++;
                p2++;
                p3++;
                p4++;
               
            }
            p += 2;
            p1 += 2;
            p2 += 2;
            p3 += 2;
            p4 += 2;
            
        }
        return result;
    }
    
    private static IGradsGrid TwoGridOperation(IGradsGrid grid1, IGradsGrid grid2, Func<double, double, double> func)
    {
        IGradsGrid result = grid1.CloneGrid();

        for (int j = 0; j < grid1.GridData.Length; j++)
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