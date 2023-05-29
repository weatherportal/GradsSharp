using GradsSharp.Drawing;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data;

internal interface IGriddedDataReader
{
    public double[] ReadData(GradsCommon pcmn, GradsFile file, VariableDefinition definition);
}