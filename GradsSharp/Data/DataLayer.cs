using GradsSharp.Drawing;
using GradsSharp.Models;

namespace GradsSharp.Data;

public class DataLayer
{
    private DrawingContext _context;

    public DataLayer()
    {
        _context = new DrawingContext();
    }

    public void Open(string file, DataFormat format)
    {
        _context.GaUser.Open(file, format);
    }

    public List<VariableDefinition> GetAvailableVariables()
    {
        if (_context.CommonData.fnum == 0)
        {
            throw new Exception("No File open yet");
        }

        List<VariableDefinition> result = new();
        
        _context.CommonData.pfid.pvar1.ForEach(x => result.Add(x.variableDefinition));

        return result;
    }
}