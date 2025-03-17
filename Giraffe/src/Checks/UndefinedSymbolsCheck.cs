using Giraffe.GIR;

namespace Giraffe.Checks;

/// <summary>
/// Check if the Grammar has rules with undefined symbols on the right-hand side.
/// </summary>
/// <param name="grammar">The Grammar to check.</param>
public class UndefinedSymbolsCheck(Grammar grammar) : Check(grammar) {
  public override CheckResult Evaluate() {
    List<Symbol> undefined = Grammar.Rules
                                    .SelectMany(r => r.Symbols
                                                      .Where(s => (s is Terminal && !Grammar.Terminals.Contains(s)) ||
                                                                  (s is Nonterminal && !Grammar.Nonterminals.Contains(s))))
                                    .ToList();

    return undefined.Count == 0
             ? new(true)
             : new(false, $"Grammar contains undefined symbol(s): {string.Join(", ",
                                                                               undefined.Select(s => $"\"{s.Value}\""))}");
  }
}