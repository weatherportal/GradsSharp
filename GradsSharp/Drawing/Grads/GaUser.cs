using System.Diagnostics;
using GradsSharp.Data;
using GradsSharp.Models;
using Microsoft.VisualBasic;
using NGrib;
using NGrib.Grib1;
using NGrib.Grib2.Templates.GridDefinitions;
using NGrib.Grib2.Templates.ProductDefinitions;

namespace GradsSharp.Drawing.Grads;

internal class GaUser
{
    private DrawingContext _drawingContext;
    private gacmn pcm;

    private bool isReinit = false;
    private gafile pfi;

    private int yin = -999;
    private int xin = -999;
    private int bwin = -999;
    private int fmtflg = 0;
    private string bgImage = "";
    private string fgImage = "";
    private int tcolor = -1;
    private double border = -1.0;

    public GaUser(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
        pcm = drawingContext.CommonData;
    }

    public void stack()
    {
        // no op
    }

    public void reset(bool doReinit = false)
    {
        pcm.xsiz = pcm.pxsize;
        pcm.ysiz = pcm.pysize;
        _drawingContext.GaSubs.gxvpag(pcm.xsiz, pcm.ysiz, 0.0, pcm.xsiz, 0.0, pcm.ysiz);
        /* reset gacmn and call gacln */
        pcm.InitData();
        gacln(1);
        gacln(2);
        gacln(3);
        gacln(4);
        isReinit = false;
        if (doReinit) reinit();
        if (pcm.fnum > 0 && pcm.pfi1.Count > 0)
        {
            pcm.pfid = pcm.pfi1.First();
            pcm.dfnum = 1;
            var pfi = pcm.pfi1.First();
            if (pfi.type == 2 || pfi.wrap > 0) SetLon(0, 360);
            else
            {
                SetX(1, pfi.dnum[0]);
            }

            if (pfi.type == 2)
            {
                SetLat(-90, 90);
                SetLev(500);
            }
            else
            {
                SetY(1, pfi.dnum[1]);

                /* set z to max if x or y = 1 */
                if (pfi.type == 1 && pfi.dnum[2] > 1
                                  && ((pfi.dnum[0] == 1) || (pfi.dnum[1] == 1)))
                {
                    if (pfi.dnum[2] <= 1)
                    {
                        SetZ(1);
                    }
                    else
                    {
                        SetZ(1, pfi.dnum[2]);
                    }
                }
                else
                {
                    SetZ(1);
                }
            }

            SetT(1);
            SetE(1);
        }

        if (isReinit)
        {
            //TODO
        }

        _drawingContext.GxChpl.gxchdf(0);
        _drawingContext.GaSubs.gxcolr(1);
        _drawingContext.GxDb.gxdbsettransclr(-1);
        _drawingContext.GaSubs.gxfrme(1);
    }

    public void close()
    {
        int fnum = pcm.pfi1.Count;
        var file = pcm.pfi1.LastOrDefault();
        if (file != null)
        {
            if (file.mfile != null) file.mfile.Close();
            if (file.infile != null) file.infile.Close();
            pcm.pfi1.Remove(file);
            pcm.fnum--;
        }

        if (pcm.dfnum == fnum)
        {
            pcm.dfnum = 1;
            pcm.pfid = pcm.pfi1.FirstOrDefault();
        }

        if (pcm.fnum == 0)
        {
            pcm.dfnum = 0;
            pcm.pfi1 = null;
            pcm.pfid = null;
        }
    }

    public void clear(ClearAction action)
    {
        int iAc = (int)action;
        if (action == ClearAction.NoReset)
        {
            _drawingContext.GaSubs.gxfrme(1);
        }
        else if (action == ClearAction.Events)
        {
        }
        else if (action == ClearAction.Graphics)
        {
            _drawingContext.GaSubs.gxfrme(7);
        }
        else if (action == ClearAction.Mask)
        {
            _drawingContext.GaSubs.gxmaskclear();
        }

        if (action == ClearAction.NoReset)
        {
            gacln(0);
        }
        else if (iAc < 5)
        {
            gacln(1);
            pcm.dbflg = 0;
        }
        else if (action == ClearAction.SdfWrite)
        {
            gacln(2);
        }
        else if (action == ClearAction.Shp)
        {
            gacln(3);
        }
    }

    public void printim(string path, OutputFormat format = OutputFormat.Undefined, string backgroundImage = "",
        string foregroundImage = "",
        BackgroundColor backColor = BackgroundColor.Default, int transparentColor = -1,
        int horizontalSize = -999, int verticalSize = -999, double borderWidth = -1.0)
    {
        xin = horizontalSize;
        yin = verticalSize;
        border = borderWidth;
        bgImage = backgroundImage;
        fgImage = foregroundImage;
        bwin = (int)backColor;
        tcolor = transparentColor;
        fmtflg = (int)format;

        if (format == OutputFormat.Undefined)
        {
            if (path.EndsWith(".eps")) fmtflg = 1;
            if (path.EndsWith(".ps")) fmtflg = 2;
            if (path.EndsWith(".pdf")) fmtflg = 3;
            if (path.EndsWith(".svg")) fmtflg = 4;
            if (path.EndsWith(".png")) fmtflg = 5;
            if (path.EndsWith(".gif")) fmtflg = 6;
            if (path.EndsWith(".jpg")) fmtflg = 7;
            if (path.EndsWith(".jpeg")) fmtflg = 7;
        }

        if (fmtflg == (int)OutputFormat.Undefined)
        {
            throw new Exception("Unable to determine output format");
        }

        if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3 || fmtflg == 4)
        {
            if (horizontalSize > 0 || verticalSize > 0)
            {
                GaGx.gaprnt(1,
                    $"Warning: Image size specifications are incompatible with the {format} format and will be ignored");
                xin = -999;
                yin = -999;
            }
        }

