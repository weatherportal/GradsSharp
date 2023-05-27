using System.Text;
using GradsSharp.Utils;

namespace GradsSharp.Drawing.Grads;

internal class GaUtil
{
    static string[] mons =
    {
        "jan", "feb", "mar", "apr", "may", "jun",
        "jul", "aug", "sep", "oct", "nov", "dec"
    };

    static int[] mosiz = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    static int[] momn =
    {
        0, 44640, 40320, 44640, 43200, 44640, 43200,
        44640, 44640, 43200, 44640, 43200, 44640
    };

    static string[] monc =
    {
        "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG",
        "SEP", "OCT", "NOV", "DEC"
    };

    static int[] mnacum =
    {
        0, 0, 44640, 84960, 129600, 172800, 217440,
        260640, 305280, 349920, 393120, 437760, 480960
    };

    static int[] mnacul =
    {
        0, 0, 44640, 86400, 131040, 174240, 218880,
        262080, 306720, 351360, 394560, 439200, 482400
    };

    public static int dequal(double op1, double op2, double tolerance)
    {
        if (Math.Abs(op1 - op2) <= tolerance) return (0);
        else return (1);
    }

    public static double hypot(double x, double y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }


    /* Add an offset to a time.  Output to dto.                          */

    public static void timadd(dt dtim, dt dto)
    {
        long i;
        bool cont;

        /* First add months and years.  Normalize as needed.               */
        dto.mo += dtim.mo;
        dto.yr += dtim.yr;

        while (dto.mo > 12)
        {
            dto.mo -= 12;
            dto.yr++;
        }

        /* Add minutes, hours, and days directly.  Then normalize
         to days, then normalize extra days to months/years.             */

        dto.mn += dtim.mn;
        dto.hr += dtim.hr;
        dto.dy += dtim.dy;

        if (dto.mn > 59)
        {
            i = dto.mn / 60;
            dto.hr += i;
            dto.mn = dto.mn - (i * 60);
        }

        if (dto.hr > 23)
        {
            i = dto.hr / 24;
            dto.dy += i;
            dto.hr = dto.hr - (i * 24);
        }

        cont = true;
        while (dto.dy > mosiz[dto.mo] && cont)
        {
            if (dto.mo == 2 && qleap(dto.yr))
            {
                if (dto.dy == 29) cont = false;
                else
                {
                    dto.dy -= 29;
                    dto.mo++;
                }
            }
            else
            {
                dto.dy -= mosiz[dto.mo];
                dto.mo++;
            }

            while (dto.mo > 12)
            {
                dto.mo -= 12;
                dto.yr++;
            }
        }
    }

    /* Subtract an offset from a time.  Subtract minutes/hours/days
   first so that we will exactly reverse the operation of timadd     */

    static void timsub(dt dtim, dt dto)
    {
        long s1, s2;

        /* Subtract minutes, hour, and days directly.  Then normalize
         to days, then normalize deficient days from months/years.       */

        dto.mn = dtim.mn - dto.mn;
        dto.hr = dtim.hr - dto.hr;
        dto.dy = dtim.dy - dto.dy;
        s1 = dto.mo;
        s2 = dto.yr;
        dto.mo = dtim.mo;
        dto.yr = dtim.yr;

        while (dto.mn < 0)
        {
            dto.mn += 60;
            dto.hr--;
        }

        while (dto.hr < 0)
        {
            dto.hr += 24;
            dto.dy--;
        }

        while (dto.dy < 1)
        {
            dto.mo--;
            if (dto.mo < 1)
            {
                dto.mo = 12;
                dto.yr--;
            }

            if (dto.mo == 2 && qleap(dto.yr)) dto.dy += 29;
            else dto.dy += mosiz[dto.mo];
        }

        /* Now subtract months and years.  Normalize as needed.            */

        dto.mo = dto.mo - s1;
        dto.yr = dto.yr - s2;

        while (dto.mo < 1)
        {
            dto.mo += 12;
            dto.yr--;
        }

        /* Adjust for leaps */

        if (dto.mo == 2 && dto.dy == 29 && !qleap(dto.yr))
        {
            dto.mo = 3;
            dto.dy = 1;
        }
    }

