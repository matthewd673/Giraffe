using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Giraffe;

public class Grammar(Dictionary<string, Regex> terminalDefs,
                     List<Production> productions) {
  public const string Eof = "$$";

  public ImmutableList<Production> Productions { get; } =
    ImmutableList.CreateRange(productions);

  private readonly HashSet<string> hasEpsilon = [];
  private readonly Dictionary<string, HashSet<string>> first = [];
  private readonly Dictionary<string, HashSet<string>> follow = [];
  private readonly Dictionary<int, HashSet<string>> predict = [];

  public ImmutableList<string> Terminals { get; } =
    // If we don't manually add Eof as a terminal, consumers that read the table
    // will be confused when they encounter it.
    ImmutableList.CreateRange(terminalDefs.Keys).Add(Eof);
  public ImmutableList<string> Nonterminals { get; } =
    ImmutableList.CreateRange(productions.Select(p => p.Name).Distinct().ToList());

  public Regex GetTerminalRule(string terminal) => terminalDefs[terminal];

  /// <summary>
  /// Compute the First, Follow, and Predict sets for the grammar. This also
  /// marks nonterminals that have epsilon productions.
  /// </summary>
  public void ComputeSets() {
    foreach (Production epsilonProduction in productions.Where(p => p.Count == 0)) {
      hasEpsilon.Add(epsilonProduction.Name);
    }

    foreach (string nonterminal in Nonterminals) {
      first.Add(nonterminal, ComputeFirst(nonterminal));
    }
    foreach (string nonterminal in Nonterminals) {
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
  /// Check if a nonterminal has an epsilon production. Requires that sets for this
  /// grammar have been computed.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to check.</param>
  /// <returns>
  ///   <c>true</c> if the nonterminal has an epsilon production,
  ///   <c>false</c> otherwise.
  /// </returns>
  public bool HasEpsilon(string nonterminal) => hasEpsilon.Contains(nonterminal);

  /// <summary>
  /// Enumerate the FIRST set of a nonterminal. Requires that sets for this grammar
  /// have been computed.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to enumerate the FIRST set for.</param>
  /// <returns>An IEnumerable of the terminals in the nonterminal's FIRST set.</returns>
  public IEnumerable<string> First(string nonterminal) => first[nonterminal];

  /// <summary>
  /// Enumerate the FOLLOW set of a nonterminal. Requires that sets for this grammar
  /// have been computed.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to enumerate the FOLLOW set for.</param>
  /// <returns>An IEnumerable of the terminals in the nonterminal's FOLLOW set.</returns>
  public IEnumerable<string> Follow(string nonterminal) => follow[nonterminal];

  /// <summary>
  /// Enumerate the PREDICT set of a production. Requires that sets for this grammar
  /// have been computed.
  /// </summary>
  /// <param name="index">
  ///   The index in the productions list of the production to enumerate the
  ///   PREDICT set for.
  /// </param>
  /// <returns>An IEnumerable of the terminals in the production's PREDICT set.</returns>
  public IEnumerable<string> Predict(int index) => predict[index];

  private HashSet<string> ComputeFirst(string nonterminal) {
    // Re-use value if this has been computed in a previous step.
    if (first.TryGetValue(nonterminal, out HashSet<string>? existingFirst)) {
      return existingFirst;
    }

    HashSet<string> firstSet = [];

    // Check every production of the nonterminal (except for epsilon).
    foreach (Production production in
             productions.Where(p => p.Name.Equals(nonterminal))) {
      firstSet.UnionWith(ComputeProductionFirst(production));
    }

    return firstSet;
  }

  private HashSet<string> ComputeProductionFirst(Production production) {
    HashSet<string> firstSet = [];

    foreach (string term in production) {
      // If the first item is a terminal then it is in FIRST and we're done
      if (IsTerminal(term)) {
        firstSet.Add(term);
        break;
      }

      // If the first item is a nonterminal, then its entire FIRST set is in FIRST
      // Additionally, if it has an epsilon production then we'll need to add the
      // next item in the sequence to FIRST as well.
      firstSet.UnionWith(ComputeFirst(term));
      if (!HasEpsilon(term)) {
        break;
      }
    }

    return firstSet;
  }

  private HashSet<string> ComputeFollow(string nonterminal) {
    // Re-use value if this has been computed in a previous step.
    if (follow.TryGetValue(nonterminal, out HashSet<string>? existingFollow)) {
      return existingFollow;
    }

    HashSet<string> followSet = [];

    foreach (Production production in productions) {
      // For every occurrence of the given nonterminal in the production...
      foreach (int ntInd in IndexOfAll(production, nonterminal)) {
        for (int i = ntInd + 1; i < production.Count; i++) {
          // If we've reached the end of the production, then add the
          // productions' FOLLOW set to FOLLOW
          if (i == production.Count) {
            followSet.UnionWith(ComputeFollow(production.Name));
            break;
          }

          string next = production[i];
          // If it is followed by a terminal, then add that terminal to FOLLOW
          // If it is followed by a nonterminal, then add that nonterminal's FIRST set to FOLLOW
          followSet.UnionWith(IsTerminal(next) ? [next] : First(next));

          // If it is followed by a nonterminal with an epsilon production, keep looking
          if (!HasEpsilon(next)) {
            break;
          }
        }
      }
    }

    return followSet;
  }

  private HashSet<string> ComputePredict(Production production) {
    HashSet<string> predictSet = [];

    HashSet<string> productionFirst = ComputeProductionFirst(production);
    predictSet.UnionWith(productionFirst);

    // If this production is an epsilon production, or if every item in the production can be epsilon,
    // then we should also add the FOLLOW set to the PREDICT set.
    int nonEpsCt = production.Count(e => IsTerminal(e) || !HasEpsilon(e));

    if (nonEpsCt == 0) {
      predictSet.UnionWith(Follow(production.Name));
    }

    return predictSet;
  }

  private static IEnumerable<int> IndexOfAll<T>(IEnumerable<T> sequence, T element) =>
    sequence.Select((e, i) => (e, i))
      .Where(t => t.e!.Equals(element))
      .Select(t => t.i);

  public static bool IsTerminal(string name) =>
    char.IsLower(name[0]) || name.Equals(Eof);
}
