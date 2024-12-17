namespace Giraffe;

public class ParseTable : Dictionary<(string, string), List<int>> {
  /// <summary>
  /// Insert a new entry into the parse table.
  /// </summary>
  /// <param name="nonterminal">The nonterminal of the entry.</param>
  /// <param name="terminal">The terminal of the entry.</param>
  /// <param name="prodInd">The production index to store in the table.</param>
  public void Insert(string nonterminal, string terminal, int prodInd) {
    if (TryGetValue((nonterminal, terminal), out List<int>? prodList)) {
      prodList.Add(prodInd);
    }
    else {
      Add((nonterminal, terminal), [prodInd]);
    }
  }

  /// <summary>
  /// Get the list of productions for a given nonterminal and terminal.
  /// </summary>
  /// <param name="nonterminal">The nonterminal of the entry.</param>
  /// <param name="terminal">The terminal of the entry.</param>
  /// <returns>
  ///   A list of indices of productions in the grammar corresponding with the
  ///   given nonterminal and terminal in the table.
  /// </returns>
  public List<int> Get(string nonterminal, string terminal) =>
    TryGetValue((nonterminal, terminal), out List<int>? prodIndList)
      ? prodIndList
      : [];

  /// <summary>
  /// Check if the grammar is LR(1). Runs in O(n*m) time where n is the number
  /// of nonterminals and m is the number of terminals.
  /// </summary>
  /// <returns>
  ///   <c>true</c> if the grammar is LR(1). <c>false</c> otherwise.
  /// </returns>
  public bool IsLR1() => !Values.Any(l => l.Count > 1);
}
