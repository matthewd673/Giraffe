using System.Text.RegularExpressions;

namespace Giraffe;

public class ManualScanner {
  public readonly struct Token(int type, string image) {
    public int Type { get; } = type;
    public string Image { get; } = image;
  }

  public class ScannerException(int index, string message) : Exception(message) {
    public int Index { get; } = index;
  }

  // tokenDef is generated
  private readonly Dictionary<int, Regex> tokenDef = new()
  { {
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

  // Eof is generated
  private const int Eof = 5;
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
      return new(Eof, "");
    }

    // Find the best possible token match from defs list
    Token? best = null;
    foreach (int t in tokenDef.Keys) {
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