namespace Giraffe.Checks;

/// <summary>
/// Check if the Grammar has rules with undefined symbols on the right-hand side.
/// </summary>
/// <param name="grammar">The Grammar to check.</param>
public class UndefinedSymbolsCheck(Grammar grammar) : Check(grammar) {
  public override CheckResult Evaluate() {
    List<Symbol> undefined = Grammar.Rules
                                    .SelectMany(r => r.Symbols
                                                      .Where(s => (s.IsTerminal && !Grammar.Terminals.Contains(s.Value)) ||
                                                                  (!s.IsTerminal && !Grammar.Nonterminals.Contains(s.Value))))
                                    .ToList();

    return undefined.Count == 0
             ? new(true)
             : new(false, $"Grammar contains undefined symbols: [{string.Join(", ", undefined)}]");
  }
}