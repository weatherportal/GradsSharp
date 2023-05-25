using GradsSharp.Drawing;
using GradsSharp.Models;

namespace GradsSharp.Data;

internal interface IGriddedDataReader
{
    public double[] ReadData(gafile file, VariableDefinition definition);
}