using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class DiscardedSymbolAnalysisTest {
  [Fact]
  public void GivenGrammarWithNoDiscardedSymbols_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = G(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b")) },
      { T("c"), new(new("c")) },
    },
    [R("S", [T("a"), T("b"), T("c")])],
    [Nt("S")]);

    DiscardedSymbolAnalysis discardedSymbolAnalysis = new(grammar);
    Assert.Empty(discardedSymbolAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithSymbolsDiscardedSometimes_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = G(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b")) },
      { T("c"), new(new("c")) },
    },
    [R("S", [T("a"), T("b"), T("c")]),
     R("S", [T("a") with { Transformation = new(Discard: true) }]),
     R("S", [T("b") with { Transformation = new(Discard: true) },
             T("c") with { Transformation = new(Discard: true) }]),
    ],
    [Nt("S")]);

    DiscardedSymbolAnalysis discardedSymbolAnalysis = new(grammar);
    Assert.Empty(discardedSymbolAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithSymbolsAlwaysDiscarded_WhenAnalyzeCalled_ThenSymbolsReturned() {
    Grammar grammar = G(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b")) },
      { T("c"), new(new("c")) },
    },
    [R("S", [T("a") with { Transformation = new(Discard: true) },
             T("b") with { Transformation = new(Discard: true) },
             T("c") with { Transformation = new(Discard: true) }]),
     R("S", [T("a") with { Transformation = new(Discard: true) }]),
     R("S", [T("b") with { Transformation = new(Discard: true) },
             T("c") with { Transformation = new(Discard: true) }]),
    ],
    [Nt("S")]);

    DiscardedSymbolAnalysis discardedSymbolAnalysis = new(grammar);
    Assert.Equal([T("a"), T("b"), T("c")], discardedSymbolAnalysis.Analyze());
  }
}