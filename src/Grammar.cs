using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Giraffe;

public class Grammar(Dictionary<string, Regex> terminals,
                     List<Production> productions) {
  private Dictionary<string, Regex> terminals = terminals;
  private List<Production> productions = productions;

  public ParseTable BuildParseTable() {
    ParseTable table = new();
    Dictionary<string, HashSet<string>> First = [];
    Dictionary<string, HashSet<string>> Follow = [];

    foreach (string nonterminal in productions.Select(p => p.Name).ToHashSet()) {
      First.Add(nonterminal, ComputeFirst(nonterminal));
      Follow.Add(nonterminal, ComputeFollow(nonterminal));
    }

    return table;
  }

  private HashSet<string> ComputeFirst(string nonterminal) {
    HashSet<string> first = [];
    // Check every production of the nonterminal (except for epsilon).
    foreach (List<string> production in
             productions.Where(p => p.Name.Equals(nonterminal) && p.Count > 0)) {
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

  private HashSet<string> ComputePredict(Production production,
                                         Dictionary<string, HashSet<string>> firstSets,
                                         Dictionary<string, HashSet<string>> followSets) {
    HashSet<string> predict = [];

    if (production.Count == 0) {
      predict = followSets(production.Name);
    }
    else if (IsTerminal(production[0])) {
      predict = [production[0]];
    }
    else {
      predict = firstSets(production[0]);
    }

    return predict;
  }

  private static IEnumerable<int> IndexOfAll<T>(IEnumerable<T> sequence, T element) =>
    sequence.Select((e, i) => (e, i))
      .Where((e, i) => e.Equals(element))
      .Select((e, i) => i);

  private static bool IsTerminal(string name) => char.IsLower(name[0]);
}
