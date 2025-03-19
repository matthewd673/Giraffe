using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class DirectLeftRecursionAnalysisTest {
  [Fact]
  public void GivenGrammarWithNoDirectLeftRecursion_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = G([], [R("S", []), R("S", [Nt("A")]), R("A", [])], []);
    DirectLeftRecursionAnalysis directLeftRecursionAnalysis = new(grammar);

    Assert.Empty(directLeftRecursionAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithDirectLeftRecursion_WhenAnalyzeCalled_ThenExpectedRulesReturned() {
    Grammar grammar = G([], [R("S", []), R("S", [Nt("A")]), R("A", [Nt("A")]), R("B", []), R("B", [Nt("B")])], []);
    DirectLeftRecursionAnalysis directLeftRecursionAnalysis = new(grammar);

    Assert.Equal([R("A", [Nt("A")]), R("B", [Nt("B")])], directLeftRecursionAnalysis.Analyze());
  }
}