    static bool qleap(long year)
    {
        long i, y;

/*mf - disable if 365 day calendar mf*/

        //if (mfcmn.cal365 == 1) return (0);

        y = year;

        i = y / 4;
        i = (i * 4) - y;
        if (i != 0) return false;

        i = y / 100;
        i = (i * 100) - y;
        if (i != 0) return true;

        i = y / 400;
        i = (i * 400) - y;
        if (i != 0) return false;

        return true;
    }


    public static void gr2t(double[] vals, double gr, out dt dtim)
    {
        dt stim = new();
        double moincr, mnincr;
        double v;

        /* Get constants associated with this conversion                   */
        stim.yr = (int)(vals[0] + 0.1);
        stim.mo = (int)(vals[1] + 0.1);
        stim.dy = (int)(vals[2] + 0.1);
        stim.hr = (int)(vals[3] + 0.1);
        stim.mn = (int)(vals[4] + 0.1);
        moincr = vals[5];
        mnincr = vals[6];

        /* Initialize output time*/
        dtim = new dt();
        dtim.yr = 0;
        dtim.mo = 0;
        dtim.dy = 0;
        dtim.hr = 0;
        dtim.mn = 0;

        /* Do conversion if increment is in minutes.                       */
        if (mnincr > 0.1)
        {
            v = mnincr * (gr - 1.0);
            if (v > 0.0) v = v + 0.5; /* round */
            else v = v - 0.5;
            dtim.mn = (int)v;
            if (dtim.mn < 0)
            {
                dtim.mn = -1 * dtim.mn;
                timsub(stim, dtim);
            }
            else
            {
                timadd(stim, dtim);
            }

            return;

            /* Do conversion if increment is in months.  Same as for minutes,
         except special handling is required for partial months.
         JMA There is a bug here, and some precision decisions that need attention */
        }
        else
        {
            v = moincr * (gr - 1.0);
            if (v < 0.0) dtim.mo = (int)(v - 0.9999); /* round (sort of)       */
            else dtim.mo = (int)(v + 0.0001);
            v = v - (double)dtim.mo; /* Get fractional month  */
            if (dtim.mo < 0)
            {
                dtim.mo = -1 * dtim.mo;
                timsub(stim, dtim);
            }
            else timadd(stim, dtim);

            if (v < 0.0001) return; /* if fraction small, return       */

            if (dtim.mo == 2 && qleap(dtim.yr))
            {
                v = v * 41760.0;
            }
            else
            {
                v = v * (double)momn[dtim.mo];
            }

            stim = dtim;
            dtim.yr = 0;
            dtim.mo = 0;
            dtim.dy = 0;
            dtim.hr = 0;
            dtim.mn = (int)(v + 0.5);
            timadd(stim, dtim);
            return;
        }
    }

    public static int gat2ch(dt dtim, int tinc, out string ch, int chlen)
    {
        long mn1, mn2, hr1, hr2, dy1, dy2, len, mnth;

        mnth = dtim.mo - 1L;
        mn1 = dtim.mn / 10L;
        mn2 = dtim.mn - (mn1 * 10);
        hr1 = dtim.hr / 10L;
        hr2 = dtim.hr - (hr1 * 10);
        dy1 = dtim.dy / 10L;
        dy2 = dtim.dy - (dy1 * 10);
        if (tinc == 1)
        {
            ch = String.Format("{0:04i}", dtim.yr);
        }
        else if (tinc == 2)
        {
            if (dtim.yr == 9999L)
            {
                ch = monc[mnth];
            }
            else
            {
                ch = String.Format("{0}{1:04i}", monc[mnth], dtim.yr);
            }
        }
        else if (tinc == 3)
        {
            ch = String.Format("{0}{1}{2}{3:04i}", dy1, dy2, monc[mnth], dtim.yr);
        }
        else if (tinc == 4)
        {
            ch = String.Format("{0}{1}Z{2}{3}{4}{5:04i}", hr1, hr2, dy1, dy2, monc[mnth], dtim.yr);
        }
        else if (tinc == 5)
        {
            ch = String.Format("{0}{1}:{2}{3}Z{4}{5}{6}{7:04i}", hr1, hr2, mn1, mn2, dy1, dy2, monc[mnth], dtim.yr);
        }
        else ch = "???";

        return ch.Length;
    }

    /* Get minimum and maximum grid value.  Set rmin and rmax in the
   grid descriptor.                                                  */

