using GradsSharp.Data.GridFunctions;
using GradsSharp.Models;
using GradsSharp.Models.Internal;
using Shouldly;

namespace GradsSharp.UnitTests.MathTests;

public class GridMathTests
{
    private IGradsGrid grid1;
    private IGradsGrid grid2;
    
    [SetUp]
    public void Setup()
    {
        grid1 = new GradsGrid();
        grid1.GridData = new double[] { 1, 2, 3, 4 };
        grid1.Undef = -9999;
        
        grid2 = new GradsGrid();
        grid2.GridData = new double[] { 5, 6, 7, 8 };
        grid2.Undef = -9999;
    }
    
    [Test]
    public void AddPrimitiveTest()
    {
        var result = grid1.Add(1);
        
        result.GridData[0].ShouldBe(2);
        result.GridData[1].ShouldBe(3);
        result.GridData[2].ShouldBe(4);
        result.GridData[3].ShouldBe(5);

        result = grid1 + 2;
        
        result.GridData[0].ShouldBe(3);
        result.GridData[1].ShouldBe(4);
        result.GridData[2].ShouldBe(5); 
        result.GridData[3].ShouldBe(6);
        
    }

    [Test]
    public void AddTwoGridTest()
    {
        var result = grid1.Add(grid2);
        
        result.GridData[0].ShouldBe(6);
        result.GridData[1].ShouldBe(8);
        result.GridData[2].ShouldBe(10);
        result.GridData[3].ShouldBe(12);
        
        result = grid1 + grid2;
        
        result.GridData[0].ShouldBe(6);
        result.GridData[1].ShouldBe(8);
        result.GridData[2].ShouldBe(10);
        result.GridData[3].ShouldBe(12);
    }
    
    [Test]
    public void SubtractPrimitiveTest()
    {
        var result = grid1.Subtract(1);
        
        result.GridData[0].ShouldBe(0);
        result.GridData[1].ShouldBe(1);
        result.GridData[2].ShouldBe(2);
        result.GridData[3].ShouldBe(3);

        result = grid1 - 2;
        
        result.GridData[0].ShouldBe(-1);
        result.GridData[1].ShouldBe(0);
        result.GridData[2].ShouldBe(1); 
        result.GridData[3].ShouldBe(2);
        
    }
    
    [Test]
    public void SubtractTwoGridTest()
    {
        var result = grid1.Subtract(grid2);
        
        result.GridData[0].ShouldBe(-4);
        result.GridData[1].ShouldBe(-4);
        result.GridData[2].ShouldBe(-4);
        result.GridData[3].ShouldBe(-4);
        
        result = grid1 - grid2;
        
        result.GridData[0].ShouldBe(-4);
        result.GridData[1].ShouldBe(-4);
        result.GridData[2].ShouldBe(-4);
        result.GridData[3].ShouldBe(-4);
    }
    
    [Test]
    public void MultiplyPrimitiveTest()
    {
        var result = grid1.Multiply(2);
        
        result.GridData[0].ShouldBe(2);
        result.GridData[1].ShouldBe(4);
        result.GridData[2].ShouldBe(6);
        result.GridData[3].ShouldBe(8);

        result = grid1 * 2;
        
        result.GridData[0].ShouldBe(2);
        result.GridData[1].ShouldBe(4);
        result.GridData[2].ShouldBe(6); 
        result.GridData[3].ShouldBe(8);
        
    }
    
    [Test]
    public void MultiplyTwoGridTest()
    {
        var result = grid1.Multiply(grid2);
        
        result.GridData[0].ShouldBe(5);
        result.GridData[1].ShouldBe(12);
        result.GridData[2].ShouldBe(21);
        result.GridData[3].ShouldBe(32);
        
        result = grid1 * grid2;
        
        result.GridData[0].ShouldBe(5);
        result.GridData[1].ShouldBe(12);
        result.GridData[2].ShouldBe(21);
        result.GridData[3].ShouldBe(32);
    }
    
    [Test]
    public void DividePrimitiveTest()
    {
        var result = grid1.Divide(2);
        
        result.GridData[0].ShouldBe(0.5);
        result.GridData[1].ShouldBe(1);
        result.GridData[2].ShouldBe(1.5);
        result.GridData[3].ShouldBe(2);

        result = grid1 / 2;
        
        result.GridData[0].ShouldBe(0.5);
        result.GridData[1].ShouldBe(1);
        result.GridData[2].ShouldBe(1.5); 
        result.GridData[3].ShouldBe(2);
        
    }
    
