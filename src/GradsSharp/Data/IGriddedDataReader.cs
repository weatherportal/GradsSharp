using GradsSharp.Drawing;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data;

public interface IGriddedDataReader
{
    InputFile OpenFile(Stream stream, string file);
    InputFile OpenFile(string fileName);
    void CloseFile();
    void ReadData(IGradsGrid grid, VariableDefinition definition);
}