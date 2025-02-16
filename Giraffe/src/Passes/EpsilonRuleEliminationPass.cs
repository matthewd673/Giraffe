namespace Giraffe.Passes;

/// <summary>
/// Remove epsilon rules from a grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 4.2.3.1
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class EpsilonRuleEliminationPass(Grammar grammar) : Pass(grammar) {
  public override void Run() {
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
        Grammar.Nonterminals.Add(derivation);

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
    Grammar.Rules.Where(p => p.IsEpsilon).Select(p => p.Name).ToHashSet();

  private string GetDerivationOfNonterminal(string originalNt) => $"{originalNt}'";

  private bool ReplaceOccurrencesOfEpsilonNonterminal(string originalNt, string newNt) {
    bool changed = false;

    // Find all occurrences of the original NT in the grammar
    while (true) {
      HashSet<Rule> rulesWithNt = Grammar.Rules.Where(r => r.Symbols.Contains(originalNt)).ToHashSet();
      if (rulesWithNt.Count == 0) {
        break;
      }

      changed = true;

      foreach (Rule r in rulesWithNt) {
        // Replace the original NT with the new version (which should have no epsilon rule)
        Grammar.Rules.Remove(r);
        int ntInd = r.Symbols.FindIndex(s => s.Equals(originalNt));
        Grammar.Rules.Add(r with { Symbols = r.Symbols.SetItem(ntInd, newNt) });

        // Create a copy of the production with NT removed (simulating the case where it goes to epsilon)
        Rule ruleWithoutNt = r with { Symbols = r.Symbols.RemoveAt(ntInd) };
        Grammar.Rules.Add(ruleWithoutNt);
      }
    }

    return changed;
  }

  private void RemoveNonterminalIfNoRules(string nonterminal) {
    if (!Grammar.Rules.Any(r => r.Name.Equals(nonterminal))) {
      Grammar.RemoveAllOccurrencesOfNonterminal(nonterminal);
    }
  }

  private void CopyNonEpsilonRules(string originalNt, string newNt) {
    List<Rule> originalRules = Grammar.Rules.Where(r => r.Name.Equals(originalNt) && !r.IsEpsilon).ToList();

    foreach (Rule r in originalRules) {
      Grammar.Rules.Add(r with { Name = newNt });
    }
  }
}