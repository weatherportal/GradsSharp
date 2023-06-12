namespace GradsSharp.Drawing.Grads;

/* Struct to form linked list for the meta buffer */
/* The buffer area is allocated as float, to insure at least four bytes
   per element.  In some cases, ints and chars will get stuffed into a 
   float (via pointer casting).  */
/* Don't use gafloat or int for meta buffer stuffing.  */

class gxmbuf
{
    public gxmbuf? fpmbuf; /* forward pointer */
    public float[] buff; /* Buffer area */
    public int len; /* Length of Buffer area */
    public int used; /* Amount of buffer used */
};

internal class GxMeta
{
    private DrawingContext _drawingContext;
    private const int BWORKSZ = 250000;

    private gxmbuf? mbufanch;
    private gxmbuf? mbuflast;
    private gxmbuf? mbufanch2;
    private gxmbuf? mbuflast2;
    private int dbmode;
    private int mbuferror = 0; /* Indicate an error state; suspends buffering */

    public GxMeta(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public void gxhnew(double xsiz, double ysiz, int hbufsz)
    {
        int rc;
        mbufanch = null;
        mbuflast = null;
        mbuferror = 0;

        rc = mbufget();
        if (rc > 0)
        {
            throw new Exception("Error in gx initialization: Unable to allocate meta buffer\n");
            mbuferror = 99;
        }
    }


/* Add command with 0 args to metafile buffer */

    public void hout0(int cmd)
    {
        int rc;
        int ch;
        if (mbuferror>0) return;
        if (mbuflast.len - mbuflast.used < 3)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.used+=2;
    }

/* Add a command with one small integer argument to the metafile buffer.
   The argument is assumed to fit into a signed char (-127 to 128).  */

    public void hout1c(int cmd, int opt)
    {
        int rc;
        int ch;
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 4)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.buff[ch + 2] = opt;
        mbuflast.used+=3;
    }

/* Add command with one integer argument to metafile buffer */

    public void hout1(int cmd, int opt)
    {
        int rc;
        int ch;
        int iii;
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 4)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.buff[ch + 2] = opt;
        mbuflast.used+=3;
    }

/* Metafile buffer, command plus two double args */

    public void hout2(int cmd, double x, double y)
    {
        int rc;
        int ch;
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 5)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.buff[ch + 2] = (float)x;
        mbuflast.buff[ch + 3] = (float)y;
        mbuflast.used+=4;
    }

/* Metafile buffer, command plus two integer args */

    public void hout2i(int cmd, int i1, int i2)
    {
        int rc;
        int ch;
        
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 5)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.buff[ch + 2] = i1;
        mbuflast.buff[ch + 3] = i2;
        mbuflast.used+=4;
    }

/* Metafile buffer, command plus three integer args */

    public void hout3i(int cmd, int i1, int i2, int i3)
    {
        int rc;
        int ch;
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 6)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.buff[ch + 2] = i1;
        mbuflast.buff[ch + 3] = i2;
        mbuflast.buff[ch + 4] = i3;
        mbuflast.used+=5;
    }

/* Metafile buffer, command plus four integer args */

    public void hout5i(int cmd, int i1, int i2, int i3, int i4, int i5)
    {
        int rc;
        int ch;
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 8)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.buff[ch + 2] = i1;
        mbuflast.buff[ch + 3] = i2;
        mbuflast.buff[ch + 4] = i3;
        mbuflast.buff[ch + 5] = i4;
        mbuflast.buff[ch + 6] = i5;
        mbuflast.used+=7;
    }

/* Metafile buffer, command plus four double args */

    public void hout4(int cmd, double xl, double xh, double yl, double yh)
    {
        int rc;
        int ch;
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 7)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ch = mbuflast.used;
        mbuflast.buff[ch] = 99;;
        mbuflast.buff[ch + 1] = cmd;
        mbuflast.buff[ch + 2] = (float)xl;
        mbuflast.buff[ch + 3] = (float)xh;
        mbuflast.buff[ch + 4] = (float)yl;
        mbuflast.buff[ch + 5] = (float)yh;
        mbuflast.used+=6;
        
    }

