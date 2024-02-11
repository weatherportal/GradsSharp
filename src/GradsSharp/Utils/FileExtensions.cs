namespace GradsSharp.Utils;
using System.Text;

internal static class FileExtensions
{
    public static void WriteLine(this FileStream file, string line)
    {
        if(!line.EndsWith("\n")) line += "\n";
        
        file.Write(line);
    }
    
    public static void Write(this FileStream file, string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        file.Write(bytes, 0, bytes.Length);
    }
}