    [Test]
    public void DivideTwoGridTest()
    {
        var result = grid1.Divide(grid2);
        
        result.GridData[0].ShouldBe(0.2);
        result.GridData[1].ShouldBe(1.0 / 3);
        result.GridData[2].ShouldBe(3.0 / 7);
        result.GridData[3].ShouldBe(0.5);

        result = grid1 / grid2;
        
        result.GridData[0].ShouldBe(0.2);
        result.GridData[1].ShouldBe(1.0 / 3);
        result.GridData[2].ShouldBe(3.0 / 7);
        result.GridData[3].ShouldBe(0.5);
    }
    
    [Test]
    public void SqrtTest()
    {
        var result = grid1.Sqrt();
        
        result.GridData[0].ShouldBe(Math.Sqrt(1));
        result.GridData[1].ShouldBe(Math.Sqrt(2));
        result.GridData[2].ShouldBe(Math.Sqrt(3));
        result.GridData[3].ShouldBe(Math.Sqrt(4));
        
        result = GridMath.Sqrt(grid1);
        result.GridData[0].ShouldBe(Math.Sqrt(1));
        result.GridData[1].ShouldBe(Math.Sqrt(2));
        result.GridData[2].ShouldBe(Math.Sqrt(3));
        result.GridData[3].ShouldBe(Math.Sqrt(4));
    }
    
    [Test]
    public void PowTest()
    {
        var result = grid1.Pow(2);
        
        result.GridData[0].ShouldBe(Math.Pow(1, 2));
        result.GridData[1].ShouldBe(Math.Pow(2, 2));
        result.GridData[2].ShouldBe(Math.Pow(3, 2));
        result.GridData[3].ShouldBe(Math.Pow(4, 2));
        
        result = GridMath.Pow(grid1, 2);
        result.GridData[0].ShouldBe(Math.Pow(1, 2));
        result.GridData[1].ShouldBe(Math.Pow(2, 2));
        result.GridData[2].ShouldBe(Math.Pow(3, 2));
        result.GridData[3].ShouldBe(Math.Pow(4, 2));
        
        result = grid1 ^ 2;
        result.GridData[0].ShouldBe(Math.Pow(1, 2));
        result.GridData[1].ShouldBe(Math.Pow(2, 2));
        result.GridData[2].ShouldBe(Math.Pow(3, 2));
        result.GridData[3].ShouldBe(Math.Pow(4, 2));
    }
    
    [Test]
    public void ExpTest()
    {
        var result = grid1.Exp();
        
        result.GridData[0].ShouldBe(Math.Exp(1));
        result.GridData[1].ShouldBe(Math.Exp(2));
        result.GridData[2].ShouldBe(Math.Exp(3));
        result.GridData[3].ShouldBe(Math.Exp(4));
        
        result = GridMath.Exp(grid1);
        result.GridData[0].ShouldBe(Math.Exp(1));
        result.GridData[1].ShouldBe(Math.Exp(2));
        result.GridData[2].ShouldBe(Math.Exp(3));
        result.GridData[3].ShouldBe(Math.Exp(4));
    }
    
    [Test]
    public void LogTest()
    {
        var result = grid1.Log();
        
        result.GridData[0].ShouldBe(Math.Log(1));
        result.GridData[1].ShouldBe(Math.Log(2));
        result.GridData[2].ShouldBe(Math.Log(3));
        result.GridData[3].ShouldBe(Math.Log(4));
        
        result = GridMath.Log(grid1);
        result.GridData[0].ShouldBe(Math.Log(1));
        result.GridData[1].ShouldBe(Math.Log(2));
        result.GridData[2].ShouldBe(Math.Log(3));
        result.GridData[3].ShouldBe(Math.Log(4));
    }
    
    [Test]
    public void AvgTest()
    {
        var result = GridMath.Avg(grid1, grid2);
        
        result.GridData[0].ShouldBe(3);
        result.GridData[1].ShouldBe(4);
        result.GridData[2].ShouldBe(5);
        result.GridData[3].ShouldBe(6);
    }
}