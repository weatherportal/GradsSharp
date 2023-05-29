using GradsSharp.Drawing;
using GradsSharp.Models;

namespace GradsSharp.Functions;

internal class ColorBar
{
    private DrawingContext _context;
    public ColorBar(ColorBarSettings settings, DrawingContext context)
    {
        Settings = settings;
        _context = context;
    }

    public ColorBarSettings Settings { get; private set; }

    public void DrawColorBar()
    {

        var pcm = _context.CommonData; 
        
        if (pcm.shdcnt < 1)
        {
            throw new Exception("No shading information.  Drawn a chart yet?");
        }

        int cnum = pcm.shdcnt;

        int i = 1;
        int[] col = new int[3600];
        double[] hi = new double[3600];
        while (i < cnum)
        {
            col[i] = pcm.shdcls[i - 1];
            hi[i] = pcm.shdlvs[i - 1];
            i++;
        }
        
        // Console.WriteLine(String.Join(",", col.Take(cnum)));
        // Console.WriteLine(String.Join(",", hi.Take(cnum)));

        double xmin = Settings.XMin + Settings.XOffset;
        double xmax = Settings.XMax + Settings.XOffset;
        double xmid = (xmax + xmin) / 2;
        double ymin = Settings.YMin + Settings.YOffset;
        double ymax = Settings.YMax + Settings.YOffset;
        double ymid = (ymax + ymin) / 2;

        if (Settings.Direction != ColorBarDirection.Horizontal && Settings.Direction != ColorBarDirection.Vertical)
        {
            if (xmax - xmin >= ymax - ymin)
                Settings.Direction = ColorBarDirection.Horizontal;
            else
                Settings.Direction = ColorBarDirection.Vertical;
        }

        int xdir = 0;
        int ydir = 0;

        if (Settings.Direction == ColorBarDirection.Horizontal)
        {
            xdir = 1;
            ydir = 0;
        }
        else {
            xdir = 0;
            ydir = 1;
        }

        double fwidth = Settings.FontWidth;
        double fheight = Settings.FontHeight;
        double ftickness = fheight * 40;
        int fstep = (int)Settings.LabelInterval;
        int foffset = 0;
        if (Settings.LabelOffset == "center")
        {
            if ((cnum % 2) == 0)
            {
                foffset = (cnum / 2 - 1) % fstep;
            }
            else
            {
                foffset = ((cnum +1) / 2 - 1) % fstep;
            }
        }

        int fcolor = Settings.LabelColorNumber;
        OnOffSetting line = Settings.Lines;
        int linecolor = Settings.LineColor;

        string edge = "box";
        
        double xdif = xdir * (xmax - xmin) / cnum;
        double ydif = ydir * (ymax - ymin) / cnum;

        i = 1;
        double x1 = xmin - xdif;
        double x2 = xmin * xdir + xmax * ydir;
        double y1 = ymin - ydif;
        double y2 = ymin * ydir + ymax * xdir;

        double fxoffset = Settings.LabelXOffset;
        double fyoffset = Settings.LabelXOffset;
        
        int maxstr = 0;

        while (i < cnum)
        {
            x1 = x1 + xdif;
            x2 = x2 + xdif;
            y1 = y1 + ydif;
            y2 = y2 + ydif;
            double xmoji = x2 + (0.5 * fwidth) * ydir + fxoffset;
            double ymoji = (y1 - 0.5 * fheight) * xdir + y2 * ydir + fyoffset;
            
            _context.GaSubs.gxcolr(col[i]);
            _context.GaUser.SetStringSize(fwidth, fheight);
            
            if (edge == "box")
            {
                _context.GaSubs.gxrecf(x1, x2, y1, y2);
                if (line == OnOffSetting.On)
                {
                    drawrec(linecolor, x1, y1, x2, y2); 
                }
            }

            if (i != cnum && i - Settings.LabelInterval > 0 &&
                Convert.ToInt32((i - 1 - foffset) / fstep) * fstep == i - 1 - foffset)
            {
                if (Settings.Direction == ColorBarDirection.Horizontal)
                {
                    _context.GaUser.SetStringOptions(fcolor, StringJustification.TopCenter, (int)ftickness, 0);
                }
                else
                {
                    _context.GaUser.SetStringOptions(fcolor, StringJustification.Left, (int)ftickness, 0);
                }

                var lev = hi[i];
                var lbl = lev.ToString();
                if (!String.IsNullOrEmpty(Settings.LabelFormat))
                {
                    lbl = lev.ToString(Settings.LabelFormat);
                }

                _context.GaUser.DrawString(xmoji, ymoji, lbl);
            }
            
            
            i++;
        }

        
    }


    void drawrec(int linecolor, double xmin, double ymin, double xmax, double ymax)
    {
        drawpoly(linecolor, xmin, ymin, xmin, ymax, xmax, ymax, xmax, ymin);
    }




    void drawpoly(params object[] args)
    {
        int linecolor = Convert.ToInt32(args[0]);
        double xstart = Convert.ToDouble(args[1]);
        double ystart = Convert.ToDouble(args[2]);
        double xmin = xstart;
        double ymin = ystart;

        int i = 3;
        while (true)
        {
            double xmax = Convert.ToDouble(args[i]);
            double ymax = Convert.ToDouble(args[i+1]);
         
            _context.GaUser.SetCThick(1);
            _context.GaSubs.gxcolr(linecolor);
            _context.GaSubs.gxmove(xmin, ymin);
            _context.GaSubs.gxdraw(xmax, ymax);
            xmin = xmax;
            ymin = ymax;
            
            i += 2;
            if (i >= args.Length) break;
        }
        
        _context.GaUser.SetCThick(1);
        _context.GaSubs.gxcolr(linecolor);
        _context.GaSubs.gxmove(xmin, ymin);
        _context.GaSubs.gxdraw(xstart, ystart);
    }
}