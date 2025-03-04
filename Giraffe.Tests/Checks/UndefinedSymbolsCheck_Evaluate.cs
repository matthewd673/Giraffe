using Giraffe.Checks;
using static Giraffe.GrammarFactory;

namespace Giraffe.Tests.Checks;

public class UndefinedSymbolsCheck_Evaluate {
  [Fact]
  public void GivenGrammarWithNoUndefinedSymbols_WhenEvaluateCalled_ThenCheckPasses() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    },
    [
      R("S", [Nt("A"), Nt("B")]),
      R("A", [T("a")]),
      R("B", [Nt("A"), Nt("B")]),
      R("B", [T("b")]),
      R("B", []),
    ],
    [Nt("S")]);

    UndefinedSymbolsCheck undefinedSymbolsCheck = new(grammar);
    Assert.True(undefinedSymbolsCheck.Evaluate().Pass);
  }

  [Fact]
  public void GivenGrammarWithUndefinedNonterminals_WhenEvaluateCalled_ThenCheckFails() {
    Grammar grammar = new(new() {
      { "a", new("a") },
    },
    [
      R("S", [Nt("A"), Nt("B")]),
      R("S", [Nt("B"), Nt("C")]),
      R("A", [T("a")]),
      R("B", [Nt("A"), Nt("B")]),
      R("B", []),
    ],
    [Nt("S")]);

    UndefinedSymbolsCheck undefinedSymbolsCheck = new(grammar);
    Assert.False(undefinedSymbolsCheck.Evaluate().Pass);
  }

  [Fact]
  public void GivenGrammarWithUndefinedTerminals_WhenEvaluateCalled_ThenCheckFails() {
    Grammar grammar = new(new() {
      { "a", new("a") },
    }, [
      R("S", [Nt("A"), Nt("B")]),
      R("A", [T("a")]),
      R("B", [T("b")]),
    ],
    [Nt("S")]);

    UndefinedSymbolsCheck undefinedSymbolsCheck = new(grammar);
    Assert.False(undefinedSymbolsCheck.Evaluate().Pass);
  }
}