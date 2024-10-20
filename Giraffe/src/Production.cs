namespace Giraffe;

public class Production : List<string> {
  public string Name { get; }

  /// <summary>
  /// Construct a new epsilon production for a given nonterminal.
  /// </summary>
  /// <param name="name">The name of the nonterminal.</param>
  public Production(string name) {
    Name = name;
  }

  /// <summary>
  /// Construct a new production for a given nonterminal.
  /// </summary>
  /// <param name="name">The name of the nonterminal.</param>
  /// <param name="sequence">The elements in the production.</param>
  public Production(string name, IEnumerable<string> sequence) {
    Name = name;
    AddRange(sequence);
  }
}
