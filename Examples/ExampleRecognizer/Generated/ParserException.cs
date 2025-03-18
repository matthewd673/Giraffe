namespace ExampleRecognizer.Generated;
public class ParserException(string message, int index, int row, int column) : FrontendException(message, index, row, column)
{
};