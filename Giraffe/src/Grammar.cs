using System.Text.RegularExpressions;

namespace Giraffe;

public class Grammar(Dictionary<string, Regex> terminals,
                     List<Production> productions) {
  public const string Eof = "$$";
  public const string Epsilon = "{}";

  private Dictionary<string, Regex> terminals = terminals;
  private List<Production> productions = productions;

  private readonly Dictionary<string, HashSet<string>> first = [];
  private readonly Dictionary<string, HashSet<string>> follow = [];
  private readonly Dictionary<int, HashSet<string>> predict = [];

  public IEnumerable<string> Terminals => terminals.Keys;

  public Regex GetTerminalRule(string terminal) => terminals[terminal];

  public IEnumerable<string> NonTerminals => productions.Select(p => p.Name).Distinct();

  /// <summary>
  /// Compute the First, Follow, and Predict sets for the grammar. This also
  /// marks nonterminals that have epsilon productions.
  /// </summary>
  public void ComputeSets() {
    HashSet<string> nonterminals = productions.Select(p => p.Name).ToHashSet();

    foreach (string nonterminal in nonterminals) {
      first.Add(nonterminal, ComputeFirst(nonterminal));
    }
    foreach (string nonterminal in nonterminals) {
      follow.Add(nonterminal, ComputeFollow(nonterminal));
    }
    for (int i = 0; i < productions.Count; i++) {
      predict.Add(i, ComputePredict(productions[i]));
    }
  }

  public ParseTable BuildParseTable() {
    ParseTable table = [];
    for (int i = 0; i < productions.Count; i++) {
      foreach (string terminal in Predict(i)) {
        table.Insert(productions[i].Name, terminal, i);
      }
    }
    return table;
  }

  /// <summary>
  /// Check if a nonterminal has an epsilon production. Requires sets for this
  /// grammar to be computed.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to check.</param>
  /// <returns>
  ///   <c>true</c> if the nonterminal has an epsilon production,
  ///   <c>false</c> otherwise.
  /// </returns>
  public bool HasEpsilon(string nonterminal) =>
    ComputeFirst(nonterminal).Contains(Epsilon);

  /// <summary>
  /// Enumerate the FIRST set of a nonterminal. Requires sets for this grammar
  /// to be computed.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to enumerate the FIRST set for.</param>
  /// <returns>An IEnumerable of the terminals in the nonterminal's FIRST set.</returns>
  public IEnumerable<string> First(string nonterminal) =>
    first[nonterminal].Where(t => !t.Equals(Epsilon));

  /// <summary>
  /// Enumerate the FOLLOW set of a nonterminal. Requires sets for this grammar
  /// to be computed.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to enumerate the FOLLOW set for.</param>
  /// <returns>An IEnumerable of the terminals in the nonterminal's FOLLOW set.</returns>
  public IEnumerable<string> Follow(string nonterminal) =>
    follow[nonterminal].Where(t => !t.Equals(Epsilon));

  /// <summary>
  /// Enumerate the PREDICT set of a production. Requires sets for this grammar
  /// to be computed.
  /// </summary>
  /// <param name="index">
  ///   The index in the productions list of the production to enumerate the
  ///   PREDICT set for.
  /// </param>
  /// <returns>An IEnumerable of the terminals in the production's PREDICT set.</returns>
  public IEnumerable<string> Predict(int index) =>
    predict[index].Where(t => !t.Equals(Epsilon));

  private HashSet<string> ComputeFirst(string nonterminal) {
    // Re-use value if this has been computed in a previous step.
    if (this.first.TryGetValue(nonterminal, out HashSet<string>? existingFirst)) {
      return existingFirst;
    }

    HashSet<string> first = [];

    // Check every production of the nonterminal (except for epsilon).
    foreach (Production production in
             productions.Where(p => p.Name.Equals(nonterminal))) {
      first.UnionWith(ComputeFirst(production));
    }

    return first;
  }

  private HashSet<string> ComputeFirst(Production production) {
    // If this is an epsilon production, epsilon is added to FIRST
    if (production.Count == 0) {
      return [Epsilon];
    }

    HashSet<string> first = [];

    foreach (string term in production) {
      // If the first item is a terminal then it is in FIRST and we're done
      if (IsTerminal(term)) {
        first.Add(term);
        break;
      }

      // If the first item is a nonterminal, then its entire FIRST set is in FIRST
      // Additionally, if it has an epsilon production then we'll need to add the
      // next item in the sequence to FIRST as well.
      first.UnionWith(ComputeFirst(term));
      if (!HasEpsilon(term)) {
        break;
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
        for (int i = ntInd + 1; i < production.Count; i++) {
          // If we've reached the end of the production, then add the
          // productions' FOLLOW set to FOLLOW
          if (i == production.Count) {
            follow.UnionWith(ComputeFollow(production.Name));
            break;
          }

          string next = production[i];
          // If it is followed by a terminal, then add that terminal to FOLLOW
          // If it is followed by a nonterminal, then add that nonterminal's FIRST set to FOLLOW
          follow.UnionWith(IsTerminal(next) ? [next] : First(next));

          // If it is followed by a nonterminal with an epsilon production, keep looking
          if (!HasEpsilon(next)) {
            break;
          }
        }
      }
    }

    return follow;
  }

  private HashSet<string> ComputePredict(Production production) {
    HashSet<string> predict = ComputeFirst(production);
    if (predict.Contains(Epsilon)) {
      predict.UnionWith(Follow(production.Name));
    }
    return predict;
  }

  private static IEnumerable<int> IndexOfAll<T>(IEnumerable<T> sequence, T element) =>
    sequence.Select((e, i) => (e, i))
      .Where(t => t.e!.Equals(element))
      .Select(t => t.i);

  private static bool IsTerminal(string name) =>
    char.IsLower(name[0]) || name.Equals(Eof);
}
