using Giraffe.GIR;

namespace Giraffe.Analyses;

public class UndefinedSymbolsAnalysis(Grammar grammar) : Analysis<HashSet<Symbol>>(grammar) {
  public override HashSet<Symbol> Analyze() =>
    Grammar.Rules
           .SelectMany(r => r.Symbols
                             .Where(s => (s is Terminal && !Grammar.Terminals.Contains(s)) ||
                                         (s is Nonterminal && !Grammar.Nonterminals.Contains(s))))
           .ToHashSet();
}