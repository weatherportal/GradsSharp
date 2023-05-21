namespace GradsSharp.Drawing.Grads;

internal class GaExpr
{
    public static int gagchk(gagrid pgr1, gagrid pgr2, int dim)
    {
        double gmin1, gmax1, gmin2, gmax2, fuz1, fuz2, fuzz;
        Func<double[], double, double> conv1, conv2;
        double[] vals1, vals2;
        int i1, i2, i, siz1, siz2, rc;
        dt dtim1 = new(), dtim2 = new();

        if (dim < 0) return (0);

        if (dim == pgr1.idim)
        {
            conv1 = pgr1.igrab;
            vals1 = pgr1.ivals;
            i1 = pgr1.ilinr;
            siz1 = pgr1.isiz;
        }
        else if (dim == pgr1.jdim)
        {
            conv1 = pgr1.jgrab;
            vals1 = pgr1.jvals;
            i1 = pgr1.jlinr;
            siz1 = pgr1.jsiz;
        }
        else return (1);

        if (dim == pgr2.idim)
        {
            conv2 = pgr2.igrab;
            vals2 = pgr2.ivals;
            i2 = pgr2.ilinr;
            siz2 = pgr2.isiz;
        }
        else if (dim == pgr2.jdim)
        {
            conv2 = pgr2.jgrab;
            vals2 = pgr2.jvals;
            i2 = pgr2.jlinr;
            siz2 = pgr2.jsiz;
        }
        else return (1);

        if (siz1 != siz2)
        {
            GaGx.gaprnt(0, "Error in gagchk: axis sizes are not the same");
            return (1);
        }

        gmin1 = pgr1.dimmin[dim];
        gmax1 = pgr1.dimmax[dim];
        gmin2 = pgr2.dimmin[dim];
        gmax2 = pgr2.dimmax[dim];

        if (dim == 3)
        {
            /* Dimension is time.      */
            rc = 0;
            GaUtil.gr2t(vals1, gmin1, out dtim1);
            GaUtil.gr2t(vals2, gmin2, out dtim2);
            if (dtim1.yr != dtim2.yr) rc = 1;
            if (dtim1.mo != dtim2.mo) rc = 1;
            if (dtim1.dy != dtim2.dy) rc = 1;
            if (dtim1.hr != dtim2.hr) rc = 1;
            if (dtim1.mn != dtim2.mn) rc = 1;
            GaUtil.gr2t(vals1, gmax1, out dtim1);
            GaUtil.gr2t(vals2, gmax2, out dtim2);
            if (dtim1.yr != dtim2.yr) rc = 1;
            if (dtim1.mo != dtim2.mo) rc = 1;
            if (dtim1.dy != dtim2.dy) rc = 1;
            if (dtim1.hr != dtim2.hr) rc = 1;
            if (dtim1.mn != dtim2.mn) rc = 1;
            if (rc > 0)
            {
                GaGx.gaprnt(0, "Error in gagchk: time axis endpoint values are not equivalent\n");
                return (1);
            }

            return (0);
        }

        /* Check endpoints.  If unequal, then automatic no match.        */

        fuz1 = Math.Abs(conv1(vals1, gmax1) - conv1(vals1, gmin1)) * Gx.FUZZ_SCALE;
        fuz2 = Math.Abs(conv2(vals2, gmax2) - conv2(vals2, gmin2)) * Gx.FUZZ_SCALE;
        fuzz = (fuz1 + fuz2) * 0.5;

        rc = 0;
        if (Math.Abs((conv1(vals1, gmin1)) - (conv2(vals2, gmin2))) > fuzz) rc = 1;
        if (Math.Abs((conv1(vals1, gmax1)) - (conv2(vals2, gmax2))) > fuzz) rc = 1;
        if (rc > 0)
        {
            GaGx.gaprnt(0, "Error in gagchk: axis endpoint values are not equivalent\n");
            return (1);
        }

        if (i1 != i2)
        {
            GaGx.gaprnt(0, "Error in gagchk: one axis is linear and the other is non-linear\n");
            return (1);
        }

        if (i1 > 0) return (0); /* If linear then matches  */

        /* Nonlinear, but endpoints match.  Check every grid point for a
           match.  If any non-matches, then not a match.     */

        for (i = 0; i < siz1; i++)
        {
            if (Math.Abs((conv1(vals1, gmin1 + (double)i)) - (conv2(vals2, gmin2 + (double)i))) > fuzz)
            {
                GaGx.gaprnt(0, "Error in gagchk: axis values are not all the same\n");
                return (1);
            }
        }

        return (0);
    }
}