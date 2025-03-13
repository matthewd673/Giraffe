namespace Giraffe.Frontend;
public class ScannerException(string message, int index, int row, int column) : FrontendException(message, index, row, column)
{
};