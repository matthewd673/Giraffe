using System.Text.RegularExpressions;

namespace Giraffe.Frontend;
public class Scanner(string input)
{
    private readonly Regex[] tokenDef = [new("[a-z][a-z0-9_]*"), new("[A-Z][A-Z0-9_]*"), new("->"), new(";"), new("/(\\\\/|[^/])+/"), new("\\*"), new("\\.\\."), new("_"), new("\\s+")];
    private readonly string[] names = ["term_name", "nonterm_name", "arrow", "end", "regex", "star", "expand", "discard", "ws", "eof"];
    private readonly TokenKind[] ignored = [TokenKind.Ws];
    private int scanIndex;
    private int row;
    private int column;
    private Token? nextToken;
    public string NameOf(TokenKind terminal) => names[(int)terminal];
    public Token Peek()
    {
        nextToken ??= SkipIgnored();
        return nextToken;
    }

    public Token Eat()
    {
        nextToken ??= SkipIgnored();
        Token consumed = nextToken;
        nextToken = SkipIgnored();
        return consumed;
    }

    private Token SkipIgnored()
    {
        Token next;
        do
        {
            next = ScanNext();
        }
        while (ignored.Contains(next.Kind));
        return next;
    }

    private Token ScanNext()
    {
        if (scanIndex >= input.Length)
        {
            return new(TokenKind.Eof, "", scanIndex, row, column);
        }

        Token? best = null;
        for (int t = 0; t < tokenDef.Length; t++)
        {
            Match match = tokenDef[t].Match(input, scanIndex);
            if (!match.Success || match.Index > scanIndex)
            {
                continue;
            }

            best ??= new((TokenKind)t, match.Value, scanIndex, row, column);
            if (match.Length > best.Image.Length)
            {
                best = new((TokenKind)t, match.Value, scanIndex, row, column);
            }
        }

        if (best is null)
        {
            throw new ScannerException($"Illegal character '{input[scanIndex]}' at {row + 1}:{column + 1}");
        }

        scanIndex += best.Image.Length;
        foreach (char c in best.Image)
        {
            if (c == '\n')
            {
                column = 0;
                row += 1;
            }
            else if (!char.IsControl(c) || c == '\t')
            {
                column += 1;
            }
        }

        return best;
    }
}