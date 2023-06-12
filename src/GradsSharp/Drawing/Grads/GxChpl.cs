using System.Reflection;
using CSJ2K.j2k.image;

namespace GradsSharp.Drawing.Grads;

internal class GxChpl
{
    private DrawingContext _drawingContext;

    private byte[][] fch = new byte[10][]; /* Pointers to font data once it is read in */
    private int[][] foff = new int[10][]; /* Pointers to character offsets */
    private int[][] flen = new int[10][]; /* Pointers to character lengths */
    private int dfont = 15; /* Default font */

    public GxChpl(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public void gxchii()
    {
        int i;
        for (i = 0; i < 10; i++) fch[i] = Array.Empty<byte>();
        dfont = 0;
    }

    /* Change default font */

    public void gxchdf(int df)
    {
        if (df < 0 || df > 99) return;
        dfont = df;
    }

    public int gxqdf()
    {
        return dfont;
    }

    /* Plot character string */

    public void gxchpl(string chrs, int len, double x, double y, double height, double width, double angle)
    {
        double h, w, xoff, yoff, wact;
        int fn, supsub, nfn;

        fn = dfont;
        angle = angle * Math.PI / 180.0; /* convert angle from degrees to radians */
        supsub = 0;
        int idx = 0;
        while (idx < chrs.Length && len > 0)
        {
            while (chrs[idx] == '`')
            {
                if (chrs[idx + 1] >= '0' && chrs[idx + 1] <= '9')
                {
                    /* get 1-digit font number */
                    fn = (int)chrs[idx + 1] - 48;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'a')
                {
                    /* superscript */
                    supsub = 1;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'b')
                {
                    /* subscript */
                    supsub = 2;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'n')
                {
                    /* normal */
                    supsub = 0;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'f')
                {
                    /* get 2-digit font number */
                    if (len > 3)
                    {
                        nfn = 10 * ((int)chrs[idx + 2] - 48) + ((int)chrs[idx + 3] - 48);
                        if (nfn >= 0 && nfn < 100) fn = nfn;
                    }

                    chrs += 4;
                    len -= 4;
                }
                else break;
            }

            if (chrs[idx] != '\0' && len > 0)
            {
                if (supsub>0)
                {
                    /* adjust size and position for superscripts and subscripts */
                    h = height * 0.45;
                    w = width * 0.45;
                    if (supsub == 1) yoff = height * 0.58;
                    else yoff = -0.20 * height;
                }
                else
                {
                    h = height;
                    w = width;
                    yoff = 0.0;
                }

                xoff = yoff * Math.Sin(angle);
                yoff = yoff * Math.Cos(angle);

                /* plot the character */
                wact = _drawingContext.GradsDrawingInterface.gxdrawch(chrs[idx], fn, x - xoff, y + yoff, w, h, angle);
                if (wact < -900.0 && fn < 6)
                {
                    /* draw with Hershey font */
                    //TODO: hershey font
                    wact = gxchplc(chrs[idx], fn, x - xoff, y + yoff, w, h, angle);
                    if (wact < -900.0) return;
                }

                x = x + wact * Math.Cos(angle);
                y = y + wact * Math.Sin(angle);

                idx++;
                len--;
            }
        }
    }

/* Get actual width of a single character in the indicated Hershey font */

    double gxchqlc(char ccc, int fn, double width)
    {
        double xs, w;
        int cnt, ic, jc;
        byte[] cdat;
    
        xs = width / 21.0;
        cdat = gxchgc((int)ccc, fn, out cnt);
        if (cdat == null) return (-999.9);
        ic = (int)(cdat[3]) - 82;
        jc = (int)(cdat[4]) - 82;
        w = (double)(jc - ic) * xs * 1.05;
        return (w);
    }

/* plot a single char in the indicated hershey font with the indicated location, size, and angle, 
   and return the distance to advance after plotting */

    public double gxchplc(char ccc, int fn, double x, double y, double width, double height, double angle)
    {
        double xs, ys, w, d, xc, yc, ang, rx, ry;
        int i, ic, jc, cnt, ipen;
        byte[] cdat;
    
        xs = width / 21.0;
        ys = height / 22.0;
        cdat = gxchgc((int)ccc, fn, out cnt);
        if (cdat == null) return (-999.9);
        ic = (int)(cdat[3]) - 82;
        jc = (int)(cdat[4]) - 82;
        w = (double)(jc - ic) * xs * 1.05;
        d = GaUtil.hypot(w / 2.0, height * 0.42);
        ang = Math.Atan2(height * 0.42, w / 2.0) + angle;
        xc = x + d * Math.Cos(ang);
        yc = y + d * Math.Sin(ang);

        int cdatcnt = 5;
        
        ipen = 3;
        for (i = 1; i < cnt; i++)
        {
            ic = (int)cdat[cdatcnt];
            jc = (int)cdat[cdatcnt + 1];
            if (ic == 32) ipen = 3;
            else
            {
                ic = ic - 82;
                jc = jc - 82;
                rx = ((double)ic) * xs;
                ry = -1.0 * ((double)jc) * ys;
                if (rx == 0.0 && ry == 0.0)
                {
                    d = 0.0;
                    ang = 0.0;
                }
                else
                {
                    d = GaUtil.hypot(rx, ry);
                    ang = Math.Atan2(ry, rx) + angle;
                }
    
                rx = xc + d * Math.Cos(ang);
                ry = yc + d * Math.Sin(ang);
                if (ipen == 3) _drawingContext.GradsDrawingInterface.MoveToPoint(rx, ry);
                else _drawingContext.GradsDrawingInterface.DrawLineToPoint(rx, ry);
                ipen = 2;
            }
    
            cdatcnt += 2;
        }
    
        return (w);
    }

/* Determine the length of a character string without plotting it. */

    public int gxchln(string chrs, int len, double width, out double wret)
    {
        double w, wact, cw;
        int fn, supsub, nfn;

        fn = dfont;
        supsub = 0;
        cw = 0;
        int idx = 0;
        while (idx < chrs.Length && len > 0)
        {
            while (chrs[idx] == '`')
            {
                if (chrs[idx + 1] >= '0' && chrs[idx + 1] <= '9')
                {
                    fn = (int)chrs[idx + 1] - 48;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'a')
                {
                    supsub = 1;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'b')
                {
                    supsub = 2;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'n')
                {
                    supsub = 0;
                    chrs += 2;
                    len -= 2;
                }
                else if (chrs[idx + 1] == 'f')
                {
                    if (len > 3)
                    {
                        nfn = 10 * ((int)chrs[idx + 2] - 48) + ((int)chrs[idx + 3] - 48);
                        if (nfn >= 0 && nfn < 100) fn = nfn;
                    }

                    chrs += 4;
                    len -= 4;
                }
                else break;
            }

            if (chrs[idx] != '\0' && len > 0)
            {
                if (supsub>0)
                {
                    w = width * 0.45;
                }
                else
                {
                    w = width;
                }

                /* First see if the rendering back end wants to plot this, or punt.
                   If it wants to punt, we get a -999 back, so we use Hershey instead  */

                wact = _drawingContext.GradsDrawingInterface.gxqchl(chrs[idx], fn, w);
                if (wact < -900.0)
                {
                    //TODO: Hershey font
                    wact = gxchqlc(chrs[idx], fn, w);
                }

                cw = cw + wact;
                idx++;
                len--;
            }
        }

        wret = cw;
        return (int)(cw);
    }

/* Get location and length of particular character info
   for particular font */

    byte[]? gxchgc(int ch, int fn, out int cnt)
    {
        int[] clen, coff;
        int rc;
        byte[] fdat;
    
        if (fch[fn].Length == 0 )
        {
            rc = gxchrd(fn);
            cnt = 0;
            if (rc>0) return (null);
        }
    
        clen = flen[fn];
        coff = foff[fn];
        fdat = fch[fn];
        if (ch < 32 || ch > 127) ch = 32;
        ch = ch - 32;
        cnt = clen[ch];
        return fdat.Skip(coff[ch]).ToArray();
    }

/* Read in a font file */

    int gxchrd(int fn)
    {
        Stream ifile;
        int i, j, rc, tlen;
        int[] coff, clen;
        bool flag;
        byte[] fdat;
        
        string buff = $"font{fn}.dat";
        
        
        string fname = "GradsSharp.Data." + buff;
        ifile = Assembly.GetExecutingAssembly().GetManifestResourceStream(fname) ??
                throw new FileNotFoundException(fname);;

        tlen = (int)ifile.Length;
        
        fdat = new byte[tlen + 1];
        
        coff = new int[95];
        clen = new int[95];

        rc = ifile.Read(fdat, 0, tlen);
        if (rc != tlen)
        {
            throw new IOException("Error reading font data: I/O Error");
            return (1);
        }
        
        fdat[tlen] = 0;
        
        /* Determine the locations of the start of each character */
        
        i = 0;
        j = 1;
        coff[0] = 0;
        flag = false;
        while (fdat[i]!=0)
        {
            if (fdat[i] < ' ')
            {
                flag = true;
            }
            else
            {
                if (flag)
                {
                    coff[j] = i;
                    j++;
                }
        
                flag = false;
            }
        
            i++;
        }
        
        /* Determine the count on each character */
        
        for (i = 0; i < 95; i++)
        {
            char[] bbuff = new char[3];
            for (j = 0; j < 3; j++) bbuff[j] = (char)fdat[coff[i] + j];
            String s = new string(bbuff);
            
            clen[i] = Convert.ToInt32(s);
        }
        
        flen[fn] = clen;
        foff[fn] = coff;
        fch[fn] = fdat;

        return (0);
    }

    void gxchplo(string chrs, int len, double x, double y, double height, double width, double angle)
    {
        // double xc, yc, xscl, yscl, xs, ys, w, d, ang, rx, ry, yoff;
        // int i, fn, ic, jc, cnt, ipen, supsub;
        // char* cdat;
        //
        // xscl = width / 21.0;
        // yscl = height / 22.0;
        // fn = dfont;
        // angle = angle * 3.1416 / 180.0;
        // supsub = 0;
        // int idx = 0;
        // while (chrs[idx] != '\0' && len > 0)
        // {
        //     while (chrs[idx] == '`')
        //     {
        //         if (chrs[idx + 1] >= '0' && chrs[idx + 1] <= '9')
        //         {
        //             fn = (int)chrs[idx + 1] - 48;
        //             chrs += 2;
        //             len -= 2;
        //         }
        //         else if (chrs[idx + 1] == 'a')
        //         {
        //             supsub = 1;
        //             chrs += 2;
        //             len -= 2;
        //         }
        //         else if (chrs[idx + 1] == 'b')
        //         {
        //             supsub = 2;
        //             chrs += 2;
        //             len -= 2;
        //         }
        //         else if (chrs[idx + 1] == 'n')
        //         {
        //             supsub = 0;
        //             chrs += 2;
        //             len -= 2;
        //         }
        //         else break;
        //     }
        //
        //     if (chrs[idx] != '\0' && len > 0)
        //     {
        //         if (angle == 0.0)
        //         {
        //             /* Fast path for ang=0 */
        //             cdat = gxchgc((int)*(chrs), fn, &cnt);
        //             if (cdat == NULL) return;
        //             ic = (int)(*(cdat + 3)) - 82;
        //             jc = (int)(*(cdat + 4)) - 82;
        //             if (supsub)
        //             {
        //                 xs = xscl * 0.45;
        //                 ys = yscl * 0.45;
        //                 if (supsub == 1) yoff = height * 0.35;
        //                 else yoff = -1.0 * height * 0.42;
        //             }
        //             else
        //             {
        //                 xs = xscl;
        //                 ys = yscl;
        //                 yoff = 0.0;
        //             }
        //
        //             w = (double)(jc - ic) * xs * 1.05;
        //             xc = x + w / 2.0;
        //             yc = y + height * 0.42 + yoff;
        //             cdat += 5;
        //             ipen = 3;
        //             for (i = 1; i < cnt; i++)
        //             {
        //                 ic = (int)*cdat;
        //                 jc = (int)*(cdat + 1);
        //                 if (ic == 32) ipen = 3;
        //                 else
        //                 {
        //                     ic = ic - 82;
        //                     jc = jc - 82;
        //                     rx = xc + ((double)ic) * xs;
        //                     ry = yc - ((double)jc) * ys;
        //                     if (ipen == 3) _drawingContext.GaSubs.gxmove(rx, ry);
        //                     else _drawingContext.GaSubs.gxdraw(rx, ry);
        //                     ipen = 2;
        //                 }
        //
        //                 cdat += 2;
        //             }
        //
        //             x = x + w;
        //             idx++;
        //             len--;
        //         }
        //         else
        //         {
        //             cdat = gxchgc((int)*(chrs), fn, &cnt);
        //             if (cdat == NULL) return;
        //             ic = (int)(*(cdat + 3)) - 82;
        //             jc = (int)(*(cdat + 4)) - 82;
        //             if (supsub)
        //             {
        //                 xs = xscl * 0.45;
        //                 ys = yscl * 0.45;
        //                 if (supsub == 1) yoff = height * 0.35;
        //                 else yoff = -1.0 * height * 0.42;
        //             }
        //             else
        //             {
        //                 xs = xscl;
        //                 ys = yscl;
        //                 yoff = 0.0;
        //             }
        //
        //             w = (double)(jc - ic) * xs * 1.05;
        //             d = GaUtil.hypot(w / 2.0, height * 0.42 + yoff);
        //             ang = Math.Atan2(height * 0.42 + yoff, w / 2.0) + angle;
        //             xc = x + d * Math.Cos(ang);
        //             yc = y + d * Math.Sin(ang);
        //             cdat += 5;
        //             ipen = 3;
        //             for (i = 1; i < cnt; i++)
        //             {
        //                 ic = (int)*cdat;
        //                 jc = (int)*(cdat + 1);
        //                 if (ic == 32) ipen = 3;
        //                 else
        //                 {
        //                     ic = ic - 82;
        //                     jc = jc - 82;
        //                     rx = ((double)ic) * xs;
        //                     ry = -1.0 * ((double)jc) * ys;
        //                     if (rx == 0.0 && ry == 0.0)
        //                     {
        //                         d = 0.0;
        //                         ang = 0.0;
        //                     }
        //                     else
        //                     {
        //                         d = GaUtil.hypot(rx, ry);
        //                         ang = Math.Atan2(ry, rx) + angle;
        //                     }
        //
        //                     rx = xc + d * Math.Cos(ang);
        //                     ry = yc + d * Math.Sin(ang);
        //                     if (ipen == 3) _drawingContext.GaSubs.gxmove(rx, ry);
        //                     else _drawingContext.GaSubs.gxdraw(rx, ry);
        //                     ipen = 2;
        //                 }
        //
        //                 cdat += 2;
        //             }
        //
        //             x = x + w * Math.Cos(angle);
        //             y = y + w * Math.Sin(angle);
        //             ifx++;
        //             len--;
        //         }
        //     } 
        // }
    }
}