        if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3 || fmtflg == 4)
        {
            if (bgImage != "" || fgImage != "")
            {
                GaGx.gaprnt(1,
                    "Warning: Background/Foreground images are incompatible with the %s format and will be ignored");
                bgImage = "";
                fgImage = "";
            }
        }

        if (border < 1.0)
        {
            border = 0.0;
            if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3) border = 0.15625;
        }

        int retcod = 0;
        int rc = _drawingContext.GaSubs.DrawingEngine.gxprint(path, xin, yin, bwin, fmtflg, bgImage, fgImage, tcolor,
            border);
        if (rc == 1) GaGx.gaprnt(0, "GXPRINT error: something went wrong during rendering of the output file\n");
        if (rc == 2) GaGx.gaprnt(0, "GXPRINT error: output error\n");
        if (rc == 3) GaGx.gaprnt(0, "GXPRINT error: background image open error\n");
        if (rc == 4) GaGx.gaprnt(0, "GXPRINT error: foreground image open error\n");
        if (rc == 5) GaGx.gaprnt(0, "GXPRINT error: background image must be .png\n");
        if (rc == 6) GaGx.gaprnt(0, "GXPRINT error: foreground image must be .png\n");
        if (rc == 7) GaGx.gaprnt(0, "GXPRINT error: failed to import background image\n");
        if (rc == 8) GaGx.gaprnt(0, "GXPRINT error: failed to import foreground image\n");
        if (rc == 9) GaGx.gaprnt(0, "GXPRINT error: unsupported output format\n");
        if (rc == 10) GaGx.gaprnt(0, "GXPRINT error: background image must be the same size as output\n");
        if (rc == 11) GaGx.gaprnt(0, "GXPRINT error: foreground image must be the same size as output\n");
        if (rc > 0) retcod = 1;
        else
        {
            // TODO: output info
            // retcod = 0;
            // snprintf(pout, 1255, "Created %s file %s", formats[fmtflg - 1], cc);
            // gaprnt(2, pout);
            // if (*bgImage) {
            //     snprintf(pout, 1255, " ; bgImage = %s", bgImage);
            //     gaprnt(2, pout);
            // }
            // if (*fgImage) {
            //     snprintf(pout, 1255, " ; fgImage = %s", fgImage);
            //     gaprnt(2, pout);
            // }
            // gaprnt(2, "\n");
        }
    }

    public void swap()
    {
        if (pcm.dbflg > 0) _drawingContext.GaSubs.gxfrme(2);
        gacln(1);
    }

    public void reinit()
    {
        isReinit = true;
        pcm.pdf1 = null;
        //TODO Collections
        pcm.pfi1 = null;
        pcm.pfid = null;
        pcm.fnum = 0;
        pcm.dfnum = 0;
        pcm.undef = -9.99e8; /* default undef value */
    }

    void gacln(int flg)
    {
        gaattr attr, nextattr;
        int i;

        /* The 'basic' clean */
        pcm.pass = 0;
        for (i = 0; i < 10; i++) pcm.gpass[i] = 0;
        if (pcm.ylpflg == 0) pcm.yllow = 0.0;
        pcm.xexflg = 0;
        pcm.yexflg = 0;
        pcm.shdcnt = 0;
        pcm.cntrcnt = 0;
        pcm.lastgx = 0;
        pcm.xdim = -1;
        pcm.ydim = -1;
        pcm.xgr2ab = null;
        pcm.ygr2ab = null;
        pcm.xab2gr = null;
        pcm.yab2gr = null;
        pcm.aaflg = 1;

        /* reset sdfwrite parameters */
        if (flg == 2)
        {
            // pcm.ncwid = -999;
            // pcm.sdfwtype = 1;
            // pcm.sdfwpad = 0;
            // pcm.sdfrecdim = 0;
            // pcm.sdfchunk = 0;
            // pcm.xchunk = 0;
            // pcm.ychunk = 0;
            // pcm.zchunk = 0;
            // pcm.tchunk = 0;
            // pcm.echunk = 0;
            // pcm.sdfzip = 0;
            // pcm.sdfprec = 8;
            // if (pcm.sdfwname) {
            //     gree(pcm.sdfwname, "g225");
            //     pcm.sdfwname = null;
            // }
            // while (pcm.attr != null) {
            //     attr = pcm.attr;           /* point to first block in chain */
            //     if (attr.next == null) {
            //         pcm.attr = null;         /* first block is only block */
            //     } else {        /* move start of chain from 1st to 2nd block */
            //         nextattr = attr.next;
            //         pcm.attr = nextattr;
            //     }
            //     if (attr.value) gree(attr.value, "g85");  /* release memory from 1st block */
            //     gree(attr, "g86");
            // }
        }

        /* reset shapefile fields */
        if (flg == 3)
        {
// #if USESHP == 1
//                                                                                                                                 /* reset shapefile type to line and no fill */
//     pcm.shptype=2;
//     pcm.fillpoly = -1;
//     /* release file name */
//     if (pcm.shpfname) {
//       gree (pcm.shpfname,"g89");
//       pcm.shpfname = null;
//     }
//     /* release chain of data base fields */
//     while (pcm.dbfld != null) {
//       /* point to first block in chain */
//       fld = pcm.dbfld;
//       if (fld.next == null) {
// 	/* first block is only block */
// 	pcm.dbfld = null;
//       }
//       else {
// 	/* move start of chain from 1st to 2nd block */
// 	nextfld = fld.next;
// 	pcm.dbfld = nextfld;
//       }
//       /* release memory from 1st block */
//       if (fld.value != null) gree(fld.value,"g87");
//       gree(fld,"g88");
//     }
// #endif
        }

        /* reset KML and GeoTIFF options */
        if (flg == 4)
        {
            // pcm.gtifflg = 1;               /* reset GeoTIFF output to float */
            // pcm.kmlflg = 1;                /* reset KML output to img */
            // if (pcm.kmlname) {           /* release KML file name */
            //     gree(pcm.kmlname, "g90");
            //     pcm.kmlname = null;
            // }
            // if (pcm.tifname) {           /* release TIFF file name */
            //     gree(pcm.tifname, "g91");
            //     pcm.tifname = null;
            // }
            // if (pcm.gtifname) {          /* release GeoTIFF file name */
            //     gree(pcm.gtifname, "g92");
            //     pcm.gtifname = null;
            // }
        }

        /* reset user options */
        if (flg == 1)
        {
            pcm.cstyle = -9;
            pcm.ccolor = -9;
            pcm.cthick = 4;
            pcm.cmark = -9;
            pcm.cint = 0;
            pcm.cflag = 0;
            pcm.ccflg = 0;
            pcm.cmin = -9.99e33;
            pcm.cmax = 9.99e33;
            pcm.blkflg = false;
            pcm.aflag = 0;
            pcm.aflag2 = 0;
            pcm.axflg = 0;
            pcm.ayflg = 0;
            pcm.gridln = -9;
            pcm.rainmn = pcm.rainmx = 0.0;
            pcm.grdsflg = true;
            pcm.arrflg = false;
            pcm.hemflg = -1;
            pcm.rotate = false;
            pcm.xflip = false;
            pcm.yflip = false;
            pcm.xlstr = null;
            pcm.ylstr = null;
            pcm.clstr = null;
            pcm.xlabs = null;
            pcm.ylabs = null;
            pcm.xlint = 0.0;
            pcm.ylint = 0.0;
            pcm.xlflg = 0;
            pcm.ylflg = 0;
            pcm.xlpos = 0.0;
            pcm.ylpos = 0.0;
            pcm.ylpflg = 0;
            pcm.yllow = 0.0;
            pcm.xlside = 0;
            pcm.ylside = 0;
            pcm.tlsupp = 0;
            pcm.ptflg = 0;
            pcm.cachesf = 1;
            //setcachesf(pcm.cachesf);
            pcm.marktype = 3;
            pcm.marksize = 0.05;
        }
    }

    public void SetGridLine(GridLine gridLine, int color = -1)
    {
        if (gridLine == GridLine.Auto) pcm.gridln = -9;
        if (gridLine == GridLine.Off) pcm.gridln = -1;
        if (gridLine == GridLine.Color) pcm.gridln = color;
    }

    public void SetVpage(OnOffSetting page, int xlo = 0, int xhi = 0, int ylo = 0, int yhi = 0)
    {
        if (page == OnOffSetting.Off)
        {
            pcm.xsiz = pcm.pxsize;
            pcm.ysiz = pcm.pysize;
            _drawingContext.GaSubs.gxvpag(pcm.xsiz, pcm.ysiz, 0.0, pcm.xsiz, 0.0, pcm.ysiz);
            gacln(1);
            return;
        }

        if (xlo < 0.0 || ylo < 0.0 || xhi > pcm.pxsize || yhi > pcm.pysize)
        {
            GaGx.gaprnt(0, "SET Error: vpage values beyond real page limits");
            throw new Exception("vpage values beyond real page limits");
        }

        if (yhi <= ylo || xhi <= xlo) throw new Exception("SET error:  Missing or invalid arguments for vpage option");
        if ((yhi - ylo) / (xhi - xlo) > pcm.pysize / pcm.pxsize)
        {
            pcm.ysiz = pcm.pysize;
            pcm.xsiz = pcm.pysize * (xhi - xlo) / (yhi - ylo);
        }
        else
        {
            pcm.xsiz = pcm.pxsize;
            pcm.ysiz = pcm.pxsize * (yhi - ylo) / (xhi - xlo);
        }

        _drawingContext.GaSubs.gxvpag(pcm.xsiz, pcm.ysiz, xlo, xhi, ylo, yhi);
        GaGx.gaprnt(2, $"Virtual page size = {pcm.xsiz} {pcm.ysiz}");
        gacln(1);
    }

    public void SetColor(int colorNr, int red, int green, int blue, int alpha = 255)
    {
        if (colorNr < 16 || colorNr > 2048)
        {
            GaGx.gaprnt(0, "SET RGB Error:  Color number must be between 16 and 2048 \n");
            throw new Exception("SET RGB Error:  Color number must be between 16 and 2048");
        }

        if (red < 0 || red > 255 ||
            green < 0 || green > 255 ||
            blue < 0 || blue > 255)
        {
            GaGx.gaprnt(0, "SET RGB Error:  RGB values must be between 0 and 255 \n");
            throw new Exception("SET RGB Error:  RGB values must be between 0 and 255");
        }

        if (alpha < -255 || alpha > 255)
        {
            GaGx.gaprnt(0, "SET RGB Error:  Alpha value must be between -255 and 255 \n");
            throw new Exception("SET RGB Error:  Alpha value must be between -255 and 255");
        }

        _drawingContext.GaSubs.gxacol(colorNr, red, green, blue, alpha);
    }

    public void SetPArea(OnOffSetting onOff, int xlo = 0, int xhi = 0, int ylo = 0, int yhi = 0)
    {
        if (onOff == OnOffSetting.Off)
        {
            pcm.paflg = false;
        }
        else
        {
            if (xlo < 0.0 || ylo < 0.0 || xhi > pcm.xsiz || yhi > pcm.ysiz)
            {
                GaGx.gaprnt(0, "SET Error: parea values beyond page limits");
                throw new Exception("SET Error: parea values beyond page limits");
            }

            if (yhi <= ylo || xhi <= xlo)
                throw new Exception("SET error:  Missing or invalid arguments for parea option");
            pcm.pxmin = xlo;
            pcm.pxmax = xhi;
            pcm.pymin = ylo;
            pcm.pymax = yhi;
            pcm.paflg = true;
        }
    }

    public void SetCInt(int val)
    {
        if (val <= 0)
        {
            GaGx.gaprnt(0, "SET Error: cint must be greater than 0.0\n");
        }
        else
        {
            pcm.cint = val;
            GaGx.gaprnt(2, $"cint = {pcm.cint}");
        }
    }

    public void SetMPVals(OnOffSetting onOff, double lonmin = 0.0, double lonmax = 0.0, double latmin = 0.0,
        double latmax = 0.0)
    {
        if (onOff == OnOffSetting.Off)
        {
            pcm.mpflg = 0;
            GaGx.gaprnt(2, "mpvals have been turned off");
        }
        else
        {
            pcm.mpvals[0] = lonmin;
            pcm.mpvals[1] = lonmax;
            pcm.mpvals[2] = latmin;
            pcm.mpvals[3] = latmax;
            pcm.mpflg = 1;
            GaGx.gaprnt(2, "mpvals have been set");
        }
    }

    public void SetCLevs(double[] levels)
    {
        if (levels.Length > 254)
        {
            throw new Exception("Too many levels");
        }

        pcm.cflag = levels.Length;
        pcm.clevs = levels;

        GaGx.gaprnt(2, $"Number of clevs = {pcm.cflag}");
        for (int i = 1; i < pcm.cflag; i++)
        {
            if (pcm.clevs[i] <= pcm.clevs[i - 1])
            {
                GaGx.gaprnt(1, "Warning: Contour levels are not strictly increasing");
                GaGx.gaprnt(1, "         This may lead to errors or undesired results");
                break;
            }
        }
    }

    public void SetCCols(int[] cols)
    {
        if (cols.Length > 255)
        {
            throw new Exception("Too many levels");
        }

        pcm.ccflg = cols.Length;
        pcm.ccols = cols;

        GaGx.gaprnt(2, $"Number of ccols = {pcm.ccflg}");
        if (pcm.cflag == 0)
        {
            GaGx.gaprnt(2, "ccols won't take effect unless clevs are set.");
        }
    }

    public void SetCMin(double cmin)
    {
        pcm.cmin = cmin;
        GaGx.gaprnt(2, $"cmin = {cmin}");
    }

    public void SetCMax(double cmax)
    {
        pcm.cmax = cmax;
        GaGx.gaprnt(2, $"cmax = {cmax}");
    }

    public void SetCMark(double cmark)
    {
        pcm.cmax = cmark;
        GaGx.gaprnt(2, $"cmark = {cmark}");
    }

    public void SetMProjection(Projection projection)
    {
        pcm.mproj = (int)projection;
    }

    public void SetMapResolution(MapResolution mapResolution)
    {
        if (mapResolution == MapResolution.HighResolution)
        {
            pcm.mpdset[0] = "hires";
        }
        else if (mapResolution == MapResolution.MediumResolution)
        {
            pcm.mpdset[0] = "mres";
        }
        else if (mapResolution == MapResolution.LowResolution)
        {
            pcm.mpdset[0] = "lowres";
        }
    }

    public void SetGraphicsOut(GxOutSetting setting)
    {
        if (setting == GxOutSetting.Contour) pcm.gout2a = 1;
        if (setting == GxOutSetting.Shaded) pcm.gout2a = 16;
        if (setting == GxOutSetting.Shade1) pcm.gout2a = 2;
        if (setting == GxOutSetting.Shade2) pcm.gout2a = 16;
        if (setting == GxOutSetting.Shade2b) pcm.gout2a = 17;
        if (setting == GxOutSetting.Grid)
        {
            pcm.gout2a = 3;
            pcm.gout2b = 3;
        }

        if (setting == GxOutSetting.Vector)
        {
            pcm.gout2b = 4;
            pcm.goutstn = 6;
            pcm.gout1a = 1;
        }

        if (setting == GxOutSetting.Scatter) pcm.gout2b = 5;
        if (setting == GxOutSetting.FGrid) pcm.gout2a = 6;
        if (setting == GxOutSetting.FWrite) pcm.gout2a = 7;
        if (setting == GxOutSetting.Stream) pcm.gout2b = 8;
        if (setting == GxOutSetting.GridFill) pcm.gout2a = 10;
        if (setting == GxOutSetting.GeoTiff) pcm.gout2a = 12;
        if (setting == GxOutSetting.Kml) pcm.gout2a = 13;
        if (setting == GxOutSetting.Imap) pcm.gout2a = 14;
        if (setting == GxOutSetting.Shape)
        {
            pcm.gout2a = 15;
            pcm.goutstn = 9;
        }

        if (setting == GxOutSetting.StationValues) pcm.goutstn = 1;
        if (setting == GxOutSetting.Barb)
        {
            pcm.goutstn = 2;
            pcm.gout2b = 9;
            pcm.gout1a = 2;
        }

        if (setting == GxOutSetting.FindStation) pcm.goutstn = 3;
        if (setting == GxOutSetting.Model) pcm.goutstn = 4;
        if (setting == GxOutSetting.WXSymbol) pcm.goutstn = 5;
        if (setting == GxOutSetting.StationMark) pcm.goutstn = 7;
        if (setting == GxOutSetting.StationWrt) pcm.goutstn = 8;
        if (setting == GxOutSetting.Line)
        {
            pcm.gout1 = 1;
            pcm.tser = 0;
        }

        if (setting == GxOutSetting.Bar) pcm.gout1 = 2;
        if (setting == GxOutSetting.ErrorBar) pcm.gout1 = 3;
        if (setting == GxOutSetting.LineFill) pcm.gout1 = 4;
        if (setting == GxOutSetting.Stat) pcm.gout0 = 1;
        if (setting == GxOutSetting.Print) pcm.gout0 = 2;
        if (setting == GxOutSetting.TimeSeriesWeatherSymbols) pcm.tser = 1;
        if (setting == GxOutSetting.TimeSeriesBarb) pcm.tser = 2;

        if (pcm.gout0 == 9) pcm.gout0 = 0;
    }

    public void SetCLab(LabelOption option, string format = "")
    {
        string str = pcm.clstr;
        int i1 = pcm.clab;

        if (option == LabelOption.Auto)
        {
            str = "";
        }
        else if (option == LabelOption.Off)
        {
            i1 = 0;
        }
        else if (option == LabelOption.Forced)
        {
            i1 = 2;
        }
        else if (option == LabelOption.On)
        {
            i1 = 1;
        }
        else if (option == LabelOption.Masked)
        {
            i1 = 3;
        }

        if (format != "")
        {
            str = format;
        }

        pcm.clstr = str;
        pcm.clab = i1;
    }

    public void SetXLab(LabelOption option, string format = "")
    {
        string str = pcm.xlstr;
        int i1 = pcm.xlab;

        if (option == LabelOption.Auto)
        {
            str = "";
        }
        else if (option == LabelOption.Off)
        {
            i1 = 0;
        }
        else if (option == LabelOption.Forced)
        {
            i1 = 2;
        }
        else if (option == LabelOption.On)
        {
            i1 = 1;
        }
        else if (option == LabelOption.Masked)
        {
            i1 = 3;
        }

        if (format != "")
        {
            str = format;
        }

        pcm.xlstr = str;
        pcm.xlab = i1;
    }

    public void SetYLab(LabelOption option, string format = "")
    {
        string str = pcm.ylstr;
        int i1 = pcm.ylab;

        if (option == LabelOption.Auto)
        {
            str = "";
        }
        else if (option == LabelOption.Off)
        {
            i1 = 0;
        }
        else if (option == LabelOption.Forced)
        {
            i1 = 2;
        }
        else if (option == LabelOption.On)
        {
            i1 = 1;
        }
        else if (option == LabelOption.Masked)
        {
            i1 = 3;
        }

        if (format != "")
        {
            str = format;
        }

        pcm.ylstr = str;
        pcm.ylab = i1;
    }

    public void SetGridOptions(GridOption option, int style = -1, int color = -1, int thickness = -1)
    {
        pcm.grflag = (int)option;
        if (style > 0)
        {
            pcm.grstyl = style;
        }

        if (color > -1)
        {
            pcm.grcolr = color;
        }

        if (thickness > -1)
        {
            pcm.grthck = thickness;
        }
    }

    public void SetContourLineOptions(int size = -1, int color = -1, int thickness = -1)
    {
        if (size > 0)
        {
            pcm.clsiz = size;
        }

        if (color > -1)
        {
            pcm.clcol = color;
        }

        if (thickness > -1)
        {
            pcm.clthck = thickness;
        }
    }

    public void SetStringOptions(int color, StringJustification justification = StringJustification.Undefined,
        int thickness = -1, double rotation = -1)
    {
        pcm.strcol = color;
        if (justification != StringJustification.Undefined)
        {
            pcm.strjst = (int)justification;
        }

        if (thickness > -1)
        {
            pcm.strthk = thickness;
        }

        if (rotation > -1)
        {
            pcm.strrot = rotation;
        }
    }

    public void SetGrads(OnOffSetting onOffSetting)
    {
        if (onOffSetting == OnOffSetting.Off)
        {
            pcm.grdsflg = false;
            pcm.timelabflg = false;
        }
        else
        {
            pcm.grdsflg = true;
            pcm.timelabflg = true;
        }
    }

    public void SetStringSize(double hsize, double vsize = -1)
    {
        pcm.strhsz = hsize;
        if (vsize > -1)
        {
            pcm.strvsz = vsize;
        }
        else
        {
            pcm.strvsz = hsize;
        }
    }

    public void SetCThick(int cthck)
    {
        pcm.cthick = cthck;
        GaGx.gaprnt(2, $"cthick = {pcm.cthick}");
    }

    public void SetCStyle(LineStyle style)
    {
        pcm.cstyle = (int)style;
        GaGx.gaprnt(2, $"cstyle = {pcm.cstyle}");
    }

    public void SetCColor(int color)
    {
        pcm.ccolor = color;
        GaGx.gaprnt(2, $"ccolor = {pcm.ccolor}");
    }

    public void SetCSmooth(SmoothOption option)
    {
        pcm.csmth = (int)option;
        GaGx.gaprnt(2, $"csmth = {pcm.csmth}");
    }

    private void SetDimensionData(int dim, double min, double max = Double.MaxValue)
    {
        if (pcm.pfid == null) throw new Exception("No open file");
        int num = 1;
        pcm.dmin[dim] = min;
        if (max != Double.MaxValue)
        {
            num = 2;
            pcm.dmax[dim] = max;
            if (GaUtil.dequal(pcm.dmin[dim], pcm.dmax[dim], 1.0e-8) == 0) num = 1;
        }

        pcm.vdim[dim] = num - 1;
        pfi = pcm.pfid;
        if (pfi.type == 1 && num == 1)
        {
            pcm.vdim[dim] = 0;
            var conv = pfi.ab2gr[dim];
            var vals = pfi.abvals[dim];
            var v1 = conv(vals, pcm.dmin[dim]);
            v1 = Math.Floor(v1 + 0.5);
            conv = pfi.gr2ab[dim];
            vals = pfi.grvals[dim];
            pcm.dmin[dim] = conv(vals, v1);
            pcm.dmax[dim] = pcm.dmin[dim];
        }
    }

    public void SetLon(double min, double max = Double.MaxValue)
    {
        SetDimensionData(0, min, max);
    }

    public void SetLat(double min, double max = Double.MaxValue)
    {
        SetDimensionData(1, min, max);
    }

    public void SetLev(double lev)
    {
        SetDimensionData(2, lev);
    }

    public void SetX(double xMin, double xMax)
    {
    }

    public void SetY(double yMin, double yMax)
    {
    }

    public void SetZ(double zMin)
    {
        _drawingContext.CommonData.dmin[2] = zMin;
        _drawingContext.CommonData.dmax[2] = zMin;
    }

    public void SetZ(double zMin, double zMax)
    {
    }

    public void SetT(double tMin)
    {
        _drawingContext.CommonData.vdim[3] = 0;

        var vals = _drawingContext.CommonData.pfid.grvals[3];
        GaUtil.gr2t(vals, tMin, out _drawingContext.CommonData.tmin);
        _drawingContext.CommonData.tmax = _drawingContext.CommonData.tmin;
        Console.Write("TMIN=");
    }

    public void SetE(double eMin)
    {
        _drawingContext.CommonData.vdim[4] = 0;
        _drawingContext.CommonData.dmax[4] = 1;
        _drawingContext.CommonData.dmin[4] = 1;
    }


    public void Define(string varName, string formula)
    {
        gadef(varName, formula, 0);
    }

    int gadef(string variable, string formula, int impf)
    {
        gagrid pgr, pgr1;
        gastat? pst;
        gafile pfi, pfiv, pfic;
        gadefn? pdf, pcurr, psave;
        gadefn prev;
        dt tmin, tmax;

        Func<double[], double, double>? conv;
        double vmin, vmax, zmin, zmax, emin, emax;
        int res, resu, gr, gru;
        int itmin, itmax, it, izmin, izmax, iz, iemin, iemax, ie;
        int i, rc, gsiz, vdz, vdt, vde;
        long sz, siz;


        string name;

        pdf = null;
        pst = null;
        pgr1 = null;
        pfiv = null;

        /* Save user dim limits */
        zmin = pcm.dmin[2];
        zmax = pcm.dmax[2];
        tmin = pcm.tmin;
        tmax = pcm.tmax;
        emin = pcm.dmin[4];
        emax = pcm.dmax[4];
        vdz = pcm.vdim[2];
        vdt = pcm.vdim[3];
        vde = pcm.vdim[4];

        /* Get the define name */
        if (impf == 0)
        {
        }

        formula = formula.Replace(" ", "");
        name = variable;

        /* We are now pointing to the expression.  We need to set up
         our looping environment -- we are going to always loop
         through Z and T and E.                                */

        pfi = pcm.pfid;
        if (pfi.type == 2 || pfi.type == 3)
        {
            GaGx.gaprnt(0, "DEFINE error: Define not yet valid for station data\n");
            GaGx.gaprnt(0, "              Default file is a station data file\n");
            goto retrn;
        }

        conv = pfi.ab2gr[2]; /* Get Z grid limits */
        vmin = conv(pfi.abvals[2], zmin);
        vmax = conv(pfi.abvals[2], zmax);
        if (GaUtil.dequal(vmin, vmax, 1.0e-08) == 0)
        {
            if (vmin < 0.0) izmin = (int)(vmin - 0.5);
            else izmin = (int)(vmin + 0.5);
            izmax = izmin;
        }
        else
        {
            izmin = (int)(Math.Floor(vmin + 0.001));
            izmax = (int)(Math.Ceiling(vmax - 0.001));
        }


        vmin = GaUtil.t2gr(pfi.abvals[3], tmin); /* Get T grid limits */
        vmax = GaUtil.t2gr(pfi.abvals[3], tmax);
        if (GaUtil.dequal(vmin, vmax, 1.0e-08) == 0)
        {
            if (vmin < 0.0) itmin = (int)(vmin - 0.5);
            else itmin = (int)(vmin + 0.5);
            itmax = itmin;
        }
        else
        {
            itmin = (int)(Math.Floor(vmin + 0.001));
            itmax = (int)(Math.Ceiling(vmax - 0.001));
        }

        conv = pfi.ab2gr[4]; /* Get E grid limits */
        vmin = conv(pfi.abvals[4], emin);
        vmax = conv(pfi.abvals[4], emax);
        if (GaUtil.dequal(vmin, vmax, 1.0e-08) == 0)
        {
            if (vmin < 0.0) iemin = (int)(vmin - 0.5);
            else iemin = (int)(vmin + 0.5);
            iemax = iemin;
        }
        else
        {
            iemin = (int)(Math.Floor(vmin + 0.001));
            iemax = (int)(Math.Ceiling(vmax - 0.001));
        }

        /* Fix Z and T and E dimensions */
        pcm.dmax[2] = pcm.dmin[2];
        pcm.tmax = pcm.tmin;
        pcm.dmax[4] = pcm.dmin[4];
        pcm.vdim[2] = 0;
        pcm.vdim[3] = 0;
        pcm.vdim[4] = 0;

        /* Get the first grid */
        pst = getpst(pcm);
        if (pst == null) goto retrn;

        conv = pfi.gr2ab[2];
        pst.dmin[2] = conv(pfi.grvals[2], (double)izmin);
        pst.dmax[2] = pst.dmin[2];
        GaUtil.gr2t(pfi.grvals[3], (double)itmin, out pst.tmin);
        pst.tmax = pst.tmin;
        conv = pfi.gr2ab[4];
        pst.dmin[4] = conv(pfi.grvals[4], (double)iemin);
        pst.dmax[4] = pst.dmin[4];

        rc = _drawingContext.GaExpr.gaexpr(formula, pst);
        if (rc == 0) rc = _drawingContext.CommonData.sig;
        if (rc > 0)
        {
            GaGx.gaprnt(0, "DEFINE error:  Invalid expression. ");
            goto retrn;
        }

        if (pst.type != 1)
        {
            GaGx.gaprnt(0, "DEFINE Error:  Define does not yet support station data");
            GaGx.gaprnt(0, "    Expression results in station data object");
            goto retrn;
        }

        /* Based on the grid we just got, we can now figure out the size of
         the final defined grid and fill in the gafile structure for the
         defined grid.  Allocate all the necessary stuff and fill it all
         in.  */

        /* Allocate the pdf and pfi blocks */
        pdf = new gadefn();
        pfiv = new gafile();
        pdf.pfi = pfiv;
        pfiv.rbuf = null;
        pfiv.sbuf = null;
        pfiv.ubuf = null;

        /* Fill in the pfi block */
        pgr1 = pst.result.pgr;
        pfiv.type = 4;
        pfiv.climo = 0;
        pfiv.calendar = pfi.calendar;
        pfiv.undef = pgr1.undef;
        pfiv.dnum[2] = 1 + izmax - izmin;
        pfiv.dnum[3] = 1 + itmax - itmin;
        pfiv.dnum[4] = 1 + iemax - iemin;
        pfiv.gr2ab[2] = pfi.gr2ab[2];
        pfiv.ab2gr[2] = pfi.ab2gr[2];
        pfiv.gr2ab[4] = pfi.gr2ab[4];
        pfiv.ab2gr[4] = pfi.ab2gr[4];

        if ((pfiv.grvals[2] = GaUtil.cpscal(pfi.grvals[2], pfi.linear[2], 0, 2)) == null) goto etrn;
        if ((pfiv.abvals[2] = GaUtil.cpscal(pfi.abvals[2], pfi.linear[2], 1, 2)) == null) goto etrn;

        if ((pfiv.grvals[3] = GaUtil.cpscal(pfi.grvals[3], pfi.linear[3], 0, 3)) == null) goto etrn;
        if ((pfiv.abvals[3] = GaUtil.cpscal(pfi.abvals[3], pfi.linear[3], 1, 3)) == null) goto etrn;

        if ((pfiv.grvals[4] = GaUtil.cpscal(pfi.grvals[4], pfi.linear[4], 0, 4)) == null) goto etrn;
        if ((pfiv.abvals[4] = GaUtil.cpscal(pfi.abvals[4], pfi.linear[4], 1, 4)) == null) goto etrn;

        pfiv.linear[2] = pfi.linear[2];
        pfiv.linear[3] = pfi.linear[3];
        pfiv.linear[4] = pfi.linear[4];
        pfiv.dimoff[2] = izmin - 1;
        pfiv.dimoff[3] = itmin - 1;
        pfiv.dimoff[4] = iemin - 1;
        pfiv.ppflag = 0;

        if (pgr1.idim > -1 && pgr1.jdim > -1)
        {
            /* I and J are varying */
            pfiv.gr2ab[0] = pgr1.igrab;
            pfiv.ab2gr[0] = pgr1.iabgr;
            if ((pfiv.grvals[0] = GaUtil.cpscal(pgr1.ivals, pgr1.ilinr, 0, pgr1.idim)) == null) goto etrn;
            if ((pfiv.abvals[0] = GaUtil.cpscal(pgr1.iavals, pgr1.ilinr, 1, pgr1.idim)) == null) goto etrn;
            pfiv.linear[0] = pgr1.ilinr;
            pfiv.dimoff[0] = pgr1.dimmin[0] - 1;
            pfiv.dnum[0] = pgr1.isiz;

            pfiv.gr2ab[1] = pgr1.jgrab;
            pfiv.ab2gr[1] = pgr1.jabgr;
            if ((pfiv.grvals[1] = GaUtil.cpscal(pgr1.jvals, pgr1.jlinr, 0, pgr1.jdim)) == null) goto etrn;
            if ((pfiv.abvals[1] = GaUtil.cpscal(pgr1.javals, pgr1.jlinr, 1, pgr1.jdim)) == null) goto etrn;
            pfiv.linear[1] = pgr1.jlinr;
            pfiv.dimoff[1] = pgr1.dimmin[1] - 1;
            pfiv.dnum[1] = pgr1.jsiz;
        }
        else if (pgr1.idim > -1 && pgr1.jdim == -1)
        {
            /* I is varying, J is fixed */
            if (pgr1.idim == 0)
            {
                /* I is X */
                pfiv.gr2ab[0] = pgr1.igrab;
                pfiv.ab2gr[0] = pgr1.iabgr;
                if ((pfiv.grvals[0] = GaUtil.cpscal(pgr1.ivals, pgr1.ilinr, 0, pgr1.idim)) == null) goto etrn;
                if ((pfiv.abvals[0] = GaUtil.cpscal(pgr1.iavals, pgr1.ilinr, 1, pgr1.idim)) == null) goto etrn;
                pfiv.linear[0] = pgr1.ilinr;
                pfiv.dimoff[0] = pgr1.dimmin[0] - 1;
                pfiv.dnum[0] = pgr1.isiz;

                pfiv.gr2ab[1] = pfi.gr2ab[1];
                pfiv.ab2gr[1] = pfi.ab2gr[1];
                if ((pfiv.grvals[1] = GaUtil.cpscal(pfi.grvals[1], pfi.linear[1], 0, 1)) == null) goto etrn;
                if ((pfiv.abvals[1] = GaUtil.cpscal(pfi.abvals[1], pfi.linear[1], 1, 1)) == null) goto etrn;
                pfiv.linear[1] = pfi.linear[1];
                pfiv.dimoff[1] = 0;
                pfiv.dnum[1] = 1;
            }
            else
            {
                /* I is Y */
                pfiv.gr2ab[1] = pgr1.igrab;
                pfiv.ab2gr[1] = pgr1.iabgr;
                if ((pfiv.grvals[1] = GaUtil.cpscal(pgr1.ivals, pgr1.ilinr, 0, pgr1.idim)) == null) goto etrn;
                if ((pfiv.abvals[1] = GaUtil.cpscal(pgr1.iavals, pgr1.ilinr, 1, pgr1.idim)) == null) goto etrn;
                pfiv.linear[1] = pgr1.ilinr;
                pfiv.dimoff[1] = pgr1.dimmin[1] - 1;
                pfiv.dnum[1] = pgr1.isiz;

                pfiv.gr2ab[0] = pfi.gr2ab[0];
                pfiv.ab2gr[0] = pfi.ab2gr[0];
                if ((pfiv.grvals[0] = GaUtil.cpscal(pfi.grvals[0], pfi.linear[0], 0, 0)) == null) goto etrn;
                if ((pfiv.abvals[0] = GaUtil.cpscal(pfi.abvals[0], pfi.linear[0], 1, 0)) == null) goto etrn;
                pfiv.linear[0] = pfi.linear[0];
                pfiv.dimoff[0] = 0;
                pfiv.dnum[0] = 1;
            }
        }
        else
        {
            /* I and J are fixed */
            pfiv.gr2ab[0] = pfi.gr2ab[0];
            pfiv.ab2gr[0] = pfi.ab2gr[0];
            if ((pfiv.grvals[0] = GaUtil.cpscal(pfi.grvals[0], pfi.linear[0], 0, 0)) == null) goto etrn;
            if ((pfiv.abvals[0] = GaUtil.cpscal(pfi.abvals[0], pfi.linear[0], 1, 0)) == null) goto etrn;
            pfiv.linear[0] = pfi.linear[0];
            pfiv.dimoff[0] = 0;
            pfiv.dnum[0] = 1;

            pfiv.gr2ab[1] = pfi.gr2ab[1];
            pfiv.ab2gr[1] = pfi.ab2gr[1];
            if ((pfiv.grvals[1] = GaUtil.cpscal(pfi.grvals[1], pfi.linear[1], 0, 1)) == null) goto etrn;
            if ((pfiv.abvals[1] = GaUtil.cpscal(pfi.abvals[1], pfi.linear[1], 1, 1)) == null) goto etrn;
            pfiv.linear[1] = pfi.linear[1];
            pfiv.dimoff[1] = 0;
            pfiv.dnum[1] = 1;
        }

        /* If the first grid is all the data we need, then we are pretty much done.  */

        if (izmin == izmax && itmin == itmax && iemin == iemax)
        {
            if (pgr1.idim < 0)
            {
                /* grid is a single data value */
                sz = sizeof(double);
                pfiv.rbuf = new double[1];
                sz = sizeof(char);
                pfiv.ubuf = new byte[1];
                pfiv.rbuf[0] = pgr1.grid[0];
                pfiv.ubuf[0] = pgr1.umask[0];
            }
            else
            {
                pfiv.rbuf = pgr1.grid;
                pfiv.ubuf = pgr1.umask;
            }

            pgr1.grid = null;
            pgr1.umask = null;
            siz = pgr1.isiz;
            siz = siz * pgr1.jsiz;
        }
        else
        {
            /* We need to get multiple grids.  Set this up.  */

            /* Calculate size and then allocate the storage for the defined object */
            gsiz = pgr1.isiz * pgr1.jsiz;
            siz = (long)gsiz * (long)pfiv.dnum[2] * (long)pfiv.dnum[3] * (long)pfiv.dnum[4];
            pfiv.rbuf = new double[siz];
            sz = siz * sizeof(char);
            pfiv.ubuf = new byte[siz];

            /* Now we can loop and get all the data */

            res = 0;
            resu = 0;
            for (ie = iemin; ie <= iemax; ie++)
            {
                for (it = itmin; it <= itmax; it++)
                {
                    for (iz = izmin; iz <= izmax; iz++)
                    {
                        /* fix dmin and dmax values for Z, T, E dimensions */
                        conv = pfi.gr2ab[2];
                        pst.dmin[2] = conv(pfi.grvals[2], (double)iz);
                        pst.dmax[2] = pst.dmin[2];
                        GaUtil.gr2t(pfi.grvals[3], (double)it, out pst.tmin);
                        pst.tmax = pst.tmin;
                        conv = pfi.gr2ab[4];
                        pst.dmin[4] = conv(pfi.grvals[4], (double)ie);
                        pst.dmax[4] = pst.dmin[4];

                        /* reuse first grid evaluated above */
                        if (ie == iemin && it == itmin && iz == izmin)
                        {
                            //gr = pgr1.grid;
                            //gru = pgr1.umask;
                            gr = 0;
                            gru = 0;
                            /* copy the grid into defined variable space */
                            for (i = 0; i < gsiz; i++)
                            {
                                pfiv.rbuf[res] = pgr1.grid[gr];
                                pfiv.ubuf[resu] = pgr1.umask[gru];
                                /* make sure defined vars have undef values, use pcm.undef */
                                if (pgr1.umask[gru] == 0) pfiv.rbuf[res] = pcm.undef;
                                res++;
                                resu++;
                                gr++;
                                gru++;
                            }
                        }
                        else
                        {
                            /* evaluate the expression again */
                            rc = _drawingContext.GaExpr.gaexpr(formula, pst);
                            if (rc == 0) rc = _drawingContext.CommonData.sig;
                            if (rc > 0)
                            {
                                GaGx.gaprnt(0, "DEFINE error:  Invalid expression. \n");
                                goto retrn;
                            }

                            pgr = pst.result.pgr;
                            if (pgr.idim != pgr1.idim || pgr.jdim != pgr1.jdim ||
                                GaExpr.gagchk(pgr, pgr1, pgr1.idim) > 0 ||
                                GaExpr.gagchk(pgr, pgr1, pgr1.jdim) > 0)
                            {
                                GaGx.gaprnt(0, "Define Error: Internal Logic Check 4\n");
                                goto retrn;
                            }

                            gr = 0;
                            gru = 0;

                            for (i = 0; i < gsiz; i++)
                            {
                                pfiv.rbuf[res] = pgr.grid[gr];
                                pfiv.ubuf[resu] = pgr.umask[gru];
                                /* make sure defined vars have undef values, use pcm.undef */
                                if (pgr.umask[gru] == 0) pfiv.rbuf[res] = pcm.undef;
                                res++;
                                resu++;
                                gr++;
                                gru++;
                            }
                        }


                        /* free the result grid  */
                        if (ie == iemin && it == itmin && iz == izmin)
                        {
                            if (pgr1.idim > -1)
                            {
                            }

                            pgr1.grid = null;
                            pgr1.umask = null;
                            pst.result.pgr = null;
                        }
                        else
                        {
                            //gafree(pst);
                        }
                    }
                }
            }
        }

        // siz = siz * sizeof(double);
        // snprintf(pout, 1255, "Define memory allocation size = %ld bytes\n", siz);
        // GaGx.gaprnt(2, pout);

        /* Now we will chain our new object to the chain of define blocks
         hung off the common area */


        var curr = pcm.pdf1.FirstOrDefault(x => x.abbrv == name);

        if (curr != null)
        {
            GaGx.gaprnt(2, "Name already DEFINEd:  ");
            GaGx.gaprnt(2, name);
            GaGx.gaprnt(2, ".   Will be deleted and replaced.\n");
            pcm.pdf1.Remove(curr);
        }

        pcm.pdf1.Add(pdf);
        pdf.abbrv = name;

        /* Restore user dim limits*/
        pcm.dmax[2] = zmax;
        pcm.tmax = tmax;
        pcm.dmax[4] = emax;
        pcm.vdim[2] = vdz;
        pcm.vdim[3] = vdt;
        pcm.vdim[4] = vde;
        return (0);

        etrn:
        GaGx.gaprnt(0, "Memory allocation error for Define\n");

        retrn:

        pcm.tmax = tmax; /* Restore user dim limits*/
        pcm.dmax[2] = zmax;
        pcm.vdim[2] = vdz;
        pcm.vdim[3] = vdt;
        return (1);
    }

    public void Open(string dataFile, DataFormat format)
    {
        // for now default to GRIB2
        if (format == DataFormat.Unknown) format = DataFormat.GFS_GRIB2;
        if (format == DataFormat.GFS_GRIB2)
        {
            ReadGrib2File(dataFile);
            _drawingContext.CommonData._variableMapping = new GfsVariables();
        }


        if (_drawingContext.CommonData.fnum == 1)
        {
            SetT(1);
            SetE(1);
            SetZ(1);
        }
    }

    private void ReadGrib2File(string dataFile)
    {
        GaGx.gaprnt(0, $"Loading datafile {dataFile}");

        gafile gf = new gafile();
        gf.name = dataFile;

        Grib2Reader rdr = new Grib2Reader(dataFile);
        var datasets = rdr.ReadAllDataSets();
        var dataset = datasets.First();

        ProductDefinition0000 pd;

        List<double> levels = new List<double>();

        foreach (var ds in datasets)
        {
            var pdef = ds.ProductDefinitionSection.ProductDefinition as ProductDefinition0000;
            if (pdef.FirstFixedSurfaceType == NGrib.Grib2.CodeTables.FixedSurfaceType.IsobaricSurface)
            {
                if (!levels.Contains((pdef.FirstFixedSurfaceValue ?? 0) / 100.0))
                {
                    levels.Add((pdef.FirstFixedSurfaceValue ?? 0) / 100.0);
                }
            }
        }


        levels = levels.OrderByDescending(x => x).ToList();
        levels.Insert(0, levels.Count);
        levels.Add(-999.9);
        levels.Add(0);
        levels.Add(0);
        levels.Add(0);
        if (dataset.GridDefinitionSection.GridDefinition is LatLonGridDefinition gd)
        {
            var lonMin = gd.Lo1;
            var lonMax = gd.Lo2;
            if (lonMin > lonMax)
            {
                lonMin -= 360;
            }


            gf.type = 1;
            gf.dnum[0] = (int)gd.Nx;
            gf.grvals[0] = new double[] { gd.Dx, lonMin - gd.Dx, -999.9 };
            gf.abvals[0] = new double[] { 1.0 / gd.Dx, -1.0 * ((lonMin - gd.Dx) / gd.Dx), -999.9 };
            gf.ab2gr[0] = GaUtil.liconv;
            gf.gr2ab[0] = GaUtil.liconv;
            gf.linear[0] = 1;

            double v1, v2;

            v2 = gf.grvals[0][0];
            v1 = gf.grvals[0][1] + v2;
            double temp = v1 + ((double)(gf.dnum[0])) * v2;
            temp = temp - 360.0;
            if (Math.Abs(temp - v1) < 0.01) gf.wrap = 1;

            gf.dnum[1] = (int)gd.Ny;
            gf.grvals[1] = new double[] { gd.Dy, gd.La1 - gd.Dy, -999.9 };
            gf.abvals[1] = new double[] { 1.0 / gd.Dy, -1.0 * ((gd.La1 - gd.Dy) / gd.Dy), -999.9 };
            gf.ab2gr[1] = GaUtil.liconv;
            gf.gr2ab[1] = GaUtil.liconv;
            gf.linear[1] = 1;


            gf.dnum[2] = levels.Count;
            gf.abvals[2] = levels.ToArray();
            gf.grvals[2] = levels.ToArray();
            gf.ab2gr[2] = GaUtil.lev2gr;
            gf.gr2ab[2] = GaUtil.gr2lev;
            gf.linear[2] = 0;

            var dt = dataset.Message.IdentificationSection.ReferenceTime;
            var vals = new double[] { dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 1, 0, 0 };
            gf.dnum[3] = 1; //TODO this can be multiple timesteps too
            gf.grvals[3] = vals;
            gf.abvals[3] = vals;
            gf.linear[3] = 1;

            gf.dnum[4] = 1;
            v1 = 1;
            v2 = 1;
            vals = new double[] { v2, v2 - v1, -999.9 };
            gf.grvals[4] = vals;
            vals = new double[] { -1.0 * ((v1 - v2) / v2), 1.0 / v2, -999.9 };
            gf.abvals[4] = vals;
            gf.linear[4] = 1;
            gf.ab2gr[4] = GaUtil.liconv;
            gf.gr2ab[4] = GaUtil.liconv;

            gf.gsiz = gf.dnum[0] * gf.dnum[1];
            if (gf.ppflag > 0) gf.gsiz = gf.ppisiz * gf.ppjsiz;
            /* add the XY header/trailer to gsiz */
            // if (pfi.xyhdr) {
            //     if (pvar.dfrm == 1) {
            //         pfi.xyhdr = pfi.xyhdr * 4 / 1;
            //     } else if (pvar.dfrm == 2 || pvar.dfrm == -2) {
            //         pfi.xyhdr = pfi.xyhdr * 4 / 2;
            //     }
            //     pfi.gsiz = pfi.gsiz + pfi.xyhdr;
            // }
        }

        if (_drawingContext.CommonData.pfi1 == null)
        {
            _drawingContext.CommonData.pfi1 = new List<gafile>();
        }

        _drawingContext.CommonData.pfi1.Add(gf);
        _drawingContext.CommonData.fnum = _drawingContext.CommonData.pfi1.Count;
        if (_drawingContext.CommonData.fnum == 1)
        {
            _drawingContext.CommonData.pfid = gf;
            _drawingContext.CommonData.dfnum = 1;
        }


        foreach (var ds in datasets)
        {
            if (ds.Parameter != null)
            {
                string name = ds.Parameter.Value.Name;
                var ps = ds.ProductDefinitionSection.ProductDefinition as ProductDefinition0000;
                FixedSurfaceType sfcType = (FixedSurfaceType)ps.FirstFixedSurfaceType;
                var heightValue = ps.FirstFixedSurfaceValue;


                gavar gv = new gavar();

                gv.variableDefinition = new VariableDefinition
                {
                    HeightType = sfcType,
                    HeightValue = heightValue ?? Double.MinValue,
                    VariableName = gv.abbrv,
                    VariableType = GetVarType(name)
                };
                gv.abbrv = gv.variableDefinition.GetVarName();
                gf.pvar1.Add(gv);
            }
        }

        gf.vnum = gf.pvar1.Count;

        gf.DataReader = new GfsGribDataReader();
    }


    public void Define()
    {
    }

    public int Display(string variable)
    {
        gastat? pst;
        gafile pfi;

        Func<double[], double, double>? conv = null;
        double[] vals;
        double v;
        double xl, yl, s1, s2;
        int llen, rcode, labsv;
        int l, l1, l2, vcnt, i, lflg, ldim, rc;
        string lab;

        int[] dcolor = { -1, 1, 3, 7, 2, 6, 9, 10, 11, 12 };
        // if (pcm.impflg > 0)
        // {
        //     gacmd(pcm.impnam, pcm, 0);
        // }

        rcode = 1;
        if (String.IsNullOrEmpty(variable))
        {
            GaGx.gaprnt(0, "Display command error:  No expression provided");
            return (1);
        }

        //garemb(cmd);

        pst = getpst(pcm);
        if (pst == null) return (1);

        vcnt = 0;
        for (i = 0; i < 5; i++)
            if (pcm.vdim[i] > 0)
                vcnt++;
        lflg = pcm.loopflg;
        if (vcnt > 2) lflg = 1;
        pcm.pass = pcm.pass % Int32.MaxValue;
        if (lflg == 0)
        {
            /* not looping */
            pcm.mcolor = 8;
            if (pcm.ccolor == -9)
            {
                if (vcnt == 2)
                    pcm.ccolor = dcolor[pcm.pass % 10];
                else
                    pcm.ccolor = dcolor[(pcm.pass + 1) % 10];
            }

            if (pcm.cmark == -9)
            {
                pcm.cmark = pcm.pass + 2;
                while (pcm.cmark > 5) pcm.cmark -= 5;
            }

            if (pcm.ccolor < 0) pcm.mcolor = 15;
            rc = gapars(variable, pst, pcm);
            if (rc > 0) goto retrn;
            _drawingContext.GaGx.gaplot();
            gagrel();
            pcm.pass++;
            pcm.ccolor = -9;
            pcm.cstyle = -9;
            pcm.cmark = -9;
            pcm.cint = 0.0;
            pcm.cflag = 0;
            pcm.ccflg = 0;
            pcm.cmin = -9.99e33;
            pcm.cmax = 9.99e33;
            pcm.blkflg = false;
            pcm.rainmn = 0.0;
            pcm.rainmx = 0.0;
            pcm.arrflg = false;
            pcm.ptflg = 0;
            pcm.xexflg = 0;
            pcm.yexflg = 0;
        }

        else
        {
            /* looping */
            pcm.mcolor = 15;
            if (pcm.ccolor == -9)
            {
                pcm.ccolor = -1;
                if (vcnt < 2) pcm.ccolor = 1;
            }

            if (pcm.cmark == -9)
            {
                pcm.cmark = pcm.pass + 2;
                while (pcm.cmark > 5) pcm.cmark -= 5;
            }

            pfi = pcm.pfid;
            ldim = pcm.loopdim;
            if (pfi.type > 1 && ldim < 3)
            {
                GaGx.gaprnt(0, "Display command error:  Invalid looping environment");
                GaGx.gaprnt(0, "  Cannot loop on stn data through X, Y, or Z");
                return (1);
            }

            if (ldim == 3)
            {
                /* loop on T */
                vals = pfi.abvals[3];
                v = GaUtil.t2gr(vals, pcm.tmin);
                l1 = (int)v;
                v = GaUtil.t2gr(vals, pcm.tmax);
                l2 = (int)(v + 0.999);
            }
            else
            {
                /* loop on X, Y, Z or E */
                conv = pfi.ab2gr[ldim];
                vals = pfi.abvals[ldim];
                v = conv(vals, pcm.dmin[ldim]);
                l1 = (int)v;
                v = conv(vals, pcm.dmax[ldim]);
                l2 = (int)(v + 0.999);
            }

            vals = pfi.grvals[ldim];
            if (ldim != 3) conv = pfi.gr2ab[ldim];
            _drawingContext.GaSubs.gxfrme(2);
            pcm.pass = 0;
            labsv = pcm.clab;
            for (l = l1; l <= l2; l++)
            {
                if (ldim == 3)
                {
                    GaUtil.gr2t(vals, (double)l, out pst.tmin);
                    pst.tmax = pst.tmin;
                }
                else
                {
                    pst.dmin[ldim] = conv(vals, (double)l);
                    pst.dmax[ldim] = pst.dmin[ldim];
                }

                rc = gapars(variable, pst, pcm);
                if (rc > 0) goto retrn;
                pcm.clab = 0;
                if (l == l2) pcm.clab = labsv;
                _drawingContext.GaGx.gaplot();
                if (ldim == 3)
                {
                    lab = $"{pst.tmin.yr}:{pst.tmin.mo}:{pst.tmin.dy}:{pst.tmin.hr}";
                }
                else
                {
                    lab = pst.dmin[ldim].ToString("g2");
                }

                llen = lab.Length;

                xl = pcm.xsiz - (0.11 * (double)(llen));
                xl -= 0.02;
                yl = 0.02;
                s1 = 0.13;
                s2 = 0.11;
                _drawingContext.GaSubs.gxwide(1);
                _drawingContext.GaSubs.gxchpl(lab, llen, xl, yl, s1, s2, 0.0);
                _drawingContext.GaSubs.gxfrme(2);
                gagrel();
                pcm.aflag = -1;
                pcm.aflag2 = -1;
            }

            pcm.dbflg = 0;
            pcm.pass = 0;
            pcm.ccolor = -9;
            pcm.cstyle = -9;
            pcm.cmark = -9;
            pcm.cint = 0.0;
            pcm.cflag = 0;
            pcm.ccflg = 0;
            pcm.cmin = -9.99e33;
            pcm.cmax = 9.99e33;
            pcm.blkflg = false;
            pcm.rainmn = pcm.rainmx = 0.0;
            pcm.aflag = 0;
            pcm.aflag2 = 0;
            pcm.axflg = 0;
            pcm.ayflg = 0;
            pcm.grdsflg = true;
            pcm.arrflg = false;
            pcm.ptflg = 0;
            pcm.xexflg = 0;
            pcm.yexflg = 0;
        }

        return (0);

        retrn:

        return (1);
    }

    private int gapars(string cmd, gastat pst, gacmn pcm)
    {
        int pos;
        string expr;
        int num, i, rc;

        expr = cmd.ToLower();

        /* Convert all the ;'s to nulls and count the number of
         sub-expressions.                                           */

        string[] cmds = expr.Split(';');


        /* Evaluate all the subexpressions */

        for (i = 0; i < cmds.Length; i++)
        {
            rc = _drawingContext.GaExpr.gaexpr(cmds[i], pst);
            if (rc == 0) rc = _drawingContext.CommonData.sig;
            if (rc > 0) goto err;
            pcm.type[i] = pst.type;
            pcm.result[i] = pst.result;
        }

        pcm.numgrd = cmds.Length;
        pcm.relnum = cmds.Length;

        return (0);

        err:
        GaGx.gaprnt(0, "DISPLAY error:  Invalid expression \n");
        GaGx.gaprnt(0, "  Expression = ");
        GaGx.gaprnt(0, cmds[i]);
        GaGx.gaprnt(0, "\n");
        pcm.numgrd = i;
        pcm.relnum = i;
        gagrel();
        return (1);
    }

    void gagrel()
    {
        int i;

        for (i = 0; i < pcm.relnum; i++)
        {
            if (pcm.type[i] == 1)
                pcm.result[i].pgr = null;
            else
                pcm.result[i].stn = null;
        }

        pcm.numgrd = 0;
        pcm.relnum = 0;
    }

    private gastat? getpst(gacmn pcm)
    {
        gastat pst = new gastat();
        int i, vcnt, lflg, ll;


        pst.pfi1 = pcm.pfi1;
        pst.pfid = pcm.pfid;
        pst.fnum = pcm.fnum;
        //pst.pclct = (struct gaclct **) &(pcm.clct);
        for (i = 0; i < 5; i++)
        {
            if (i == 3)
            {
                pst.tmin = pcm.tmin;
                pst.tmax = pcm.tmax;
            }
            else
            {
                pst.dmin[i] = pcm.dmin[i];
                pst.dmax[i] = pcm.dmax[i];
            }
        }

        pst.type = 1;
        pst.result.pgr = null;
        pst.pdf1 = pcm.pdf1;

        vcnt = 0;
        for (i = 0; i < 5; i++)
            if (pcm.vdim[i] > 0)
                vcnt++;
        lflg = pcm.loopflg;
        if (vcnt > 2) lflg = 1;

        ll = 0;
        pst.idim = -1;
        pst.jdim = -1;
        if (pcm.vdim[0] > 0)
        {
            /* X is varying */
            if (pcm.dmin[0] > pcm.dmax[0])
            {
                GaGx.gaprnt(0, "Operation error:  Invalid dimension environment\n");
                GaGx.gaprnt(0, $"  Min longitude > max longitude: {pcm.dmin[0]} {pcm.dmax[0]} \n");
                goto err;
            }

            if (pcm.loopdim != 0 || lflg == 0)
            {
                pst.idim = 0;
                ll++;
            }
        }

        if (pcm.vdim[1] > 0)
        {
            /* Y is varying */
            if (pcm.dmin[1] > pcm.dmax[1])
            {
                GaGx.gaprnt(0, "Operation error:  Invalid dimension environment\n");
                GaGx.gaprnt(0, $"  Min longitude > max longitude: {pcm.dmin[1]} {pcm.dmax[1]} \n");
                goto err;
            }

            if (pcm.loopdim != 1 || lflg == 0)
            {
                if (ll > 0)
                    pst.jdim = 1;
                else
                    pst.idim = 1;
                ll++;
            }
        }

        if (pcm.vdim[2] > 0)
        {
            /* Z is varying */
            if (pcm.loopdim != 2 || lflg == 0)
            {
                if (ll > 0)
                    pst.jdim = 2;
                else
                    pst.idim = 2;
                ll++;
            }
        }

        if (pcm.vdim[3] > 0)
        {
            /* T is varying */
            if (pcm.loopdim != 3 || lflg == 0)
            {
                if (ll > 0)
                    pst.jdim = 3;
                else
                    pst.idim = 3;
                ll++;
            }
        }

        if (pcm.vdim[4] > 0)
        {
            /* E is varying */
            if (pcm.loopdim != 4 || lflg == 0)
            {
                if (ll > 0)
                    pst.jdim = 4;
                else
                    pst.idim = 4;
                ll++;
            }
        }

        if (lflg > 0 && (vcnt == ll))
        {
            GaGx.gaprnt(0, "Operation error:  Invalid dimension environment\n");
            GaGx.gaprnt(0, "  Looping dimension does not vary\n");
            goto err;
        }

        if (ll > 2)
        {
            GaGx.gaprnt(0, "Operation error:  Invalid dimension environment\n");
            GaGx.gaprnt(0, "  Too many varying dimensions \n");
            goto err;
        }

        return (pst);

        err:

        return (null);
    }


    /* handle draw command */

    static double[] justx = { 0.0, 0.5, 1.0, 0.0, 0.5, 1.0, 0.0, 0.5, 1.0 };
    static double[] justy = { 0.0, 0.0, 0.0, 0.5, 0.5, 0.5, 1.0, 1.0, 1.0 };


    public int DrawString(double x, double y, string text)
    {
        _drawingContext.GaSubs.gxwide(pcm.strthk);
        _drawingContext.GaSubs.gxcolr(pcm.strcol);

        double swide = 0.2;
        _drawingContext.GxChpl.gxchln(text, text.Length, pcm.strhsz, out swide);
        double shite = pcm.strvsz;

        double ang = pcm.strrot * Math.PI / 180.0;
        x = x - justx[pcm.strjst] * swide * Math.Cos(ang);
        y = y - justx[pcm.strjst] * swide * Math.Sin(ang);
        x = x - justy[pcm.strjst] * shite * Math.Cos(ang + 1.5708);
        y = y - justy[pcm.strjst] * shite * Math.Sin(ang + 1.5708);

        _drawingContext.GaSubs.gxchpl(text, text.Length, x, y, pcm.strvsz, pcm.strhsz, pcm.strrot);
        return (0);

        errst:
        GaGx.gaprnt(0, "DRAW error: Syntax is DRAW STRING x y string\n");
        return (1);
    }

    /* Execute command on behalf of the scripting language.
   Return the result in a dynamically allocated buffer */
    public string gagsdo(string cmd, out int rc)
    {
        int savflg, tlen, i;
        gacmn pcm = _drawingContext.CommonData;
        
        // savflg = msgflg;
        // msgflg = 1;          /* Buffer messages */
        //pcm = savpcm;        /* Get common pointer */
        //msgstk = null;

        rc = gacmd(cmd, pcm, 0);

        /* Set up output buffer */

        // if (msgstk == null)
        // {
        //     msgflg = savflg;
        //     return (null);
        // }
        //
        // tlen = 0;
        // msgcurr = msgstk;
        // while (msgcurr)
        // {
        //     tlen += msgcurr.len;
        //     msgcurr = msgcurr.forw;
        // }
        //
        // mbuf = (char*)malloc(tlen + 1);
        // if (mbuf == null)
        // {
        //     printf("Memory allocation error: Message Return Buffer\n");
        //     msgflg = savflg;
        //     return (null);
        // }
        //
        // msgcurr = msgstk;
        // ch = mbuf;
        // while (msgcurr != null)
        // {
        //     for (i = 0; i < msgcurr.len; i++)
        //     {
        //         cmd[ch] = *(msgcurr.msg + i);
        //         ch++;
        //     }
        //
        //     msgcurr = msgcurr.forw;
        // }
        //
        // msgcurr = msgstk;
        // while (msgcurr != null)
        // {
        //     if (msgcurr.msg) free(msgcurr.msg);
        //     msgstk = msgcurr.forw;
        //     free(msgcurr);
        //     msgcurr = msgstk;
        // }
        //
        // *(mbuf + tlen) = '\0';
        // msgflg = savflg;
        // return (mbuf);
        return "";
    }

    int gacmd(string com, gacmn pcm, int exflg)
    {
        gafile? pfi, pfi2;
        gadefn? pdf, pdf2;
        //gaclct *clct, *clct2;
        int rc, reinit, fnum, i, j, len, retcod, flag, xin, yin, bwin, fmtflg, tcolor, pos, size;
        double border;
        string cc;
        string cmd;
        string? rslt;
        int ch;

        string[] formats = { "EPS", "PS", "PDF", "SVG", "PNG", "GIF", "JPG" };
        Stream? pdefid = null;

        //gaiomg();   /* enable interpolation message */

        cmd = com;
        int cmdpos = 0;
        int compos = 0;
        while (cmd[cmdpos] == ' ') cmdpos++;
        while (com[compos] == ' ') compos++;

        retcod = 0;

        /* Check for implied define */
        flag = 0;
        if (cmd[cmdpos] >= 'a' && cmd[cmdpos] <= 'z')
        {
            i = 0;
            ch = cmdpos;
            /* defined variable names are alphanumeric, and must start with a letter */
            while ((cmd[ch] >= 'a' && cmd[ch] <= 'z') || (cmd[ch] >= '0' && cmd[ch] <= '9'))
            {
                i++;
                if (i > 16) break;
                ch++;
            }

            if (i < 17)
            {
                while (cmd[ch] == ' ') ch++;
                if (cmd[ch] == '=')
                {
                    flag = 1;
                    ch++;
                    while (cmd[ch] == ' ') ch++;
                }
            }

            if (flag > 0)
            {
                if (pcm.pfid == null)
                {
                    GaGx.gaprnt(0, "DEFINE error:  no file open yet\n");
                    retcod = 1;
                    goto retrn;
                }

                string[] cparts = cmd.Substring(ch).Split('=');
                retcod = gadef(cparts[0], cparts[1], 1);
                goto retrn;
            }
        }

        /* OpenGrADS User Defined Extensions */

        retcod = (int)gaudc(com);
        if (retcod >= 0) goto retrn; /* found a udc, all done */
        else retcod = 0; /* not found, keep going */


        /* if command is NOT clear ...*/
        if (!("clear" == cmd || "c" ==cmd))
        {
            _drawingContext.GaSubs.gxfrme(9); /* ... clear the X request buffer */
        }

        if (compos == com.Length || com[compos] == '\n') goto retrn;

        cmd = cmd.Substring(cmdpos);
        string[] parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (cmd[cmdpos] == '!')
        {
            Process.Start(com.Substring(1)).WaitForExit();
            goto retrn;
        }
       
        else if (cmd.StartsWith("reset") || cmd.StartsWith("reinit"))
        {
            reset(cmd.StartsWith("reinit"));
            goto retrn;
        }
        else if (cmd.StartsWith("close"))
        {
            close();
            goto retrn;
        }
        else if (cmd.StartsWith("clear") || cmd.StartsWith("c"))
        {
            rc = 0;
            if (parts.Length>1)
            {
                rc = 99;
                if ("norset" == parts[1]) rc = 1; /* clears without resetting user options */
                if ("events" == parts[1]) rc = 2; /* flushes the Xevents buffer */
                if ("graphics" == parts[1]) rc = 3; /* clears the graphics but not the widgets */
                if ("hbuff" == parts[1]) rc = 4; /* clears the buffered display and turns off double buffer mode */
                if ("button" == parts[1]) rc = 5; /* clears a button */
                if ("rband" == parts[1]) rc = 6; /* clears a rubber band */
                if ("dropmenu" == parts[1]) rc = 7; /* clears a dropmenu */
                if ("sdfwrite" == parts[1]) rc = 8; /* clears sdfwrite filename and attributes only */
                if ("mask" == parts[1]) rc = 9; /* clears contour label mask */
                if ("shp" == parts[1]) rc = 10; /* clears shapefile output filename and attributs only */
            }

            if (rc == 99)
            {
                GaGx.gaprnt(0, "Invalid option on clear command\n");
                goto retrn;
            }

            
            clear((ClearAction)rc);
            goto retrn;
        }
        else if ("swap" == cmd)
        {
            if (pcm.dbflg>0) _drawingContext.GaSubs.gxfrme(2);
            gacln(1);
            goto retrn;
        }
        else if (cmd.StartsWith("exec"))
        {
            retcod = gaexec(com, pcm);
            goto retrn;
        }
        else if (cmd.StartsWith("run"))
        {
            if (parts.Length == 1)
            {
                GaGx.gaprnt(0, "RUN error:  No file name specified\n");
                retcod = 1;
                goto retrn;
            }

            rslt = _drawingContext.GScript.gsfile(cmd, out rc, 0);
            if (rc == 0 && rslt != null) GaGx.gaprnt(2, rslt);
            retcod = rc;
            goto retrn;
        }
        else if (cmd.StartsWith("draw"))
        {
            if (parts.Length > 1)
            {
                if (parts[1] == "string")
                {
                    if (parts.Length == 5)
                    {
                        _drawingContext.GaUser.DrawString(Convert.ToDouble(parts[2]), Convert.ToDouble(parts[3]), String.Join(' ', parts.Skip(4).ToArray()));    
                    }
                    
                }
            }
            _drawingContext.GaSubs.gxsignal(1); /* tell rendering layer and metabuffer draw is done */
            _drawingContext.GaSubs.gxfrme(9); /* flush X request buffer */
            goto retrn;
        }
        else if (parts[0] == "gxprint" || parts[0] == "printim")
        {
            /* check for output file name */
            if (parts.Length == 1)
            {
                GaGx.gaprnt(0, "GXPRINT error:  missing output file name\n");
                retcod = 1;
                goto retrn;
            }

            cc = parts[1]; /* copy output file name into cc */

            /* advance past the output file name in mixed and lower case versions of the command */
            
            /* initialize */
            xin = -999;
            yin = -999;
            bwin = -999;
            fmtflg = 0;
            bgImage = "";
            fgImage = "";
            tcolor = -1;
            border = -1.0;

            int partpos = 2;

            /* parse the user-provided options */
            while (partpos < parts.Length)
            {
                /* set backgroud/foreground colors */
                string p = parts[partpos];
                if ("black"== p) bwin = 0;
                else if ("white"== p) bwin = 1;
                /* explicit format types -- the union of all possibilities from all rendering engines */
                else if ("eps"== p) fmtflg = 1;
                else if ("ps"== p) fmtflg = 2;
                else if ("pdf"== p) fmtflg = 3;
                else if ("svg"== p) fmtflg = 4;
                /* ... image formats start here ... */
                else if ("png"== p) fmtflg = 5;
                else if ("gif"== p) fmtflg = 6;
                else if ("jpg"== p) fmtflg = 7;
                else if ("jpeg"== p) fmtflg = 7;
                /* get background image filename */
                else if ("-b" == p)
                {
                    partpos++;
                    if (partpos == parts.Length)
                    {
                        GaGx.gaprnt(1, "GXPRINT warning: Foreground image file name not provided\n");
                        break;
                    }

                    bgImage = parts[partpos];
                    partpos++;
                }
                /* get foreground image filename */
                else if ("-f"== p)
                {
                    partpos++;
                    if (partpos == parts.Length)
                    {
                        GaGx.gaprnt(1, "GXPRINT warning: Foreground image file name not provided\n");
                        break;
                    }

                    fgImage = parts[partpos];
                    partpos++;


                }
                /* set transparent color number */
                else if ("-t" == p)
                {
                    partpos++;
                    if (partpos == parts.Length)
                    {
                        GaGx.gaprnt(1, "GXPRINT warning: Missing transparent color number\n");
                        break;
                    }

                    /* set transparent color number */
                    tcolor = Convert.ToInt32(parts[partpos++]);
                }
                /* set horizontal image size */
                else if (p[0] == 'x')
                {
                    try
                    {
                        xin = Convert.ToInt32(p.Substring(1));
                    }
                    catch (Exception ex)
                    {
                        GaGx.gaprnt(0, "GXPRINT error:  Invalid x option; ignored\n");
                        xin = -999;
                    }

                    partpos++;
                }
                /* set vertical image size */
                else if (p[0] == 'y')
                {
                    try
                    {
                        yin = Convert.ToInt32(p.Substring(1));
                    }
                    catch (Exception ex)
                    {
                        GaGx.gaprnt(0, "GXPRINT error:  Invalid y option; ignored\n");
                        yin = -999;
                    }
                    partpos++;
                }
                /* set width of border around the edge of the plot */
                else if ("-e"== p)
                {
                    partpos++;
                    if (parts.Length == partpos)
                    {
                        GaGx.gaprnt(1, "GXPRINT warning: Missing edge width \n");
                        break;
                    }

                    try
                    {
                        border = Convert.ToDouble(parts[partpos++]);
                    }
                    catch (Exception ex)
                    {
                        GaGx.gaprnt(1, "GXPRINT warning: Invalid edge width \n");
                    }
                }
                else
                {
                    GaGx.gaprnt(0, "GXPRINT error: Invalid option; ignored\n");
                }
            }
            
            printim(cc, (OutputFormat)fmtflg, bgImage, fgImage, (BackgroundColor)bwin, tcolor, xin, yin, border);
            
            goto retrn;
        }
        else if (cmd.ToLower().StartsWith("set"))
        {
            retcod = gaset(cmd, com, pcm);
            goto retrn;
        }
        else if ("open"== parts[0].ToLower())
        {
            //Open();
            
            if (parts.Length == 1)
            {
                GaGx.gaprnt(0, "OPEN error:  missing data description file name\n");
                retcod = 1;
                goto retrn;
            }

            cc = parts[1];
            Open(cc, DataFormat.Unknown);
            
            goto retrn;

        }
        // else if ("sdfopen"== p)
        // {
        //     if ((cmd = nxtwrd(com)) == null)
        //     {
        //         gaprnt(0, "SDFOPEN error:  missing self-describing file pathname\n");
        //         retcod = 1;
        //         goto retrn;
        //     }
        //
        //     retcod = gasdfopen(cmd, pcm);
        //     if (!retcod) mygreta(cmd); /* (for IGES only) keep track of user's opened files */
        //     goto retrn;
        // }
        // else if ("xdfopen"== p)
        // {
        //     if ((cmd = nxtwrd(com)) == null)
        //     {
        //         gaprnt(0, "XDFOPEN error:  missing data descriptor file name\n");
        //         retcod = 1;
        //         goto retrn;
        //     }
        //
        //     retcod = gaxdfopen(cmd, pcm);
        //     if (!retcod) mygreta(cmd); /* (for IGES only) keep track of user's opened files */
        //     goto retrn;
        // }
        else if ("d"== parts[0].ToLower() || "display" == parts[0].ToLower())
        {
            if (pcm.pfid == null)
            {
                GaGx.gaprnt(0, "DISPLAY error:  no file open yet\n");
                retcod = 1;
                goto retrn;
            }

            retcod = Display(string.Join(' ', parts.Skip(1).ToArray()));
            _drawingContext.GaSubs.gxsignal(1); /* tell rendering engine and metafile buffer display is finished */
            _drawingContext.GaSubs.gxfrme(9); /* flush any buffers as needed */
            goto retrn;
        }
        // else if ("coll", cmd) || "collect", cmd))
        // {
        //     if (pcm.pfid == null)
        //     {
        //         gaprnt(0, "COLLECT error:  no file open yet\n");
        //         retcod = 1;
        //         goto retrn;
        //     }
        //
        //     retcod = gacoll(cmd, pcm);
        //     goto retrn;
        // }
        else if (parts[0].ToLower() == "define")
        {
            if (pcm.pfid == null)
            {
                GaGx.gaprnt(0, "DEFINE error:  no file open yet\n");
                retcod = 1;
                goto retrn;
            }

            string[] def = parts[1].Split('=');
            retcod = gadef(def[0], def[1], 0);
            goto retrn;
        }
        else if (parts[0].ToLower() == "undefine")
        {
            if (pcm.pfid == null)
            {
                GaGx.gaprnt(0, "DEFINE error: no file open yet\n");
                retcod = 1;
                goto retrn;
            }

            retcod = gaudef(cmd, pcm);
            goto retrn;
        }
        else if (parts[0].ToLower() == "modify")
        {
            if (pcm.pfid == null)
            {
                GaGx.gaprnt(0, "MODIFY error: no file open yet\n");
                retcod = 1;
                goto retrn;
            }

            retcod = gamodf(cmd, pcm);
            goto retrn;
        }
//         else if ("sdfwrite", cmd))
//         {
//             if (pcm.pdf1 == null)
//             {
//                 gaprnt(0, "SDFWRITE error: no defined variables\n");
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             retcod = ncwrite(cmd, pcm);
//             goto retrn;
//         }
//         else if ("pdefwrite", cmd))
//         {
//             if (pcm.pfid == null)
//             {
//                 gaprnt(0, "PDEFWRITE error: no file open yet\n");
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             if (pcm.pfid.ppflag == 0)
//             {
//                 gaprnt(0, "PDEFWRITE error: default file does not use PDEF\n");
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             if (pcm.pfid.ppflag == 7 || pcm.pfid.ppflag == 8)
//             {
//                 gaprnt(0, "PDEFWRITE error: default file already has an external PDEF file\n");
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             /* Get the name of the output file and open it */
//             if ((cmd = nxtwrd(cmd)) != null)
//             {
//                 if (strlen(cmd) < 256)
//                 {
//                     getwrd(pdefname, cmd, 255);
//                 }
//                 else
//                 {
//                     gaprnt(0, "PDEFWRITE error: name of output file is missing \n");
//                     retcod = 1;
//                     goto retrn;
//                 }
//             }
//
//             if ((pdefid = fopen(pdefname, "w")) == null)
//             {
//                 gaprnt(0, "PDEFWRITE error: failed to open output file \n");
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             /* allocate memory for grids to be written out to file */
//             size = pcm.pfid.dnum[0] * pcm.pfid.dnum[1];
//             if ((ivals = (float*)galloc(size * sizeof(float), "ppivals")) == null)
//             {
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             if ((jvals = (float*)galloc(size * sizeof(float), "ppjvals")) == null)
//             {
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             if ((rvals = (float*)galloc(size * sizeof(float), "pprvals")) == null)
//             {
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             ival = ivals; /* save these initial pointers for the fwrite commands */
//             jval = jvals;
//
//             /* Fill grids of file offsets and weights (dx,dy) calculated for pdef grid interpolation */
//             ioff = pcm.pfid.ppi[0];
//             dx = pcm.pfid.ppf[0];
//             dy = pcm.pfid.ppf[1];
//             for (j = 0; j < pcm.pfid.dnum[1]; j++)
//             {
//                 for (i = 0; i < pcm.pfid.dnum[0]; i++)
//                 {
//                     if (*ioff == -1)
//                     {
//                         *ivals = -999;
//                         *jvals = -999;
//                     }
//                     else
//                     {
//                         rem = *ioff % (pcm.pfid.ppisiz); /* 0-based x index */
//                         *ivals = rem + 1 + (float)*dx; /* make x index 1-based and add the interp wgt */
//                         *jvals = ((*ioff - rem) / pcm.pfid.ppisiz) + 1 +
//                                  (float)*dy; /* 1-based y index plus interp wgt */
//                     }
//
//                     ioff++;
//                     dx++;
//                     dy++;
//                     ivals++;
//                     jvals++;
//                 }
//             }
//
//             rc = fwrite(ival, sizeof(float), size, pdefid);
//             if (rc != size)
//             {
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             rc = fwrite(jval, sizeof(float), size, pdefid);
//             if (rc != size)
//             {
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             /* Fill the grid of rotation values, then write it out */
//             if (pcm.pfid.ppwrot)
//                 for (i = 0; i < size; i++)
//                     *(rvals + i) = (float)(*(pcm.pfid.ppw + i));
//             else
//                 for (i = 0; i < size; i++)
//                     *(rvals + i) = -999;
//             rc = fwrite(rvals, sizeof(float), size, pdefid);
//             if (rc != size)
//             {
//                 retcod = 1;
//                 goto retrn;
//             }
//
//             fclose(pdefid);
//             snprintf(pout, 1255, "pdef %d %d bilin stream binary-", pcm.pfid.ppisiz, pcm.pfid.ppjsiz);
//             gaprnt(2, pout);
// #if BYTEORDER == 1
//             snprintf(pout, 1255, "big");
// #else
//         snprintf(pout, 1255, "little");
// #endif
//             gaprnt(2, pout);
//             snprintf(pout, 1255, " ^%s\n", pdefname);
//             gaprnt(2, pout);
//
//             if (ival != null) gree(ival, "f194a");
//             if (jval != null) gree(jval, "f194a");
//             if (rvals != null) gree(rvals, "f194a");
//
//             retcod = 0;
//             goto retrn;
//         }
        else
        {
            if (pcm.impcmd>0)
            {
                rslt = _drawingContext.GScript.gsfile(com, out rc, 1);
                if (rc == 0 && rslt != null) GaGx.gaprnt(2, rslt);
                retcod = rc;
                goto retrn;
            }

            GaGx.gaprnt(0, "Unknown command: ");
            GaGx.gaprnt(0, cmd);
            GaGx.gaprnt(0, "\n");
            retcod = 1;
            goto retrn;
        }

        retrn:
       
        return (retcod);
    }