/* Add a single character to the metafile buffer, along with the font number (less
   than 100), location (x,y), and size/rotation specs (4 floats).  Uses -21 as a
   cmd value.   */

    public void houtch(char ch, int fn, double x, double y,
        double w, double h, double ang)
    {
        int rc, ccc;
        
        if (mbuferror > 0) return;
        if (mbuflast.len - mbuflast.used < 8)
        {
            rc = mbufget();
            if (rc > 0)
            {
                gxmbuferr();
                return;
            }
        }

        ccc = mbuflast.used;
        mbuflast.buff[ccc] = 99;
        mbuflast.buff[ccc + 1] = -21;
        mbuflast.buff[ccc + 2] = ch;
        mbuflast.buff[ccc + 3] = fn;
        mbuflast.buff[ccc + 4] = (float)x;
        mbuflast.buff[ccc + 5] = (float)y;
        mbuflast.buff[ccc + 6] = (float)w;
        mbuflast.buff[ccc + 7] = (float)h;
        mbuflast.buff[ccc + 8] = (float)ang;
        mbuflast.used+=9;
    }

/* User has issued a clear.  
   This may also indicate the start or end of double buffering.  
   If we are not double buffering, just free up the memory buffer and return.  
   If we are starting up double buffering, we need another buffer chain.  
   If we are ending double buffering, all memory needs to be released.  
   If we are in the midst of double buffering, do a "swap" and free the foreground buffer. 

   Values for action are:
      0 -- new frame (clear display), wait before clearing.
      1 -- new frame, no wait.
      2 -- New frame in double buffer mode.  If not supported
           has same result as action=1.  Usage involves multiple
           calls with action=2 to obtain an animation effect.  
      7 -- new frame, but just clear graphics.  Do not clear  
           event queue; redraw buttons. 
      8 -- clear only the event queue.
      9 -- clear only the X request buffer
*/

    public void gxhfrm(int iact)
    {
        gxmbuf pmbuf, pmbufl;

        /* Start up double buffering */
        if (iact == 2 && dbmode == 0)
        {
            mbufrel(1);
            if (mbufanch == null) mbufget();
            mbufanch2 = mbufanch;
            mbuflast2 = mbuflast;
            mbufanch = null;
            mbuflast = null;
            mbufget();
            dbmode = 1;
        }

        /* End of double buffering */
        if (iact != 2 && dbmode == 1)
        {
            mbufrel(0);
            mbufanch = mbufanch2;
            mbufrel(1);
            dbmode = 0;
            mbuferror = 0;
            return;
        }

        /* If double buffering, swap buffers */
        if (dbmode>0)
        {
            var tmp = mbufanch; /* Save pointer to background buffer */
            pmbufl = mbuflast;
            mbufanch = mbufanch2;
            mbufrel(1); /* Get rid of former foreground buffer */
            
            mbufanch2 = tmp; /* Set foreground to former background */
            mbuflast2 = pmbufl;
        }
        else
        {
            /* Not double buffering, so just free buffers */
            mbufrel(1);
        }

        if (dbmode == 0) mbuferror = 0; /* Reset error state on clear command */
    }


