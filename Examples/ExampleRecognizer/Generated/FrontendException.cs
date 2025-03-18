namespace ExampleRecognizer.Generated;
public class FrontendException(string message, int index, int row, int column) : Exception(message)
{
    public int Index { get; } = index;
    public int Row { get; } = row;
    public int Column { get; } = column;
}