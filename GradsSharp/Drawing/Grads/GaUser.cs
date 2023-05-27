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

    public void SetStringSize(int hsize, int vsize = -1)
    {
        pcm.strhsz = hsize;
        if (vsize > -1)
        {
            pcm.strvsz = vsize;
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
                if (!levels.Contains(pdef.FirstFixedSurfaceValue ?? 0))
                {
                    levels.Add(pdef.FirstFixedSurfaceValue ?? 0 / 100.0);
                }
            }
        }


        
        levels = levels.OrderByDescending(x => x).ToList();
        levels.Insert(0, 0);
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
            var vals = new double[] { dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 1, 0, 0};
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

    private string GetVarName(string name, FixedSurfaceType sfc)
    {
        return name + "_" + sfc.ToString();
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