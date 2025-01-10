using Giraffe.Checks;

namespace Giraffe.Tests.Checks;

public class UndefinedSymbolsCheck_Evaluate {
  [Fact]
  public void GivenGrammarWithNoUndefinedSymbols_WhenEvaluateCalled_ThenCheckPasses() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    },
    [
      new("S", ["A", "B"]),
      new("A", ["a"]),
      new("B", ["A", "B"]),
      new("B", ["b"]),
      new("B", []),
    ],
    ["S"]);

    UndefinedSymbolsCheck undefinedSymbolsCheck = new(grammar);
    Assert.True(undefinedSymbolsCheck.Evaluate().Pass);
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

    UndefinedSymbolsCheck undefinedSymbolsCheck = new(grammar);
    Assert.False(undefinedSymbolsCheck.Evaluate().Pass);
  }

  [Fact]
  public void GivenGrammarWithUndefinedTerminals_WhenEvaluateCalled_ThenCheckFails() {
    Grammar grammar = new(new() {
                            { "a", new("a") },
                          }, [
                               new("S", ["A", "B"]),
                               new("A", ["a"]),
                               new("B", ["b"]),
                             ],
                          ["S"]);

    UndefinedSymbolsCheck undefinedSymbolsCheck = new(grammar);
    Assert.False(undefinedSymbolsCheck.Evaluate().Pass);
  }
}