using GradsSharp.Drawing.Grads;
using GradsSharp.Models;

namespace GradsSharp.Data.GridFunctions;

public static class GridInterpolationFunctions
{
    /// <summary>
    /// function to interpolate within a 3-D grid to a specified
    /// pressure level.  Can also be used on non-pressure level data, such
    /// as sigma or eta-coordinate output where pressure is a function
    /// of time and grid level.
    /// </summary>
    /// <param name="grid">Grid to interpolate</param>
    /// <param name="level">
    /// name of 3-D grid holding pressure values at each gridpoint
    /// If you are using regular pressure-level data, this should be
    /// set to the builtin GrADS variable 'lev'
    /// </param>
    /// <param name="pressureData">Pressure level at which to interpolate</param>
    /// <returns></returns>
    public static IGradsGrid Interpolate(this IGradsGrid grid, IGradsGrid level, IGradsGrid pressureData, IGradsCommandInterface cmd)
    {
        // function pinterp(field,pgrid,plev)
        var gcmd = cmd as GradsCommandInterface;
        
        double zmin, zmax;
        double pmin, pmax;
        var dimInfo = cmd.QueryDimensionInfo();
        if (dimInfo.DimensionTypeZ == DimensionType.Fixed)
        {
            zmin = dimInfo.ZMin;
            zmax = zmin;
        }
        else
        {
            zmin = dimInfo.ZMin;
            zmax = dimInfo.ZMax;
        }

        double epsilon = 0.0001;

        var filInfo = cmd.QueryFileInfo();
        cmd.SetZ(1, filInfo.NumberOfLevels);
        dimInfo = cmd.QueryDimensionInfo();
        pmin = dimInfo.ZMin;
        pmax = dimInfo.ZMax;
        
        

        cmd.SetZ(1, filInfo.NumberOfLevels - 1);
        //var pabove = grid.MaskOut(pressureData.Add(epsilon).Subtract(level)).Multiply(0.5).Add(level.MaskOut())

        return grid;
    }
}