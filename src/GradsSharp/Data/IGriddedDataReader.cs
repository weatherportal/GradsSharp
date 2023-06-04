using GradsSharp.Drawing;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data;

public interface IGriddedDataReader
{
    InputFile OpenFile(string fileName);
    void ReadData(IGradsGrid grid, VariableDefinition definition);
}