int gaset(string cmd, string com)
{
    var pcm = _drawingContext.CommonData;
    dt tdef;
    gafile? pfi;
    gaens? ns;
    gaattr? attr, newattr = null;
    Func<double[], double, double>? conv;
    double v1, v2, tt; 
    double[] vals;
    double val1, val2, xlo, xhi, ylo, yhi;
    int kwrd, i, i1, i2, num, id, itt, itt1, itt2;
    int[] itmp = new int[20];
    int enum1, enum2;
    int xx, yy, red, green, blue;
    int? ch = null, ch2 = null;
    int strng, pat, cmd1;
    string ename1, ename2;
    long sz;
    string? tileimg = null;
    string[] kwds = {"X", "Y", "Z", "T", "LON", "LAT", "LEV", "TIME",
                              "CINT", "CSTYLE", "CCOLOR", "LOOPDIM",
                              "LOOPING", "LOOPINCR", "DFILE", "VRANGE",
                              "CSMOOTH", "GRID", "CMARK", "XAXIS", "YAXIS",
                              "GXOUT", "BLACK", "DIGNUM", "DIGSIZ", "CMIN",
                              "CMAX", "ARRSCL", "ARRSIZ", "CLEVS", "STID",
                              "GRADS", "CLAB", "MPROJ", "CTERP", "XSIZE",
                              "CCOLS", "MPVALS", "MPDSET", "VPAGE", "PAREA",
                              "LINE", "STRING", "STRSIZ", "RGB", "FGVALS",
                              "MAP", "BARBASE", "BARGAP", "CTHICK", "MPDRAW",
                              "POLI", "DBUFF", "XYREV", "XFLIP", "YFLIP",
                              "ANNOT", "DISPLAY", "BACKGROUND", "RBCOLS",
                              "RBRANGE", "MISSCONN", "IMPRUN", "ZLOG", "STRMDEN",
                              "FRAME", "CLIP", "VRANGE2", "ARROWHEAD", "MDLOPTS",
                              "XLOPTS", "YLOPTS", "CLOPTS", "XLAB", "YLAB",
                              "XLEVS", "YLEVS", "XLINT", "YLINT", "MISSWARN",
                              "BUTTON", "DEFVAL", "BAROPTS", "LFCOLS", "WXCOLS",
                              "FONT", "FWRITE", "XLPOS", "YLPOS", "CLSKIP", "RBAND",
                              "ARRLAB", "MPT", "WXOPT", "XLABS", "YLABS", "FILL",
                              "DROPMENU", "LATS", "TIMELAB", "WARN", "STNPRINT",
                              "STAT", "TLSUPP", "GRIDLN", "HEMPREF", "PRNOPTS",
                              "DATAWARN", "DIALOG", "WRITEGDS", "COSLAT",
                              "E", "ENS", "SDFWRITE", "SDFATTR", "GEOTIFF", "KML",
                              "UNDEF", "CHUNKSIZE", "CACHESF", "SHPOPTS",
                              "SHP", "SHPATTR", "LOG1D", "STRMOPTS", "TILE",
                              "HERSHEY", "LWID", "ANTIALIAS", "BARBOPTS"};

    strng = null;
    kwrd = -1;
    tt = 0;
    pfi = pcm.pfid;
    cmd1 = cmd;
    if ((cmd = nxtwrd(cmd)) == null) {
        gaprnt(0, "SET error:  missing operand\n");
        return (1);
    } else if (cmpwrd("defval", cmd)) {
        i1 = gaqdef(cmd, pcm, 1);
        return (i1);
    } else if (cmpwrd("hempref", cmd)) {
        kwrd = 105;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("auto", cmd)) pcm.hemflg = -1;
        else if (cmpwrd("shem", cmd)) pcm.hemflg = 1;
        else if (cmpwrd("nhem", cmd)) pcm.hemflg = 0;
        else goto err;
    } else if (cmpwrd("gridln", cmd)) {
        kwrd = 104;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        i = 1;
        if (cmpwrd("auto", cmd)) {
            pcm.gridln = -9;
            i = 0;
        }
        if (cmpwrd("off", cmd)) {
            pcm.gridln = -1;
            i = 0;
        }
        if (i) {
            if (intprs(cmd, &itt) == null) goto err;
            pcm.gridln = itt;
        }
        return (0);
    } else if (cmpwrd("tlsupp", cmd)) {
        kwrd = 103;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("yr", cmd)) pcm.tlsupp = 1;
        if (cmpwrd("year", cmd)) pcm.tlsupp = 1;
        if (cmpwrd("mo", cmd)) pcm.tlsupp = 2;
        if (cmpwrd("month", cmd)) pcm.tlsupp = 2;
        return (0);
    } else if (cmpwrd("baropts", cmd)) {
        kwrd = 82;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("outline", cmd)) pcm.barolin = 1;
        if (cmpwrd("filled", cmd)) pcm.barolin = 0;
        return (0);
    } else if (cmpwrd("barbopts", cmd)) {
        kwrd = 129;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("outline", cmd)) pcm.barbolin = 0;
        if (cmpwrd("filled", cmd)) pcm.barbolin = 1;
        return (0);
    } else if (cmpwrd("barbase", cmd)) {
        kwrd = 47;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("bottom", cmd)) {
            pcm.barflg = 0;
            return (0);
        }
        if (cmpwrd("top", cmd)) {
            pcm.barflg = -1;
            return (0);
        }
        if (getdbl(cmd, &pcm.barbase) == null) goto err;
        pcm.barflg = 1;
    } else if (cmpwrd("mdlopts", cmd)) {
        kwrd = 69;
        while ((cmd = nxtwrd(cmd)) != null) {
            i1 = 0;
            if (cmpwrd("noblank", cmd)) {
                pcm.mdlblnk = 0;
                i1 = 1;
            }
            if (cmpwrd("blank", cmd)) {
                pcm.mdlblnk = 1;
                i1 = 1;
            }
            if (cmpwrd("dig3", cmd)) {
                pcm.mdldig3 = 1;
                i1 = 1;
            }
            if (cmpwrd("nodig3", cmd)) {
                pcm.mdldig3 = 0;
                i1 = 1;
            }
            if (i1 == 0) goto err;
        }
    } else if (cmpwrd("bargap", cmd)) {
        kwrd = 48;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if (itt < 0 || itt > 100) {
            gaprnt(0, "SET BARGAP Error: gap must be 0 to 99\n");
            return (1);
        }
        pcm.bargap = itt;
    } else if (cmpwrd("lwid", cmd)) {
        kwrd = 127;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if (itt < 13 || itt > 256) {
            gaprnt(0, "SET LWID Error: line thickness index number must be between 13 and 256\n");
            return (1);
        }
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &v1) == null) goto err;
        gxdbsetwid(itt, v1);
    } else if (cmpwrd("font", cmd)) {
        kwrd = 85;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        /* check if graphics backend supports fonts */
        if (dsubs.gxdckfont() == 0 || psubs.gxpckfont() == 0) {
            if (itt < 0 || itt > 9) {
                if (dsubs.gxdckfont() == 0 && psubs.gxpckfont() == 0)
                    gaprnt(0, "SET FONT Error: Graphics plug-ins only support fonts between 0 and 9\n");
                if (dsubs.gxdckfont() == 0 && psubs.gxpckfont() == 1)
                    gaprnt(0, "SET FONT Error: Graphics display plug-in only supports fonts between 0 and 9\n");
                if (dsubs.gxdckfont() == 1 && psubs.gxpckfont() == 0)
                    gaprnt(0, "SET FONT Error: Graphics printing plug-in only supports fonts between 0 and 9\n");
                return (1);
            }
        } else {
            if (itt < 0 || itt > 99) {
                gaprnt(0, "SET FONT Error: font must be 0 to 99\n");
                return (1);
            }
        }
        gxchdf(itt);  /* change the default font in gxchpl.c */
        if ((cmd = nxtwrd(cmd)) != null) {
            /* check for 'file' keyword */
            if (cmpwrd("file", cmd)) {
                if ((cmd = nxtwrd(cmd)) != null) {
                    /* user has provided a file to define a new font */
                    if (itt < 10) {
                        gaprnt(0, "SET FONT Error: Cannot define new font for font numbers less than 10\n");
                        return (1);
                    } else {
                        /* get the length of the user-supplied filename */
                        itt2 = 0;
                        while (*(cmd + itt2) != '\n' && *(cmd + itt2) != '\0') itt2++; /* spaces are allowed */
                        /* allocate memory for tmp filename */
                        ch = null;
                        ch = (char *) malloc(itt2 + 1);
                        if (ch == null) {
                            gaprnt(0, "Memory allocation error for tmp font file name\n");
                            goto err;
                        }
                        /* copy the user-supplied filename (mixed case) to ch */
                        i2 = cmd - cmd1;
                        for (i1 = 0; i1 < itt2; i1++) *(ch + i1) = *(com + i1 + i2);
                        *(ch + itt2) = '\0';      /* make sure it is null-terminated */
                        gxdbsetfn(itt, ch);
                        free(ch);
                    }
                }
            } else {
                gaprnt(0, "SET FONT Error: Missing keyword \"file\" in new font definition\n");
                return (1);
            }
        }
    } else if (cmpwrd("hershey", cmd)) {
        kwrd = 126;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) gxdbsethersh(0);  /* 0, use hershey fonts */
        else if (cmpwrd("off", cmd)) {
            /* check if graphics backend supports fonts */
            if (dsubs.gxdckfont() == 0 || psubs.gxpckfont() == 0) {
                if (dsubs.gxdckfont() == 0 && psubs.gxpckfont() == 0)
                    gaprnt(0, "SET Error: Graphics plug-ins do not support non-Hershey fonts\n");
                if (dsubs.gxdckfont() == 0 && psubs.gxpckfont() == 1)
                    gaprnt(0, "SET Error: Graphics display plug-in does not support non-Hershey fonts\n");
                if (dsubs.gxdckfont() == 1 && psubs.gxpckfont() == 0)
                    gaprnt(0, "SET Error: Graphics printing plug-in does not support non-Hershey fonts\n");
                return (1);
            } else gxdbsethersh(1);  /* 1, use emulation */
        } else goto err;
    } else if (cmpwrd("antialias", cmd)) {
        kwrd = 128;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) {
            pcm.aaflg = 0;   /* set flag in gacmn */
            gxsignal(2);      /* tell hardware and metabuffer to disable anti-aliasing */
        } else if (cmpwrd("on", cmd)) {
            pcm.aaflg = 1;   /* set flag in gacmn */
            gxsignal(3);      /* tell hardware and metabuffer to enable anti-aliasing */
        } else goto err;
    } else if (cmpwrd("clip", cmd)) {
        kwrd = 66;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &xlo) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &xhi) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &ylo) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &yhi) == null) goto err;
        if (xlo < 0.0 || ylo < 0.0) goto err;
        if (xhi > pcm.pxsize || yhi > pcm.pysize) goto err;
        if (yhi <= ylo || xhi <= xlo) goto err;
        gxclip(xlo, xhi, ylo, yhi);
    } else if (cmpwrd("vpage", cmd)) {
        kwrd = 39;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) {
            pcm.xsiz = pcm.pxsize;
            pcm.ysiz = pcm.pysize;
            gxvpag(pcm.xsiz, pcm.ysiz, 0.0, pcm.xsiz, 0.0, pcm.ysiz);
            gacln(pcm, 1);
            return (0);
        }
        if (getdbl(cmd, &xlo) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &xhi) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &ylo) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &yhi) == null) goto err;
        if (xlo < 0.0 || ylo < 0.0 || xhi > pcm.pxsize || yhi > pcm.pysize) {
            gaprnt(0, "SET Error: vpage values beyond real page limits\n");
            return (1);
        }
        if (yhi <= ylo || xhi <= xlo) goto err;
        if ((yhi - ylo) / (xhi - xlo) > pcm.pysize / pcm.pxsize) {
            pcm.ysiz = pcm.pysize;
            pcm.xsiz = pcm.pysize * (xhi - xlo) / (yhi - ylo);
        } else {
            pcm.xsiz = pcm.pxsize;
            pcm.ysiz = pcm.pxsize * (yhi - ylo) / (xhi - xlo);
        }
        gxvpag(pcm.xsiz, pcm.ysiz, xlo, xhi, ylo, yhi);
        snprintf(pout, 1255, "Virtual page size = %g %g \n", pcm.xsiz, pcm.ysiz);
        gaprnt(2, pout);
        gacln(pcm, 1);
    } else if (cmpwrd("mpt", cmd)) {
        kwrd = 92;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("*", cmd)) itt = -999;
        else {
            if (intprs(cmd, &itt) == null) goto err;
            if (itt < 0) itt = 0;
            if (itt > 255) itt = 255;
        }
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        green = -999;
        blue = -999;
        if (cmpwrd("off", cmd)) red = -9;
        else {
            if (intprs(cmd, &red) == null) goto err;
            if (red < -1) red = -1;
            if ((cmd = nxtwrd(cmd)) != null) {
                if (intprs(cmd, &green) == null) goto err;
                if (green < 1) green = 1;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (intprs(cmd, &blue) == null) goto err;
                    if (blue < 1) blue = 1;
                }
            }
        }
        if (itt == -999) {
            itt1 = 0;
            itt2 = 256;
        } else {
            itt1 = itt;
            itt2 = itt + 1;
        }
        for (i = itt1; i < itt2; i++) {
            pcm.mpcols[i] = red;
            if (green != -999) pcm.mpstls[i] = green;
            if (blue != -999) pcm.mpthks[i] = blue;
        }
    } else if (cmpwrd("rgb", cmd)) {
        kwrd = 44;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("tile", cmd)) {
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (intprs(cmd, &green) == null) goto err;
            red = -9;
            blue = -9;
        } else {
            if (intprs(cmd, &red) == null) goto err;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (intprs(cmd, &green) == null) goto err;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (intprs(cmd, &blue) == null) goto err;
            if ((cmd = nxtwrd(cmd)) == null)
                itt2 = 255;
            else {
                if (intprs(cmd, &itt2) == null) goto err;
            }
            if (itt < 16 || itt > 2048) {
                gaprnt(0, "SET RGB Error:  Color number must be between 16 and 2048 \n");
                return (1);
            }
            if (red < 0 || red > 255 ||
                green < 0 || green > 255 ||
                blue < 0 || blue > 255) {
                gaprnt(0, "SET RGB Error:  RGB values must be between 0 and 255 \n");
                return (1);
            }
            if (itt2 < -255 || itt2 > 255) {
                gaprnt(0, "SET RGB Error:  Alpha value must be between -255 and 255 \n");
                return (1);
            }
        }
        gxacol(itt, red, green, blue, itt2);
    } else if (cmpwrd("stat", cmd)) {
        kwrd = 102;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) pcm.statflg = 0;
        else if (cmpwrd("on", cmd)) pcm.statflg = 1;
        else goto err;
    } else if (cmpwrd("arrlab", cmd)) {
        kwrd = 91;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) pcm.arlflg = 0;
        else if (cmpwrd("on", cmd)) pcm.arlflg = 1;
        else goto err;
    } else if (cmpwrd("parea", cmd)) {
        kwrd = 40;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) {
            pcm.paflg = 0;
            return (0);
        }
        if (getdbl(cmd, &xlo) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &xhi) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &ylo) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &yhi) == null) goto err;
        if (xlo < 0.0 || ylo < 0.0 || xhi > pcm.xsiz || yhi > pcm.ysiz) {
            gaprnt(0, "SET Error: parea values beyond page limits\n");
            return (1);
        }
        if (yhi <= ylo || xhi <= xlo) goto err;
        pcm.pxmin = xlo;
        pcm.pxmax = xhi;
        pcm.pymin = ylo;
        pcm.pymax = yhi;
        pcm.paflg = 1;
    } else if (cmpwrd("arrowhead", cmd)) {
        kwrd = 68;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.ahdsiz)) == null) goto err;
        snprintf(pout, 1255, "Arrowhead = %g \n", pcm.ahdsiz);
        gaprnt(2, pout);
    } else if (cmpwrd("cint", cmd)) {
        kwrd = 8;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &val1) == null) goto err;
        if (val1 <= 0) {
            gaprnt(0, "SET Error: cint must be greater than 0.0\n");
        } else {
            pcm.cint = val1;
            snprintf(pout, 1255, "cint = %g \n", pcm.cint);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("xlint", cmd)) {
        kwrd = 77;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.xlint)) == null) goto err;
        snprintf(pout, 1255, "xlint = %g \n", pcm.xlint);
        gaprnt(2, pout);
    } else if (cmpwrd("ylint", cmd)) {
        kwrd = 78;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.ylint)) == null) goto err;
        snprintf(pout, 1255, "ylint = %g \n", pcm.ylint);
        gaprnt(2, pout);
    } else if (cmpwrd("xsize", cmd)) {
        kwrd = 35;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &xx) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &yy) == null) goto err;
        dsubs.gxdxsz(xx, yy);
        gxfrme(9);
    } else if (cmpwrd("mpvals", cmd)) {
        kwrd = 37;
        i1 = 0;
        if ((ch = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", ch)) {
            pcm.mpflg = 0;
            gaprnt(2, "mpvals have been turned off\n");
        } else {
            while ((cmd = nxtwrd(cmd)) != null) {
                if (getdbl(cmd, &(pcm.mpvals[i1])) == null) goto err;
                i1++;
                if (i1 > 9) goto err;
            }
            pcm.mpflg = i1;
            gaprnt(2, "mpvals have been set\n");
        }
    } else if (cmpwrd("fgvals", cmd)) {
        kwrd = 45;
        i1 = 0;
        while ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &(pcm.fgvals[i1])) == null) goto err;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (intprs(cmd, &(pcm.fgcols[i1])) == null) goto err;
            i1++;
            if (i1 > 48) goto err;
        }
        pcm.fgcnt = i1;
        gaprnt(2, "fgvals set\n");
    } else if (cmpwrd("clevs", cmd)) {
        kwrd = 29;
        i1 = 0;
        while ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &(pcm.clevs[i1])) == null) goto err;
            i1++;
            if (i1 > 254) goto err;
        }
        pcm.cflag = i1;
        snprintf(pout, 1255, "Number of clevs = %i \n", i1);
        gaprnt(2, pout);
        for (i = 1; i < pcm.cflag; i++) {
            if (pcm.clevs[i] <= pcm.clevs[i - 1]) {
                gaprnt(1, "Warning: Contour levels are not strictly increasing\n");
                gaprnt(1, "         This may lead to errors or undesired results\n");
            }
        }
    } else if (cmpwrd("xlevs", cmd)) {
        kwrd = 75;
        i1 = 0;
        while ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &(pcm.xlevs[i1])) == null) goto err;
            i1++;
            if (i1 > 49) goto err;
        }
        pcm.xlflg = i1;
        snprintf(pout, 1255, "Number of xlevs = %i \n", i1);
        gaprnt(2, pout);
    } else if (cmpwrd("ylevs", cmd)) {
        kwrd = 76;
        i1 = 0;
        while ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &(pcm.ylevs[i1])) == null) goto err;
            i1++;
            if (i1 > 49) goto err;
        }
        pcm.ylflg = i1;
        snprintf(pout, 1255, "Number of ylevs = %i \n", i1);
        gaprnt(2, pout);
    } else if (cmpwrd("rbcols", cmd)) {
        kwrd = 59;
        i1 = 0;
        while ((cmd = nxtwrd(cmd)) != null) {
            if (i1 == 0 && cmpwrd("auto", cmd)) break;
            if (intprs(cmd, &(pcm.rbcols[i1])) == null) goto err;
            i1++;
            if (i1 > 255) goto err;
        }
        pcm.rbflg = i1;
        if (i1 == 0) gaprnt(2, "Rainbow colors set to auto\n");
        else {
            snprintf(pout, 1255, "Number of rainbow colors = %i\n", i1);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("dropmenu", cmd)) {
        kwrd = 97;
        i1 = 0;
        while ((cmd = nxtwrd(cmd)) != null) {
            if (i1 > 14) goto drerr;
            if (intprs(cmd, &(pcm.drvals[i1])) == null) goto drerr;
            i1++;
        }
        if (i1 == 0) goto drerr;
    } else if (cmpwrd("ccols", cmd)) {
        kwrd = 36;
        i1 = 0;
        while ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &(pcm.ccols[i1])) == null) goto err;
            i1++;
            if (i1 > 255) goto err;
        }
        pcm.ccflg = i1;
        snprintf(pout, 1255, "Number of ccols = %i\n", i1);
        gaprnt(2, pout);
        if (pcm.cflag == 0) {
            gaprnt(2, "ccols won't take effect unless clevs are set.\n");
        }
    } else if (cmpwrd("cmin", cmd)) {
        kwrd = 25;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.cmin)) == null) goto err;
        snprintf(pout, 1255, "cmin = %g \n", pcm.cmin);
        gaprnt(2, pout);
    } else if (cmpwrd("cmax", cmd)) {
        kwrd = 26;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.cmax)) == null) goto err;
        snprintf(pout, 1255, "cmax = %g \n", pcm.cmax);
        gaprnt(2, pout);
    } else if (cmpwrd("cmark", cmd)) {
        kwrd = 18;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &(pcm.cmark)) == null) goto err;
        snprintf(pout, 1255, "cmark = %i \n", pcm.cmark);
        gaprnt(2, pout);
    } else if (cmpwrd("mproj", cmd)) {
        kwrd = 33;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) pcm.mproj = 0;
        else if (cmpwrd("scaled", cmd)) pcm.mproj = 1;
        else if (cmpwrd("latlon", cmd)) pcm.mproj = 2;
        else if (cmpwrd("nps", cmd)) pcm.mproj = 3;
        else if (cmpwrd("sps", cmd)) pcm.mproj = 4;
        else if (cmpwrd("robinson", cmd)) pcm.mproj = 5;
        else if (cmpwrd("mollweide", cmd)) pcm.mproj = 6;
        else if (cmpwrd("orthogr", cmd)) pcm.mproj = 7;
        else if (cmpwrd("orthographic", cmd)) pcm.mproj = 7;
        else if (cmpwrd("ortho", cmd)) pcm.mproj = 7;
        else if (cmpwrd("lambert", cmd)) pcm.mproj = 13;
        else goto err;
    } else if (cmpwrd("xyrev", cmd)) {
        kwrd = 53;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) pcm.rotate = 0;
        else if (cmpwrd("on", cmd)) pcm.rotate = 1;
        else goto err;
    } else if (cmpwrd("xflip", cmd)) {
        kwrd = 54;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) pcm.xflip = 0;
        else if (cmpwrd("on", cmd)) pcm.xflip = 1;
        else goto err;
    } else if (cmpwrd("yflip", cmd)) {
        kwrd = 55;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) pcm.yflip = 0;
        else if (cmpwrd("on", cmd)) pcm.yflip = 1;
        else goto err;
    } else if (cmpwrd("writegds", cmd)) {
        wgds = pcm.wgds;
        kwrd = 109;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (*cmd == '-') {
            itt = 0;
            while (*(cmd + itt) != ' ' && *(cmd + itt) != '\n' && *(cmd + itt) != '\0') itt++;
            sz = itt + 2;
            ch = (char *) galloc(sz, "writegds");
            if (ch == null) {
                gaprnt(0, "Memory allocation Error\n");
                goto err;
            }
            i2 = cmd - cmd1;
            for (i1 = 0; i1 < itt; i1++) *(ch + i1) = *(com + i1 + i2);
            *(ch + i1) = '\0';
            if (wgds.opts) gree(wgds.opts, "f223");
            wgds.opts = ch;
        }
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        itt = 0;
        while (*(cmd + itt) != ' ' && *(cmd + itt) != '\n' && *(cmd + itt) != '\0') itt++;
        sz = itt + 2;
        ch = (char *) galloc(sz, "writegds2");
        if (ch == null) {
            gaprnt(0, "Memory allocation Error\n");
            goto err;
        }
        i2 = cmd - cmd1;
        for (i1 = 0; i1 < itt; i1++) *(ch + i1) = *(com + i1 + i2);
        *(ch + i1) = '\0';
        if (wgds.fname) gree(wgds.fname, "f224");
        wgds.fname = ch;
        if (wgds.opts) {
            snprintf(pout, 1255, "WRITEGDS file name = %s  Opts = %s\n", ch, wgds.opts);
        } else {
            snprintf(pout, 1255, "WRITEGDS file name = %s\n", ch);
        }
        gaprnt(2, pout);
    } else if (cmpwrd("shp", cmd)) {
        kwrd = 121;
#if USESHP == 1
                                                                                                                                if ((cmd = nxtwrd (cmd)) == null) goto err;
    /* parse arguments to 'set shp' command */
    while (cmpwrd("-pt",cmd) || cmpwrd("-point",cmd) ||
	   cmpwrd("-ln",cmd) || cmpwrd("-line",cmd)  ||
           cmpwrd("-poly",cmd) || cmpwrd("-fmt",cmd)) {
      if (cmpwrd("-pt",cmd) || cmpwrd("-point",cmd)) pcm.shptype = 1;
      if (cmpwrd("-ln",cmd) || cmpwrd("-line",cmd))  pcm.shptype = 2;
      if (cmpwrd("-poly",cmd)) pcm.shptype = 3;
      if (cmpwrd("-fmt",cmd)) {
	if ((cmd = nxtwrd (cmd)) == null) goto err;
	if (intprs(cmd,&itt) == null ) goto err;
	pcm.dblen = itt;
	if ((cmd = nxtwrd (cmd)) == null) goto err;
	if (intprs(cmd,&itt) == null ) goto err;
	pcm.dbprec = itt;
      }
      if ((cmd = nxtwrd (cmd)) == null) goto err;
    }
    /* Parse the shapefile output filename root */
    itt = 0;
    while (*(cmd+itt)!=' '&&*(cmd+itt)!='\n'&&*(cmd+itt)!='\0') itt++;
    sz = itt+1;
    ch = (char *)galloc(sz,"shpname");
    if (ch==null) {
      gaprnt (0,"Memory allocation error for shapefile name\n");
      goto err;
    }

    i2 = cmd - cmd1;
    for (i1=0; i1<itt; i1++) *(ch+i1) = *(com+i1+i2);
    *(ch+itt) ='\0';

    /* release previously set filenames for shapefile output */
    if (pcm.shpfname) gree(pcm.shpfname,"f225e");
    pcm.shpfname = ch;

    snprintf(pout,1255,"Shapefile output file name root: %s\n",pcm.shpfname);
    gaprnt (2,pout);
    if (pcm.shptype==1) gaprnt(2,"Shapefile output type: point\n");
    if (pcm.shptype==2) gaprnt(2,"Shapefile output type: line\n");
    if (pcm.shptype==3) gaprnt(2,"Shapefile output type: polygon\n");
    snprintf(pout,1255,"Shapefile format string is \%%%d.%df\n",pcm.dblen,pcm.dbprec);
    gaprnt(2,pout);
#else
        gaprnt(0, "Error: This version of GrADS does not support shapefile output\n");
        return (1);
#endif
    } else if (cmpwrd("kml", cmd)) {
        kwrd = 116;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        pcm.kmlflg = 1;                               /* set default value to image output */
        while (cmpwrd("-img", cmd) || cmpwrd("-image", cmd) ||
               cmpwrd("-ln", cmd) || cmpwrd("-line", cmd) ||
               cmpwrd("-poly", cmd)) {
            if (cmpwrd("-img", cmd) || cmpwrd("-image", cmd)) pcm.kmlflg = 1;     /* image output */
            if (cmpwrd("-ln", cmd) || cmpwrd("-line", cmd)) pcm.kmlflg = 2;     /* contour output */
            if (cmpwrd("-poly", cmd)) pcm.kmlflg = 3;     /* polygon output */
            if ((cmd = nxtwrd(cmd)) == null) goto err;
        }
        /* make sure TIFF output is enabled */
#if GEOTIFF != 1
        if (pcm.kmlflg == 1) {
            gaprnt(0, "Error: Creating TIFF images for KML output is not supported in this build.\n");
            gaprnt(0, "  Try the \'-line\' option with \'set kml\' to output contour lines in KML format instead\n");
            return (1);
        }
#endif
        /* Parse the KML output filename root */

        /* get the length of the user-supplied filename */
        itt = 0;
        while (*(cmd + itt) != ' ' && *(cmd + itt) != '\n' && *(cmd + itt) != '\0') itt++;
        /* allocate memory for KML filename */
        sz = itt + 6;
        ch = null;
        ch = (char *) galloc(sz, "kmlname");
        if (ch == null) {
            gaprnt(0, "Memory allocation error for KML file name\n");
            goto err;
        }
        /* copy the user-supplied filename to ch */
        i2 = cmd - cmd1;
        for (i1 = 0; i1 < itt; i1++) *(ch + i1) = *(com + i1 + i2);
        /* check if user already put ".kml" onto the filename */
        if (*(ch + itt - 4) == '.' && *(ch + itt - 3) == 'k' && *(ch + itt - 2) == 'm' && *(ch + itt - 1) == 'l') {
            *(ch + itt) = '\0';      /* make sure it is null-terminated */
            itt = itt - 4;         /* shorten the length in case we need the file root for TIFF image */
        } else {
            /* add ".kml" to the end of the filename */
            *(ch + i1 + 0) = '.';
            *(ch + i1 + 1) = 'k';
            *(ch + i1 + 2) = 'm';
            *(ch + i1 + 3) = 'l';
            *(ch + i1 + 4) = '\0';
        }
        if (pcm.kmlflg == 1) {
            /* A second file will be created by GrADS containing the TIFF image */
            ch2 = (char *) galloc(sz, "tifname");
            if (ch2 == null) {
                gaprnt(0, "Memory allocation error for KML image file name\n");
                goto err;
            }
            /* copy the user-supplied filename root to ch2 and add ".tif" */
            for (i1 = 0; i1 < itt; i1++) *(ch2 + i1) = *(com + i1 + i2);
            *(ch2 + i1 + 0) = '.';
            *(ch2 + i1 + 1) = 't';
            *(ch2 + i1 + 2) = 'i';
            *(ch2 + i1 + 3) = 'f';
            *(ch2 + i1 + 4) = '\0';
        }
        /* release previously set filenames for KML output */
        if (pcm.kmlname) gree(pcm.kmlname, "f225d");
        pcm.kmlname = ch;
        if (pcm.kmlflg == 1) {
            if (pcm.tifname) gree(pcm.tifname, "f225c");
            pcm.tifname = ch2;
            gaprnt(2, "KML output file names: \n");
            snprintf(pout, 1255, "%s (TIFF image) \n", pcm.tifname);
            gaprnt(2, pout);
            snprintf(pout, 1255, "%s (KML text file) \n", pcm.kmlname);
            gaprnt(2, pout);
            gaprnt(2, "KML output type: image\n");
        } else {
            gaprnt(2, "KML output file name: \n");
            snprintf(pout, 1255, "%s (KML text file)\n", pcm.kmlname);
            gaprnt(2, pout);
            if (pcm.kmlflg == 2)
                gaprnt(2, "KML output type: contour lines\n");
            else
                gaprnt(2, "KML output type: polygons\n");
        }
    } else if (cmpwrd("geotiff", cmd)) {
        kwrd = 115;
#if GEOTIFF == 1
                                                                                                                                if ((cmd = nxtwrd (cmd)) == null) goto err;
    pcm.gtifflg = 1;                             /* set default value to 4-byte depth */
    while (cmpwrd("-dbl",cmd) || cmpwrd("-flt",cmd)) {
      if (cmpwrd("-flt",cmd) ) pcm.gtifflg = 1;  /* 4-byte depth in GeoTIFF output file */
      if (cmpwrd("-dbl",cmd) ) pcm.gtifflg = 2;  /* 8-byte depth in GeoTIFF output file */
      if ((cmd = nxtwrd (cmd)) == null) goto err;
    }
    /* parse the geotiff output filename root -- .tif will be added by GrADS */
    /* get the length of the user-supplied filename */
    itt = 0;
    while (*(cmd+itt)!=' '&&*(cmd+itt)!='\n'&&*(cmd+itt)!='\0') itt++;
    /* allocate memory for GeoTIFF filename */
    sz = itt+6;
    ch = (char *)galloc(sz,"gtifname");
    if (ch==null) {
      gaprnt (0,"Memory allocation error for GeoTIFF file name\n"); goto err;
    }
    /* copy the user-supplied filename to ch */
    i2 = cmd - cmd1;
    for (i1=0; i1<itt; i1++) *(ch+i1) = *(com+i1+i2);
    /* check if user already put ".tif" onto the filename */
    if (*(ch+itt-4)=='.' &&  *(ch+itt-3)=='t' &&  *(ch+itt-2)=='i' &&  *(ch+itt-1)=='f') {
      *(ch+itt)='\0';      /* make sure it is null-terminated */
    }
    else {
      /* add ".tif" to the end of the filename */
      *(ch+i1+0)='.';
      *(ch+i1+1)='t';
      *(ch+i1+2)='i';
      *(ch+i1+3)='f';
      *(ch+i1+4)='\0';
    }
    /* reset geotiff filename */
    if (pcm.gtifname) gree(pcm.gtifname,"f225b");
    pcm.gtifname = ch;
    snprintf(pout,1255,"GeoTIFF file name = %s\n",pcm.gtifname);
    gaprnt (2,pout);
    if (pcm.gtifflg==1) gaprnt(2,"GeoTIFF format is float  \n");
    if (pcm.gtifflg==2) gaprnt(2,"GeoTIFF format is double \n");
#else
        gaprnt(0, "Error: This version of GrADS does not support GeoTIFF output\n");
        return (1);
#endif
    }

        /* Following is for the so-called 'exact fwrite' to
     workaround the bug with scaling for hires files */
    else if (cmpwrd("fwex", cmd)) {
        pcm.fwexflg = 1;
    } else if (cmpwrd("fwrite", cmd)) {
        kwrd = 86;
        if (pcm.ffile) {
            gaprnt(0, "SET FWrite Error:  fwrite file is open\n");
            gaprnt(0, "Use DISABLE FWRITE command to close file\n");
        } else {
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            while (cmpwrd("-be", cmd) || cmpwrd("-le", cmd) ||
                   cmpwrd("-ap", cmd) || cmpwrd("-cl", cmd) ||
                   cmpwrd("-sq", cmd) || cmpwrd("-st", cmd) ||
                   cmpwrd("-ex", cmd)) {
                if (cmpwrd("-be", cmd)) pcm.fwenflg = 1;
                if (cmpwrd("-le", cmd)) pcm.fwenflg = 0;
                if (cmpwrd("-sq", cmd)) pcm.fwsqflg = 1;
                if (cmpwrd("-st", cmd)) pcm.fwsqflg = 0;
                if (cmpwrd("-ap", cmd)) pcm.fwappend = 1;
                if (cmpwrd("-cl", cmd)) pcm.fwappend = 0;
                if ((cmd = nxtwrd(cmd)) == null) goto err;
            }
            itt = 0;
            while (*(cmd + itt) != ' ' && *(cmd + itt) != '\n' && *(cmd + itt) != '\0') itt++;
            sz = itt + 2;
            ch = (char *) galloc(sz, "fwrite1");
            if (ch == null) {
                gaprnt(0, "Memory allocation Error\n");
                goto err;
            }
            i2 = cmd - cmd1;
            for (i1 = 0; i1 < itt; i1++) *(ch + i1) = *(com + i1 + i2);
            *(ch + i1) = '\0';
            if (pcm.fwname) gree(pcm.fwname, "f225");
            pcm.fwname = ch;
            snprintf(pout, 1255, "FWrite file name = %s\n", ch);
            gaprnt(2, pout);
            if (pcm.fwenflg == 0) {
                gaprnt(2, "FWwrite byte order is little_endian; format is ");
            } else {
                gaprnt(2, "FWwrite byte order is big_endian; format is ");
            }
            if (pcm.fwsqflg == 1) gaprnt(2, "sequential\n");
            else gaprnt(2, "stream\n");
            if (pcm.fwappend) {
                gaprnt(2, "Fwrite appending to an existing file\n");
            } else {
                gaprnt(2, "Fwrite replacing an existing file\n");
            }
        }
    } else if (cmpwrd("sdfwrite", cmd)) {
        kwrd = 113;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        /* options for padding the output and setting a record dimension
       are set to 'off' with each new call to 'set sdfwrite' */
        pcm.sdfwpad = 0;
        pcm.sdfrecdim = 0;
        /* parse any arguments to 'set sdfwrite' command */
        while (cmpwrd("-4d", cmd) || cmpwrd("-4de", cmd) || cmpwrd("-5d", cmd) ||
               cmpwrd("-3dz", cmd) || cmpwrd("-3dt", cmd) ||
               cmpwrd("-flt", cmd) || cmpwrd("-dbl", cmd) ||
               cmpwrd("-nc3", cmd) || cmpwrd("-nc4", cmd) ||
               cmpwrd("-rt", cmd) || cmpwrd("-re", cmd) ||
               cmpwrd("-zip", cmd) || cmpwrd("-chunk", cmd)) {
            if (cmpwrd("-3dz", cmd)) pcm.sdfwpad = 1;
            if (cmpwrd("-3dt", cmd)) pcm.sdfwpad = 2;
            if (cmpwrd("-4d", cmd)) pcm.sdfwpad = 3;
            if (cmpwrd("-4de", cmd)) pcm.sdfwpad = 4;
            if (cmpwrd("-5d", cmd)) pcm.sdfwpad = 5;
            if (cmpwrd("-dbl", cmd)) pcm.sdfprec = 8;
            if (cmpwrd("-flt", cmd)) pcm.sdfprec = 4;
            if (cmpwrd("-nc3", cmd)) pcm.sdfwtype = 1;
            if (cmpwrd("-nc4", cmd)) pcm.sdfwtype = 2;
            if (cmpwrd("-rt", cmd)) pcm.sdfrecdim += 1;
            if (cmpwrd("-re", cmd)) pcm.sdfrecdim += 2;
            if (cmpwrd("-zip", cmd)) pcm.sdfzip = 1;
            if (cmpwrd("-chunk", cmd)) pcm.sdfchunk = 1;

            if ((cmd = nxtwrd(cmd)) == null) goto err;
        }
        /* parse the sdfwrite output filename */
        itt = 0;
        while (*(cmd + itt) != ' ' && *(cmd + itt) != '\n' && *(cmd + itt) != '\0') itt++;
        sz = itt + 2;
        ch = (char *) galloc(sz, "sdfwname");
        if (ch == null) {
            gaprnt(0, "Memory allocation error for sdfwrite file name\n");
            goto err;
        }
        i2 = cmd - cmd1;
        for (i1 = 0; i1 < itt; i1++) *(ch + i1) = *(com + i1 + i2);
        *(ch + i1) = '\0';
        /* release previously set sdfwrite filename */
        if (pcm.sdfwname) gree(pcm.sdfwname, "f225a");
        pcm.sdfwname = ch;
        snprintf(pout, 1255, "SDFWrite file name = %s\n", ch);
        gaprnt(2, pout);
        gaprnt(2, "SDFWrite will replace an existing file\n");
        if (pcm.sdfwpad == 0)
            gaprnt(2, "SDFwrite file will have the same number of dimensions as the defined variable\n");
        if (pcm.sdfwpad == 1)
            gaprnt(2, "SDFwrite file will have 3 dimensions: lon, lat, and lev\n");
        if (pcm.sdfwpad == 2)
            gaprnt(2, "SDFwrite file will have 3 dimensions: lon, lat, and time\n");
        if (pcm.sdfwpad == 3)
            gaprnt(2, "SDFwrite file will have 4 dimensions: lon, lat, lev, and time\n");
        if (pcm.sdfwpad == 4)
            gaprnt(2, "SDFwrite file will have 4 dimensions: lon, lat, time, and ens\n");
        if (pcm.sdfwpad == 5)
            gaprnt(2, "SDFwrite file will have 5 dimensions: lon, lat, lev, time, and ens\n");
#if HAVENETCDF4 != 1
        /* reset flags if we dont' have netcdf-4 */
        if (pcm.sdfwtype == 2) {
            gaprnt(2, "Warning: This build is not enabled to write netCDF-4 format\n");
            gaprnt(2, "         sdfwrite output will be netCDF classic format instead\n");
            pcm.sdfwtype = 1;
            if (pcm.sdfchunk == 1) {
                gaprnt(2, "         without chunking\n");
                pcm.sdfchunk = 0;
            }
            if (pcm.sdfzip == 1) {
                gaprnt(2, "         without compression\n");
                pcm.sdfzip = 0;
            }
        }
#endif
        /* check for two record dimensions */
        if (pcm.sdfrecdim == 3 && pcm.sdfwtype == 1) {
            gaprnt(0, "Only one record dimension is allowed with netCDF classic format. \n");
            gaprnt(0, "  Use the \"-nc4\" option to enable multiple record dimensions. \n");
            goto err;
        }
        if (pcm.sdfrecdim == 1)
            gaprnt(2, "SDFwrite file will have 1 record dimension: time\n");
        else if (pcm.sdfrecdim == 2)
            gaprnt(2, "SDFwrite file will have 1 record dimension: ens\n");
        else if (pcm.sdfrecdim == 3)
            gaprnt(2, "SDFwrite file will have 2 record dimensions: time and ens\n");
        else
            gaprnt(2, "SDFwrite file will not have a record dimension\n");

        /* If -zip, make sure we have -nc4 and -chunk */
        if (pcm.sdfzip) {
            if (pcm.sdfwtype == 1) {
                gaprnt(0, "Compression is not available with netCDF classic format. \n");
                gaprnt(0, "  Use the \"-nc4\" option to enable compression or \n");
                gaprnt(0, "  reset the compression option with \"clear sdfwrite\". \n");
                goto err;
            }
            pcm.sdfwtype = 2;
            pcm.sdfchunk = 1;
        }

    } else if (cmpwrd("sdfattr", cmd)) {
        kwrd = 114;
        com = nxtwrd(com); /* advance past the 'sdfattr' in the mixed case version */
        if ((com = nxtwrd(com)) == null) goto err;
        /* parse the sdf attribute */
        newattr = parseattr(com);
        if (newattr == null) goto err;
        /* hang the new attribute off the gacmn structure */
        if (pcm.attr) {
            attr = pcm.attr;
            while (attr.next) attr = attr.next;      /* advance to end of chain */
            attr.next = newattr;                      /* add new link */
        } else {
            pcm.attr = newattr;                       /* set first link */
        }
    }
