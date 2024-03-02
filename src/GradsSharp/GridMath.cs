using GradsSharp.Data.GridFunctions;
using GradsSharp.Drawing.Grads;
using GradsSharp.Enums;
using GradsSharp.Models;
using GradsSharp.Models.Internal;


/// <summary>
/// Contains methods for performing mathematical operations on grids
/// </summary>
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
    /// <param name="grid">First grid</param>
    /// <param name="otherGrids">Other grids</param>
    /// <returns>Average of all grids</returns>
    public static IGradsGrid Avg(IGradsGrid grid, params IGradsGrid[] otherGrids)
    {
        return grid.Average(otherGrids);
    }

    /// <summary>
    /// Performs a centered difference operation on a grid in the direction specified by dimension. The difference is done in the grid space, and no adjustment is performed for unequally spaced grids. The result value at each grid point is the value at the grid point plus one minus the value at the grid point minus one.
    /// The dimension argument specifies the dimension over which the difference is to be taken.
    ///
    /// Result values at the grid boundaries are set to missing.
    /// </summary>
    /// <param name="grid">Grid to operate on</param>
    /// <param name="dimension">Direction to differentiate</param>
    /// <returns>Differentiated grid</returns>
    public static IGradsGrid CDiff(IGradsGrid grid, Dimension dimension)
    {
        return grid.CDiff(dimension);
    }


    /// <summary>
    /// This function takes an areal average over a grid
    /// </summary>
    /// <param name="grid">Grid to calculate the function on</param>
    /// <param name="func">Type of calculation</param>
    /// <returns>The resulting grid with a constant value</returns>
    public static IGradsGrid AreaCalculation(IGradsGrid grid, AreaFunction func)
    {
        double[] ivals, jvals, gr;
        double d2r, sum, w1, w2 = 0, y1, x1, abs, alo, ahi, alen, wt;
        int i, j;
        byte[] gru;
        byte sumu = 0;

        GradsGrid pgr = (GradsGrid)grid;

        d2r = Math.PI / 180.0;
        var iconv = pgr.igrab;
        var jconv = pgr.jgrab;
        ivals = pgr.ivals;
        jvals = pgr.jvals;
        sum = 0.0;
        wt = 0.0;
        gr = pgr.GridData;
        gru = pgr.UndefinedMask;

        int idx = 0;

        for (j = 0; j < pgr.JSize; j++)
        {
            y1 = (double)(j + pgr.DimensionMinimum[1]);
            abs = jconv(jvals, y1);
            alo = jconv(jvals, y1 - 0.5);
            ahi = jconv(jvals, y1 + 0.5);
            alen = Math.Abs(ahi - alo); /* length of the grid side in world coord */
            
            // can this happen ?
            // if (alo < dmin1) alo = dmin1;
            // if (alo > dmax1) alo = dmax1;
            // if (ahi < dmin1) ahi = dmin1;
            // if (ahi > dmax1) ahi = dmax1;
            // if (alo < -90.0) alo = -90.0;
            // if (ahi < -90.0) ahi = -90.0;
            // if (alo > 90.0) alo = 90.0;
            // if (ahi > 90.0) ahi = 90.0;
            w1 = 1.0;
            if (func == GradsSharp.Enums.AreaFunction.Average || func == GradsSharp.Enums.AreaFunction.Total)
            {
                w1 = Math.Abs(Math.Sin(ahi * d2r) - Math.Sin(alo * d2r)); /* for aave and atot, area weighting by latitude */
            }
            else if (func == GradsSharp.Enums.AreaFunction.Mean  )
            {
                w1 = Math.Abs(ahi - alo); /* for amean, weight is length of interval in world coords */
            }
            else if (func == GradsSharp.Enums.AreaFunction.Sum)
            {
                if (alen > 1e-5)
                {
                    /* grid weighting (asum), weighted by length of interval in grid coords */
                    w1 = Math.Abs(ahi - alo) / alen;
                }
                else
                {
                    w1 = 0.0;
                }
            }

            for (i = 0; i < pgr.ISize; i++)
            {
                x1 = (double)(i + pgr.DimensionMinimum[0]);
                alo = iconv(ivals, x1 - 0.5);
                ahi = iconv(ivals, x1 + 0.5);
                alen = Math.Abs(ahi - alo);
                // if (alo < dmin0) alo = dmin0;
                // if (alo > dmax0) alo = dmax0;
                // if (ahi < dmin0) ahi = dmin0;
                // if (ahi > dmax0) ahi = dmax0;

                if (func == GradsSharp.Enums.AreaFunction.Average  || func == GradsSharp.Enums.AreaFunction.Mean )
                {
                    w2 = ahi - alo; /* for aave and amean */
                }
                else if (func == GradsSharp.Enums.AreaFunction.Total )
                {
                    w2 = d2r * (ahi - alo); /* for atot */
                }
                else if (func == GradsSharp.Enums.AreaFunction.Sum)
                {
                    if (alen > 1e-5)
                    {
                        /* grid weighting (asum) */
                        w2 = Math.Abs(ahi - alo) / alen;
                    }
                    else
                    {
                        w2 = 0.0;
                    }
                }
                else if (func == GradsSharp.Enums.AreaFunction.SumG )
                {
                    w2 = 1.0; /* no weighting (asumg) */
                }

                if (gru[idx] != 0)
                {
                    if (func == GradsSharp.Enums.AreaFunction.SumG )
                    {
                        sum = sum + gr[idx]; /* no weighting for asumg */
                    }
                    else
                    {
                        sum = sum + (gr[idx] * w1 * w2); /* otherwise apply weights */
                    }

                    wt = wt + (w1 * w2);
                }

                idx++;
            }
        }

        if (wt > 0.0)
        {
            sumu = 1;
            if ((int)func <= 2)
            {
                sum = sum / wt;
            }
        }
        else
        {
            sumu = 0;
            sum = pgr.Undef;
        }

        GradsGrid result = GaExpr.gagrvl(sum);
        result.umin = sumu;
        
        return result;
    }
}