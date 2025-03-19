using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class IsLl1AnalysisTest {
  [Fact]
  public void GivenLl1Grammar_WhenAnalyzeCalled_ThenTrueReturned() {
    Grammar grammar = G([], [
      R("S", []),
      R("S", [Nt("A")]),
      R("A", [T("b"), T("a")]),
      R("A", [Nt("C")]),
      R("C", [T("c")]),
      R("C", []),
    ], []);

    IsLl1Analysis isLl1Analysis = new(grammar);
    Assert.True(isLl1Analysis.Analyze());
  }

  [Fact]
  public void GivenNonLl1Grammar_WhenAnalyzeCalled_ThenTrueReturned() {
    Grammar grammar = G([], [
      R("S", []),
      R("S", [Nt("A")]),
      R("A", [T("b"), T("a")]),
      R("A", [Nt("C")]),
      R("C", [T("c")]),
      R("C", [T("b"), T("c")]),
      R("C", []),
    ], []);

    IsLl1Analysis isLl1Analysis = new(grammar);
    Assert.False(isLl1Analysis.Analyze());
  }
}