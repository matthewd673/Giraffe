using Giraffe.GIR;
using Giraffe.Utils;

namespace Giraffe.Analyses;

/// <summary>
/// Compute the FIRST, FOLLOW, and PREDICT sets for the grammar.
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class SetsAnalysis(Grammar grammar) : Analysis<GrammarSets>(grammar) {
  private readonly Dictionary<Nonterminal, HashSet<Terminal>> first = [];
  private readonly Dictionary<Nonterminal, HashSet<Terminal>> follow = [];
  private readonly Dictionary<Rule, HashSet<Terminal>> predict = [];

  public override GrammarSets Analyze() {
    foreach (Nonterminal nt in Grammar.Nonterminals) {
      GetFirst(nt);
    }

    foreach (Nonterminal nt in Grammar.Nonterminals) {
      GetFollow(nt, []);
    }

    foreach (Rule rule in Grammar.Rules) {
      GetPredict(rule);
    }

    return new(Grammar, first, follow, predict);
  }

  private HashSet<Terminal> GetFirst(Symbol symbol) {
    if (symbol is Terminal t) {
      return [t];
    }

    if (symbol is not Nonterminal nt) {
      throw new ArgumentException($"Cannot get first set for symbol of type {symbol.GetType()}");
    }

    if (first.TryGetValue(nt, out HashSet<Terminal>? set)) {
      return set;
    }

    first.Add(nt, ComputeFirst(nt));
    return first[nt];
  }

  private HashSet<Terminal> GetFirst(Rule rule) {
    if (rule.IsEpsilon) {
      return [];
    }

    HashSet<Terminal> firstSet = [];
    foreach (Symbol s in rule.Symbols) {
      firstSet.UnionWith(GetFirst(s));
      if (!HasEpsilon(s)) {
        break;
      }
    }

    return firstSet;
  }

  private HashSet<Terminal> GetFollow(Nonterminal nt, HashSet<Nonterminal> seen) {
    if (follow.TryGetValue(nt, out HashSet<Terminal>? set)) {
      return set;
    }

    follow[nt] = ComputeFollow(nt, seen);
    return follow[nt];
  }

  private HashSet<Terminal> GetPredict(Rule rule) {
    if (predict.TryGetValue(rule, out HashSet<Terminal>? set)) {
      return set;
    }

    predict[rule] = ComputePredict(rule);
    return predict[rule];
  }

  private HashSet<Terminal> ComputeFirst(Symbol symbol) => symbol switch {
    Terminal t => [t],
    Nonterminal nt => Grammar.GetAllRulesForNonterminal(nt)
                             .Aggregate(new HashSet<Terminal>(),
                                        (acc, rule) => acc.Union(GetFirst(rule)).ToHashSet()),
    _ => throw new ArgumentOutOfRangeException($"Cannot compute first set for symbol of type {symbol.GetType()}"),
  };

  private HashSet<Terminal> ComputeFollow(Nonterminal nt, HashSet<Nonterminal> seen) {
    // If this is an entry nonterminal, it must have EOF in its FOLLOW set.
    HashSet<Terminal> followSet = Grammar.EntryNonterminals.Contains(nt) ? [Grammar.Eof] : [];
    foreach (Rule rule in Grammar.Rules.Where(r => r.Symbols.Contains(nt))) {
      int searchIndex = 0;
      while (true) {
        int index = CollectionUtils.IndexOf(rule.Symbols, nt, searchIndex);
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

        if (searchIndex == rule.Symbols.Count && !seen.Contains(rule.Nonterminal)) {
          followSet.UnionWith(GetFollow(rule.Nonterminal, seen.Union([rule.Nonterminal]).ToHashSet()));
        }
      }
    }

    return followSet;
  }

  private HashSet<Terminal> ComputePredict(Rule rule) {
    HashSet<Terminal> predictSet = GetFirst(rule);

    // If this rule is epsilon, or if every item within it can be epsilon,
    // then the PREDICT set also includes the FOLLOW set.
    if (rule.IsEpsilon || rule.Symbols.All(HasEpsilon)) {
      predictSet.UnionWith(GetFollow(rule.Nonterminal, []));
    }

    return predictSet;
  }

  private bool HasEpsilon(Symbol symbol) =>
    symbol is Nonterminal nt && Grammar.GetAllRulesForNonterminal(nt).Any(r => r.IsEpsilon);
}