using Giraffe.Checks;

namespace Giraffe.Tests.Checks;

public class UndefinedNonterminalsCheck_Evaluate {
  [Fact]
  public void GivenGrammarWithNoUndefinedNonterminals_WhenEvaluateCalled_ThenCheckPasses() {
    Grammar grammar = new(new() {
        { "a", new("a") },
      },
      [
        new("S", ["A", "B"]),
        new("A", ["a"]),
        new("B", ["A", "B"]),
        new("B", []),
      ],
      ["S"]);

    UndefinedNonterminalsCheck undefinedNonterminalsCheck = new(grammar);
    Assert.True(undefinedNonterminalsCheck.Evaluate().Pass);
  }

  [Fact]
  public void GivenGrammarWithUndefinedNonterminals_WhenEvaluateCalled_ThenCheckFails() {
    Grammar grammar = new(new() {
        { "a", new("a") },
      },
      [
        new("S", ["A", "B"]),
        new("S", ["B", "C"]),
        new("A", ["a"]),
        new("B", ["A", "B"]),
        new("B", []),
      ],
      ["S"]);

    UndefinedNonterminalsCheck undefinedNonterminalsCheck = new(grammar);
    Assert.False(undefinedNonterminalsCheck.Evaluate().Pass);
  }
}