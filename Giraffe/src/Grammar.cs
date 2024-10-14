using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Giraffe;

public class Grammar(Dictionary<string, Regex> terminals,
                     List<Production> productions) {
  private Dictionary<string, Regex> terminals = terminals;
  private List<Production> productions = productions;

  private readonly Dictionary<string, bool> epsilon = [];
  private readonly Dictionary<string, HashSet<string>> first = [];
  private readonly Dictionary<string, HashSet<string>> follow = [];

  /// <summary>
  /// Compute the First, Follow, and Predict sets for the grammar.
  /// </summary>
  public void ComputeSets() {
    foreach (string nonterminal in productions.Select(p => p.Name).ToHashSet()) {
      first.Add(nonterminal, ComputeFirst(nonterminal));
      follow.Add(nonterminal, ComputeFollow(nonterminal));
    }

    // TODO: Compute Predict set
  }

  public bool HasEpsilon(string nonterminal) =>
    epsilon.GetValueOrDefault(nonterminal, false);

  public HashSet<string> First(string nonterminal) => first[nonterminal];

  public HashSet<string> Follow(string nonterminal) => follow[nonterminal];

  private HashSet<string> ComputeFirst(string nonterminal) {
    // Re-use value if this has been computed in a previous step.
    if (this.first.TryGetValue(nonterminal, out HashSet<string>? existingFirst)) {
      return existingFirst;
    }

    HashSet<string> first = [];
    // Check every production of the nonterminal (except for epsilon).
    foreach (List<string> production in
             productions.Where(p => p.Name.Equals(nonterminal))) {
      // Note if this is an epsilon production and then skip it
      if (production.Count == 0) {
        epsilon[nonterminal] = true;
        continue;
      }

      // If the first item is a terminal then it is in FIRST
      if (IsTerminal(production[0])) {
        first.Add(production[0]);
      }
      // If the first item is a nonterminal, then its entire FIRST set is in FIRST
      else {
        first.UnionWith(ComputeFirst(production[0]));
      }
    }

    return first;
  }

  private HashSet<string> ComputeFollow(string nonterminal) {
    // Re-use value if this has been computed in a previous step.
    if (this.follow.TryGetValue(nonterminal, out HashSet<string>? existingFollow)) {
      return existingFollow;
    }

    HashSet<string> follow = [];

    foreach (Production production in productions) {
      // For every occurrence of the given nonterminal in the production...
      foreach (int ntInd in IndexOfAll(production, nonterminal)) {
        if (ntInd < production.Count - 1) {
          // If it is followed by a terminal, then add that terminal to FOLLOW
          // If it is followed by a nonterminal, then add that nonterminal's FIRST set to FOLLOW
          string next = production[ntInd + 1];
          follow.UnionWith(IsTerminal(next) ? [next] : ComputeFirst(next));
        }
        // If it occurs at the end of the production, then add the production's nonterminal's FOLLOW set to FOLLOW
        else {
          follow.UnionWith(ComputeFollow(production.Name));
        }
      }
    }

    return follow;
  }

  private HashSet<string> ComputePredict(Production production) {
    if (production.Count == 0) {
      return follow[production.Name];
    }
    else if (IsTerminal(production[0])) {
      return [production[0]];
    }
    else {
      return first[production[0]];
    }
  }

  private static IEnumerable<int> IndexOfAll<T>(IEnumerable<T> sequence, T element) =>
    sequence.Select((e, i) => (e, i))
      .Where((e, i) => e.Equals(element))
      .Select((e, i) => i);

  private static bool IsTerminal(string name) => char.IsLower(name[0]);
}
