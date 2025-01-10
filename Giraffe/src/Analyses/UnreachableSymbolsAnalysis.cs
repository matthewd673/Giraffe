namespace Giraffe.Analyses;

/// <summary>
/// Find the terminals and nonterminals in the Grammar that are unreachable (can never be produced).
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class UnreachableSymbolsAnalysis(Grammar grammar) : Analysis<IEnumerable<string>>(grammar) {
  public override IEnumerable<string> Analyze() {
    HashSet<string> all = [..Grammar.Terminals, ..Grammar.Nonterminals];
    HashSet<string> seen = [];
    HashSet<string> toSee = [..Grammar.EntryNonterminals];

    while (toSee.Count > 0) {
      string current = toSee.First();
      toSee.Remove(current);

      if (!Grammar.IsTerminal(current)) {
        IEnumerable<string> rightHandSymbols = Grammar.Rules.Where(r => r.Name.Equals(current))
                                                            .SelectMany(r => r.Symbols);

        // Mark any terminal on the right hand sides as seen
        seen.UnionWith(rightHandSymbols.Where(Grammar.IsTerminal));

        // Recurse on any nonterminals in the right hand sides
        toSee.UnionWith(rightHandSymbols.Where(s => !Grammar.IsTerminal(s))
                                        .Where(s => !seen.Contains(s)));
      }

      seen.Add(current);
    }

    return all.Except(seen);
  }
}