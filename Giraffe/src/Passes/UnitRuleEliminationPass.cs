namespace Giraffe.Passes;

/// <summary>
/// Remove unit rules from a grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 4.2.3.2
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class UnitRuleEliminationPass(Grammar grammar) : Pass(grammar) {
  public override void Run() {
    HashSet<(string, string)> seenUnits = [];

    while (true) {
      bool sawUnit = false;

      List<Rule> currentUnitRules = Grammar.Rules.Where(rule => rule.Symbols.Count == 1 &&
                                                                !Grammar.IsTerminal(rule.Symbols[0])).ToList();
      foreach (Rule rule in currentUnitRules) {
        sawUnit = true;

        string nonterminal = rule.Symbols[0];
        // If a rule has form `A -> A`, always remove it from the grammar.
        if (rule.Name.Equals(nonterminal)) {
          Grammar.Rules.Remove(rule);
          continue;
        }

        // If a unit rule has been propagated before, remove it to avoid a loop.
        // This may indicate that the grammar is infinitely ambiguous.
        if (!seenUnits.Add((rule.Name, nonterminal))) {
          Grammar.Rules.Remove(rule);
          continue;
        }

        // Copy rules associated with `nonterminal` to `rule.Name`
        List<Rule> copiedRules = Grammar.GetAllRulesForNonterminal(nonterminal)
                                        .Select(ntRule => ntRule with { Name = rule.Name }).ToList();
        Grammar.Rules.UnionWith(copiedRules);
      }

      if (!sawUnit) {
        break;
      }
    }
  }
}