#if USESHP == 1
                                                                                                                                else if (cmpwrd("shpattr",cmd)) {
    kwrd = 122;
    /* advance past the 'set shpattr' in the mixed case version */
    com=nxtwrd(com);
    if ((com = nxtwrd (com)) == null) goto err;
    /* parse the attribute (data base field) */
    newfld = parsedbfld(com);
    if (newfld==null) goto err;
    /* hang the new field off the gacmn structure */
    if (pcm.dbfld) {
      fld=pcm.dbfld;
      while (fld.next) fld = fld.next;      /* advance to end of chain */
      fld.next = newfld;                     /* add new link */
    }
    else {
      pcm.dbfld = newfld;                       /* set first link */
    }
  }
#endif
#if HAVENETCDF4 == 1
                                                                                                                                else if (cmpwrd("chunksize",cmd)) {
    kwrd = 118;
    if ((cmd = nxtwrd (cmd)) == null) goto err;
    if (intprs(cmd,&itt) == null) goto err;
    pcm.xchunk = itt;
    if ((cmd = nxtwrd (cmd)) != null) {
      if (intprs(cmd,&itt) == null) goto err;
      pcm.ychunk = itt;
      if ((cmd = nxtwrd (cmd)) != null) {
	if (intprs(cmd,&itt) == null) goto err;
	pcm.zchunk = itt;
	if ((cmd = nxtwrd (cmd)) != null) {
	  if (intprs(cmd,&itt) == null) goto err;
	  pcm.tchunk = itt;
	  if ((cmd = nxtwrd (cmd)) != null) {
	    if (intprs(cmd,&itt) == null) goto err;
	    pcm.echunk = itt;
	  }
	}
      }
    }
  }
