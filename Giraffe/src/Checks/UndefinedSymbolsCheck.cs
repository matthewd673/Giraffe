namespace Giraffe.Checks;

/// <summary>
/// Check if the Grammar has rules with undefined symbols on the right-hand side.
/// </summary>
/// <param name="grammar">The Grammar to check.</param>
public class UndefinedSymbolsCheck(Grammar grammar) : Check(grammar) {
  public override CheckResult Evaluate() {
    List<string> undefined = Grammar.Rules
                                    .SelectMany(r => r.Symbols
                                                      .Where(s => (Grammar.IsTerminal(s) && !Grammar.Terminals.Contains(s)) ||
                                                                  (!Grammar.IsTerminal(s) && !Grammar.Nonterminals.Contains(s))))
                                    .ToList();

    return undefined.Count == 0
             ? new(true)
             : new(false, $"Grammar contains undefined symbols: [{string.Join(", ", undefined)}]");
  }
}