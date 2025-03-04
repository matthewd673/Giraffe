using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Passes;

/// <summary>
/// Perform left-factoring on the rules in the grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 8.2.5.2
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class LeftFactoringPass(Grammar grammar) : Pass(grammar) {
  public override void Run() {
    bool changed = true;
    while (changed) {
      changed = Grammar.Nonterminals.Aggregate(false, (acc, nt) => acc | LeftFactorNonterminalRules(nt));
    }
  }

  private bool LeftFactorNonterminalRules(Nonterminal nt) {
    bool changed = false;
    Dictionary<Symbol, List<Rule>> firstSymbolMap = [];

    foreach (Rule rule in Grammar.GetAllRulesForNonterminal(nt).Where(r => !r.IsEpsilon)) {
      Symbol firstSymbol = rule.Symbols[0];
      if (firstSymbolMap.TryGetValue(firstSymbol, out List<Rule>? symbolRules)) {
        symbolRules.Add(rule);
      }
      else {
        firstSymbolMap.Add(firstSymbol, [rule]);
      }
    }

    foreach (Symbol firstSymbol in firstSymbolMap.Keys) {
      List<Rule> rules = firstSymbolMap[firstSymbol];
      if (rules.Count < 2) {
        continue;
      }

      changed = true;

      Nonterminal tailNt = Nt($"{nt.Value}-{firstSymbol.Value}#tail");
      List<Rule> tails = rules.Select(r => new Rule(tailNt, r.Symbols.RemoveAt(0))).ToList();
      Grammar.Rules.RemoveWhere(r => rules.Contains(r));

      Rule newHead = new(nt, [firstSymbol, tailNt]);

      Grammar.Nonterminals.Add(tailNt);
      Grammar.Rules.UnionWith(tails);
      Grammar.Rules.Add(newHead);
    }

    return changed;
  }
}