namespace Giraffe.Analyses;

/// <summary>
/// Compute the FIRST, FOLLOW, and PREDICT sets for the grammar.
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class SetsAnalysis(Grammar grammar) : Analysis<GrammarSets>(grammar) {
  private readonly Dictionary<string, HashSet<string>> first = [];
  private readonly Dictionary<string, HashSet<string>> follow = [];
  private readonly Dictionary<Rule, HashSet<string>> predict = [];

  public override GrammarSets Analyze() {
    foreach (string nonterminal in Grammar.Nonterminals) {
      GetFirst(nonterminal);
    }

    foreach (string nonterminal in Grammar.Nonterminals) {
      GetFollow(nonterminal, []);
    }

    foreach (Rule rule in Grammar.Rules) {
      GetPredict(rule);
    }

    return new(Grammar, first, follow, predict);
  }

  private HashSet<string> GetFirst(string symbol) {
    if (Grammar.IsTerminal(symbol)) {
      return [symbol];
    }

    if (first.TryGetValue(symbol, out HashSet<string>? set)) {
      return set;
    }

    first[symbol] = ComputeFirst(symbol);
    return first[symbol];
  }

  private HashSet<string> GetFirst(Rule rule) {
    if (rule.IsEpsilon) {
      return [];
    }

    HashSet<string> firstSet = [];
    foreach (string s in rule.Symbols) {
      firstSet.UnionWith(GetFirst(s));
      if (!HasEpsilon(s)) {
        break;
      }
    }

    return firstSet;
  }

  private HashSet<string> GetFollow(string symbol, HashSet<string> seen) {
    if (follow.TryGetValue(symbol, out HashSet<string>? set)) {
      return set;
    }

    follow[symbol] = ComputeFollow(symbol, seen);
    return follow[symbol];
  }

  private HashSet<string> GetPredict(Rule rule) {
    if (predict.TryGetValue(rule, out HashSet<string>? set)) {
      return set;
    }

    predict[rule] = ComputePredict(rule);
    return predict[rule];
  }

  private HashSet<string> ComputeFirst(string symbol) =>
    Grammar.IsTerminal(symbol)
      ? [symbol]
      : Grammar.GetAllRulesForNonterminal(symbol)
               .Aggregate(new HashSet<string>(),
                          (acc, rule) => acc.Union(GetFirst(rule)).ToHashSet());

  private HashSet<string> ComputeFollow(string symbol, HashSet<string> seen) {
    if (Grammar.IsTerminal(symbol)) {
      throw new($"Cannot compute follow set for terminal \"{symbol}\"");
    }

    // If this is an entry nonterminal, it must have EOF in its FOLLOW set.
    HashSet<string> followSet = Grammar.EntryNonterminals.Contains(symbol) ? [Grammar.Eof] : [];
    foreach (Rule rule in Grammar.Rules.Where(r => r.Symbols.Contains(symbol))) {
      int searchIndex = 0;
      while (true) {
        int index = Utils.IndexOf(rule.Symbols, symbol, searchIndex);
        if (index == -1) {
          break;
        }

        searchIndex = index + 1;

        // Scan forward from the index to build the FOLLOW set.
        // We will continue scanning until we reach a symbol that doesn't have an epsilon production,
        // we reach the end of the rule, or we reach another occurrence of ourselves.
        while (searchIndex < rule.Symbols.Count) {
          followSet.UnionWith(GetFirst(rule.Symbols[searchIndex]));

          if (!HasEpsilon(rule.Symbols[searchIndex])) {
            break;
          }

          searchIndex += 1;
        }

        if (searchIndex == rule.Symbols.Count && !seen.Contains(rule.Name)) {
          followSet.UnionWith(GetFollow(rule.Name, seen.Union([rule.Name]).ToHashSet()));
        }
      }
    }

    return followSet;
  }

  private HashSet<string> ComputePredict(Rule rule) {
    HashSet<string> predictSet = GetFirst(rule);

    // If this rule is epsilon, or if every item within it can be epsilon,
    // then the PREDICT set also includes the FOLLOW set.
    if (rule.IsEpsilon || rule.Symbols.All(HasEpsilon)) {
      predictSet.UnionWith(GetFollow(rule.Name, []));
    }

    return predictSet;
  }

  private bool HasEpsilon(string symbol) =>
    !Grammar.IsTerminal(symbol) && Grammar.GetAllRulesForNonterminal(symbol).Any(r => r.IsEpsilon);
}