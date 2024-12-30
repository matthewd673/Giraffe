using System.Text.RegularExpressions;

// ManualScanner is generated
public class ManualScanner
{
    private readonly Dictionary<int, Regex> tokenDef = new()
    {
        {
            0,
            new("a")
        },
        {
            1,
            new("b")
        },
        {
            2,
            new("c")
        },
        {
            3,
            new("d")
        },
        {
            4,
            new("e")
        },
    };
    private const int Eof = 5;
    public readonly struct Token(int type, string image)
    {
        public int Type { get; } = type;
        public string Image { get; } = image;
    }

    public class ScannerException(int index, string message) : Exception(message)
    {
        public int Index { get; } = index;
    }

    private readonly string text;
    private int scanIndex = 0;
    private Token? nextToken = null;
    public ManualScanner(string text)
    {
        this.text = text;
        nextToken = ScanNext();
    }

    public Token Peek() => nextToken!.Value;
    public Token Eat()
    {
        Token consumed = nextToken!.Value;
        nextToken = ScanNext();
        return consumed;
    }

    private Token ScanNext()
    {
        if (scanIndex >= text.Length)
        {
            return new(Eof, "");
        }

        Token? best = null;
        foreach (int t in tokenDef.Keys)
        {
            Match match = tokenDef[t].Match(text, scanIndex);
            if (!match.Success || match.Index > scanIndex)
            {
                continue;
            }

            if (!best.HasValue)
            {
                best = new(t, match.Value);
                continue;
            }

            if (match.Length > best.Value.Image.Length)
            {
                best = new(t, match.Value);
            }
        }

        if (!best.HasValue)
        {
            throw new ScannerException(scanIndex, $"Illegal character: '{text[scanIndex]}'");
        }

        scanIndex += best.Value.Image.Length;
        return best.Value;
    }
}
