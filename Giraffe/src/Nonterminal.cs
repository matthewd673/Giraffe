namespace Giraffe;

public record Nonterminal(string Value) : Symbol(Value) {
  public virtual bool Equals(Nonterminal? other) => other is not null && Value.Equals(other.Value);

  public override int GetHashCode() => Value.GetHashCode();
}