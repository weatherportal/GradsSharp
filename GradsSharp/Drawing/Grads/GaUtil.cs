namespace GradsSharp.Drawing.Grads;

internal class GaUtil
{
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

    static int[] mnacum = {0, 0, 44640, 84960, 129600, 172800, 217440,
        260640, 305280, 349920, 393120, 437760, 480960};
    static int[] mnacul = {0, 0, 44640, 86400, 131040, 174240, 218880,
        262080, 306720, 351360, 394560, 439200, 482400};
    
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

    public static void gamnmx(gagrid pgr) {
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
        int cntrmask=0;
        int cntr=0;
        
        for (i = 0; i < size; i++) {
            if (rmask[cntrmask] == 1) {
                cnt++;
                if (pgr.rmin > r[cntr]) {
                    pgr.rmin = r[cntr];
                }
                if (pgr.rmax < r[cntr]) pgr.rmax = r[cntr];
            }
            cntr++;
            cntrmask++;
        }
        if (cnt == 0 || pgr.rmin == 9.99e35 || pgr.rmax == -9.99e35) {
            pgr.rmin = pgr.undef;
            pgr.rmax = pgr.undef;
            pgr.umin = pgr.umax = 0;
        } else {
            pgr.umin = pgr.umax = 1;
        }
    }
    
    public static long timdif(dt dtim1, dt dtim2,  bool flag) {
        long min1, min2, mon1, mon2, yr;
        dt temp = new();
        bool swap;
        long mo1, mo2;

        swap = false;
        if (dtim1.yr > dtim2.yr) {
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
        while (yr < dtim2.yr) {
            if (qleap(yr)) min2 += 527040L;
            else min2 += 525600L;
            mon2 += 12;
            yr++;
        }

        if (flag) {
            /* return months */
            mon1 = dtim1.mo;
            mon2 += dtim2.mo;
            if (swap) return (mon1 - mon2);
            else return (mon2 - mon1);
        } else {
            /* return minutes */
            mo1 = dtim1.mo;
            mo2 = dtim2.mo;
            if (qleap(dtim1.yr)) {
                min1 = min1 + mnacul[mo1] + (dtim1.dy * 1440L) + (dtim1.hr * 60L) + dtim1.mn;
            } else {
                min1 = min1 + mnacum[mo1] + (dtim1.dy * 1440L) + (dtim1.hr * 60L) + dtim1.mn;
            }
            if (qleap(dtim2.yr)) {
                min2 = min2 + mnacul[mo2] + (dtim2.dy * 1440L) + (dtim2.hr * 60L) + dtim2.mn;
            } else {
                min2 = min2 + mnacum[mo2] + (dtim2.dy * 1440L) + (dtim2.hr * 60L) + dtim2.mn;
            }
            if (swap) return (min1 - min2);
            else return (min2 - min1);
        }
    }
    
    public static double t2gr(double[] vals, dt dtim) {
        dt stim = new ();
        long eyear, mins, mons;
        double val, moincr, mnincr, rdiff;

        /* Get constants associated with this conversion                   */

        stim.yr = (int) (vals[0] + 0.1);
        stim.mo = (int) (vals[1] + 0.1);
        stim.dy = (int) (vals[2] + 0.1);
        stim.hr = (int) (vals[3] + 0.1);
        stim.mn = (int) (vals[4] + 0.1);

        moincr = vals[5];
        mnincr = vals[6];

        /* If the increment for this conversion is days, hours, or minutes,
         then we do our calculations in minutes.  If the increment is
         months or years, we do our calculations in months.              */


        if (mnincr > 0.1) {
            mins = timdif(stim, dtim, false);
            rdiff = (double) mins;
            val = rdiff / (mnincr);
            val += 1.0;
            return (val);
        } else {
            mons = timdif(stim, dtim, true);
            eyear = stim.yr;
            if (stim.yr > dtim.yr) eyear = dtim.yr;
            rdiff = (((dtim.yr - eyear) * 12) + dtim.mo) -
                    (((stim.yr - eyear) * 12) + stim.mo);
            stim.yr = dtim.yr;
            stim.mo = dtim.mo;
            mins = timdif(stim, dtim, false);
            if (mins > 0) {
                if (dtim.mo == 2 && qleap(dtim.yr)) {
                    rdiff = rdiff + (((double) mins) / 41760.0);
                } else {
                    rdiff = rdiff + (((double) mins) / ((double) momn[dtim.mo]));
                }
            }
            val = rdiff / (moincr);
            val += 1.0;
            return (val);
        }
    }
    public static  double liconv (double val1, double val2, double v) {
        return ( (val1 * v) + val2);
    }
}