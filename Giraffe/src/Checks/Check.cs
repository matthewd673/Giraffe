using Giraffe.GIR;

namespace Giraffe.Checks;

public abstract class Check(Grammar grammar) {
  protected Grammar Grammar { get; private set; } = grammar;

  /// <summary>
  /// Determine if the Grammar passes the check.
  /// </summary>
  /// <returns>A CheckResult containing the result of the check.</returns>
  public abstract CheckResult Evaluate();
}