/* Redraw based on contents of current buffers.  Items that persist from plot
   to plot ARE NOT IN THE META BUFFER; these items are set in the hardware attribute
   database and are queried by the backend. 

   This routine is called from gxX (ie, a lower level of the backend rendering), 
   and this routine calls back into gxX.  This is not, however, implemented as
   true recursion -- events are disabled in gxX during this redraw, so addtional
   levels of recursion are not allowed.  

   If dbflg, draw from the background buffer.  Otherwise draw from the 
   foreground buffer. */

    public void gxhdrw(int dbflg, int pflg)
    {
        gxmbuf? pmbuf;
        float[] buff;
        int iii;
        double r, s, x, y, w, h, ang;
        double[] xybuf = Array.Empty<double>();
        int ppp, cmd, op1, op2, op3, op4, op5, fflag, xyc = 0, fn, sig;
        int ch;
        char ccc;

        if (dbflg == 1 && dbmode==0)
        {
            Console.WriteLine("Logic error 0 in Redraw.  Contact Developer.\n");
            return;
        }

        var psubs = _drawingContext.GradsDrawingInterface.DrawingEngine;

        
        if (dbflg == 1) pmbuf = mbufanch2;
        else pmbuf = mbufanch;

        fflag = 0;
        
        while (pmbuf != null)
        {
            ppp = 0;
            while (ppp < pmbuf.used)
            {
                /* Get message type */

                ch = ppp;
                cmd = (int)(pmbuf.buff[ch]);
                if (cmd != 99)
                {
                    Console.WriteLine("Metafile buffer is corrupted");
                    Console.WriteLine("Unable to complete redraw and/or print operation");
                    
                    return;
                }

                cmd = (int)(pmbuf.buff[ch + 1]);
                ppp+=2;


                /* Handle various message types */
                /* -9 is end of file.  Should not happen. */

                if (cmd == -9)
                {
                    Console.WriteLine("Logic Error 4 in Redraw.  Notify Developer\n");
                    return;
                }
                /*  -1 indicates start of file.  Should not occur. */

                else if (cmd == -1)
                {
                    Console.WriteLine("Logic Error 8 in Redraw.  Notify Developer\n");
                    return;
                }
                /* -2 indicates new frame.  Also should not occur */

                else if (cmd == -2)
                {
                    Console.WriteLine("Logic Error 12 in Redraw.  Notify Developer\n");
                    return;
                }
                /* -3 indicates new color.  One arg; color number.  */

                else if (cmd == -3)
                {
                    iii = ppp;
                    op1 = (int)(pmbuf.buff[iii]);
                    if (pflg > 0)
                        psubs.gxpcol(op1); /* for printing */
                    ppp++;
                }
                /* -4 indicates new line thickness.  It has two arguments */

                else if (cmd == -4)
                {
                    iii = ppp;
                    op1 = (int)(pmbuf.buff[iii]);
                    if (pflg > 0)
                        psubs.gxpwid(op1); /* for printing */
                    ppp += 2;
                }
                /*  -5 defines a new color, in rgb.  It has five int args */

                else if (cmd == -5)
                {
                    iii = ppp;
                    op1 = (int)(pmbuf.buff[iii]);
                    iii = ppp + 1;
                    op2 = (int)(pmbuf.buff[iii]);
                    iii = ppp + 2;
                    op3 = (int)(pmbuf.buff[iii]);
                    iii = ppp + 3;
                    op4 = (int)(pmbuf.buff[iii]);
                    iii = ppp + 4;
                    op5 = (int)(pmbuf.buff[iii]);
                    _drawingContext.GradsDatabase.gxdbacol(op1, op2, op3, op4, op5); /* update the data base */
                    if (pflg > 0)
                        psubs.gxpacol(op1); /* for printing (no-op for cairo) */
                    ppp += 5;
                }
                /* -6 is for a filled rectangle.  It has four args. */

                else if (cmd == -6)
                {
                    iii = ppp;
                    r = (double)(pmbuf.buff[iii++]);
                    s = (double)(pmbuf.buff[iii++]);
                    x = (double)(pmbuf.buff[iii++]);
                    y = (double)(pmbuf.buff[iii]);
                    if (pflg > 0)
                        psubs.gxprec(r, s, x, y); /* for printing */
                    ppp += 4;
                }
                /* -7 indicates the start of a polygon fill.  It has one arg, 
                   the length of the polygon.  We allocate an array for the entire
                   polygon, so we can present it to the hardware backend in 
                   on piece. */

                else if (cmd == -7)
                {
                    iii = ppp;
                    op1 = (int)(pmbuf.buff[iii]);
                    xybuf = new double[op1 * 2];
                    xyc = 0;
                    fflag = 1;
                    ppp += 1;
                    /* tell printing layer about new polygon. */
                    if (pflg > 0) psubs.gxpbpoly();
                }
                /* -8 is to terminate polygon fill.  It has no args */

                else if (cmd == -8)
                {
                    if (xybuf.Length == 0)
                    {
                        Console.WriteLine("Logic Error 16 in Redraw.  Notify Developer\n");
                        return;
                    }

                    if (pflg > 0)
                        psubs.gxpepoly(xybuf, xyc); /* for printing */
                   
                    
                    xybuf = Array.Empty<double>();
                    fflag = 0;
                }
                /* -10 is a move to instruction.  It has two double args */

                else if (cmd == -10)
                {
                    iii = ppp;
                    x = (double)(pmbuf.buff[iii++]);
                    y = (double)(pmbuf.buff[iii++]);
                    if (fflag>0)
                    {
                        xybuf[xyc * 2] = x;
                        xybuf[xyc * 2 + 1] = y;
                        xyc++;
                    }

                    if (pflg > 0)
                        psubs.gxpmov(x, y); /* for printing */
                    ppp += 2;
                }
                /*  -11 is draw to.  It has two double args. */

                else if (cmd == -11)
                {
                    iii = ppp;
                    x = (double)(pmbuf.buff[iii++]);
                    y = (double)(pmbuf.buff[iii++]);
                    if (fflag>0)
                    {
                        xybuf[xyc * 2] = x;
                        xybuf[xyc * 2 + 1] = y;
                        xyc++;
                    }

                    if (pflg > 0)
                        psubs.gxpdrw(x, y); /* for printing */
                    ppp += 2;
                }
                /* -12 indicates new fill pattern.  It has three arguments. */

                else if (cmd == -12)
                {
                    /* This is a no-op for cairo; X-based pattern drawing */
                    iii = ppp;
                    //dsubs.gxdptn((int)*(buff + 0), (int)*(buff + 1), (int)*(buff + 2));
                    if (pflg > 0)
                        psubs.gxpflush(1);
                    ppp += 3;
                }
                /* -20 is a draw widget.  We will redraw it in current state. */

                else if (cmd == -20)
                {
                    /* This is a no-op for cairo; X-based buttonwidget drawing */
                    iii = ppp;
                    //dsubs.gxdpbn((int)*(buff + 0), NULL, 1, 0, -1);
                    if (pflg > 0)
                        psubs.gxpflush(1);
                    ppp += 1;
                }
                /* -21 is for drawing a single character in the indicated font and size */

                else if (cmd == -21)
                {
                    ch = ppp;
                    ccc = (char)pmbuf.buff[ch];
                    fn = (int)(pmbuf.buff[ch + 1]);
                    x = (double)(pmbuf.buff[ch + 2]);
                    y = (double)(pmbuf.buff[ch + 3]);
                    w = (double)(pmbuf.buff[ch + 4]);
                    h = (double)(pmbuf.buff[ch + 5]);
                    ang = (double)(pmbuf.buff[ch + 6]);
                    if (pflg > 0)
                        r = psubs.gxpch(ccc, fn, x, y, w, h, ang); /* print a character */
                    
                    ppp += 7;
                }
                /* -22 is for a signal. It has one signed character argument */

                else if (cmd == -22)
                {
                    ch = ppp;
                    sig = (int)(pmbuf.buff[ch]);
                    if (pflg > 0)
                        psubs.gxpsignal(sig);
                    ppp++;
                }
                /* -23 is for the clipping area. It has four args. */

                else if (cmd == -23)
                {
                    iii = ppp;
                    r = (double)(pmbuf.buff[iii++]);
                    s = (double)(pmbuf.buff[iii++]);
                    x = (double)(pmbuf.buff[iii++]);
                    y = (double)(pmbuf.buff[iii++]);
                    if (pflg > 0)
                        psubs.gxpclip(r, s, x, y); /* for printing */
                    ppp += 4;
                }
                /* Any other command would be invalid */

                else
                {
                    Console.WriteLine("Logic Error 20 in Redraw.  Notify Developer\n");
                    return;
                }
            }

            if (pmbuf == mbuflast) break;
            pmbuf = pmbuf.fpmbuf;
        }

        /* tell hardware and printing layer we are finished */
        if (pflg > 0) psubs.gxpflush(1);
        //dsubs.gxdopt(4);
    }