#endif
    else if (cmpwrd("cachesf", cmd)) {
        kwrd = 119;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        pcm.cachesf = (long) itt;
        setcachesf(pcm.cachesf); /* update gaio.c */
        snprintf(pout, 1255, "Global cache scale factor is %ld\n", pcm.cachesf);
        gaprnt(2, pout);
    } else if (cmpwrd("imprun", cmd)) {
        kwrd = 62;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) {
            if (pcm.impflg) gree(pcm.impnam, "f226");
            pcm.impflg = 0;
            gaprnt(2, "IMPrun is off\n");
        } else {
            itt = 0;
            while (*(cmd + itt) != ' ' && *(cmd + itt) != '\n' && *(cmd + itt) != '\0') itt++;
            sz = itt + 6;
            ch = (char *) galloc(sz, "imprun");
            if (ch == null) {
                gaprnt(0, "Memory allocation Error\n");
                goto err;
            }
            for (i1 = 0; i1 < itt; i1++) *(ch + i1 + 4) = *(cmd + i1);
            *(ch + i1 + 4) = '\0';
            *ch = 'r';
            *(ch + 1) = 'u';
            *(ch + 2) = 'n';
            *(ch + 3) = ' ';
            if (pcm.impflg) gree(pcm.impnam, "f227");
            pcm.impflg = 1;
            pcm.impnam = ch;
            snprintf(pout, 1255, "Imprun file name = %s\n", ch);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("log1d", cmd)) {
        kwrd = 123;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("loglog", cmd)) pcm.log1d = 3;
        else if (cmpwrd("logh", cmd)) pcm.log1d = 1;
        else if (cmpwrd("logv", cmd)) pcm.log1d = 2;
        else if (cmpwrd("off", cmd)) pcm.log1d = 0;
        else goto err;
    } else if (cmpwrd("zlog", cmd)) {
        kwrd = 63;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.zlog = 1;
        else if (cmpwrd("off", cmd)) pcm.zlog = 0;
        else goto err;
    } else if (cmpwrd("coslat", cmd)) {
        kwrd = 110;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.coslat = 1;
        else if (cmpwrd("off", cmd)) pcm.coslat = 0;
        else goto err;
    } else if (cmpwrd("missconn", cmd)) {
        kwrd = 61;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.miconn = 1;
        else if (cmpwrd("off", cmd)) pcm.miconn = 0;
        else goto err;
    } else if (cmpwrd("mpdraw", cmd)) {
        kwrd = 50;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.mpdraw = 1;
        else if (cmpwrd("off", cmd)) pcm.mpdraw = 0;
        else goto err;
    } else if (cmpwrd("dbuff", cmd)) {
        kwrd = 52;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) {
            pcm.dbflg = 1;
            gxfrme(2);
            gacln(pcm, 1);
        } else if (cmpwrd("off", cmd)) {
            if (pcm.dbflg) {
                pcm.dbflg = 0;
                gxfrme(1);
                gacln(pcm, 1);
            }
        } else goto err;
    } else if (cmpwrd("poli", cmd)) {
        kwrd = 51;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) {
            if (pcm.mpcols[1] == -9) pcm.mpcols[1] = -1;
            if (pcm.mpcols[2] == -9) pcm.mpcols[2] = -1;
        } else if (cmpwrd("off", cmd)) {
            pcm.mpcols[1] = -9;
            pcm.mpcols[2] = -9;
        } else goto err;
    } else if (cmpwrd("mpdset", cmd)) {
        kwrd = 38;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        for (xx = 0; xx < 8; xx++) {
            if (pcm.mpdset[xx]) gree(pcm.mpdset[xx], "f228");
            pcm.mpdset[xx] = null;
        }
        xx = 0;
        itt = 0;
        while (xx < 8) {
            itt2 = itt;
            while (*(cmd + itt) != ' ' && *(cmd + itt) != '\n' && *(cmd + itt) != '\0') itt++;
            sz = itt + 2 - itt2;
            ch = (char *) galloc(sz, "setmpds");
            if (ch == null) {
                gaprnt(0, "Memory allocation Error\n");
                goto err;
            }
            for (i1 = itt2; i1 < itt; i1++) *(ch + i1 - itt2) = *(cmd + i1);
            *(ch + i1 - itt2) = '\0';
            pcm.mpdset[xx] = ch;
            snprintf(pout, 1255, "MPDSET file name = %s\n", ch);
            gaprnt(2, pout);
            while (*(cmd + itt) == ' ') itt++;
            if (*(cmd + itt) == '\n' || *(cmd + itt) == '\0') break;
            xx++;
        }
    } else if (cmpwrd("rbrange", cmd)) {
        kwrd = 60;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &val1) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &val2) == null) goto err;
        if (val1 >= val2) goto err;
        pcm.rainmn = val1;
        pcm.rainmx = val2;
    } else if (cmpwrd("black", cmd)) {
        kwrd = 22;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("off", cmd)) pcm.blkflg = 0;
        else {
            if (getdbl(cmd, &(pcm.blkmin)) == null) goto err;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (getdbl(cmd, &(pcm.blkmax)) == null) goto err;
            pcm.blkflg = 1;
        }
    } else if (cmpwrd("display", cmd)) {
        kwrd = 57;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("color", cmd) || cmpwrd("grey", cmd) || cmpwrd("greyscale", cmd)) itt = 2; /* this will be a no-op */
        else if (cmpwrd("white", cmd)) itt = 1;
        else if (cmpwrd("black", cmd)) itt = 0;
        else goto err;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (cmpwrd("white", cmd)) itt = 1;
            else if (cmpwrd("black", cmd)) itt = 0;
            else goto err;
        }
        if (itt == 0 || itt == 1) gxdbck(itt);
    } else if (cmpwrd("gxout", cmd)) {
        kwrd = 21;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        pcm.gout0 = 9;
        if (cmpwrd("contour", cmd)) pcm.gout2a = 1;
        else if (cmpwrd("shaded", cmd)) pcm.gout2a = 16;
        else if (cmpwrd("shade1", cmd)) pcm.gout2a = 2;
        else if (cmpwrd("shade2", cmd)) pcm.gout2a = 16;
        else if (cmpwrd("shade2b", cmd)) pcm.gout2a = 17;
        else if (cmpwrd("grid", cmd)) {
            pcm.gout2a = 3;
            pcm.gout2b = 3;
        }
        else if (cmpwrd("vector", cmd)) {
            pcm.gout2b = 4;
            pcm.goutstn = 6;
            pcm.gout1a = 1;
        }
        else if (cmpwrd("scatter", cmd)) pcm.gout2b = 5;
        else if (cmpwrd("fgrid", cmd)) pcm.gout2a = 6;
        else if (cmpwrd("fwrite", cmd)) pcm.gout2a = 7;
        else if (cmpwrd("stream", cmd)) pcm.gout2b = 8;
        else if (cmpwrd("grfill", cmd)) pcm.gout2a = 10;
        else if (cmpwrd("geotiff", cmd)) {
#if GEOTIFF == 1
            pcm.gout2a = 12;
#else
            gaprnt(0, "Creating GeoTIFF files is not supported in this build\n");
            goto err;
#endif
        } else if (cmpwrd("kml", cmd)) pcm.gout2a = 13;
        else if (cmpwrd("imap", cmd)) pcm.gout2a = 14;
        else if (cmpwrd("shp", cmd)) {
#if USESHP == 1
            pcm.gout2a = 15; pcm.goutstn = 9;}
#else
            gaprnt(0, "Creating shapefiles is not supported in this build\n");
            goto err;
        }
