namespace Giraffe.Passes;

/// <summary>
/// Remove direct left recursion from a grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 6.4
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class DirectLeftRecursionEliminationPass(Grammar grammar) : Pass(grammar) {
  public override void Run() {
    foreach (Nonterminal nt in Grammar.Nonterminals) {
      EliminateDirectLeftRecursionForNonterminal(nt);
    }
  }

  private void EliminateDirectLeftRecursionForNonterminal(Nonterminal nt) {
    string tailName = $"{nt}#tail";
    string tailsName = $"{nt}#tails";
    string headName = $"{nt}#head";

    HashSet<Rule> nonterminalRules = Grammar.GetAllRulesForNonterminal(nt).ToHashSet();
    HashSet<Rule> directLeftRecursive = nonterminalRules.Where(IsDirectLeftRecursive).ToHashSet();
    HashSet<Rule> others = nonterminalRules.Except(directLeftRecursive).ToHashSet();

    if (directLeftRecursive.Count == 0) {
      return;
    }

    // If every rule is direct left recursive, it's an infinite loop
    if (others.Count == 0) {
      throw new($"Grammar contains loop in rule for nonterminal \"{nt}\"");
    }

    Grammar.Rules.UnionWith(others.Select(r => r with { Nonterminal = new($"{r.Nonterminal.Value}#head") }));

    Grammar.Rules.UnionWith(directLeftRecursive
                              .Select(r => new Rule(new($"{r.Nonterminal.Value}#tail"), r.Symbols.RemoveAt(0))));

    Grammar.Rules.RemoveWhere(r => r.Nonterminal.Equals(nt));

    Grammar.Rules.Add(new(new(tailsName), [tailName, tailsName]));
    Grammar.Rules.Add(new(new(tailsName), [tailName]));
    Grammar.Rules.Add(new(nt, [headName, tailsName]));
    Grammar.Rules.Add(new(nt, [headName]));
  }

  private static bool IsDirectLeftRecursive(Rule rule) =>
    !rule.IsEpsilon && rule.Nonterminal.Equals(rule.Symbols[0]);
}