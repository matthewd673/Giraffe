using Giraffe.GIR;

namespace Giraffe.Analyses;

/// <summary>
/// Determine if the given grammar is LL(1).
/// </summary>
/// <param name="grammar">The Grammar to check.</param>
public class IsLl1Analysis(Grammar grammar) : Analysis<bool>(grammar) {
  public override bool Analyze() {
    GrammarSets sets = new SetsAnalysis(Grammar).Analyze();

    foreach (Nonterminal nt in Grammar.Nonterminals) {
      HashSet<Terminal> seenFirst = [];
      foreach (Rule rule in Grammar.Rules.Where(r => r.Nonterminal.Equals(nt))) {
        if (seenFirst.Intersect(sets.Predict[rule]).Any()) {
          return false;
        }

        seenFirst.UnionWith(sets.Predict[rule]);
      }
    }

    return true;
  }
}