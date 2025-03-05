namespace Giraffe.GIR;

public abstract record Symbol {
  public string Value { get; }
  public SymbolTransformation Transformation { get; init; }

  protected Symbol(string value, SymbolTransformation? transformation = null) {
    Value = value;
    Transformation = transformation ?? new();
  }
}