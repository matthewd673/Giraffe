using System.Text.RegularExpressions;

namespace ExprParser.Generated;
public class Scanner(string input)
{
    private readonly Regex[] tokenDef = [new("[0-9]+"), new("\\+"), new("-"), new("\\*"), new("/")];
    private readonly string[] names = ["number", "add", "sub", "mul", "div", "<end of input>"];
    private int scanIndex;
    private Token? nextToken;
    public string NameOf(TokenKind terminal) => names[(int)terminal];
    public Token Peek()
    {
        nextToken ??= ScanNext();
        return nextToken;
    }

    public Token Eat()
    {
        nextToken ??= ScanNext();
        Token consumed = nextToken;
        nextToken = ScanNext();
        return consumed;
    }

    private Token ScanNext()
    {
        if (scanIndex >= input.Length)
        {
            return new(TokenKind.eof, "");
        }

        Token? best = null;
        for (int t = 0; t < tokenDef.Length; t++)
        {
            Match match = tokenDef[t].Match(input, scanIndex);
            if (!match.Success || match.Index > scanIndex)
            {
                continue;
            }

            best ??= new((TokenKind)t, match.Value);
            if (match.Length > best.Image.Length)
            {
                best = new((TokenKind)t, match.Value);
            }
        }

        if (best is null)
        {
            throw new ScannerException($"Illegal character '{input[scanIndex]}' at index {scanIndex}");
        }

        scanIndex += best.Image.Length;
        return best;
    }
}