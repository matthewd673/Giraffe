using Giraffe.Analyses;

namespace Giraffe.Tests.Analyses;

public class FindUnreachableGrammars_Analyze {
  [Fact]
  public void GivenGrammarWithNoUnreachableSymbols_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, [
      new("S", ["A", "B", Grammar.Eof]),
      new("A", ["a"]),
      new("B", ["b", "C"]),
      new("C", ["a", "b"]),
      new("C", []),
    ], ["S"]);

    UnreachableSymbolsAnalysis unreachableSymbolsAnalysis = new(grammar);
    Assert.Equal([], unreachableSymbolsAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithEmptyEntrySet_WhenAnalyzeCalled_ThenAllSymbolsReturned() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, [
      new("S", ["A", "B", Grammar.Eof]),
      new("A", ["a"]),
      new("B", ["b", "C"]),
      new("C", ["a", "b"]),
      new("C", []),
    ], []);

    UnreachableSymbolsAnalysis unreachableSymbolsAnalysis = new(grammar);
    Assert.Equal(["a", "b", Grammar.Eof, "S", "A", "B", "C"], unreachableSymbolsAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithUnreachableSymbols_WhenAnalyzeCalled_ThenUnreachableSymbolsReturned() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
      { "c", new("c") },
    }, [
      new("S", ["A", "B", Grammar.Eof]),
      new("A", ["a"]),
      new("B", ["b"]),
      new("C", ["a", "b"]),
      new("C", ["D"]),
      new("C", ["c"]),
      new("D", []),
    ], ["S"]);

    UnreachableSymbolsAnalysis unreachableSymbolsAnalysis = new(grammar);
    Assert.Equal(["c", "C", "D"], unreachableSymbolsAnalysis.Analyze());
  }
}