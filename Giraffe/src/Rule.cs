using System.Collections.Immutable;
using Giraffe.RDT;

namespace Giraffe;

public record Rule {
  public string Name { get; init; }
  public ImmutableList<string> Symbols { get; init; }
  public SemanticAction SemanticAction { get; init; }
  public Dictionary<int, List<string>> SymbolArguments { get; init; }
  public List<string> Output { get; init; }

  public bool IsEpsilon => Symbols.Count == 0;

  public Rule(string name,
              ImmutableList<string> symbols,
              SemanticAction? semanticAction = null,
              Dictionary<int, List<string>>? symbolArguments = null,
              List<string>? output = null) {
    Name = name;
    Symbols = symbols;
    SemanticAction = semanticAction ?? new();
    SymbolArguments = symbolArguments ?? new();
    Output = output ?? [..symbols]; // If no output is provided, default to a flat structure of all symbols in order
  }

  public override int GetHashCode() =>
    HashCode.Combine(Name,
                     Utils.GetCollectionHashCode(Symbols),
                     SemanticAction,
                     Utils.GetCollectionHashCode(SymbolArguments));

  public virtual bool Equals(Rule? other) =>
    other is not null &&
    Name.Equals(other.Name) &&
    Symbols.SequenceEqual(other.Symbols) &&
    SemanticAction.Equals(other.SemanticAction) &&
    SymbolArguments.SequenceEqual(other.SymbolArguments);
}