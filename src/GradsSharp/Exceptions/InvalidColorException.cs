namespace GradsSharp.Exceptions;

public class InvalidColorException : Exception
{
    /// <summary>
    /// Exception thrown when an invalid color is being defined
    /// </summary>
    /// <param name="message"></param>
    public InvalidColorException(string message) : base(message)
    {
    }
    
}   