namespace Giraffe;

public class ParseTable : Dictionary<ParseTableKey, HashSet<Rule>> {
  /// <summary>
  /// Insert a new entry into the parse table.
  /// </summary>
  /// <param name="nonterminal">The nonterminal of the entry.</param>
  /// <param name="terminal">The terminal of the entry.</param>
  /// <param name="rule">The rule to store in the table.</param>
  public void Insert(string nonterminal, string terminal, Rule rule) {
    if (TryGetValue(new(nonterminal, terminal), out HashSet<Rule>? rules)) {
      rules.Add(rule);
    }
    else {
      Add(new(nonterminal, terminal), [rule]);
    }
  }

  /// <summary>
  /// Get a set of rules for a given nonterminal and terminal.
  /// </summary>
  /// <param name="nonterminal">The nonterminal of the entry.</param>
  /// <param name="terminal">The terminal of the entry.</param>
  /// <returns>
  ///   A set of rules in the grammar corresponding with the given nonterminal and terminal in the table.
  /// </returns>
  public HashSet<Rule> Get(string nonterminal, string terminal) =>
    TryGetValue(new(nonterminal, terminal), out HashSet<Rule>? rules)
      ? rules
      : [];

  /// <summary>
  /// Check if the grammar is LL(1). Runs in O(n*m) time, where n is the number
  /// of nonterminals and m is the number of terminals.
  /// </summary>
  /// <returns>
  ///   <c>true</c> if the grammar is LL(1). <c>false</c> otherwise.
  /// </returns>
  public bool IsLl1() => !Values.Any(l => l.Count > 1);
}
