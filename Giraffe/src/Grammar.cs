using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Giraffe;

public class Grammar(Dictionary<string, Regex> terminalDefs,
                     HashSet<Rule> rules,
                     HashSet<string> entryNonterminals) {
  public const string Eof = "$$";

  public HashSet<Rule> Rules { get; } = rules; // Copy just in case

  private readonly HashSet<string> hasEpsilon = [];
  private readonly Dictionary<string, HashSet<string>> first = [];
  private readonly Dictionary<string, HashSet<string>> follow = [];
  private readonly Dictionary<Rule, HashSet<string>> predict = [];

  public HashSet<string> Terminals { get; } = [..terminalDefs.Keys, Eof];
  public HashSet<string> Nonterminals { get; } = rules.Select(p => p.Name).ToHashSet();

  public Regex GetTerminalRule(string terminal) => terminalDefs[terminal];

  public ImmutableHashSet<string> EntryNonterminals = ImmutableHashSet.CreateRange(entryNonterminals);

  /// <summary>
  /// Compute the First, Follow, and Predict sets for the grammar. This also
  /// marks nonterminals that have epsilon productions.
  /// </summary>
  public void ComputeSets() {
    foreach (Rule epsilonRule in Rules.Where(r => r.IsEpsilon)) {
      hasEpsilon.Add(epsilonRule.Name);
    }

    foreach (string nonterminal in Nonterminals) {
      first.Add(nonterminal, ComputeFirst(nonterminal));
    }
    foreach (string nonterminal in Nonterminals) {
      follow.Add(nonterminal, ComputeFollow(nonterminal));
    }
    foreach (Rule rule in Rules) {
      predict.Add(rule, ComputePredict(rule));
    }
  }

  public ParseTable BuildParseTable() {
    ParseTable table = [];
    foreach (Rule rule in Rules) {
      foreach (string terminal in Predict(rule)) {
        table.Insert(rule.Name, terminal, rule);
      }
    }
    return table;
  }

  /// <summary>
  /// Check if a nonterminal has an epsilon rule. Requires that sets for this
  /// grammar have been computed.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to check.</param>
  /// <returns>
  ///   <c>true</c> if the nonterminal has an epsilon rule, <c>false</c> otherwise.
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
  public IEnumerable<string> Predict(Rule rule) => predict[rule];

  public IEnumerable<Rule> GetAllRulesForNonterminal(string nonterminal) =>
    Rules.Where(rule => rule.Name.Equals(nonterminal));

  /// <summary>
  /// Remove a nonterminal from the grammar. Also remove all rules associated with the nonterminal and all rules
  /// where the nonterminal appears on the right-hand side.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to remove.</param>
  public void RemoveAllOccurrencesOfNonterminal(string nonterminal) {
    Nonterminals.Remove(nonterminal);
    Rules.RemoveWhere(r => r.Name.Equals(nonterminal));
    Rules.RemoveWhere(r => r.Symbols.Contains(nonterminal));
  }

  private HashSet<string> ComputeFirst(string nonterminal) {
    // Re-use value if this has been computed in a previous step.
    if (first.TryGetValue(nonterminal, out HashSet<string>? existingFirst)) {
      return existingFirst;
    }

    HashSet<string> firstSet = [];

    // Check every production of the nonterminal (except for epsilon).
    foreach (Rule rule in
             Rules.Where(p => p.Name.Equals(nonterminal))) {
      firstSet.UnionWith(ComputeProductionFirst(rule));
    }

    return firstSet;
  }

  private HashSet<string> ComputeProductionFirst(Rule rule) {
    HashSet<string> firstSet = [];

    foreach (string symbol in rule.Symbols) {
      // If the first item is a terminal then it is in FIRST and we're done
      if (IsTerminal(symbol)) {
        firstSet.Add(symbol);
        break;
      }

      // If the first item is a nonterminal, then its entire FIRST set is in FIRST
      // Additionally, if it has an epsilon production then we'll need to add the
      // next item in the sequence to FIRST as well.
      firstSet.UnionWith(ComputeFirst(symbol));
      if (!HasEpsilon(symbol)) {
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

    foreach (Rule rule in Rules) {
      // For every occurrence of the given nonterminal in the production...
      foreach (int ntInd in IndexOfAll(rule.Symbols, nonterminal)) {
        for (int i = ntInd + 1; i < rule.Symbols.Count; i++) {
          // If we've reached the end of the production, then add the
          // productions' FOLLOW set to FOLLOW
          if (i == rule.Symbols.Count) {
            followSet.UnionWith(ComputeFollow(rule.Name));
            break;
          }

          string next = rule.Symbols[i];
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

  private HashSet<string> ComputePredict(Rule rule) {
    HashSet<string> predictSet = [];

    HashSet<string> productionFirst = ComputeProductionFirst(rule);
    predictSet.UnionWith(productionFirst);

    // If this production is an epsilon production, or if every item in the production can be epsilon,
    // then we should also add the FOLLOW set to the PREDICT set.
    int nonEpsCt = rule.Symbols.Count(e => IsTerminal(e) || !HasEpsilon(e));

    if (nonEpsCt == 0) {
      predictSet.UnionWith(Follow(rule.Name));
    }

    return predictSet;
  }

  private static IEnumerable<int> IndexOfAll<T>(IEnumerable<T> sequence, T element) =>
    sequence.Select((e, i) => (e, i))
      .Where(t => t.e!.Equals(element))
      .Select(t => t.i);

  public static bool IsTerminal(string name) => char.IsLower(name[0]) || name.Equals(Eof);
}
