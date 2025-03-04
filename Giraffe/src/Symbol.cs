namespace Giraffe;

public readonly record struct Symbol {
  public string Value { get; }
  public SymbolTransformation Transformation { get; }

  public bool IsTerminal => char.IsLower(Value[0]) || Value.Equals(Grammar.Eof);

  public Symbol(string value, SymbolTransformation? transformation = null) {
    Value = value;
    Transformation = transformation ?? new();
  }
}