/* Allocate and chain another buffer area */

    int mbufget()
    {
        gxmbuf pmbuf;

        if (mbufanch == null)
        {
            pmbuf = new gxmbuf();
            mbufanch = pmbuf;
            mbuflast = pmbuf; /* ... and also as the last one */
            pmbuf.buff = new float[BWORKSZ]; /* allocate a buffer */
            pmbuf.len = BWORKSZ; /* set the buffer length */
            pmbuf.used = 0; /* initialize the buffer as unused */
        }
        else
        {
             
            if (mbuflast.fpmbuf == null)
            {
                /* no more buffers in the chain */
                pmbuf = new gxmbuf();
                mbuflast.fpmbuf = pmbuf;
                mbuflast = pmbuf; /* reset mbuflast to the newest buffer structure in the chain */
                pmbuf.buff = new float[BWORKSZ];
                pmbuf.len = BWORKSZ; /* set the buffer length */
                pmbuf.used = 0; /* initialize the buffer as unused */
                pmbuf.fpmbuf = null;
            }
            else
            {
                /* we'll just re-use what's already been chained up */
                pmbuf = mbuflast.fpmbuf; /* get the next buffer in the chain */
                pmbuf.used = 0; /* reset this buffer to unused */
                mbuflast = pmbuf; /* set mbuflast to point to this buffer */
            }
        }

        return (0);
    }

/* Free buffer chain.  
   If flag is 1, leave allocated buffers alone and mark the anchor as unused 
   If flag is 0, free all buffers, including the anchor
*/

    void mbufrel(int flag)
    {
        int i;

        i = flag;
        var pmbuf = mbufanch;

        while (pmbuf != null)
        {
            if (i == 0)
            {
                pmbuf.buff = new float[BWORKSZ];
            }

            pmbuf = pmbuf.fpmbuf;
        }
        
        if (flag == 0)
        {
            mbufanch = null; /* no more metabuffer */
        }
        else
        {
            if (mbufanch != null) mbufanch.used = 0;
        }

        mbuflast = mbufanch;
    }

    void gxmbuferr()
    {
        Console.WriteLine("Error in gxmeta: Unable to allocate meta buffer\n");
        Console.WriteLine("                 Buffering for the current plot is disabled\n");
        mbuferror = 1;
        mbufrel(0);
    }
}