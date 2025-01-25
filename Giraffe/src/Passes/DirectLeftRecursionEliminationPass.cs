namespace Giraffe.Passes;

/// <summary>
/// Remove direct left recursion from a grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 6.4
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class DirectLeftRecursionEliminationPass(Grammar grammar) : Pass(grammar) {
  public override void Run() {
    foreach (string nonterminal in Grammar.Nonterminals) {
      EliminateDirectLeftRecursionForNonterminal(nonterminal);
    }
  }

  private void EliminateDirectLeftRecursionForNonterminal(string nonterminal) {
    string tailName = $"{nonterminal}#tail";
    string tailsName = $"{nonterminal}#tails";
    string headName = $"{nonterminal}#head";

    HashSet<Rule> nonterminalRules = Grammar.GetAllRulesForNonterminal(nonterminal).ToHashSet();
    HashSet<Rule> directLeftRecursive = nonterminalRules.Where(IsDirectLeftRecursive).ToHashSet();
    HashSet<Rule> others = nonterminalRules.Except(directLeftRecursive).ToHashSet();

    if (directLeftRecursive.Count == 0) {
      return;
    }

    // If every rule is direct left recursive, its an infinite loop
    if (others.Count == 0) {
      throw new($"Grammar contains loop in rule for nonterminal \"{nonterminal}\"");
    }

    Grammar.Rules.UnionWith(others.Select(r => r with { Name = $"{r.Name}#head" }));

    Grammar.Rules.UnionWith(directLeftRecursive.Select(r => new Rule($"{r.Name}#tail", r.Symbols[1..])));

    Grammar.Rules.RemoveWhere(r => r.Name.Equals(nonterminal));

    Grammar.Rules.Add(new(tailsName, [tailName, tailsName]));
    Grammar.Rules.Add(new(tailsName, [tailName]));
    Grammar.Rules.Add(new(nonterminal, [headName, tailsName]));
    Grammar.Rules.Add(new(nonterminal, [headName]));
  }

  private static bool IsDirectLeftRecursive(Rule rule) =>
    !rule.IsEpsilon && rule.Name.Equals(rule.Symbols[0]);
}