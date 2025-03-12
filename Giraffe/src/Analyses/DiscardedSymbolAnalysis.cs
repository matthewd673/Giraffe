using Giraffe.GIR;

namespace Giraffe.Analyses;

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