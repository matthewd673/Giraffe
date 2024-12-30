using System.Text.RegularExpressions;

namespace Giraffe;

public class ManualParser(string text) {
  public string Text { get; } = text;

  public readonly struct Token(TokenType type, string image) {
    public TokenType Type { get; } = type;
    public string Image { get; } = image;
  }

  public class ScannerException(int index, string message) : Exception(message) {
    public int Index { get; } = index;
  }

  public class ParserException(Token token, string message) : Exception(message) {
    public Token Token { get; } = token;
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

  // productions is generated
  private readonly List<List<int>> productions = [[2, 3, 4, 5, 6, -6, ], [-1, ], [], [-2, ], [], [-3, ], [-4, ], [], [-5, ], [], ];

  // parseTable is generated
  private readonly Dictionary<(int, int), int> parseTable = new()
  {
    {
      (0, 0),
      0
    },
    {
      (0, 1),
      0
    },
    {
      (0, 2),
      0
    },
    {
      (1, 0),
      1
    },
    {
      (1, 1),
      2
    },
    {
      (1, 2),
      2
    },
    {
      (2, 1),
      3
    },
    {
      (2, 2),
      4
    },
    {
      (3, 2),
      5
    },
    {
      (4, 3),
      6
    },
    {
      (4, 4),
      7
    },
    {
      (4, 5),
      7
    },
    {
      (5, 4),
      8
    },
    {
      (5, 5),
      9
    },
  };

  private int scanIndex = 0;
  private Token nextToken;
  private const int EntryNonterminal = 0;

  public List<Token> Parse() {
    nextToken = ScanNext();
    return ParseNonterminal(EntryNonterminal).ToList();
  }

  // Temporary debug helper
  private string ProductionToReadableString(List<int> production) =>
    $"[{string.Join(", ",
                    production.Select(i =>
                                        i < 0
                                          ? $"{(TokenType)(-i - 1)}"
                                          : $"NT_{i - 1}"))}]";

  private IEnumerable<Token> ParseNonterminal(int nonterminal) {
    Console.WriteLine($"Parsing: NT {nonterminal}");

    if (parseTable.TryGetValue((nonterminal, (int)nextToken.Type),
                               out int production)) {
      foreach (Token token in ParseProduction(production)) {
        yield return token;
      }
    }
    else {
      throw new ParserException(nextToken, $"{nextToken.Type} is not in FIRST(NT_{nonterminal})");
    }
  }

  private IEnumerable<Token> ParseProduction(int production) {
    Console.WriteLine($"Production: {production} ({ProductionToReadableString(productions[production])})");

    for (int currInd = 0; currInd < productions[production].Count; currInd += 1) {
      int nextInd = productions[production][currInd];

      if (nextInd > 0) { // It's a nonterminal
        foreach (Token t in ParseNonterminal(nextInd - 1)) {
          yield return t;
        }
        continue;
      }

      // It's a terminal
      int nextTerm = -nextInd - 1;
      Console.WriteLine($"Expecting: {nextTerm} ({(TokenType)nextTerm})");
      Console.WriteLine($"Seeing next: {nextToken.Type} ({(int)nextToken.Type})");
      if (nextTerm != (int)nextToken.Type) {
        throw new ParserException(nextToken, $"Unexpected token {nextToken.Type} (\"{nextToken.Image}\")");
      }

      Console.WriteLine($"Consuming: {nextToken.Type} (\"{nextToken.Image}\")");
      yield return nextToken;
      nextToken = ScanNext();
    }

    Console.WriteLine("Done!");
  }

  private Token ScanNext() {
    // Return end of file once we reach it
    if (scanIndex >= Text.Length) {
      return new(TokenType.Eof, "");
    }

    // Find the best possible token match from defs list
    Token? best = null;
    foreach (TokenType t in tokenDef.Keys) {
      // Find first match
      Match match = tokenDef[t].Match(Text, scanIndex);

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
      throw new ScannerException(scanIndex, $"Illegal character: '{Text[scanIndex]}'");
    }

    // Trim best match from string and return
    scanIndex += best.Value.Image.Length;
    return best.Value;
  }
}