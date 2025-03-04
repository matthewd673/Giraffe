using System.Collections.Immutable;

namespace Giraffe;

public record Rule {
  public string Name { get; init; }
  public ImmutableList<Symbol> Symbols { get; init; }
  public SemanticAction SemanticAction { get; init; }
  public Dictionary<int, List<string>> SymbolArguments { get; init; }

  public bool IsEpsilon => Symbols.Count == 0;

  public Rule(string name,
              ImmutableList<Symbol> symbols,
              SemanticAction? semanticAction = null,
              Dictionary<int, List<string>>? symbolArguments = null) {
    Name = name;
    Symbols = symbols;
    SemanticAction = semanticAction ?? new();
    SymbolArguments = symbolArguments ?? new();
  }

  public Rule(string name,
              ImmutableList<string> symbols,
              SemanticAction? semanticAction = null,
              Dictionary<int, List<string>>? symbolArguments = null)
  : this(name, ImmutableList.CreateRange(symbols.Select(s => new Symbol(s))), semanticAction, symbolArguments) {
    // Empty
  }

  public Rule(string name,
              SemanticAction? semanticAction = null,
              Dictionary<int, List<string>>? symbolArguments = null)
  : this(name, ImmutableList.Create<Symbol>(), semanticAction, symbolArguments) {
    // Empty
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