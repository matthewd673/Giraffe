namespace Giraffe;

public readonly struct ParseTableKey(string nonterminal, string terminal) {
  public string Nonterminal { get; } = nonterminal;
  public string Terminal { get; } = terminal;
}