using System.Collections.Immutable;

namespace Giraffe;

public record Rule {
  public string Name { get; init; }
  public ImmutableList<string> Symbols { get; init; }
  public ImmutableList<string> Parameters { get; init; }
  public string? SemanticAction { get; init; }

  public bool IsEpsilon => Symbols.Count == 0;

  public Rule(string name,
              ImmutableList<string> symbols,
              ImmutableList<string>? parameters = null,
              string? semanticAction = null) {
    Name = name;
    Symbols = symbols;
    Parameters = parameters ?? [];
    SemanticAction = semanticAction;
  }

  public override int GetHashCode() =>
    HashCode.Combine(Name,
                     Utils.GetCollectionHashCode(Symbols),
                     Utils.GetCollectionHashCode(Parameters),
                     SemanticAction);

  public virtual bool Equals(Rule? other) =>
    other is not null &&
    Name.Equals(other.Name) &&
    Symbols.SequenceEqual(other.Symbols) &&
    Parameters.SequenceEqual(other.Parameters) &&
    string.Equals(SemanticAction, other.SemanticAction);
}