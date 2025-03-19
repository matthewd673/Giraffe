using Giraffe.GIR;

namespace Giraffe.Analyses;

/// <summary>
/// Identify rules in a grammar that are direct left-recursive.
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class DirectLeftRecursionAnalysis(Grammar grammar) : Analysis<HashSet<Rule>>(grammar) {
  public override HashSet<Rule> Analyze() =>
    Grammar.Rules.Where(r => r.Nonterminal.Equals(r.Symbols.FirstOrDefault())).ToHashSet();
}