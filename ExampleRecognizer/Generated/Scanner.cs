using System.Text.RegularExpressions;

namespace ExampleRecognizer.Generated;
public class Scanner
{
    private readonly Regex[] tokenDef = [new("a"), new("b"), new("c"), new("d"), new("e")];
    private readonly string[] names = ["a", "b", "c", "d", "e", "$$"];
    private readonly string input;
    private int scanIndex = 0;
    private Token? nextToken = null;
    public Scanner(string input)
    {
        this.input = input;
        nextToken = ScanNext();
    }

    public string NameOf(int terminal) => names[terminal];
    public Token Peek() => nextToken!.Value;
    public Token Eat()
    {
        Token consumed = nextToken!.Value;
        nextToken = ScanNext();
        return consumed;
    }

    private Token ScanNext()
    {
        if (scanIndex >= input.Length)
        {
            return new(tokenDef.Length, "");
        }

        Token? best = null;
        for (int t = 0; t < tokenDef.Length; t++)
        {
            Match match = tokenDef[t].Match(input, scanIndex);
            if (!match.Success || match.Index > scanIndex)
            {
                continue;
            }

            best ??= new(t, match.Value);
            if (match.Length > best.Value.Image.Length)
            {
                best = new(t, match.Value);
            }
        }

        if (!best.HasValue)
        {
            throw new ScannerException($"Illegal character '{input[scanIndex]}' at index {scanIndex}");
        }

        scanIndex += best.Value.Image.Length;
        return best.Value;
    }
}