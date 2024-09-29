namespace GradsSharp.Models.Internal;

public class MapData
{
    public MapData()
    {
        FileName = null;
    }

    public string FileName { get; set; }
    public List<MapDataRecord> Records { get; set; } = new();
}

public class MapDataRecord
{
    public MapDataRecord()
    {
    }

    public int FilePos { get; set; } = 0;
    public int[] Header { get; set; } = new int[3];
    public int[] Data { get; set; } = new int[256 * 2];
}