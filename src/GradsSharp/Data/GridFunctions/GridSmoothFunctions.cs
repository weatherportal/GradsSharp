using GradsSharp.Models;

namespace GradsSharp.Data.GridFunctions;

public static class GridSmoothFunctions
{
    public static IGradsGrid Smooth9(this IGradsGrid grid, double midWeight = 1.0, double sideWeight = 0.5, double cornerWeight = 0.3)
    {
        var result = grid.CloneGrid();
        int p;
        double s, w;
        int nw = 0;
            
        int k = 0;
        for (int j = 0; j < grid.JSize; j++) {
            for (int i = 0; i < grid.ISize; i++) {
                if (grid.UndefinedMask[k] != 0) {
                    s = grid.GridData[k] * midWeight;
                    w = midWeight;
                    if (i != 0 && grid.UndefinedMask[k - 1] != 0) {
                        s = s + grid.GridData[k - 1] * sideWeight;
                        w += sideWeight;
                    }
                    if (i != grid.ISize - 1 && grid.UndefinedMask[k + 1] != 0) {
                        s = s + grid.GridData[k] * sideWeight;
                        w += sideWeight;
                    }
                    if (j != 0) {
                        p = k - grid.ISize;
                        if (grid.UndefinedMask[p] != 0) {
                            s = s + grid.GridData[p] * sideWeight;
                            w += sideWeight;
                        }
                        if (i != 0 && grid.UndefinedMask[p-1] != 0) {
                            s = s + grid.GridData[p-1] * cornerWeight;
                            w += cornerWeight;
                        }
                        if (i != grid.ISize - 1 && grid.UndefinedMask[p + 1] != 0) {
                            s = s + grid.GridData[p+1] * cornerWeight;
                            w += cornerWeight;
                        }
                    }
                    if (j != grid.JSize - 1) {
                        p = k + grid.ISize;
                        if (grid.UndefinedMask[p] != 0) {
                            s = s + grid.GridData[p] * sideWeight;
                            w += sideWeight;
                        }
                        if (i != 0 && grid.UndefinedMask[p -1 ] != 0) {
                            s = s + grid.GridData[p-1] * cornerWeight;
                            w += cornerWeight;
                        }
                        if (i != grid.ISize - 1 && grid.UndefinedMask[p + 1] != 0) {
                            s = s + grid.GridData[p+1] * cornerWeight;
                            w += cornerWeight;
                        }
                    }
                    result.GridData[nw] = s / w;
                    result.UndefinedMask[nw] = 1;
                } else {
                    result.UndefinedMask[nw] = 0;
                }
                nw++;
                k++;
            }
        }

        return result;
    }
}