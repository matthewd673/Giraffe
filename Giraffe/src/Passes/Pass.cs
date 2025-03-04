using Giraffe.GIR;

namespace Giraffe.Passes;

public abstract class Pass(Grammar grammar) {
  protected Grammar Grammar { get; private set; } = grammar;

  /// <summary>
  /// Run the pass on the grammar.
  /// </summary>
  public abstract void Run();
}