using Giraffe.GIR;

namespace Giraffe.Analyses;

/// <summary>
/// Find all non-productive rules in the grammar. A rule is non-productive if it does not produce a
/// non-empty sublanguage. (Parsing Techniques 2.9.3)
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class NonProductiveRuleAnalysis(Grammar grammar) : Analysis<HashSet<Rule>>(grammar) {
  public override HashSet<Rule> Analyze() => Grammar.Rules.Where(r => !IsProductive(r)).ToHashSet();

  private bool IsProductive(Rule rule) => IsProductive(rule, []);

  private bool IsProductive(Rule rule, HashSet<Rule> seen) {
    // If we've entered a loop, then the rule is non-productive
    if (seen.Contains(rule)) {
      return false;
    }

    // A rule is always productive if it is epsilon or if it contains only terminals
    if (!rule.Symbols.Exists(s => s is Nonterminal)) {
      return true;
    }

    seen.Add(rule);
    bool hasNonProductive = false;

    foreach (Symbol symbol in rule.Symbols) {
      if (symbol is not Nonterminal nt) {
        continue;
      }

      if (Grammar.GetAllRulesForNonterminal(nt).All(r => IsProductive(r, seen))) {
        continue;
      }

      hasNonProductive = true;
      break;
    }

    return !hasNonProductive;
  }
}