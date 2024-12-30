using System.Text.RegularExpressions;

namespace Giraffe;

public class ManualScanner {
  public readonly struct Token(TokenType type, string image) {
    public TokenType Type { get; } = type;
    public string Image { get; } = image;
  }

  public class ScannerException(int index, string message) : Exception(message) {
    public int Index { get; } = index;
  }

  // TokenType is generated
  public enum TokenType
  {
    a,
    b,
    c,
    d,
    e,
    Eof,
  }

  // tokenDef is generated
  private readonly Dictionary<TokenType, Regex> tokenDef = new()
  {
    {
      TokenType.a,
      new("a")
    },
    {
      TokenType.b,
      new("b")
    },
    {
      TokenType.c,
      new("c")
    },
    {
      TokenType.d,
      new("d")
    },
    {
      TokenType.e,
      new("e")
    },
  };

  private readonly string text;
  private int scanIndex = 0;
  private Token? nextToken = null;

  public ManualScanner(string text) {
    this.text = text;
    nextToken = ScanNext();
  }

  public Token Peek() {
    return nextToken!.Value;
  }

  public Token Eat() {
    Token consumed = nextToken!.Value;
    nextToken = ScanNext();
    return consumed;
  }

  private Token ScanNext() {
    // Return end of file once we reach it
    if (scanIndex >= text.Length) {
      return new(TokenType.Eof, "");
    }

    // Find the best possible token match from defs list
    Token? best = null;
    foreach (TokenType t in tokenDef.Keys) {
      // Find first match
      Match match = tokenDef[t].Match(text, scanIndex);

      // Skip if no match or match is not at beginning of string
      if (!match.Success || match.Index > scanIndex) {
        continue;
      }

      if (!best.HasValue) {
        best = new(t, match.Value);
        continue;
      }

      // Check if this is better than the current best (longer is better)
      if (match.Length > best.Value.Image.Length) {
        best = new(t, match.Value);
      }
    }

    if (!best.HasValue) {
      throw new ScannerException(scanIndex, $"Illegal character: '{text[scanIndex]}'");
    }

    // Trim best match from string and return
    scanIndex += best.Value.Image.Length;
    return best.Value;
  }
}