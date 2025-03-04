namespace Giraffe;

public abstract record Symbol {
  public string Value { get; }
  public SymbolTransformation Transformation { get; }

  protected Symbol(string value, SymbolTransformation? transformation = null) {
    Value = value;
    Transformation = transformation ?? new();
  }
}