    public static void gamnmx(gagrid pgr)
    {
        double[] r;
        int i, size, cnt;
        byte[] rmask;

        size = pgr.isiz * pgr.jsiz;
        if (size == 1) return;
        pgr.rmin = 9.99E35;
        pgr.rmax = -9.99E35;
        r = pgr.grid;
        rmask = pgr.umask;
        cnt = 0;
        int cntrmask = 0;
        int cntr = 0;

        for (i = 0; i < size; i++)
        {
            if (rmask[cntrmask] == 1)
            {
                cnt++;
                if (pgr.rmin > r[cntr])
                {
                    pgr.rmin = r[cntr];
                }

                if (pgr.rmax < r[cntr]) pgr.rmax = r[cntr];
            }

            cntr++;
            cntrmask++;
        }

        if (cnt == 0 || pgr.rmin == 9.99e35 || pgr.rmax == -9.99e35)
        {
            pgr.rmin = pgr.undef;
            pgr.rmax = pgr.undef;
            pgr.umin = pgr.umax = 0;
        }
        else
        {
            pgr.umin = pgr.umax = 1;
        }
    }

    public static long timdif(dt dtim1, dt dtim2, bool flag)
    {
        long min1, min2, mon1, mon2, yr;
        dt temp = new();
        bool swap;
        long mo1, mo2;

        swap = false;
        if (dtim1.yr > dtim2.yr)
        {
            temp = dtim1;
            dtim1 = dtim2;
            dtim2 = temp;
            swap = true;
        }

        /* add up minutes/months for each year between time2 and time1 */
        min1 = 0;
        min2 = 0;
        mon2 = 0;
        yr = dtim1.yr;
        while (yr < dtim2.yr)
        {
            if (qleap(yr)) min2 += 527040L;
            else min2 += 525600L;
            mon2 += 12;
            yr++;
        }

        if (flag)
        {
            /* return months */
            mon1 = dtim1.mo;
            mon2 += dtim2.mo;
            if (swap) return (mon1 - mon2);
            else return (mon2 - mon1);
        }
        else
        {
            /* return minutes */
            mo1 = dtim1.mo;
            mo2 = dtim2.mo;
            if (qleap(dtim1.yr))
            {
                min1 = min1 + mnacul[mo1] + (dtim1.dy * 1440L) + (dtim1.hr * 60L) + dtim1.mn;
            }
            else
            {
                min1 = min1 + mnacum[mo1] + (dtim1.dy * 1440L) + (dtim1.hr * 60L) + dtim1.mn;
            }

            if (qleap(dtim2.yr))
            {
                min2 = min2 + mnacul[mo2] + (dtim2.dy * 1440L) + (dtim2.hr * 60L) + dtim2.mn;
            }
            else
            {
                min2 = min2 + mnacum[mo2] + (dtim2.dy * 1440L) + (dtim2.hr * 60L) + dtim2.mn;
            }

            if (swap) return (min1 - min2);
            else return (min2 - min1);
        }
    }

    public static double t2gr(double[] vals, dt dtim)
    {
        dt stim = new();
        long eyear, mins, mons;
        double val, moincr, mnincr, rdiff;

        /* Get constants associated with this conversion                   */

        stim.yr = (int)(vals[0] + 0.1);
        stim.mo = (int)(vals[1] + 0.1);
        stim.dy = (int)(vals[2] + 0.1);
        stim.hr = (int)(vals[3] + 0.1);
        stim.mn = (int)(vals[4] + 0.1);

        moincr = vals[5];
        mnincr = vals[6];

        /* If the increment for this conversion is days, hours, or minutes,
         then we do our calculations in minutes.  If the increment is
         months or years, we do our calculations in months.              */


        if (mnincr > 0.1)
        {
            mins = timdif(stim, dtim, false);
            rdiff = (double)mins;
            val = rdiff / (mnincr);
            val += 1.0;
            return (val);
        }
        else
        {
            mons = timdif(stim, dtim, true);
            eyear = stim.yr;
            if (stim.yr > dtim.yr) eyear = dtim.yr;
            rdiff = (((dtim.yr - eyear) * 12) + dtim.mo) -
                    (((stim.yr - eyear) * 12) + stim.mo);
            stim.yr = dtim.yr;
            stim.mo = dtim.mo;
            mins = timdif(stim, dtim, false);
            if (mins > 0)
            {
                if (dtim.mo == 2 && qleap(dtim.yr))
                {
                    rdiff = rdiff + (((double)mins) / 41760.0);
                }
                else
                {
                    rdiff = rdiff + (((double)mins) / ((double)momn[dtim.mo]));
                }
            }

            val = rdiff / (moincr);
            val += 1.0;
            return (val);
        }
    }

