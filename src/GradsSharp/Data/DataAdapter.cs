
using GradsSharp.Drawing;
using GradsSharp.Models;

namespace GradsSharp.Data;

internal class DataAdapter : IDataAdapter
{
    private DrawingContext _drawingContext;

    public DataAdapter(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    public double[] GetVariable(VariableDefinition definition)
    {
        if (_drawingContext.CommonData.pdf1 != null)
        {
            foreach (var dfn in _drawingContext.CommonData.pdf1)
            {
                if (dfn.abbrv == definition.VariableName)
                {
                    return dfn.pfi.rbuf;
                }
            }
        }

        if (_drawingContext.CommonData.fnum == 0)
        {
            throw new Exception("No files open yet");
        }

        if (_drawingContext.CommonData.pfid != null)
        {
            return _drawingContext.CommonData.pfid.DataReader.ReadData(_drawingContext.CommonData,
                _drawingContext.CommonData.pfid, definition);
        }

        return Array.Empty<double>();
    }

    public void DefineVariable(string name, double[] data)
    {
        _drawingContext.GaUser.Define(name, data);
    }
}