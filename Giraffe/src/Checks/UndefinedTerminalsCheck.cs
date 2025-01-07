namespace Giraffe.Checks;

/// <summary>
/// Check if any Grammar rules contain an undefined terminal.
/// </summary>
/// <param name="grammar">The Grammar to check.</param>
public class UndefinedTerminalsCheck(Grammar grammar) : Check(grammar) {
  public override CheckResult Evaluate() {
    List<string> undefined = GetUndefinedTerminals().ToList();

    return undefined.Count == 0
             ? new(true)
             : new(false, $"Grammar contains undefined terminals: [{string.Join(", ", undefined)}]");
  }

  private IEnumerable<string> GetUndefinedTerminals() =>
    Grammar.Rules.SelectMany(r => r.Symbols.Where(s => Grammar.IsTerminal(s) && !Grammar.Terminals.Contains(s)));
}