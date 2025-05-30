using Giraffe.AST;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;
using TerminalDefinition = Giraffe.AST.TerminalDefinition;

namespace Giraffe;

public static class GrammarBuilder {
  /// <summary>
  /// Convert the given GrammarDefinition AST into a Grammar object.
  /// </summary>
  /// <param name="grammarGroup">The GrammarDefinition to convert.</param>
  /// <returns>A Grammar containing the Symbols and Rules represented in the GrammarDefinition.</returns>
  public static Grammar AstToGrammar(GrammarGroup grammarGroup) {
    Dictionary<Terminal, GIR.TerminalDefinition> terminalDefinitions = [];
    HashSet<Rule> rules = [];
    HashSet<Nonterminal> entryNonterminals = [];

    foreach (SymbolDefinition symbolDef in grammarGroup.SymbolDefinitions) {
      if (symbolDef is NonterminalDefinition nontermDef) {
        rules.UnionWith(nontermDef.Rules.Select(r => GetRuleDefinition(nontermDef.Name, r)).ToHashSet());
        if (nontermDef.Entry) {
          entryNonterminals.Add(Nt(nontermDef.Name));
        }
      }
      else if (symbolDef is TerminalDefinition termDef) {
        terminalDefinitions.Add(T(termDef.Name), GetGirTerminalDefinition(termDef.TerminalRhs, termDef.Ignore));
      }
    }

    return new(terminalDefinitions, rules, entryNonterminals, [], null, null);
  }

  private static Rule GetRuleDefinition(string nonterminalName, RuleDefinition ruleDefinition) =>
    R(Nt(nonterminalName), ruleDefinition.SymbolUsages.Select(u => u switch {
      TerminalUsage tU =>
        (Symbol)T(tU.Name) with { Transformation = new(Discard: tU.Discard) }, // Terminals cannot Expand
      NonterminalUsage ntU =>
        (Symbol)Nt(ntU.Name) with { Transformation = new(Discard: ntU.Discard, Expand: ntU.Expand) },
      _ => throw new ArgumentOutOfRangeException(),
    }));

  private static GIR.TerminalDefinition GetGirTerminalDefinition(TerminalRhs terminalRhs, bool ignore) =>
    new(terminalRhs.Regex, ignore);
}
