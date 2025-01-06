namespace Giraffe;

public record Rule(string Name, List<string> Symbols) {
  public bool IsEpsilon => Symbols.Count == 0;

  public override int GetHashCode() =>
    HashCode.Combine(Name.GetHashCode(), GetSymbolHashCode());

  public virtual bool Equals(Rule? other) =>
    other is not null && Name.Equals(other.Name) && Symbols.SequenceEqual(other.Symbols);

  // Adapted from https://stackoverflow.com/a/30758270
  private int GetSymbolHashCode() {
    const int seed = 487;
    const int modifier = 31;

    unchecked {
      return Symbols.Aggregate(seed, (a, b) => a * modifier + b.GetHashCode());
    }
  }
}