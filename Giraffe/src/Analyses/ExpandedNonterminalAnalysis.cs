using Giraffe.GIR;

namespace Giraffe.Analyses;

/// <summary>
/// Find all nonterminals which are always expanded when they appear on the right-hand side of a rule.
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class ExpandedNonterminalAnalysis(Grammar grammar) : Analysis<HashSet<Nonterminal>>(grammar) {
  public override HashSet<Nonterminal> Analyze() =>
    AllUsedNonterminals().Where(IsAlwaysExpanded)
           .ToHashSet();

  private HashSet<Nonterminal> AllUsedNonterminals() =>
    Grammar.Rules.SelectMany(rule => rule.Symbols).OfType<Nonterminal>().ToHashSet();

  private bool IsAlwaysExpanded(Nonterminal nt) =>
    Grammar.Rules.SelectMany(rule => rule.Symbols)
           .Where(s => nt.Value.Equals(s.Value))
           .All(s => s.Transformation.Expand);
}