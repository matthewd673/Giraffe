namespace Giraffe.Passes;

/// <summary>
/// Remove epsilon rules from a grammar.
/// Adapted from "Parsing Techniques: A Practical Guide" Section 4.2.3.1
/// </summary>
/// <param name="grammar">The Grammar to run the pass on. It will be modified in place.</param>
public class EpsilonRuleEliminationPass(Grammar grammar) : Pass(grammar) {
  public override void Run() {
    Dictionary<Nonterminal, Nonterminal> ntDerivations = [];

    // Run until all nonterminals with epsilon productions are removed (new ones may appear along the way)
    while (true) {
      bool changed = false;

      HashSet<Nonterminal> ntWithEps = GetNonterminalsWithEpsilonRules();
      foreach (Nonterminal nt in ntWithEps) {
        if (ntDerivations.ContainsKey(nt)) {
          continue;
        }

        Nonterminal derivation = GetDerivationOfNonterminal(nt);
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
    foreach (Nonterminal nt in ntDerivations.Keys) {
      CopyNonEpsilonRules(nt, ntDerivations[nt]);
      RemoveNonterminalIfNoRules(ntDerivations[nt]);
    }
  }

  private HashSet<Nonterminal> GetNonterminalsWithEpsilonRules() =>
    Grammar.Rules.Where(p => p.IsEpsilon).Select(p => p.Nonterminal).ToHashSet();

  private Nonterminal GetDerivationOfNonterminal(Nonterminal originalNt) => new($"{originalNt.Value}'");

  private bool ReplaceOccurrencesOfEpsilonNonterminal(Nonterminal originalNt, Nonterminal newNt) {
    bool changed = false;

    // Find all occurrences of the original NT in the grammar
    while (true) {
      HashSet<Rule> rulesWithNt = Grammar.Rules.Where(r => r.Symbols.Exists(s => s.Equals(originalNt)))
                                         .ToHashSet();
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

  private void RemoveNonterminalIfNoRules(Nonterminal nt) {
    if (!Grammar.Rules.Any(r => r.Nonterminal.Equals(nt))) {
      Grammar.RemoveAllOccurrencesOfNonterminal(nt);
    }
  }

  private void CopyNonEpsilonRules(Nonterminal originalNt, Nonterminal newNt) {
    List<Rule> originalRules = Grammar.Rules.Where(r => r.Nonterminal.Equals(originalNt) && !r.IsEpsilon).ToList();

    foreach (Rule r in originalRules) {
      Grammar.Rules.Add(r with { Nonterminal = newNt });
    }
  }
}