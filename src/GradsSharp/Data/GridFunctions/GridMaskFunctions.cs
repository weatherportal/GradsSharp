using GradsSharp.Drawing.Grads;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data.GridFunctions;

public static class GridMaskFunctions
{
    public static IGradsGrid Skip(this IGradsGrid grid, int iskip)
    {
        return Skip(grid, iskip, iskip);
    }
    
    public static IGradsGrid Skip(this IGradsGrid grid, int iskip, int jskip)
    {
        if (grid.IDimension == -1) return grid;
        
        var result = grid.CloneGrid();

        iskip = iskip - 1;
        jskip = jskip - 1;
        int jj = -1;
        int uval = 0;
        for (int j = 0; j < grid.JSize; j++) {
            jj++;
            if (jj > jskip) jj = 0;
            int ii = -1;
            for (int i = 0; i < grid.ISize; i++) {
                ii++;
                if (ii > iskip) ii = 0;
                if (ii > 0 || jj > 0) result.UndefinedMask[uval] = 0;
                uval++;
            }
        }

        return result;
    }

    public static IGradsGrid MaskOut(this IGradsGrid grid, IGradsGrid grid2)
    {
        return GaExpr.gagrop(grid as GradsGrid, grid2 as GradsGrid, 13, 1);
    }
    
}