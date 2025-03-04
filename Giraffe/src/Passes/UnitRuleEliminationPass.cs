using Giraffe.GIR;

namespace Giraffe.Passes;

/// <summary>
/// Remove unit rules from a grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 4.2.3.2
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class UnitRuleEliminationPass(Grammar grammar) : Pass(grammar) {
  public override void Run() {
    HashSet<(Nonterminal, Nonterminal)> seenUnits = [];

    while (true) {
      bool sawUnit = false;

      List<Rule> currentUnitRules = Grammar.Rules.Where(rule => rule.Symbols is [Nonterminal _]).ToList();
      foreach (Rule rule in currentUnitRules) {
        sawUnit = true;

        // This should be impossible given the above check, but it casts the symbol to a Nonterminal elegantly.
        if (rule.Symbols[0] is not Nonterminal nt) {
          throw new Exception($"Rule is not a unit rule");
        }

        // If a rule has form `A -> A`, always remove it from the grammar.
        if (rule.Nonterminal.Equals(nt)) {
          Grammar.Rules.Remove(rule);
          continue;
        }

        // If a unit rule has been propagated before, remove it to avoid a loop.
        // This may indicate that the grammar is infinitely ambiguous.
        if (!seenUnits.Add((rule.Nonterminal, nt))) {
          Grammar.Rules.Remove(rule);
          continue;
        }

        // Copy rules associated with `nonterminal` to `rule.Name`
        List<Rule> copiedRules = Grammar.GetAllRulesForNonterminal(nt)
                                        .Select(ntRule => ntRule with { Nonterminal = rule.Nonterminal }).ToList();
        Grammar.Rules.UnionWith(copiedRules);
      }

      if (!sawUnit) {
        break;
      }
    }
  }
}