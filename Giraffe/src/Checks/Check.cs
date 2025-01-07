namespace Giraffe.Checks;

public abstract class Check(Grammar grammar) {
  protected Grammar Grammar { get; private set; } = grammar;

  /// <summary>
  /// This method determines if the Grammar passes the check.
  /// </summary>
  /// <returns>A CheckResult containing the result of the check.</returns>
  public abstract CheckResult Evaluate();
}