    public static double liconv(double[] vals, double v)
    {
        return ((vals[0] * v) + vals[1]);
    }

    /* Converts strings to double */
    public static int? getdbl(string input, int start, out double val)
    {
        double res;
        int newPos = 0;
        res = StringToDouble.Parse(input, start, out newPos);
        if (newPos == start)
        {
            val = Double.MaxValue;
            return null;
        }
        else
        {
            val = res;
            return newPos;
        }
    }

    public static int? intprs(string input, int start, out int val)
    {
        int nflag, flag;

        nflag = 0;
        if (input[start] == '-')
        {
            nflag = 1;
            start++;
        }
        else if (input[start] == '+') start++;

        val = 0;
        flag = 1;

        while (input[start] >= '0' && input[start] <= '9')
        {
            val = val * 10 + (int)(input[start] - '0');
            flag = 0;
            start++;
        }

        if (flag > 0) return (null);

        if (nflag > 0) val = -1 * val;
        return (start);
    }

    public static int? dimprs(string expression, int pos, gastat pst, gafile pfi,
        out int dim, out double d, int type, out int wflag)
    {
        dt dtim = new();
        gaens ens;

        d = 0;
        dim = -1;
        wflag = -1;

        Func<double[], double, double>? conv;

        double[] cvals;
        double v = 0;
/* double g1,g2; */
        int i, op, len, enum1;

        StringBuilder nameb, enameb;
        string name, ename;

        /* parse the dimension name */
        i = 0;
        nameb = new StringBuilder();
        while (expression[pos] >= 'a' && expression[pos] <= 'z' && i < 6)
        {
            nameb.Append(expression[pos]);
            pos++;
            i++;
        }

        name = nameb.ToString();


        if (name.Length > 4)
        {
            GaGx.gaprnt(0, "Syntax Error:  Invalid dimension expression ");
            GaGx.gaprnt(0, $"  Expecting x/y/z/t/offt/e/lon/lat/lev/time/ens, found {name}");
            return (null);
        }

        /* parse the operator */
        if (expression[pos] == '=') op = 0;
        else if (expression[pos] == '+') op = 1;
        else if (expression[pos] == '-') op = 2;
        else
        {
            GaGx.gaprnt(0, "Syntax Error:  Invalid dimension expression");
            GaGx.gaprnt(0, $"  Expecting +/-/= operator, found {expression[pos]}");
            return (null);
        }

        /* dimension is TIME */
        pos++;
        int? newpos = 0;
        if ("time" == name)
        {
            if (op == 0)
            {
                if ((newpos = adtprs(expression, pos, pst.tmin, dtim)) == null)
                {
                    GaGx.gaprnt(0, "  Invalid absolute time in dimension expression");
                    return (null);
                }
            }
            else
            {
                if ((newpos = rdtprs(expression, pos, dtim)) == null)
                {
                    GaGx.gaprnt(0, "  Invalid relative time in dimension expression");
                    return (null);
                }
            }
        }
        /* dimension is ENS */
        else if ("ens" == name)
        {
            /* parse the ensemble name */
            newpos = pos;
            len = 0;
            enameb = new StringBuilder();
            while (len < 16 && expression[newpos ?? 0] != ')')
            {
                enameb.Append(expression[newpos ?? 0]);
                len++;
                newpos++;
            }

            ename = enameb.ToString();
        }
        /* all other dimensions */
        else
        {
            if ((newpos = getdbl(expression, pos, out v)) == null)
            {
                GaGx.gaprnt(0, "Syntax Error:  Invalid dimension expression");
                GaGx.gaprnt(0, "  Dimension value missing or invalid");
                return (null);
            }
        }

        pos = newpos ?? int.MaxValue;

        /* We now have all the info we need about this dimension expression to evaluate it.  */
        if ("x" == name) dim = 0;
        else if ("y" == name) dim = 1;
        else if ("z" == name) dim = 2;
        else if ("t" == name) dim = 3;
        else if ("offt" == name) dim = 3;
        else if ("e" == name) dim = 4;
        else if ("lon" == name) dim = 5;
        else if ("lat" == name) dim = 6;
        else if ("lev" == name) dim = 7;
        else if ("time" == name) dim = 8;
        else if ("ens" == name) dim = 9;
        else if (type == 0 && "r" == name) dim = 10;
        else
        {
            GaGx.gaprnt(0, "Syntax Error:  Invalid dimension expression");
            GaGx.gaprnt(0, $"  Expecting x/y/z/t/offt/e/lat/lon/lev/time/ens, found {name}");
            return (null);
        }

        /* for station expressions */
        if (dim == 10)
        {
            d = v;
            return (pos);
        }

        /* dimension expression is given in grid coordinates: x, y, z, t, offt, or e */
        wflag = 0;
        if (dim < 5)
        {
            if ("offt" == name) wflag = 2; /* trip the time offset flag */
            if (op == 0)
            {
                d = v + pfi.dimoff[dim]; /* straight override of fixed dimension value */
                return (pos);
            }
            else
            {
                /* make sure the dimension is not varying */
                if (dim == pst.idim || dim == pst.jdim)
                {
                    GaGx.gaprnt(0, "Syntax Error:  Invalid dimension expression");
                    GaGx.gaprnt(0, "  Cannot use an offset value with a varying dimension");
                    GaGx.gaprnt(0, $"  Varying dimension = {dim}");
                    return (null);
                }

                /* get current dimension value in grid coordinates from gastat structure */
                if (dim == 3)
                {
                    d = t2gr(pfi.abvals[3], pst.tmin);
                }
                else
                {
                    if (pfi.type == 1 || pfi.type == 4)
                    {
                        conv = pfi.ab2gr[dim];
                        cvals = pfi.abvals[dim];
                        d = conv(cvals, pst.dmin[dim]);
                    }
                    else
                    {
                        d = pst.dmin[dim];
                    }
                }

                /* combine offset with current dimension value */
                if (op == 1) d = d + v;
                if (op == 2) d = d - v;
                return (pos);
            }
        }
        /* dimension expression is given in world coordinates: lon, lat, lev, time, or ens */
        else
        {
            dim = dim - 5;
            wflag = 1;
/*     if (cmpwrd("offtime",name)) { */
/*       /\* determine the size of the time offset in grid units *\/ */
/*       g1 = t2gr(pfi.abvals[3],&(pst.tmin)); */
/*       timadd (&(pst.tmin),&dtim); */
/*       g2 = t2gr(pfi.abvals[3],&dtim); */
/*       v = g2 - g1; */
/*       *wflag=2;      /\* trip the time offset flag *\/ */
/*     } */
            if (op > 0)
            {
                /* check to make sure dimension isn't varying */
                if (dim == pst.idim || dim == pst.jdim)
                {
                    GaGx.gaprnt(0, "Syntax Error:  Invalid dimension expression");
                    GaGx.gaprnt(0, "  Cannot use an offset value with a varying dimension");
                    GaGx.gaprnt(0, $"  Varying dimension = {dim}");
                    return (null);
                }

                /* check to make sure dimension isn't E */
                if (dim == 4)
                {
                    GaGx.gaprnt(0, "Syntax Error:  Invalid dimension expression");
                    GaGx.gaprnt(0, "  Cannot use an offset value with an ensemble name");
                    return (null);
                }

                /* combine offset with current dimension value from gastat structure */
                if (dim == 3)
                {
                    if (op == 1) timadd((pst.tmin), dtim);
                    if (op == 2) timsub((pst.tmin), dtim);
                }
                else
                {
                    if (op == 1) v = pst.dmin[dim] + v;
                    if (op == 2) v = pst.dmin[dim] - v;
                }
            }

            if (dim == 4)
            {
                /* loop over ensembles, looking for matching name */
                // ens = pfi.ens1;
                // i = 0;
                // enum1 = -1;
                // while (i < pfi.dnum[dim]) {
                //     if (strcmp(ename, ens.name) == 0) enum1 = i;  /* grid coordinate of matching name */
                //     i++;
                //     ens++;
                // }
                // if (enum1 < 0) {
                //     gaprnt(0, "Syntax Error:  Invalid dimension expression\n");
                //     snprintf(pout, 1255, "  Ensemble name \"%s\" not found\n", ename);
                //     gaprnt(0, pout);
                //     return (null);
                // }
                // /* straight override of ensemble grid coordinate */
                // *d = enum1 + 1 + pfi.dimoff[dim];
                // return (ch);
                throw new NotImplementedException();
            }
            /* get the grid coordinate for the new (combined) dimension value */
            else if (dim == 3)
            {
                d = t2gr(pfi.abvals[3], dtim);
            }
            else
            {
                if (pfi.type == 1 || pfi.type == 4)
                {
                    /* grids  */
                    conv = pfi.ab2gr[dim];
                    cvals = pfi.abvals[dim];
                    d = conv(cvals, v);
                }
                else
                {
                    d = v; /* station data */
                }
            }

            return (pos);
        }
    }

