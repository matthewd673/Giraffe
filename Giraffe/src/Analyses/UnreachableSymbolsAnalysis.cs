namespace Giraffe.Analyses;

/// <summary>
/// Find the terminals and nonterminals in the Grammar that are unreachable (can never be produced).
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class UnreachableSymbolsAnalysis(Grammar grammar) : Analysis<IEnumerable<Symbol>>(grammar) {
  public override IEnumerable<Symbol> Analyze() {
    HashSet<Symbol> all = [..Grammar.Terminals, ..Grammar.Nonterminals];
    HashSet<Symbol> seen = [];
    HashSet<Symbol> toSee = [..Grammar.EntryNonterminals];

    while (toSee.Count > 0) {
      Symbol current = toSee.First();
      toSee.Remove(current);

      if (current is Nonterminal) {
        HashSet<Symbol> rightHandSymbols = Grammar.Rules.Where(r => r.Nonterminal.Equals(current))
                                                  .SelectMany(r => r.Symbols).ToHashSet();

        // Mark any terminal on the right hand sides as seen
        seen.UnionWith(rightHandSymbols.Where(s => s is Terminal));

        // Recurse on any nonterminals in the right hand sides
        toSee.UnionWith(rightHandSymbols.Where(s => s is Nonterminal)
                                        .Where(s => !seen.Contains(s)));
      }

      seen.Add(current);
    }

    return all.Except(seen);
  }
}