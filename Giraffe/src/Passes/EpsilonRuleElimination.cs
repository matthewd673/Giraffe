namespace Giraffe.Passes;

/// <summary>
/// Pass to remove epsilon rules from a Grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 4.2.3.1
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class EpsilonRuleElimination(Grammar grammar) {
  public void Run() {
    Dictionary<string, string> ntDerivations = [];

    // Run until all nonterminals with epsilon productions are removed (new ones may appear along the way)
    while (true) {
      bool changed = false;

      HashSet<string> ntWithEps = GetNonterminalsWithEpsilonRules();
      foreach (string nt in ntWithEps) {
        if (ntDerivations.ContainsKey(nt)) {
          continue;
        }

        string derivation = GetDerivationOfNonterminal(nt);
        ntDerivations.Add(nt, derivation);
        grammar.Nonterminals.Add(derivation);

        changed |= ReplaceOccurrencesOfEpsilonNonterminal(nt, derivation);
      }

      if (!changed) {
        break;
      }
    }

    // Copy non-epsilon rules from original nonterminals to their derivations
    // If a derivation ends up with nothing except an epsilon rule, remove all occurrences of it from the grammar.
    foreach (string nt in ntDerivations.Keys) {
      CopyNonEpsilonRules(nt, ntDerivations[nt]);
      RemoveNonterminalIfNoRules(ntDerivations[nt]);
    }
  }

  private HashSet<string> GetNonterminalsWithEpsilonRules() =>
    grammar.Rules.Where(p => p.IsEpsilon).Select(p => p.Name).ToHashSet();

  private string GetDerivationOfNonterminal(string originalNt) => $"{originalNt}'";

  private bool ReplaceOccurrencesOfEpsilonNonterminal(string originalNt, string newNt) {
    bool changed = false;

    // Find all occurrences of the original NT in the grammar
    while (true) {
      HashSet<Rule> rulesWithNt = grammar.Rules.Where(r => r.Symbols.Contains(originalNt)).ToHashSet();
      if (rulesWithNt.Count == 0) {
        break;
      }

      changed = true;

      foreach (Rule r in rulesWithNt) {
        // Replace the original NT with the new version (which should have no epsilon rule)
        grammar.Rules.Remove(r); // If we just edit the rule in place we may end up with duplicates in the set
        int ntInd = r.Symbols.FindIndex(s => s.Equals(originalNt));
        r.Symbols[ntInd] = newNt;
        grammar.Rules.Add(r);

        // Create a copy of the production with NT removed (simulating the case where it goes to epsilon)
        Rule ruleWithoutNt = r with { Symbols = [..r.Symbols] }; // Make sure we clone the symbol list
        ruleWithoutNt.Symbols.RemoveAt(ntInd);
        grammar.Rules.Add(ruleWithoutNt);
      }
    }

    return changed;
  }

  private void RemoveNonterminalIfNoRules(string nonterminal) {
    if (!grammar.Rules.Any(r => r.Name.Equals(nonterminal))) {
      grammar.RemoveAllOccurrencesOfNonterminal(nonterminal);
    }
  }

  private void CopyNonEpsilonRules(string originalNt, string newNt) {
    List<Rule> originalRules = grammar.Rules.Where(r => r.Name.Equals(originalNt) && !r.IsEpsilon).ToList();

    foreach (Rule r in originalRules) {
      grammar.Rules.Add(r with { Name = newNt });
    }
  }
}