    public static int? adtprs(string expression, int pos, dt def, dt dtim)
    {
        int val, flag, i;

        string monam;


        dtim.mn = 0;
        dtim.hr = 0;
        dtim.dy = 1;

        if (expression[pos] >= '0' && expression[pos] <= '9')
        {
            flag = 0;
            pos = intprs(expression, pos, out val) ?? throw new Exception("Parsing integer value failed");
            if (expression[pos] == ':' || Char.ToLower(expression[pos]) == 'z')
            {
                if (val > 23)
                {
                    GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time value.\n");
                    GaGx.gaprnt(0, $"  Hour = {val} -- greater than 23");
                    return (null);
                }

                dtim.hr = val;
                if (expression[pos] == ':')
                {
                    pos++;
                    if (expression[pos] >= '0' && expression[pos] <= '9')
                    {
                        pos = intprs(expression, pos, out val) ?? throw new Exception("Parsing integer value failed");
                        if (val > 59)
                        {
                            GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time value.\n");
                            GaGx.gaprnt(0, $"  Minute = {val} -- greater than 59");
                            return (null);
                        }

                        if (Char.ToLower(expression[pos]) != 'z')
                        {
                            GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time value.");
                            GaGx.gaprnt(0, "  'z' delimiter is missing ");
                            return (null);
                        }

                        dtim.mn = val;
                        pos++;
                        if (expression[pos] >= '0' && expression[pos] <= '9')
                            pos = intprs(expression, pos, out val) ??
                                  throw new Exception("Error parsing integer value");
                        else val = (int)def.dy;
                    }
                    else
                    {
                        GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time value.");
                        GaGx.gaprnt(0, "  Missing minute value ");
                        return (null);
                    }
                }
                else
                {
                    pos++;
                    if (expression[pos] >= '0' && expression[pos] <= '9')
                        pos = intprs(expression, pos, out val) ?? throw new Exception("Error parsing integer value");
                    else val = (int)def.dy;
                }
            }
            else flag = 2;

            dtim.dy = val;
        }
        else flag = 1;


        StringBuilder mnb = new StringBuilder();
        mnb.Append(Char.ToLower(expression[pos]));
        mnb.Append(Char.ToLower(expression[pos + 1]));
        mnb.Append(Char.ToLower(expression[pos + 2]));

        monam = mnb.ToString();
        i = 0;
        while (i < 12 && monam != mons[i]) i++;
        i++;

        if (i == 13)
        {
            if (flag == 1)
            {
                GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time value.\n");
                GaGx.gaprnt(0, "  Expected month abbreviation, none found\n");
                return (null);
            }

            if (flag == 2)
            {
                GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time value.\n");
                GaGx.gaprnt(0, "  Missing month abbreviation or 'z' delimiter\n");
                return (null);
            }

            dtim.mo = def.mo;
            dtim.yr = def.yr;
        }
        else
        {
            dtim.mo = i;
            pos += 3;
            /* parse year */
            if (expression[pos] >= '0' && expression[pos] <= '9')
            {
                /* use fullyear only if year 1 = 0001*/
                // if (*(ch + 2) >= '0' && *(ch + 2) <= '9') {
                //     mfcmn.fullyear = 1;   /* 4-digit year */
                // } else {
                //     mfcmn.fullyear = 0;   /* 2-digit year */
                // }
                pos = intprs(expression, pos, out val) ?? throw new Exception("Error parsing integer value");
            }
            else
            {
                val = (int)def.yr;
            }

            /* turn off setting of < 100 years to 1900 or 2000 */
            // if (mfcmn.fullyear == 0) {
            //     if (val < 50) val += 2000;
            //     else if (val < 100) val += 1900;
            // }
            dtim.yr = val;
        }

        i = mosiz[dtim.mo];
        if (dtim.mo == 2 && qleap(dtim.yr)) i = 29;
        if (dtim.dy > i)
        {
            GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time value.\n");
            GaGx.gaprnt(0, $"  Day = {dtim.dy} -- greater than {i} \n");
            return (null);
        }

        return (pos);
    }