#endif
        else if (cmpwrd("value", cmd)) pcm.goutstn = 1;
        else if (cmpwrd("barb", cmd)) {
            pcm.goutstn = 2;
            pcm.gout2b = 9;
            pcm.gout1a = 2;
        }
        else if (cmpwrd("findstn", cmd)) pcm.goutstn = 3;
        else if (cmpwrd("model", cmd)) pcm.goutstn = 4;
        else if (cmpwrd("wxsym", cmd)) pcm.goutstn = 5;
        else if (cmpwrd("stnmark", cmd)) pcm.goutstn = 7;
        else if (cmpwrd("stnwrt", cmd)) pcm.goutstn = 8;  /* undocumented */
        else if (cmpwrd("line", cmd)) {
            pcm.gout1 = 1;
            pcm.tser = 0;
        }
        else if (cmpwrd("bar", cmd)) pcm.gout1 = 2;
        else if (cmpwrd("errbar", cmd)) pcm.gout1 = 3;
        else if (cmpwrd("linefill", cmd)) pcm.gout1 = 4;
        else if (cmpwrd("stat", cmd)) pcm.gout0 = 1;
        else if (cmpwrd("print", cmd)) pcm.gout0 = 2;
        else if (cmpwrd("writegds", cmd)) pcm.gout0 = 3;    /* only for internal use */
        else if (cmpwrd("tserwx", cmd)) pcm.tser = 1;
        else if (cmpwrd("tserbarb", cmd)) pcm.tser = 2;
        else goto err;
        if (pcm.gout0 == 9) pcm.gout0 = 0;
    } else if (cmpwrd("arrscl", cmd)) {
        kwrd = 27;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.arrsiz)) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) {
            pcm.arrmag = -999.0;
        } else {
            if (getdbl(cmd, &(pcm.arrmag)) == null) goto err;
        }
        pcm.arrflg = 1;
    } else if (cmpwrd("xlabs", cmd) || cmpwrd("ylabs", cmd)) {
        if (cmpwrd("xlabs", cmd)) {
            kwrd = 94;
            strng = pcm.xlabs;
        }
        if (cmpwrd("ylabs", cmd)) {
            kwrd = 95;
            strng = pcm.ylabs;
        }
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        i1 = 0;
        if (cmpwrd("off", cmd)) {
            if (strng) gree(strng, "f229");
            strng = null;
        } else {
            com = nxtwrd(com);
            com = nxtwrd(com);
            num = 0;
            while (*(com + num) != '\0' && *(com + num) != '\n') num++;
            if (strng) gree(strng, "f230");
            sz = num + 2;
            strng = (char *) galloc(sz, "xlabs");
            if (strng == null) {
                gaprnt(0, "Memory Allocation Error: Set XLABS/YLABS\n");
                goto err;
            }
            num = 0;
            while (*(com + num) != '\0' && *(com + num) != '\n') {
                *(strng + num) = *(com + num);
                if (*(strng + num) == '|') {
                    *(strng + num) = '\0';
                    i1++;
                }
                num++;
            }
            *(strng + num) = '\0';
            i1++;
        }
        if (kwrd == 94) {
            pcm.xlabs = strng;
            pcm.ixlabs = i1;
        }
        if (kwrd == 95) {
            pcm.ylabs = strng;
            pcm.iylabs = i1;
        }
    } else if (cmpwrd("clab", cmd) || cmpwrd("xlab", cmd) || cmpwrd("ylab", cmd)) {
        if (cmpwrd("clab", cmd)) {
            kwrd = 32;
            strng = pcm.clstr;
            i1 = pcm.clab;
        }
        if (cmpwrd("xlab", cmd)) {
            kwrd = 73;
            strng = pcm.xlstr;
            i1 = pcm.xlab;
        }
        if (cmpwrd("ylab", cmd)) {
            kwrd = 74;
            strng = pcm.ylstr;
            i1 = pcm.ylab;
        }
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) i1 = 1;
        else if (cmpwrd("off", cmd)) i1 = 0;
        else if (cmpwrd("forced", cmd)) i1 = 2;
        else if (cmpwrd("masked", cmd)) i1 = 3;
        else if (cmpwrd("auto", cmd)) {
            if (strng) gree(strng, "f231");
            strng = null;
        } else {
            com = nxtwrd(com);
            com = nxtwrd(com);
            num = 0;
            while (*(com + num) != '\0' && *(com + num) != '\n') num++;
            if (strng) gree(strng, "f232");
            sz = num + 2;
            strng = (char *) galloc(sz, "clab1");
            if (strng == null) {
                gaprnt(0, "Memory Allocation Error: Set ?LAB\n");
                goto err;
            }
            num = 0;
            while (*(com + num) != '\0' && *(com + num) != '\n') {
                *(strng + num) = *(com + num);
                num++;
            }
            *(strng + num) = '\0';
            gaprnt(2, "Substitution string is: ");
            gaprnt(2, strng);
            gaprnt(2, "\n");
        }
        if (kwrd == 32) {
            pcm.clstr = strng;
            pcm.clab = i1;
        }
        if (kwrd == 73) {
            pcm.xlstr = strng;
            pcm.xlab = i1;
        }
        if (kwrd == 74) {
            pcm.ylstr = strng;
            pcm.ylab = i1;
        }
    } else if (cmpwrd("prnopts", cmd)) {
        kwrd = 106;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        com = nxtwrd(com);
        com = nxtwrd(com);
        num = 0;
        while (*(com + num) != '\0' && *(com + num) != '\n' && *(com + num) != ' ') num++;
        if (pcm.prstr) gree(pcm.prstr, "f233");
        sz = num + 2;
        strng = (char *) galloc(sz, "prnopts");
        if (strng == null) {
            gaprnt(0, "Memory Allocation Error: Set PRNOPTS\n");
            goto err;
        }
        num = 0;
        while (*(com + num) != '\0' && *(com + num) != '\n' && *(com + num) != ' ') {
            *(strng + num) = *(com + num);
            num++;
        }
        *(strng + num) = '\0';
        pcm.prstr = strng;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt) == null) goto err;
            pcm.prlnum = itt;
            if ((cmd = nxtwrd(cmd)) != null) {
                if (intprs(cmd, &itt) == null) goto err;
                pcm.prbnum = itt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    pcm.prudef = 0;
                    if (*cmd == 'u') pcm.prudef = 1;
                }
            }
        }
    } else if (cmpwrd("frame", cmd)) {
        kwrd = 65;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.frame = 1;
        else if (cmpwrd("off", cmd)) pcm.frame = 0;
        else if (cmpwrd("circle", cmd)) pcm.frame = 2;
        else goto err;
    } else if (cmpwrd("grid", cmd)) {
        kwrd = 17;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.grflag = 1;
        else if (cmpwrd("off", cmd)) pcm.grflag = 0;
        else if (cmpwrd("horizontal", cmd)) pcm.grflag = 2;
        else if (cmpwrd("vertical", cmd)) pcm.grflag = 3;
        else goto err;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt) == null) goto err;
            else pcm.grstyl = itt;
            if ((cmd = nxtwrd(cmd)) != null) {
                if (intprs(cmd, &itt) == null) goto err;
                else pcm.grcolr = itt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (intprs(cmd, &itt) == null) goto err;
                    else pcm.grthck = itt;
                }
            }
        }
        if (pcm.grflag) {
            snprintf(pout, 1255, "grid is on, style %i color %i thickness %d\n", pcm.grstyl, pcm.grcolr, pcm.grthck);
            gaprnt(2, pout);
        } else {
            gaprnt(2, "grid is off\n");
        }
    } else if (cmpwrd("clskip", cmd)) {
        kwrd = 89;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if (itt < 1) goto err;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &val1) == null) goto err;
            gxclmn(val1);
        }
        pcm.clskip = itt;
    } else if (cmpwrd("clopts", cmd)) {
        kwrd = 72;
        itt = pcm.clcol;
        itt1 = pcm.clthck;
        val1 = pcm.clsiz;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt1) == null) goto xlerr;
            else {
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (getdbl(cmd, &val1) == null) goto xlerr;
                }
            }
        }
        pcm.clcol = itt;
        pcm.clthck = itt1;
        pcm.clsiz = val1;
        snprintf(pout, 1255, "SET CLOPTS values:  Color = %i Thickness = %i", pcm.clcol, pcm.clthck);
        gaprnt(2, pout);
        snprintf(pout, 1255, " Size = %g\n", pcm.clsiz);
        gaprnt(2, pout);
    } else if (cmpwrd("shpopts", cmd)) {
        kwrd = 120;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        pcm.fillpoly = itt;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt) == null) {
                gaprnt(0, "SET SHPOPTS Error: Invalid mark type\n");
            } else {
                pcm.marktype = itt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (getdbl(cmd, &val1) == null) {
                        gaprnt(0, "SET SHPOPTS Error: Invalid mark size\n");
                    } else {
                        pcm.marksize = val1;
                    }
                }
            }
        }
        snprintf(pout, 1255, "SET SHPOPTS values:  polygon fill color = %i  ", pcm.fillpoly);
        gaprnt(2, pout);
        snprintf(pout, 1255, "mark type = %i  ", pcm.marktype);
        gaprnt(2, pout);
        snprintf(pout, 1255, "mark size = %g \n", pcm.marksize);
        gaprnt(2, pout);
    } else if (cmpwrd("wxopt", cmd)) {
        kwrd = 93;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        else if (cmpwrd("wxsym", cmd)) pcm.wxopt = 1;
        else if (cmpwrd("mark", cmd)) pcm.wxopt = 2;
        else if (cmpwrd("char", cmd)) pcm.wxopt = 3;
        else goto err;
    } else if (cmpwrd("wxcols", cmd)) {
        kwrd = 84;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, itmp) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, itmp + 1) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, itmp + 2) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, itmp + 3) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, itmp + 4) == null) goto err;
        for (i1 = 0; i1 < 5; i1++) pcm.wxcols[i1] = itmp[i1];
        gaprnt(2, "New WXCOLS have been set\n");
    } else if (cmpwrd("lfcols", cmd)) {
        kwrd = 83;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt1) == null) goto err;
        pcm.lfc1 = itt;
        pcm.lfc2 = itt1;
        snprintf(pout, 1255, "LineFill Colors: Above = %i  Below = %i\n",
                 pcm.lfc1, pcm.lfc2);
        gaprnt(2, pout);
    } else if (cmpwrd("rband", cmd)) {
        kwrd = 90;
        itt = -1;
        if ((cmd = nxtwrd(cmd)) == null) goto rbberr;
        if (intprs(cmd, &i1) == null) goto rbberr;
        if ((cmd = nxtwrd(cmd)) == null) goto rbberr;
        if (*cmd == 'm' && *(cmd + 1) == 'b') {
            cmd += 2;
            if (intprs(cmd, &(itt)) == null) goto rbberr;
            if ((cmd = nxtwrd(cmd)) == null) goto rbberr;
            if (itt > 3) itt = 3;
        }
        if (cmpwrd("box", cmd)) i2 = 1;
        else if (cmpwrd("line", cmd)) i2 = 2;
        else goto rbberr;
        if ((cmd = nxtwrd(cmd)) == null) goto rbberr;
        if (getdbl(cmd, &xlo) == null) goto rbberr;
        if ((cmd = nxtwrd(cmd)) == null) goto rbberr;
        if (getdbl(cmd, &ylo) == null) goto rbberr;
        if ((cmd = nxtwrd(cmd)) == null) goto rbberr;
        if (getdbl(cmd, &xhi) == null) goto rbberr;
        if ((cmd = nxtwrd(cmd)) == null) goto rbberr;
        if (getdbl(cmd, &yhi) == null) goto rbberr;
        dsubs.gxdrbb(i1, i2, xlo, ylo, xhi, yhi, itt);
    } else if (cmpwrd("button", cmd)) {
        kwrd = 80;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        pcm.btnfc = itt;
        pcm.btnftc = itt;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt) == null) goto err;
            pcm.btnbc = itt;
            pcm.btnbtc = itt;
            if ((cmd = nxtwrd(cmd)) != null) {
                if (intprs(cmd, &itt) == null) goto err;
                pcm.btnoc = itt;
                pcm.btnoc2 = itt;
                pcm.btnotc = itt;
                pcm.btnotc2 = itt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (intprs(cmd, &itt) == null) goto err;
                    pcm.btnoc2 = itt;
                    pcm.btnotc2 = itt;
                    if ((cmd = nxtwrd(cmd)) != null) {
                        if (intprs(cmd, &itt) == null) goto err;
                        pcm.btnftc = itt;
                        if ((cmd = nxtwrd(cmd)) != null) {
                            if (intprs(cmd, &itt) == null) goto err;
                            pcm.btnbtc = itt;
                            if ((cmd = nxtwrd(cmd)) != null) {
                                if (intprs(cmd, &itt) == null) goto err;
                                pcm.btnotc = itt;
                                pcm.btnotc2 = itt;
                                if ((cmd = nxtwrd(cmd)) != null) {
                                    if (intprs(cmd, &itt) == null) goto err;
                                    pcm.btnotc2 = itt;
                                    if ((cmd = nxtwrd(cmd)) != null) {
                                        if (intprs(cmd, &itt) == null) goto err;
                                        pcm.btnthk = itt;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        snprintf(pout, 1255, "SET BUTTON values:  Fc, Bc, Oc, Oc2 = %i %i %i %i ",
                 pcm.btnfc, pcm.btnbc, pcm.btnoc, pcm.btnoc2);
        gaprnt(2, pout);
        snprintf(pout, 1255, "Toggle Fc, Bc, Oc, Oc2 = %i %i %i %i ",
                 pcm.btnftc, pcm.btnbtc, pcm.btnotc, pcm.btnotc2);
        snprintf(pout, 1255, "Thick = %i\n", pcm.btnthk);
        gaprnt(2, pout);
    } else if (cmpwrd("dialog", cmd)) {
        kwrd = 108;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        pcm.dlgpc = itt;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt) == null) goto err;
            pcm.dlgfc = itt;
            if ((cmd = nxtwrd(cmd)) != null) {
                if (intprs(cmd, &itt) == null) goto err;
                pcm.dlgbc = itt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (intprs(cmd, &itt) == null) goto err;
                    pcm.dlgoc = itt;
                    if ((cmd = nxtwrd(cmd)) != null) {
                        if (intprs(cmd, &itt) == null) goto err;
                        pcm.dlgth = itt;
                        if ((cmd = nxtwrd(cmd)) != null) {
                            if (cmpwrd("n", cmd) || cmpwrd("numeric", cmd)) pcm.dlgnu = 1;
                        } else pcm.dlgnu = 0;
                    }
                }
            }
        }
        snprintf(pout, 1255, "SET DIALOG values:  Pc, Fc, Bc, Oc = %i %i %i %i ",
                 pcm.dlgpc, pcm.dlgfc, pcm.dlgbc, pcm.dlgoc);
        gaprnt(2, pout);
        if (pcm.dlgnu) {
            snprintf(pout, 1255, "Thick = %i ", pcm.dlgth);
            gaprnt(2, pout);
            snprintf(pout, 1255, "Args = numeric\n ");
            gaprnt(2, pout);
        } else {
            snprintf(pout, 1255, "Thick = %i\n", pcm.dlgth);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("xlpos", cmd)) {
        kwrd = 87;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &val1) == null) goto xlerr2;
        pcm.xlpos = val1;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (cmpwrd("b", cmd) || cmpwrd("bottom", cmd)) pcm.xlside = 0;
            if (cmpwrd("t", cmd) || cmpwrd("top", cmd)) pcm.xlside = 1;
        }
        if (pcm.xlside)
            snprintf(pout, 1255, "SET XLPOS values:  Offset = %g  Side = Top\n", pcm.xlpos);
        else
            snprintf(pout, 1255, "SET XLPOS values:  Offset = %g  Side = Bottom\n", pcm.xlpos);
        gaprnt(2, pout);
    } else if (cmpwrd("ylpos", cmd)) {
        kwrd = 88;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &val1) == null) goto xlerr2;
        pcm.ylpos = val1;
        pcm.ylpflg = 1;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (cmpwrd("r", cmd) || cmpwrd("right", cmd)) pcm.ylside = 1;
            if (cmpwrd("l", cmd) || cmpwrd("left", cmd)) pcm.ylside = 0;
        }
        snprintf(pout, 1255, "SET YLPOS values:  Offset = %g  Side = ", tt);
        gaprnt(2, pout);
        if (pcm.ylside) gaprnt(2, "Right\n");
        else gaprnt(2, "Left\n");
    } else if (cmpwrd("xlopts", cmd)) {
        kwrd = 70;
        itt = pcm.xlcol;
        itt1 = pcm.xlthck;
        val1 = pcm.xlsiz;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt1) == null) goto xlerr;
            else {
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (getdbl(cmd, &val1) == null) goto xlerr;
                }
            }
        }
        pcm.xlcol = itt;
        pcm.xlthck = itt1;
        pcm.xlsiz = val1;
        snprintf(pout, 1255, "SET XLOPTS values:  Color = %i  Thickness = %i  Size = %g \n",
                 pcm.xlcol, pcm.xlthck, pcm.xlsiz);
        gaprnt(2, pout);
    } else if (cmpwrd("ylopts", cmd)) {
        kwrd = 71;
        itt = pcm.ylcol;
        itt1 = pcm.ylthck;
        val1 = pcm.ylsiz;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt1) == null) goto xlerr;
            else {
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (getdbl(cmd, &val1) == null) goto xlerr;
                }
            }
        }
        pcm.ylcol = itt;
        pcm.ylthck = itt1;
        pcm.ylsiz = val1;
        snprintf(pout, 1255, "SET YLOPTS values:  Color = %i  Thickness = %i   Size = %g \n",
                 pcm.ylcol, pcm.ylthck, pcm.ylsiz);
        gaprnt(2, pout);
    } else if (cmpwrd("annot", cmd)) {
        kwrd = 56;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        pcm.anncol = itt;
        pcm.xlcol = itt;  /* set annot should change color of tic labels too */
        pcm.ylcol = itt;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt) == null) {
                gaprnt(0, "SET ANNOT Error: Invalid thickness value\n");
            } else {
                pcm.annthk = itt;
            }
        }
        snprintf(pout, 1255, "SET ANNOT values:  Color = %i  Thickness = %i\n",
                 pcm.anncol, pcm.annthk);
        gaprnt(2, pout);
    } else if (cmpwrd("line", cmd)) {
        kwrd = 41;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        pcm.lincol = itt;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (intprs(cmd, &itt) == null) {
                gaprnt(0, "SET LINE Error: Invalid linestyle value\n");
            } else {
                pcm.linstl = itt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (intprs(cmd, &itt) == null) {
                        gaprnt(0, "SET LINE Error: Invalid thickness value\n");
                    } else {
                        pcm.linthk = itt;
                    }
                }
            }
        }
        snprintf(pout, 1255, "SET LINE values:  color = %i  style = %i  thickness = %i\n",
                 pcm.lincol, pcm.linstl, pcm.linthk);
        gaprnt(2, pout);
    } else if (cmpwrd("map", cmd)) {
        kwrd = 46;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("auto", cmd)) {
            pcm.mapcol = -9;
        } else {
            if (intprs(cmd, &itt) == null) goto err;
            pcm.mapcol = itt;
            if ((cmd = nxtwrd(cmd)) != null) {
                if (intprs(cmd, &itt) == null) {
                    gaprnt(0, "SET MAP Error: Invalid linestyle value\n");
                } else {
                    pcm.mapstl = itt;
                    if ((cmd = nxtwrd(cmd)) != null) {
                        if (intprs(cmd, &itt) == null) {
                            gaprnt(0, "SET MAP Error: Invalid thickness value\n");
                        } else {
                            pcm.mapthk = itt;
                        }
                    }
                }
            }
        }
        if (pcm.mapcol < 0) {
            gaprnt(2, "SET MAP values:  auto\n");
        } else {
            snprintf(pout, 1255, "SET MAP values:  color = %i  style = %i  thickness = %i\n",
                     pcm.mapcol, pcm.mapstl, pcm.mapthk);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("string", cmd)) {
        kwrd = 42;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        pcm.strcol = itt;
        if ((cmd = nxtwrd(cmd)) != null) {
            itt = -1;
            for (i1 = 0; i1 < 9; i1++) if (cmpwrd(justs[i1], cmd)) itt = i1;
            if (itt < 0) {
                gaprnt(0, "SET STRING Error: Invalid justification value\n");
            } else {
                pcm.strjst = itt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (intprs(cmd, &itt) == null) {
                        gaprnt(0, "SET STRING Error: Invalid thickness value\n");
                    } else {
                        pcm.strthk = itt;
                        if ((cmd = nxtwrd(cmd)) != null) {
                            if (getdbl(cmd, &val1) == null) {
                                gaprnt(0, "SET STRING Error: Invalid rotation value\n");
                            } else {
                                pcm.strrot = val1;
                            }
                        }
                    }
                }
            }
        }
        snprintf(pout, 1255, "SET STRING values:  color = %i  just = %s",
                 pcm.strcol, justs[pcm.strjst]);
        gaprnt(2, pout);
        snprintf(pout, 1255, "  thickness = %i  rotation = %g\n",
                 pcm.strthk, pcm.strrot);
        gaprnt(2, pout);
    } else if (cmpwrd("strsiz", cmd)) {
        kwrd = 43;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &val1) == null) goto err;
        pcm.strhsz = val1;
        i1 = 1;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &val1) == null) {
                gaprnt(0, "SET STRSIZ Error:  Invalid vsize value\n");
            } else {
                pcm.strvsz = val1;
                i1 = 0;
            }
        }
        if (i1) pcm.strvsz = pcm.strhsz;
        snprintf(pout, 1255, "SET STRSIZ values:  hsize = %g  vsize = %g\n",
                 pcm.strhsz, pcm.strvsz);
        gaprnt(2, pout);
    } else if (cmpwrd("xaxis", cmd)) {
        kwrd = 19;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.axmin)) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.axmax)) == null) goto err;
        pcm.axflg = 1;
        pcm.axint = 0.0;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &v1) == null) goto err;
            else pcm.axint = v1;
        }
        snprintf(pout, 1255, "xaxis labels range %g %g incr %g \n", pcm.axmin, pcm.axmax, pcm.axint);
        gaprnt(2, pout);
    } else if (cmpwrd("yaxis", cmd)) {
        kwrd = 20;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.aymin)) == null) goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.aymax)) == null) goto err;
        pcm.ayflg = 1;
        pcm.ayint = 0.0;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &v1) == null) goto err;
            else pcm.ayint = v1;
        }
        snprintf(pout, 1255, "yaxis labels range %g %g incr %g \n", pcm.aymin, pcm.aymax, pcm.ayint);
        gaprnt(2, pout);
    } else if (cmpwrd("misswarn", cmd)) {
        kwrd = 79;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) itt = 1;
        else if (cmpwrd("off", cmd)) itt = 0;
        else goto err;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &id) == null) goto err;
        if (pfi == null) {
            snprintf(pout, 1255, "SET MISSWARN error:  no open files\n");
            gaprnt(0, pout);
            return (1);
        } else {
            for (i1 = 0; i1 < id - 1; i1++) {
                pfi = pfi.pforw;
                if (pfi == null) {
                    snprintf(pout, 1255, "SET MISSWARN error:  file %i not open\n", id);
                    gaprnt(0, pout);
                    return (1);
                }
            }
            pfi.errflg = itt;
        }
    } else if (cmpwrd("undef", cmd)) {
        kwrd = 117;
        if ((cmd = nxtwrd(cmd)) == null) {
            gaprnt(2, "Warning: SET UNDEF argument is missing; output undef value is unchanged.\n");
        } else if (cmpwrd("file", cmd)) {
            if ((cmd = nxtwrd(cmd)) == null) {
                gaprnt(2, "Warning: SET UNDEF FILE argument is missing; output undef value is unchanged.\n");
            } else if (intprs(cmd, &id) == null) {
                gaprnt(2, "Warning: SET UNDEF FILE argument is invalid; output undef value is unchanged.\n");
            } else {
                pfi = pcm.pfi1;
                if (pfi != null) {
                    for (i1 = 0; i1 < id - 1; i1++) {
                        pfi = pfi.pforw;
                        if (pfi == null) {
                            break;
                        }
                    }
                }
                if (pfi == null) {
                    snprintf(pout, 1255,
                             "Warning: SET UNDEF FILE -- file %d not open, output undef value is unchanged.\n", id);
                    gaprnt(2, pout);
                } else {
                    pcm.undef = pfi.undef;
                    snprintf(pout, 1255, "Output undef value copied from file %d : %s \n", id, pfi.dnam);
                    gaprnt(2, pout);
                }
            }
        } else if (cmpwrd("dfile", cmd)) {
            id = pcm.dfnum;
            pfi = pcm.pfi1;
            for (i1 = 0; i1 < id - 1; i1++) {
                pfi = pfi.pforw;
                if (pfi == null) {
                    break;
                }
            }
            if (pfi == null) {
                snprintf(pout, 1255,
                         "Warning: SET UNDEF DFILE -- default file %i not open, output undef value is unchanged\n", id);
                gaprnt(2, pout);
            } else {
                pcm.undef = pfi.undef;
                snprintf(pout, 1255, "Output undef value copied from default file %d : %s \n", id, pfi.dnam);
                gaprnt(2, pout);
            }
        } else {
            if (getdbl(cmd, &v1) == null) {
                gaprnt(2, "Warning: SET UNDEF argument is invalid, output undef value is unchanged\n");
            } else {
                pcm.undef = v1;
            }
        }
        snprintf(pout, 1255, "Output undef value is set to %12f \n", pcm.undef);
        gaprnt(2, pout);
    } else if (cmpwrd("dfile", cmd)) {
        kwrd = 14;
        if (pcm.pfid == null) goto errf;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &id) == null) goto err;
        pfi = pcm.pfi1;
        for (i1 = 0; i1 < id - 1; i1++) {
            pfi = pfi.pforw;
            if (pfi == null) {
                snprintf(pout, 1255, "SET DFILE error:  file %i not open\n", id);
                gaprnt(0, pout);
                return (1);
            }
        }
        snprintf(pout, 1255, "Default file set to: %s \n", pfi.name);
        gaprnt(2, pout);
        pcm.pfid = pfi;
        pcm.dfnum = id;
    } else if (cmpwrd("background", cmd)) {
        kwrd = 58;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        snprintf(pout, 1255, "background = %i \n", itt);
        gaprnt(2, pout);
        gxdbck(itt);
    } else if (cmpwrd("cthick", cmd)) {
        kwrd = 49;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &(pcm.cthick)) == null) goto err;
        snprintf(pout, 1255, "cthick = %i \n", pcm.cthick);
        gaprnt(2, pout);
    } else if (cmpwrd("cstyle", cmd)) {
        kwrd = 9;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &(pcm.cstyle)) == null) goto err;
        if (pcm.cstyle == 0) {
            snprintf(pout, 1255, "WARNING: cstyle=0; no lines will be plotted; try using 1 instead\n");
        } else {
            snprintf(pout, 1255, "cstyle = %i \n", pcm.cstyle);
        }
        gaprnt(2, pout);
    } else if (cmpwrd("digsiz", cmd) || cmpwrd("digsize", cmd)) {
        kwrd = 24;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.digsiz)) == null) goto err;
        snprintf(pout, 1255, "digsiz = %g \n", pcm.digsiz);
        gaprnt(2, pout);
    } else if (cmpwrd("dignum", cmd)) {
        kwrd = 23;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &itt) == null) goto err;
        if (itt < 0 || itt > 8) {
            gaprnt(0, "Invalid dignum value:  must be 0 to 8\n");
        } else {
            pcm.dignum = itt;
            snprintf(pout, 1255, "dignum = %i \n", pcm.dignum);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("axlim", cmd) || cmpwrd("vrange", cmd)) {
        kwrd = 15;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("auto", cmd)) pcm.aflag = 0;
        else {
            if (getdbl(cmd, &(pcm.rmin)) == null) goto err;
            pcm.aflag = 0;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (getdbl(cmd, &(pcm.rmax)) == null) goto err;
            pcm.aflag = -1;
            snprintf(pout, 1255, "1-D axis limits set: %g to %g \n", pcm.rmin, pcm.rmax);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("vrange2", cmd)) {
        kwrd = 67;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.rmin2)) == null) goto err;
        pcm.aflag2 = 0;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (getdbl(cmd, &(pcm.rmax2)) == null) goto err;
        pcm.aflag2 = -1;
        snprintf(pout, 1255, "Scatter Y axis limits set: %g to %g \n", pcm.rmin2, pcm.rmax2);
        gaprnt(2, pout);
    } else if (cmpwrd("strmden", cmd) || cmpwrd("strmopts", cmd)) {
        kwrd = 124;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, &(pcm.strmden)) == null) goto err;
        if ((cmd = nxtwrd(cmd)) != null) {
            if (getdbl(cmd, &tt) == null) goto err;
            if (tt > 0.001) pcm.strmarrd = tt;
            if ((cmd = nxtwrd(cmd)) != null) {
                if (getdbl(cmd, &tt) == null) goto err;
                if (tt >= 0.0) pcm.strmarrsz = tt;
                if ((cmd = nxtwrd(cmd)) != null) {
                    if (intprs(cmd, &i) == null) goto err;
                    if (i > 0 && i < 3) pcm.strmarrt = i;
                }
            }
        }
        snprintf(pout, 255, "Streamline options set to: Density %i Spacing %g Size %g Type %i\n",
                 pcm.strmden, pcm.strmarrd, pcm.strmarrsz, pcm.strmarrt);
        gaprnt(2, pout);
    } else if (cmpwrd("ccolor", cmd)) {
        kwrd = 10;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("rainbow", cmd)) {
            pcm.ccolor = -1;
            gaprnt(2, "ccolor = rainbow \n");
        } else if (cmpwrd("revrain", cmd)) {
            pcm.ccolor = -2;
            gaprnt(2, "ccolor = reverse rainbow \n");
        } else {
            if (intprs(cmd, &(pcm.ccolor)) == null) goto err;
            snprintf(pout, 1255, "ccolor = %i \n", pcm.ccolor);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("stid", cmd)) {
        kwrd = 30;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.stidflg = 1;
        else if (cmpwrd("off", cmd)) pcm.stidflg = 0;
        else goto err;
    } else if (cmpwrd("csmooth", cmd)) {
        kwrd = 16;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.csmth = 1;
        else if (cmpwrd("off", cmd)) pcm.csmth = 0;
        else if (cmpwrd("linear", cmd)) pcm.csmth = 2;
        else goto err;
    } else if (cmpwrd("cterp", cmd)) {
        kwrd = 34;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.cterp = 1;
        else if (cmpwrd("off", cmd)) pcm.cterp = 0;
        else goto err;
    } else if (cmpwrd("loopdim", cmd)) {
        kwrd = 11;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("x", cmd)) pcm.loopdim = 0;
        else if (cmpwrd("y", cmd)) pcm.loopdim = 1;
        else if (cmpwrd("z", cmd)) pcm.loopdim = 2;
        else if (cmpwrd("t", cmd)) pcm.loopdim = 3;
        else if (cmpwrd("e", cmd)) pcm.loopdim = 4;
        else goto err;
    } else if (cmpwrd("grads", cmd)) {
        kwrd = 31;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) {
            pcm.grdsflg = 1;
        } else if (cmpwrd("off", cmd)) {
            pcm.grdsflg = 0;
            pcm.timelabflg = 0;
        } else goto err;
    } else if (cmpwrd("timelab", cmd)) {
        kwrd = 100;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.timelabflg = 1;
        else if (cmpwrd("off", cmd)) pcm.timelabflg = 0;
        else goto err;
    } else if (cmpwrd("stnprint", cmd)) {
        kwrd = 102;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.stnprintflg = 1;
        else if (cmpwrd("off", cmd)) pcm.stnprintflg = 0;
        else goto err;
    } else if (cmpwrd("warn", cmd)) {
        kwrd = 101;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) mfcmn.warnflg = 1;
        else if (cmpwrd("off", cmd)) mfcmn.warnflg = 0;
        else goto err;
    } else if (cmpwrd("looping", cmd)) {
        kwrd = 12;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) {
            pcm.loopflg = 1;
            gaprnt(2, "Looping is on \n");
        } else if (cmpwrd("off", cmd)) {
            pcm.loopflg = 0;
            gaprnt(2, "Looping is off \n");
        } else goto err;
    } else if (cmpwrd("ens", cmd)) {
        kwrd = 112;
        if (pcm.pfid == null) goto errf;
        pfi = pcm.pfid;
        /* get the first ensemble name */
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        num = 1;
        getwrd(ename1, cmd, 16);
        /* get the second ensemble name */
        if ((cmd = nxtwrd(cmd)) != null) {
            num = 2;
            getwrd(ename2, cmd, 16);
            if (strcmp(ename1, ename2) == 0) num = 1;
        }
        ens = pfi.ens1;
        i = 0;
        enum1 = enum2 = -1;
        while (i < pfi.dnum[4]) {
            if (strcmp(ename1, ens.name) == 0) {
                enum1 = i;
            }
            if (num > 1) {
                if (strcmp(ename2, ens.name) == 0) enum2 = i;
            }
            i++;
            ens++;
        }
        if (enum1 < 0) goto err;
        if ((num > 1) && (enum2 < 0)) goto err;
        if (num == 1) enum2 = enum1;
        pcm.vdim[4] = num - 1;
        pcm.dmin[4] = enum1 + 1;
        pcm.dmax[4] = enum2 + 1;
        snprintf(pout, 1255, "E set to %g %g \n", pcm.dmin[4], pcm.dmax[4]);
        gaprnt(2, pout);
    } else if (cmpwrd("e", cmd)) {
        kwrd = 111;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (pcm.pfid == null) goto errf;
        pfi = pcm.pfid;
        num = 1;
        if (cmpwrd("last", cmd)) {
            v1 = pfi.dnum[4];
        } else {
            if (getdbl(cmd, &v1) == null) goto err;
        }
        v2 = v1;
        if ((cmd = nxtwrd(cmd)) != null) {
            num = 2;
            if (cmpwrd("last", cmd)) {
                v2 = pfi.dnum[4];
            } else {
                if (getdbl(cmd, &v2) == null) goto err;
            }
            if (v1 == v2) num = 1;
        }
        pcm.vdim[4] = num - 1;
        pcm.dmin[4] = v1;
        pcm.dmax[4] = v2;
        snprintf(pout, 1255, "E set to %g %g \n", pcm.dmin[4], pcm.dmax[4]);
        gaprnt(2, pout);
    } else if (cmpwrd("x", cmd) || cmpwrd("y", cmd) ||
               cmpwrd("z", cmd) || cmpwrd("t", cmd)) {
        if (*cmd == 'x') kwrd = 0;
        if (*cmd == 'y') kwrd = 1;
        if (*cmd == 'z') kwrd = 2;
        if (*cmd == 't') kwrd = 3;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (pcm.pfid == null) goto errf;
        pfi = pcm.pfid;
        num = 1;
        if (kwrd == 3 && cmpwrd("last", cmd)) {
            v1 = pfi.dnum[3];
        } else {
            if (getdbl(cmd, &v1) == null) goto err;
        }
        v2 = v1;
        if ((cmd = nxtwrd(cmd)) != null) {
            num = 2;
            if (kwrd == 3 && cmpwrd("last", cmd)) {
                v2 = pfi.dnum[3];
            } else {
                if (getdbl(cmd, &v2) == null) goto err;
            }
            if (dequal(v1, v2, 1.0e-8) == 0) num = 1;
        }
        pcm.vdim[kwrd] = num - 1;

        /* Try to save grid dims for write flag -ex */

        if (pfi.type == 1 && num == 2) {
            if (kwrd == 0) {
                pcm.xexflg = 1;
                pcm.x1ex = (int) (v1 + 0.001);
                pcm.x2ex = (int) (v2 + 0.001);
            }
            if (kwrd == 1) {
                pcm.yexflg = 1;
                pcm.y1ex = (int) (v1 + 0.001);
                pcm.y2ex = (int) (v2 + 0.001);
            }
        }

        if (pfi.type == 1 && num == 1) {
            v1 = floor(v1 + 0.5);
            pcm.vdim[kwrd] = 0;
        }
        if (kwrd == 3) {
            vals = pfi.grvals[3];
            gr2t(vals, v1, &(pcm.tmin));
            if (num == 1) pcm.tmax = pcm.tmin;
            else gr2t(vals, v2, &(pcm.tmax));
            gaprnt(2, "Time values set: ");
            snprintf(pout, 1255, "%i:%i:%i:%i ", pcm.tmin.yr, pcm.tmin.mo,
                     pcm.tmin.dy, pcm.tmin.hr);
            gaprnt(2, pout);
            snprintf(pout, 1255, "%i:%i:%i:%i \n", pcm.tmax.yr, pcm.tmax.mo,
                     pcm.tmax.dy, pcm.tmax.hr);
            gaprnt(2, pout);
        } else {
            if (pfi.type == 1) {
                conv = pfi.gr2ab[kwrd];
                vals = pfi.grvals[kwrd];
                pcm.dmin[kwrd] = conv(vals, v1);
                if (num == 1)
                    pcm.dmax[kwrd] = pcm.dmin[kwrd];
                else
                    pcm.dmax[kwrd] = conv(vals, v2);
            } else {
                pcm.dmin[kwrd] = v1;
                if (num == 1)
                    pcm.dmax[kwrd] = pcm.dmin[kwrd];
                else
                    pcm.dmax[kwrd] = v2;
            }
            snprintf(pout, 1255, "%s set to %g %g \n", kwds[kwrd + 4], pcm.dmin[kwrd], pcm.dmax[kwrd]);
            gaprnt(2, pout);
        }
    } else if (cmpwrd("lon", cmd) || cmpwrd("lat", cmd) || cmpwrd("lev", cmd)) {
        if (cmpwrd("lon", cmd)) {
            kwrd = 4;
            id = 0;
        }
        if (cmpwrd("lat", cmd)) {
            kwrd = 5;
            id = 1;
        }
        if (cmpwrd("lev", cmd)) {
            kwrd = 6;
            id = 2;
        }
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (pcm.pfid == null) goto errf;
        num = 1;
        if (getdbl(cmd, &(pcm.dmin[id])) == null) goto err;
        pcm.dmax[id] = pcm.dmin[id];
        if ((cmd = nxtwrd(cmd)) != null) {
            num = 2;
            if (getdbl(cmd, &(pcm.dmax[id])) == null) goto err;
            if (dequal(pcm.dmin[id], pcm.dmax[id], 1.0e-8) == 0) num = 1;
        }
        pcm.vdim[id] = num - 1;
        pfi = pcm.pfid;
        if (pfi.type == 1 && num == 1) {
            pcm.vdim[id] = 0;
            conv = pfi.ab2gr[id];
            vals = pfi.abvals[id];
            v1 = conv(vals, pcm.dmin[id]);
            v1 = floor(v1 + 0.5);
            conv = pfi.gr2ab[id];
            vals = pfi.grvals[id];
            pcm.dmin[id] = conv(vals, v1);
            pcm.dmax[id] = pcm.dmin[id];
        }
        snprintf(pout, 1255, "%s set to %g %g \n", kwds[id + 4], pcm.dmin[id], pcm.dmax[id]);
        gaprnt(2, pout);
    } else if (cmpwrd("time", cmd)) {
        kwrd = 7;
        id = 3;
        if (pcm.pfid == null) goto errf;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        num = 1;
        tdef = pcm.tmin;
        if (adtprs(cmd, &tdef, &(pcm.tmin)) == null) goto err;
        pcm.tmax = pcm.tmin;
        if ((cmd = nxtwrd(cmd)) != null) {
            num = 2;
            if (adtprs(cmd, &tdef, &(pcm.tmax)) == null) goto err;
        }
        pcm.vdim[3] = 1;
        if (num == 1) {
            pcm.vdim[3] = 0;
            pfi = pcm.pfid;
            vals = pfi.abvals[3];
            v1 = t2gr(vals, &(pcm.tmin));
            v1 = floor(v1 + 0.5);
            vals = pfi.grvals[3];
            gr2t(vals, v1, &(pcm.tmin));
            pcm.tmax = pcm.tmin;
        }
        gaprnt(2, "Time values set: ");
        snprintf(pout, 1255, "%i:%i:%i:%i ", pcm.tmin.yr, pcm.tmin.mo,
                 pcm.tmin.dy, pcm.tmin.hr);
        gaprnt(2, pout);
        snprintf(pout, 1255, "%i:%i:%i:%i \n", pcm.tmax.yr, pcm.tmax.mo,
                 pcm.tmax.dy, pcm.tmax.hr);
        gaprnt(2, pout);
    } else if (cmpwrd("datawarn", cmd)) {
        kwrd = 107;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (cmpwrd("on", cmd)) pcm.dwrnflg = 1;
        else if (cmpwrd("off", cmd)) pcm.dwrnflg = 0;
        else goto err;
    } else if (cmpwrd("tile", cmd)) {
        kwrd = 125;
        for (i = 0; i < 15; i++) itmp[i] = -999;
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, itmp) == null) goto err;  /* tile number */
        if (*(itmp + 0) < 0 || *(itmp + 0) >= COLORMAX) {
            snprintf(pout, 1255, "Tile number must be between 0 and %d\n", COLORMAX - 1);
            gaprnt(0, pout);
            goto err;
        }
        if ((cmd = nxtwrd(cmd)) == null) goto err;
        if (intprs(cmd, itmp + 1) == null) goto err;  /* tile type */
        if (*(itmp + 1) < 0 || *(itmp + 1) > 8) {
            snprintf(pout, 1255, "Tile type must be between 0 and 8\n");
            gaprnt(0, pout);
            goto err;
        }

        /* check for type 0 */
        if (*(itmp + 1) == 0) {
            if ((cmd = nxtwrd(cmd)) != null) {
                /* user has provided a file to define a new tile -- get the length */
                itt2 = 0;
                while (*(cmd + itt2) != '\n' && *(cmd + itt2) != '\0') itt2++; /* spaces are allowed */
                /* allocate memory for tile filename */
                tileimg = (char *) malloc(itt2 + 1);
                if (tileimg == null) {
                    gaprnt(0, "Memory allocation error for tile image filename\n");
                    goto err;
                }
                /* copy the user-supplied filename (mixed case) to tileimg */
                i2 = cmd - cmd1;
                for (i1 = 0; i1 < itt2; i1++) *(tileimg + i1) = *(com + i1 + i2);
                /* Make sure filename ends in ".png"  */
                i = itt2 - 4;
                if (i > 0) {
                    if (*(tileimg + i + 1) != 'p' || *(tileimg + i + 2) != 'n' || *(tileimg + i + 3) != 'g') {
                        if (*(tileimg + i + 1) != 'P' || *(tileimg + i + 2) != 'N' || *(tileimg + i + 3) != 'G') {
                            gaprnt(0, "Custom tile image file must be PNG format\n");
                            free(tileimg);
                            goto err;
                        }
                    }
                }
                *(tileimg + itt2) = '\0';      /* ensure tile image filename is null-terminated */
                /* check if file exists*/
                FILE *tileimgfile = null;
                tileimgfile = fopen(tileimg, "rb");
                if (tileimgfile == null) {
                    gaprnt(0, "Unable to open tile image file\n");
                    free(tileimg);
                    goto err;
                } else fclose(tileimgfile);
            }
        } else {
            /* get rest of args for on-the-fly tile generation */
            for (i = 2; i < 7; i++) {
                if ((cmd = nxtwrd(cmd)) == null) break;
                if (intprs(cmd, itmp + i) == null) goto err;
            }
        }
        gxdbsetpatt(itmp, tileimg);
        if (tileimg) free(tileimg);
    } else if (cmpwrd("fill", cmd)) {
        kwrd = 96;
        pat = (char *) galloc(7, "fillpat");
        if ((cmd = nxtwrd(cmd)) == null) goto pterr;
        if (cmpwrd("on", cmd)) {
            snprintf(pat, 6, "%s", "on");
            pcm.ptflg = 1;
        } else if (cmpwrd("off", cmd)) {
            snprintf(pat, 6, "%s", "off");
            pcm.ptflg = 0;
        } else if (cmpwrd("open", cmd)) {
            pcm.ptflg = 1;
            pcm.ptopt = 0;
            snprintf(pat, 6, "%s", "open");
        } else if (cmpwrd("solid", cmd)) {
            snprintf(pat, 6, "%s", "solid");
            pcm.ptflg = 1;
            pcm.ptopt = 1;
        } else if (cmpwrd("dot", cmd)) {
            snprintf(pat, 6, "%s", "dot");
            pcm.ptflg = 1;
            pcm.ptopt = 2;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (intprs(cmd, &num) == null) goto err;
            if (num < 1 || num > 6)
                gaprnt(0, "Invalid ptrnden value:  must be integer 1 to 6\n");
            else
                pcm.ptden = num;
        } else if (cmpwrd("line", cmd)) {
            snprintf(pat, 6, "%s", "line");
            pcm.ptflg = 1;
            pcm.ptopt = 3;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (intprs(cmd, &num) == null) goto err;
            if (num < 1 || num > 5)
                gaprnt(0, "Invalid ptrnden value:  must be integer 1 to 5\n");
            else
                pcm.ptden = num;
            if ((cmd = nxtwrd(cmd)) == null) goto err;
            if (intprs(cmd, &num) == null) goto err;
            if (num != -90 && num != 90 && num != -60 && num != 60 && num != -30 && num != 30
                && num != -45 && num != 45 && num != 0) {
                gaprnt(0, "Invalid ptrnang value:  must be -90, -60, -45 -30\n");
                gaprnt(0, " 	                        0, 30. 45, 60, or 90\n");
            } else
                pcm.ptang = num;
        } else goto err;

        if (cmpwrd("line", pat))
            snprintf(pout, 1255, "SET FILL values: %s %d %d\n", pat, pcm.ptden, pcm.ptang);
        else if (cmpwrd("dot", pat))
            snprintf(pout, 1255, "SET FILL values: %s %d\n", pat, pcm.ptden);
        else
            snprintf(pout, 1255, "SET FILL values: %s\n", pat);
        gaprnt(2, pout);
        gree(pat, "f234");
    } else {
        gaprnt(0, "SET error: Invalid operand\n");
        gaprnt(0, "  Operand = ");
        gaprnt(0, cmd);
        gaprnt(0, "\n");
        return (1);
    }
    return (0);

    err:
    gaprnt(0, "SET error:  Missing or invalid arguments ");
    snprintf(pout, 1255, "for %s option\n", kwds[kwrd]);
    gaprnt(0, pout);
    return (1);

    errf:
    gaprnt(0, "SET Error:  No files open yet\n");
    return (1);

    xlerr:
    gaprnt(0, "SET XLOPTS(YLOPTS,CLOPTS) Syntax Error");
    gaprnt(0, "  Syntax is: SET XLOPTS color thickness size");
    return (1);

    xlerr2:
    gaprnt(0, "SET XLPOS(YLPOS) Syntax Error");
    gaprnt(0, "  Syntax is: SET XLPOS offset side\n");
    return (1);

    rbberr:
    gaprnt(0, "SET RBAND Syntax Error.");
    gaprnt(0, "  Syntax is: SET RBAND num type xlo ylo xhi yhi\n");
    return (1);

    pterr:
    gaprnt(0, "SET FILL Syntax Error.");
    gaprnt(0, "  Syntax is: SET FILL type [density] [angle]\n");
    return (1);

    drerr:
    gaprnt(0, "SET DROPMENU Syntax Error.");
    gaprnt(0, "  Syntax is: SET DROPMENU fc bc oc1 oc2 tfc tbc toc1 toc2\n");
    gaprnt(0, "      bfc bbc boc1 boc2 soc1 soc2 thick\n");
    return (1);
}
    
    
    private DataVariable GetVarType(string name)
    {
        if (name == "Temperature")
        {
            return DataVariable.Temperature;
        }

        if (name == "Pressure")
        {
            return DataVariable.Pressure;
        }
        else if (name == "Pressure reduced to MSL")
        {
            return DataVariable.Pressure;
        }
        else if (name == "Cloud mixing ratio")
        {
            return DataVariable.CloudMixingRatio;
        }
        else if (name == "Ice water mixing ratio")
        {
            return DataVariable.IceWaterMixingRatio;
        }
        else if (name == "Rain mixing ratio")
        {
            return DataVariable.RainMixingRatio;
        }
        else if (name == "Snow mixing ratio")
        {
            return DataVariable.SnowMixingRatio;
        }
        else if (name == "Graupel (snow pellets)")
        {
            return DataVariable.Graupel;
        }
        else if (name == "Visibility")
        {
            return DataVariable.Visibility;
        }
        else if (name == "u-component of wind")
        {
            return DataVariable.UWind;
        }
        else if (name == "v-component of wind")
        {
            return DataVariable.VWind;
        }
        else if (name == "Wind speed (gust)")
        {
            return DataVariable.WindGust;
        }
        else if (name == "Geopotential height")
        {
            return DataVariable.GeopotentialHeight;
        }
        else if (name == "Relative humidity")
        {
            return DataVariable.RelativeHumidity;
        }
        else if (name == "Specific humidity")
        {
            return DataVariable.SpecificHumidity;
        }
        else if (name == "Vertical velocity (pressure)")
        {
            return DataVariable.VerticalVorticity;
        }
        else if (name == "Vertical velocity (geometric)")
        {
            return DataVariable.VerticalVelocity;
        }
        else if (name == "Absolute vorticity")
        {
            return DataVariable.AbsoluteVorticity;
        }
        else if (name == "Total cloud cover")
        {
            return DataVariable.TotalCloudCover;
        }
        else if (name == "Dew point temperature")
        {
            return DataVariable.Dewpoint;
        }
        else if (name == "Convective available potential energy")
        {
            return DataVariable.CAPE;
        }
        else if (name == "Convective inhibition")
        {
            return DataVariable.CIN;
        }
        else if (name == "Storm relative helicity")
        {
            return DataVariable.SRH;
        }
        else if (name == "Soil temperature")
        {
            return DataVariable.SoilTemperature;
        }
        else if (name == "Water equivalent of accumulated snow depth")
        {
            return DataVariable.WaterEquivalentOfSnowDepth;
        }
        else if (name == "Snow depth")
        {
            return DataVariable.SnowDepth;
        }
        else if (name == "Ice thickness")
        {
            return DataVariable.IceThickness;
        }
        else if (name == "Apparent Temperature ")
        {
            return DataVariable.ApparentTemperature;
        }
        else if (name == "Ice growth rate")
        {
            return DataVariable.IceGrowthRate;
        }
        else if (name == "Percent frozen precipitation")
        {
            return DataVariable.PercentFrozenPrecip;
        }
        else if (name == "Precipitation rate")
        {
            return DataVariable.PrecipRate;
        }
        else if (name == "Surface roughness")
        {
            return DataVariable.SurfaceRoughness;
        }
        else if (name == "Vegetation")
        {
            return DataVariable.Vegetation;
        }
        else if (name == "Soil type")
        {
            return DataVariable.SoilType;
        }
        else if (name == "Precipitable water")
        {
            return DataVariable.PrecipitableWater;
        }
        else if (name == "Cloud water")
        {
            return DataVariable.CloudWater;
        }
        else if (name == "Total ozone")
        {
            return DataVariable.TotalOzone;
        }
        else if (name == "Low cloud cover")
        {
            return DataVariable.LowCloudCover;
        }
        else if (name == "Medium cloud cover")
        {
            return DataVariable.MediumCloudCover;
        }
        else if (name == "High cloud cover")
        {
            return DataVariable.HighCloudCover;
        }
        else if (name == "ICAO Standard Atmosphere Reference Height")
        {
            return DataVariable.ICAOReferenceHeight;
        }
        else if (name == "Potential temperature")
        {
            return DataVariable.Potentialemperature;
        }
        else if (name == "Land cover (1=land, 2=sea)")
        {
            return DataVariable.LandCover;
        }
        else if (name == "Ice cover")
        {
            return DataVariable.IceCover;
        }

        throw new Exception($"Variable name {name} not mapped");
    }
}