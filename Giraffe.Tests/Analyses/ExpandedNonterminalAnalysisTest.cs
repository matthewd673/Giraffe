using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class ExpandedNonterminalAnalysisTest {
  [Fact]
  public void GivenGrammarWithNoExpandedNonterminals_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = G([],
    [R("S", [Nt("A"), Nt("B"), Nt("C")])],
    [Nt("S")]);

    ExpandedNonterminalAnalysis expandedNonterminalAnalysis = new(grammar);
    Assert.Empty(expandedNonterminalAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithNonterminalsExpandedSometimes_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = G([],
    [R("S", [Nt("A"), Nt("B"), Nt("C")]),
     R("S", [Nt("A") with { Transformation = new(Expand: true) }]),
     R("S", [Nt("B") with { Transformation = new(Expand: true) },
             Nt("C") with { Transformation = new(Expand: true) }]),
    ],
    [Nt("S")]);

    ExpandedNonterminalAnalysis expandedNonterminalAnalysis = new(grammar);
    Assert.Empty(expandedNonterminalAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithNonterminalsAlwaysExpanded_WhenAnalyzeCalled_ThenNonterminalsReturned() {
    Grammar grammar = G([],
    [R("S", [Nt("A") with { Transformation = new(Expand: true) },
             Nt("B") with { Transformation = new(Expand: true) },
             Nt("C") with { Transformation = new(Expand: true) }]),
     R("S", [Nt("A") with { Transformation = new(Expand: true) }]),
     R("S", [Nt("B") with { Transformation = new(Expand: true) },
             Nt("C") with { Transformation = new(Expand: true) }]),
    ],
    [Nt("S")]);

    ExpandedNonterminalAnalysis expandedNonterminalAnalysis = new(grammar);
    Assert.Equal([Nt("A"), Nt("B"), Nt("C")], expandedNonterminalAnalysis.Analyze());
  }
}