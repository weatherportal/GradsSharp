using GradsSharp.Data;

namespace GradsSharp.Models.Internal;

internal class GradsGrid : ICloneable, IGradsGrid
{
    public GradsFile pfile;
    /* Address of the associated gafile
                                    classure to get the data from
                                    (requestor block only)               */
    public double[] GridData { get; set; } /* Grid data                 */

    public int mnum; /* Number of grids when a multiple
                                  grid result.  Note in this case, *grid
                                  points to more than one grid, with the
                                  "default" result being the 1st grid  */

    public int mtype; /* Type of multiple result grid         */
    public List<int> mnums; /* See mvals  */

    public List<double> mvals; /* Metadata associated with a multiple
                                  grid result.  What is here depends on
                                  the value of mtype.                  */

    public double Undef { get; set; } /* Undefined value for this grid.       */

    public double MinimumGridValue { get; set; }
    public double MaximumGridValue { get; set; } /* Minimum/Maximum grid value
                                  (rmin is set to the grid value when
                                  isiz=jsiz=1.  *grid points to here.) */

    public byte[] UndefinedMask { get; set; } /* Mask for undefined values in the grid */

    public byte umin, umax; /* Min/max undefined mask values. 
                                  (when isiz=jsiz=1, umin is set to the 
                                  mask value and *umask points to umin) */

    public int ISize { get; internal set; } /* isiz = number of elements per row. */
    public int JSize { get; internal set; } /* jsiz = number of rows.               */


    
    /* Dimension of rows and columns.
                                  -1 = This dimension does not vary
                                   0 = X dimension (usually longitude)
                                   1 = Y dimension (usually lattitude)
                                   2 = Z dimension (usually pressure)
                                   3 = Time
                                   4 = Ensemble
                                  If both dimensions are -1, then the
                                  grid has one value, which will be
                                  placed in rmin.                      */
    public int IDimension { get; set; } 
    public int JDimension { get; set; } 

    public int iwrld, jwrld; /* World coordinates valid?             */

    /* Dimension limits for each dimension
                                  (X,Y,Z,T,E) in grid units.           */
    public int[] DimensionMinimum { get; internal set; } = new int[5];
    public int[] DimensionMaximum { get; internal set; } = new int[5];
    public double[] WorldDimensionMinimum { get; internal set; }
    public double[] WorldDimensionMaximum { get; internal set; }

    public gavar pvar; /* Pointer to the classure with info
                                  on this particular variable.  If
                                  NULL, this grid is the result of
                                  an expression evaluation where the
                                  variable type is unkown.             */
    public string exprsn; /* If grid is a 'final' result, this
                                  will point to a character string that
                                  contains the original expression.    */
    public int alocf; /* Scaling info allocated for us only  */

    
    public Func<double[], double, double>? igrab;
    public Func<double[], double, double>? jgrab;
    /* Addresses of routines to perform
   grid-to-absolute coordinate
   transforms for this grid's i and j
   dimensions (unless i or j = 3).      */
    public Func<double[], double, double> iabgr;
    public Func<double[], double, double> jabgr;
    /* Absolute to grid conversion routines */
    
    public double[] ivals, jvals;  /* Conversion info for grid to abs      */
    public double[] iavals, javals;  /* Conversion info for abs to grid      */
    public int ilinr, jlinr; /* Indicates if linear transformation   */
    public int toff; /* Indicates if T dim values are forecast offsets */

    public IGriddedDataReader DataReader { get; set; }

    public object Clone()
    {
        GradsGrid clone = new GradsGrid
        {
            alocf = alocf,
            DimensionMaximum = DimensionMaximum,
            DimensionMinimum = DimensionMinimum,
            GridData = GridData,
            iabgr = iabgr,
            iavals = iavals,
            javals = javals,
            IDimension = IDimension,
            JDimension = JDimension,
            igrab = igrab,
            ilinr = ilinr,
            jlinr = jlinr,
            ISize = ISize,
            JSize = JSize,
            ivals = ivals,
            jvals = jvals,
            iwrld = iwrld,
            jwrld = jwrld,
            jabgr = jabgr,
            jgrab = jgrab,
            mnum = mnum,
            mnums = mnums,
            mtype = mtype,
            mvals = mvals,
            MaximumGridValue = MaximumGridValue,
            MinimumGridValue = MinimumGridValue,
            toff = toff,
            UndefinedMask = UndefinedMask,
            umax = umax,
            umin = umin,
            Undef = Undef
        };

        return clone;

    }
};