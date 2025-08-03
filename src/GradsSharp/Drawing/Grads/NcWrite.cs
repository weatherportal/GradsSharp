using System.ComponentModel.DataAnnotations;
using GradsSharp.Models.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Research.Science.Data;

namespace GradsSharp.Drawing.Grads;

internal class NcWrite
{
    private DrawingContext _drawingContext;

    public NcWrite(DrawingContext context)
    {
        _drawingContext = context;
    }
    
    public void Write(string variable)
    {

        if (_drawingContext.CommonData.pdf1 == null)
        {
            _drawingContext.Logger.Log(LogLevel.Error, "No open file");
            return;
        }

        gadefn? pdf = null;
        foreach (var g in _drawingContext.CommonData.pdf1)
        {
            if (g.abbrv == variable)
            {
                pdf = g;
                break;
            }
        }

        if (pdf == null)
        {
            _drawingContext.Logger.Log(LogLevel.Error, $"Variable {variable} not found");
            return;
        }

        var pfi = pdf.pfi;
        
        var dataSet = DataSet.Open(_drawingContext.CommonData.sdfwname, ResourceOpenMode.Create);
        int i, rc, nvdims = 0;
        int[] dimids = {0,0,0,0,0};
        int[] count = {0,0,0,0,0};
        int[] start = {0,0,0,0,0};
        int[] vdims = { -999, -999, -999, -999, -999 };
        int xdimid=0,ydimid=0,zdimid=0,tdimid=0,edimid=0;
        int xvarid,yvarid,zvarid,tvarid,evarid,varid;
        int padX,padY,padZ,padT,padE,recflg;
        
        padX = padY = padZ = padT = padE = 0;
        
        if (_drawingContext.CommonData.sdfwpad > 0) {
            padX = 1;
            padY = 1;
            if (_drawingContext.CommonData.sdfwpad==1 || _drawingContext.CommonData.sdfwpad==3 || _drawingContext.CommonData.sdfwpad==5) padZ = 1;
            if (_drawingContext.CommonData.sdfwpad>=2) padT = 1;
            if (_drawingContext.CommonData.sdfwpad>=4) padE = 1;
        }
        
        if (pfi.dnum[0]>1 || padX>0) {
            /* define the dimension */

            var lonVar = WriteDimension(pfi, dataSet, 0, "lon");
            lonVar.Metadata["units"] = "degrees_east";
            lonVar.Metadata["long_name"] = "Longitude";
            
            // if (sdfdefdim (pcm->ncwid, "lon", pfi.dnum[0], &xdimid, &xvarid, 0, pcm->sdfwtype)) goto err;
            // /* assign default and user-defined attributes */
            // if (sdfwatt(pcm, xvarid, "lon", "units", "degrees_east")) goto err;
            // if (sdfwatt(pcm, xvarid, "lon", "long_name", "Longitude")) goto err;
            // if (sdfwatt(pcm, xvarid, "lon", NULL, NULL)) goto err;
            /* increment the number of varying dimensions */
            vdims[nvdims] = 0;
            nvdims++;
        }
        else {
            xdimid = -999;
            xvarid = -999;
        }
        
        if (pfi.dnum[1]>1 || padY>0) {
            /* define the dimension */

            var latVar = WriteDimension(pfi, dataSet, 1, "lat");
            latVar.Metadata["units"] = "degrees_north";
            latVar.Metadata["long_name"] = "Latitude";
            
            // if (sdfdefdim (pcm->ncwid, "lon", pfi.dnum[0], &xdimid, &xvarid, 0, pcm->sdfwtype)) goto err;
            // /* assign default and user-defined attributes */
            // if (sdfwatt(pcm, xvarid, "lon", "units", "degrees_east")) goto err;
            // if (sdfwatt(pcm, xvarid, "lon", "long_name", "Longitude")) goto err;
            // if (sdfwatt(pcm, xvarid, "lon", NULL, NULL)) goto err;
            /* increment the number of varying dimensions */
            vdims[nvdims] = 1;
            nvdims++;
        }
        else {
            ydimid = -999;
            yvarid = -999;
        }
        
        if (pfi.dnum[2]>1 || padZ>0) {
            /* define the dimension */

            var levVar = WriteDimension(pfi, dataSet, 2, "lev");
            levVar.Metadata["units"] = "millibar";
            levVar.Metadata["long_name"] = "Level";
            
            // if (sdfdefdim (pcm->ncwid, "lon", pfi.dnum[0], &xdimid, &xvarid, 0, pcm->sdfwtype)) goto err;
            // /* assign default and user-defined attributes */
            // if (sdfwatt(pcm, xvarid, "lon", "units", "degrees_east")) goto err;
            // if (sdfwatt(pcm, xvarid, "lon", "long_name", "Longitude")) goto err;
            // if (sdfwatt(pcm, xvarid, "lon", NULL, NULL)) goto err;
            /* increment the number of varying dimensions */
            vdims[nvdims] = 2;
            nvdims++;
        }
        else {
            zdimid = -999;
            zvarid = -999;
        }

        if (nvdims == 0)
        {
            _drawingContext.Logger.Log(LogLevel.Error, "defined variable has no varying dimensions");
            return;
        }
        
        for (i=0; i<nvdims; i++) {
            if      (vdims[nvdims-1-i] == 4) dimids[i] = edimid; 
            else if (vdims[nvdims-1-i] == 3) dimids[i] = tdimid; 
            else if (vdims[nvdims-1-i] == 2) dimids[i] = zdimid; 
            else if (vdims[nvdims-1-i] == 1) dimids[i] = ydimid; 
            else if (vdims[nvdims-1-i] == 0) dimids[i] = xdimid; 
        }
        
        for (i=0; i<nvdims; i++) {
            if      (vdims[nvdims-1-i] == 4) count[i] = pfi.dnum[4];
            else if (vdims[nvdims-1-i] == 3) count[i] = pfi.dnum[3];
            else if (vdims[nvdims-1-i] == 2) count[i] = pfi.dnum[2];
            else if (vdims[nvdims-1-i] == 1) count[i] = pfi.dnum[1];
            else if (vdims[nvdims-1-i] == 0) count[i] = pfi.dnum[0];
            start[i] = 0;
        }
        
        int nelems = pfi.dnum[0] * pfi.dnum[1] * pfi.dnum[2] * pfi.dnum[3] * pfi.dnum[4];
        if (_drawingContext.CommonData.sdfprec==8) {
            /* copy undef values into rbuf array where mask is 0 */
            
            double[,] data = new double[pfi.dnum[1], pfi.dnum[0]];

            int xc = 0;
            int yc = 0;
            
            for (int pos = 0; pos < nelems; pos++)
            {
                if (pfi.ubuf[pos] == 0) pfi.rbuf[pos] = _drawingContext.CommonData.undef;

                data[xc, yc] = pfi.rbuf[pos];

                yc++;
                if (yc == pfi.dnum[0])
                {
                    xc++;
                    yc = 0;
                }
            }
            /* write the grid of doubles */

            var dataVar = dataSet.AddVariable<double>(variable, data, "lat", "lon");

            dataVar.Metadata["_FillValue"] = _drawingContext.CommonData.undef;

        } 
        
        dataSet.Commit();
    }

    private Variable<double> WriteDimension(GradsFile pfi, DataSet dataSet, int dim, string name)
    {
        List<double> axis = new List<double>();
        
        if (dim == 3)
        {
            
        }
        else
        {
            var conv = pfi.gr2ab[dim];
            for (int i=1; i<=pfi.dnum[dim]; i++) {
                axis.Add(conv(pfi.grvals[dim],(double)(i+pfi.dimoff[dim])));
            }
        }

        return dataSet.AddVariable<double>(name, axis.ToArray(), name);

    }
}