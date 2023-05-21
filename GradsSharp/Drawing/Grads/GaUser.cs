using GradsSharp.Models;

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
                SetLat(-90,90);
                SetLev(500);
            }
            else
            {
                SetY(1,pfi.dnum[1]);
                
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
            if(file.mfile!=null) file.mfile.Close();
            if(file.infile!=null) file.infile.Close();
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

    public void printim(string path, OutputFormat format = OutputFormat.Undefined, string backgroundImage = "", string foregroundImage = "", 
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
        
        if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3 || fmtflg == 4) {
            if (horizontalSize > 0 || verticalSize > 0) {
                GaGx.gaprnt(1, $"Warning: Image size specifications are incompatible with the {format} format and will be ignored");
                xin = -999;
                yin = -999;
            }
        }
        if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3 || fmtflg == 4) {
            if (bgImage != "" || fgImage != "") {
                GaGx.gaprnt(1, "Warning: Background/Foreground images are incompatible with the %s format and will be ignored");
                bgImage = "";
                fgImage = "";
            }
        }
        if (border < 1.0) {
            border = 0.0;
            if (fmtflg == 1 || fmtflg == 2 || fmtflg == 3) border = 0.15625;
        }

        int retcod = 0;
        int rc = _drawingContext.GaSubs.DrawingEngine.gxprint(path, xin, yin, bwin, fmtflg, bgImage, fgImage, tcolor, border);
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
        else {
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
        if(pcm.dbflg > 0) _drawingContext.GaSubs.gxfrme(2);
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
            //     pcm.sdfwname = NULL;
            // }
            // while (pcm.attr != NULL) {
            //     attr = pcm.attr;           /* point to first block in chain */
            //     if (attr.next == NULL) {
            //         pcm.attr = NULL;         /* first block is only block */
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
//       pcm.shpfname = NULL;
//     }
//     /* release chain of data base fields */
//     while (pcm.dbfld != NULL) {
//       /* point to first block in chain */
//       fld = pcm.dbfld;
//       if (fld.next == NULL) {
// 	/* first block is only block */
// 	pcm.dbfld = NULL;
//       }
//       else {
// 	/* move start of chain from 1st to 2nd block */
// 	nextfld = fld.next;
// 	pcm.dbfld = nextfld;
//       }
//       /* release memory from 1st block */
//       if (fld.value != NULL) gree(fld.value,"g87");
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
            //     pcm.kmlname = NULL;
            // }
            // if (pcm.tifname) {           /* release TIFF file name */
            //     gree(pcm.tifname, "g91");
            //     pcm.tifname = NULL;
            // }
            // if (pcm.gtifname) {          /* release GeoTIFF file name */
            //     gree(pcm.gtifname, "g92");
            //     pcm.gtifname = NULL;
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
        
        if (xlo < 0.0 || ylo < 0.0 || xhi > pcm.pxsize || yhi > pcm.pysize) {
            GaGx.gaprnt(0, "SET Error: vpage values beyond real page limits");
            throw new Exception("vpage values beyond real page limits");
        }
        if (yhi <= ylo || xhi <= xlo) throw new Exception("SET error:  Missing or invalid arguments for vpage option");
        if ((yhi - ylo) / (xhi - xlo) > pcm.pysize / pcm.pxsize) {
            pcm.ysiz = pcm.pysize;
            pcm.xsiz = pcm.pysize * (xhi - xlo) / (yhi - ylo);
        } else {
            pcm.xsiz = pcm.pxsize;
            pcm.ysiz = pcm.pxsize * (yhi - ylo) / (xhi - xlo);
        }
        _drawingContext.GaSubs.gxvpag(pcm.xsiz, pcm.ysiz, xlo, xhi, ylo, yhi);
        GaGx.gaprnt(2, $"Virtual page size = {pcm.xsiz} {pcm.ysiz}");
        gacln(1);
    }

    public void SetColor(int colorNr, int red, int green, int blue, int alpha = 255)
    {
        if (colorNr < 16 || colorNr > 2048) {
            GaGx.gaprnt(0, "SET RGB Error:  Color number must be between 16 and 2048 \n");
            throw new Exception("SET RGB Error:  Color number must be between 16 and 2048");
        }
        if (red < 0 || red > 255 || 
            green < 0 || green > 255 ||
            blue < 0 || blue > 255) {
            GaGx.gaprnt(0, "SET RGB Error:  RGB values must be between 0 and 255 \n");
            throw new Exception("SET RGB Error:  RGB values must be between 0 and 255");
        }
        if (alpha < -255 || alpha > 255) {
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
            if (xlo < 0.0 || ylo < 0.0 || xhi > pcm.xsiz || yhi > pcm.ysiz) {
                GaGx.gaprnt(0, "SET Error: parea values beyond page limits");
                throw new Exception("SET Error: parea values beyond page limits");
            }
            if (yhi <= ylo || xhi <= xlo) throw new Exception("SET error:  Missing or invalid arguments for parea option");
            pcm.pxmin = xlo;
            pcm.pxmax = xhi;
            pcm.pymin = ylo;
            pcm.pymax = yhi;
            pcm.paflg = true;
        }
    }

    public void SetCInt(int val)
    {
        if (val <= 0) {
            GaGx.gaprnt(0, "SET Error: cint must be greater than 0.0\n");
        } else {
            pcm.cint = val;
            GaGx.gaprnt(2, $"cint = {pcm.cint}");
        }
    }

    public void SetMPVals(OnOffSetting onOff, double lonmin = 0.0, double lonmax = 0.0, double latmin = 0.0, double latmax = 0.0)
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
        for (int i = 1; i < pcm.cflag; i++) {
            if (pcm.clevs[i] <= pcm.clevs[i - 1]) {
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
        if (pcm.cflag == 0) {
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
        if (pfi.type == 1 && num == 1) {
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
        
    }
    public void SetZ(double zMin, double zMax)
    {
        
    }

    public void SetT(double tMin)
    {
        
    }
    
    public void SetE(double eMin)
    {
        
    }
    

    
    
}