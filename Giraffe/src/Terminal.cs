namespace Giraffe;

public record Terminal(string Value) : Symbol(Value) {
  public virtual bool Equals(Terminal? other) => other is not null && Value.Equals(other.Value);

  public override int GetHashCode() => Value.GetHashCode();
}