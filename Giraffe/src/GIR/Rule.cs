using System.Collections.Immutable;
using Giraffe.Utils;

namespace Giraffe.GIR;

public record Rule {
  public Nonterminal Nonterminal { get; init; }
  public ImmutableList<Symbol> Symbols { get; init; }
  public SemanticAction SemanticAction { get; init; }
  public Dictionary<int, List<string>> SymbolArguments { get; init; }

  public bool IsEpsilon => Symbols.Count == 0;

  public Rule(Nonterminal nonterminal,
              ImmutableList<Symbol> symbols,
              SemanticAction? semanticAction = null,
              Dictionary<int, List<string>>? symbolArguments = null) {
    Nonterminal = nonterminal;
    Symbols = symbols;
    SemanticAction = semanticAction ?? new();
    SymbolArguments = symbolArguments ?? new();
  }

  public override int GetHashCode() =>
    HashCode.Combine(Nonterminal,
                     CollectionUtils.GetHashCode(Symbols),
                     SemanticAction,
                     CollectionUtils.GetHashCode(SymbolArguments));

  public virtual bool Equals(Rule? other) =>
    other is not null &&
    Nonterminal.Equals(other.Nonterminal) &&
    Symbols.SequenceEqual(other.Symbols) &&
    SemanticAction.Equals(other.SemanticAction) &&
    SymbolArguments.SequenceEqual(other.SymbolArguments);

  public override string ToString() => $"R({Nonterminal}, [{string.Join(", ", Symbols)}])";
}