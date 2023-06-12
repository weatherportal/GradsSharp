using GradsSharp.Drawing.Grads;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data.GridFunctions;

public static class GridMaskFunctions
{

    public static IGradsGrid Const(this IGradsGrid grid, int cnst, ConstMode mode) 
    {
        var result = grid.CloneGrid();
        
        int cnt = grid.ISize * grid.JSize;
        int gval = 0;
        
        
        
        for (int i=0; i<cnt; i++) {
            if (mode==ConstMode.Undefined) {
                /* change valid data to a constant, missing data unchanged */
                if (grid.UndefinedMask[gval]!=0) result.GridData[gval] = cnst;
            } 
            else if (mode == ConstMode.Missing) {
                /* change missing data to a constant, update mask value */
                if (grid.UndefinedMask[gval]==0) {
                    result.GridData[gval] = cnst;
                    result.UndefinedMask[gval] = 1;       
                }
            } 
            else if (mode == ConstMode.All) {
                /* change valid and missing data to a constaont, update mask values */
                result.GridData[gval] = cnst;
                result.UndefinedMask[gval] = 1;
            }

            gval++;
        }

        return result;
    }
    
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