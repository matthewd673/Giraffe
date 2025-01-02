namespace Giraffe;

public class ReferenceParser(ReferenceScanner scanner) {
  public class ParserException(ReferenceScanner.Token token, string message) : Exception(message) {
    public ReferenceScanner.Token Token { get; } = token;
  }

  // productions is dynamic
  private readonly int[][] productions = [[2, 3, 4, 5, 6, -6, ], [-1, ], [], [-2, ], [], [-3, ], [-4, ], [], [-5, ], [], ];

  // parseTable is dynamic
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

  // TODO: EntryNonterminal should be dynamic
  private const int EntryNonterminal = 0;

  public List<ReferenceScanner.Token> Parse() {
    return ParseNonterminal(EntryNonterminal).ToList();
  }

  // TODO: Temporary debug helper
  private string ProductionToReadableString(int[] production) =>
    $"[{string.Join(", ", production.Select(i => i < 0 ? $"T_{-i - 1}" : $"NT_{i - 1}"))}]";

  private IEnumerable<ReferenceScanner.Token> ParseNonterminal(int nonterminal) {
    Console.WriteLine($"Parsing: NT {nonterminal}");

    if (parseTable.TryGetValue((nonterminal, scanner.Peek().Type), out int production)) {
      foreach (ReferenceScanner.Token token in ParseProduction(production)) {
        yield return token;
      }
    }
    else {
      throw new ParserException(scanner.Peek(), $"{scanner.Peek().Type} is not in FIRST(NT_{nonterminal})");
    }
  }

  private IEnumerable<ReferenceScanner.Token> ParseProduction(int production) {
    Console.WriteLine($"Production: {production} ({ProductionToReadableString(productions[production])})");

    for (int currInd = 0; currInd < productions[production].Length; currInd += 1) {
      int nextInd = productions[production][currInd];

      if (nextInd > 0) { // It's a nonterminal
        foreach (ReferenceScanner.Token t in ParseNonterminal(nextInd - 1)) {
          yield return t;
        }
        continue;
      }

      // It's a terminal
      int nextTerm = -nextInd - 1;
      Console.WriteLine($"Expecting: T_{nextTerm}");
      Console.WriteLine($"Seeing next: T_{scanner.Peek().Type}");
      if (nextTerm != scanner.Peek().Type) {
        throw new ParserException(scanner.Peek(), $"Unexpected token T_{scanner.Peek().Type} (\"{scanner.Peek().Image}\")");
      }

      Console.WriteLine($"Consuming: {scanner.Peek().Type} (\"{scanner.Peek().Image}\")");
      yield return scanner.Eat();
    }

    Console.WriteLine("Done!");
  }
}