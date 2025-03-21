using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class FindUnreachableGrammarsTest {
  [Fact]
  public void GivenGrammarWithNoUnreachableSymbols_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = new(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b")) },
    }, [
      R("S", [Nt("A"), Nt("B"), Grammar.Eof]),
      R("A", [T("a")]),
      R("B", [T("b"), Nt("C")]),
      R("C", [T("a"), T("b")]),
      R("C", []),
    ], [Nt("S")]);

    UnreachableSymbolsAnalysis unreachableSymbolsAnalysis = new(grammar);
    Assert.Equal([], unreachableSymbolsAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithEmptyEntrySet_WhenAnalyzeCalled_ThenAllSymbolsReturned() {
    Grammar grammar = new(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b")) },
    }, [
      R("S", [Nt("A"), Nt("B"), Grammar.Eof]),
      R("A", [T("a")]),
      R("B", [T("b"), Nt("C")]),
      R("C", [T("a"), T("b")]),
      R("C", []),
    ], []);

    UnreachableSymbolsAnalysis unreachableSymbolsAnalysis = new(grammar);
    Assert.Equal([T("a"), T("b"), Grammar.Eof, Nt("S"), Nt("A"), Nt("B"), Nt("C")],
                 unreachableSymbolsAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithUnreachableSymbols_WhenAnalyzeCalled_ThenUnreachableSymbolsReturned() {
    Grammar grammar = new(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b")) },
      { T("c"), new(new("c")) },
    }, [
      R("S", [Nt("A"), Nt("B"), Grammar.Eof]),
      R("A", [T("a")]),
      R("B", [T("b")]),
      R("C", [T("a"), T("b")]),
      R("C", [Nt("D")]),
      R("C", [T("c")]),
      R("D", []),
    ], [Nt("S")]);

    UnreachableSymbolsAnalysis unreachableSymbolsAnalysis = new(grammar);
    Assert.Equal([T("c"), Nt("C"), Nt("D")], unreachableSymbolsAnalysis.Analyze());
  }
}