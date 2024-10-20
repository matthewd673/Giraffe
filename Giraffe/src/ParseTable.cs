namespace Giraffe;

public class ParseTable() : Dictionary<(string, string), List<int>> {
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

  public List<int> Get(string nonterminal, string terminal) =>
    TryGetValue((nonterminal, terminal), out List<int>? prodIndList)
      ? prodIndList
      : [];
}
