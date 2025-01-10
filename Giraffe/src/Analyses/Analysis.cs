namespace Giraffe.Analyses;

public abstract class Analysis<T>(Grammar grammar) {
  protected Grammar Grammar { get; private set; } = grammar;

  /// <summary>
  /// Analyze the Grammar and return the resulting information.
  /// </summary>
  /// <returns>The result of the analysis.</returns>
  public abstract T Analyze();
}