    /* Parse a relative date/time (offset).  Format is:

   nn (yr/mo/dy/hr/mn)

   Examples:  5mo
              1dy12hr
              etc.

   Missing values are filled in with 0s.                             */
    public static int? rdtprs(string expression, int pos, dt dtim)
    {
        int flag, val;
        string id;

        dtim.yr = 0;
        dtim.mo = 0;
        dtim.dy = 0;
        dtim.hr = 0;
        dtim.mn = 0;

        flag = 1;

        while (expression[pos] >= '0' && expression[pos] <= '9')
        {
            flag = 0;
            pos = intprs(expression, pos, out val) ?? throw new Exception("Error parsing integer value");
            StringBuilder idb = new StringBuilder();
            idb.Append(expression[pos]);
            idb.Append(expression[pos + 1]);
            id = idb.ToString();
            if ("yr" == id) dtim.yr = val;
            else if ("mo" == id) dtim.mo = val;
            else if ("dy" == id) dtim.dy = val;
            else if ("hr" == id) dtim.hr = val;
            else if ("mn" == id) dtim.mn = val;
            else
            {
                GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time offset.");
                GaGx.gaprnt(0, "  Expecting yr/mo/dy/hr/mn, found {id}");
                return (null);
            }

            pos += 2;
        }

        if (flag > 0)
        {
            GaGx.gaprnt(0, "Syntax Error:  Invalid Date/Time offset.");
            GaGx.gaprnt(0, "  No offset value given");
            return (null);
        }

        return (pos);
    }
    
