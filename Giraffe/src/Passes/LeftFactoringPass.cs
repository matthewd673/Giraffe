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
      changed = Grammar.Nonterminals.Aggregate(false,
                                               (acc, nt) => acc | LeftFactorNonterminalRules(nt));
    }
  }

  private bool LeftFactorNonterminalRules(string nonterminal) {
    bool changed = false;
    Dictionary<string, List<Rule>> firstSymbolMap = [];

    foreach (Rule rule in Grammar.GetAllRulesForNonterminal(nonterminal).Where(r => !r.IsEpsilon)) {
      string firstSymbol = rule.Symbols[0];
      if (firstSymbolMap.TryGetValue(firstSymbol, out List<Rule>? symbolRules)) {
        symbolRules.Add(rule);
      }
      else {
        firstSymbolMap.Add(firstSymbol, [rule]);
      }
    }

    foreach (string firstSymbol in firstSymbolMap.Keys) {
      List<Rule> rules = firstSymbolMap[firstSymbol];
      if (rules.Count < 2) {
        continue;
      }

      changed = true;

      string tailName = $"{nonterminal}-{firstSymbol}#tail";
      List<Rule> tails = rules.Select(r => new Rule(tailName, r.Symbols.RemoveAt(0))).ToList();
      Grammar.Rules.RemoveWhere(r => rules.Contains(r));

      Rule newHead = new(nonterminal, [firstSymbol, tailName]);

      Grammar.Nonterminals.Add(tailName);
      Grammar.Rules.UnionWith(tails);
      Grammar.Rules.Add(newHead);
    }

    return changed;
  }
}