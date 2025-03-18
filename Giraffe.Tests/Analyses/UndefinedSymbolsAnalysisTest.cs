using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class UndefinedSymbolsAnalysisTest {
  [Fact]
  public void GivenGrammarWithNoUndefinedSymbols_WhenEvaluateCalled_ThenEmptySetReturned() {
    Grammar grammar = new(new() {
                            { T("a"), new(new("a")) },
                            { T("b"), new(new("b")) },
                          },
                          [
                            R("S", [Nt("A"), Nt("B")]),
                            R("A", [T("a")]),
                            R("B", [Nt("A"), Nt("B")]),
                            R("B", [T("b")]),
                            R("B", []),
                          ],
                          [Nt("S")]);

    UndefinedSymbolsAnalysis undefinedSymbolsAnalysis = new(grammar);
    Assert.Empty(undefinedSymbolsAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithUndefinedNonterminals_WhenEvaluateCalled_ThenUndefinedNonterminalsReturned() {
    Grammar grammar = new(new() {
                            { T("a"), new(new("a")) },
                          },
                          [
                            R("S", [Nt("A"), Nt("B")]),
                            R("S", [Nt("B"), Nt("C")]),
                            R("A", [T("a")]),
                            R("B", [Nt("A"), Nt("B")]),
                            R("B", []),
                          ],
                          [Nt("S")]);

    UndefinedSymbolsAnalysis undefinedSymbolsAnalysis = new(grammar);
    Assert.Equal([Nt("C")], undefinedSymbolsAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithUndefinedTerminals_WhenEvaluateCalled_ThenCheckFails() {
    Grammar grammar = new(new() {
                            { T("a"), new(new("a")) },
                          }, [
                               R("S", [Nt("A"), Nt("B")]),
                               R("A", [T("a")]),
                               R("B", [T("b")]),
                             ],
                          [Nt("S")]);

    UndefinedSymbolsAnalysis undefinedSymbolsAnalysis = new(grammar);
    Assert.Equal([T("b")], undefinedSymbolsAnalysis.Analyze());
  }
}