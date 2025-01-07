namespace Giraffe.Checks;

public readonly struct CheckResult(bool pass, string? message = null) {
  public bool Pass { get; } = pass;
  public string? Message { get; } = message;
}