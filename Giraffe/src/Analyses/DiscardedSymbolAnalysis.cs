using Giraffe.GIR;

namespace Giraffe.Analyses;

/// <summary>
/// Find all symbols which are always discarded when they appear on the right-hand side of a rule.
/// NOTE: Symbols in this set may still be important, e.g. nonterminals in the entry set.
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class DiscardedSymbolAnalysis(Grammar grammar) : Analysis<HashSet<Symbol>>(grammar) {
  public override HashSet<Symbol> Analyze() =>
    Grammar.Terminals.Where(IsAlwaysDiscarded)
           .Union<Symbol>(Grammar.Nonterminals.Where(IsAlwaysDiscarded))
           .Where(IsUsed)
           .ToHashSet();

  private bool IsUsed(Symbol symbol) =>
    Grammar.Rules.SelectMany(rule => rule.Symbols)
           .Any(s => symbol.Value.Equals(s.Value));

  private bool IsAlwaysDiscarded(Symbol symbol) =>
    Grammar.Rules.SelectMany(rule => rule.Symbols)
           .Where(s => symbol.Value.Equals(s.Value))
           .All(s => s.Transformation.Discard);
}