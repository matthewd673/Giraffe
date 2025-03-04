using System.Collections.Immutable;

namespace Giraffe.GIR;

public static class GrammarFactory {
  public static Terminal T(string value) => new(value);
  public static Nonterminal Nt(string value) => new(value);
  public static Rule R(Nonterminal nt, IEnumerable<Symbol> symbols) =>
    new(nt, ImmutableList.CreateRange(symbols));
  public static Rule R(string nt, IEnumerable<Symbol> symbols) =>
    R(Nt(nt), symbols);
}