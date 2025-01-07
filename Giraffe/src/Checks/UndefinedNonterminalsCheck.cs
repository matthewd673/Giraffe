namespace Giraffe.Checks;

/// <summary>
/// Check if any Grammar rules contain an undefined nonterminal.
/// </summary>
/// <param name="grammar">The Grammar to check.</param>
public class UndefinedNonterminalsCheck(Grammar grammar) : Check(grammar) {
  public override CheckResult Evaluate() {
    List<string> undefined = GetUndefinedNonterminals().ToList();

    return undefined.Count == 0
             ? new(true)
             : new(false, $"Grammar contains undefined nonterminals: [{string.Join(", ", undefined)}]");
  }

  private IEnumerable<string> GetUndefinedNonterminals() =>
    Grammar.Rules.SelectMany(r => r.Symbols.Where(s => !Grammar.IsTerminal(s) && !Grammar.Nonterminals.Contains(s)));
}