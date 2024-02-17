# Creating your own data reader

Custom readers can be built to allow GradsSharp to read data from file.  A default example implementation can be found on [Github](https://github.com/weatherportal/GradsSharp/tree/master/src/GradsSharp.DataReader.GFS)

## Implementing a reader

To implement a new reader create a new C# library project.  Create a new reader class that implements the IGriddedDataReader interface:

```csharp

public class MyOwnReader : IGriddedDataReader
{
    public InputFile OpenFile(Stream stream, string file)
    {

    }
    public InputFile OpenFile(string file)
    {

    }
    public void ReadData(IGradsGrid grid, VariableDefinition definition)
    {

    }
}

```

Implement the methods with your own logic.   The ```OpenFile``` method should return an ```InputFile``` instance with all information about the file including the dimension values.
The ```ReadData``` method actually reads data from the file.  The data should be stored in the ```grid.GridData``` property as a double array.  Also the ```Undefinedmask``` property should be set