    public static double lev2gr(double[] vals, double lev) {
        int i, num;
        double gr;
        
        num = (int) (vals[0] + 0.1);
        for (i = 1; i < num; i++) {
            if ((lev >= vals[i] && lev <= vals[i+1]) ||
                (lev <= vals[i] && lev >= vals[i+1])) {
                gr = (double) i + (lev - vals[i]) / (vals[i+1] - vals[i]);
                return (gr);
            }
        }
        if (vals[1] < vals[num]) {
            if (lev < vals[1]) {
                gr = 1.0 + ((lev - vals[1]) / (vals[2] - vals[1]));
                return (gr);
            }
            gr = (double) i + ((lev - vals[i]) / (vals[i] - vals[i-1]));
            return (gr);
        } else {
            if (lev > vals[1]) {
                gr = 1.0 + ((lev - vals[1]) / (vals[2] - vals[1]));
                return (gr);
            }
            gr = (double) i + ((lev - vals[i]) / (vals[i] - vals[i-1]));
            return (gr);
        }
    }
    public static double gr2lev(double[] vals, double gr) {
        int i;
        if (gr < 1.0) return (vals[1] + (1.0 - gr) * (vals[1] - vals[2]));
        if (gr > vals[0]) {
            i = (int) (vals[0] + 0.1);
            return (vals[i] + (gr - vals[0]) * (vals[i] - vals[i-1]));
        }
        i = (int) gr;
        return (vals[i] + ((gr - (double) i) * (vals[i+1] - vals[i])));
    }
    
    public static double[]? cpscal(double[] vals, int lin, int dir, int dim) {
        int i, num;
        double[] vvv;
        

        if (dim < 0) {
            GaGx.gaprnt(0, "cpscal error:  dim is not >= 0 ");
            return (null);
        }
        if (dim == 3) {
            num = 8;
        } else {
            if (lin == 1) num = 3;
            else num = (int) (vals[0] + 0.5) + 5;
        }

        vvv = new double[num];
        for (i = 0; i < num; i++) {
            vvv[i] = vals[i];
        }
        return (vvv);
    }

}