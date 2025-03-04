using System.Collections.Immutable;

namespace Giraffe;

public static class GrammarFactory {
  public static Terminal T(string value) => new(value);
  public static Nonterminal Nt(string value) => new(value);
  public static Rule R(string nonterminal, IEnumerable<Symbol> symbols) =>
    new(Nt(nonterminal), ImmutableList.CreateRange(symbols));
}