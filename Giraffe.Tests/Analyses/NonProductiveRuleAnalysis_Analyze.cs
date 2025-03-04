using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class NonProductiveRuleAnalysis_Analyze {
  [Fact]
  public void GivenGrammarWithNoNonProductiveRules_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = new(new() {
      { "a", new("a") },
    },
    [
      R("S", []),
      R("S", [Nt("A")]),
      R("A", [T("a")]),
    ], [Nt("S")]);

    NonProductiveRuleAnalysis nonProductiveRuleAnalysis = new(grammar);
    Assert.Equal([], nonProductiveRuleAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithInfinitelyAmbiguousRule_WhenAnalyzeCalled_ThenRulesReturned() {
    Grammar grammar = new(new() {
      { "a", new("a") },
    },
    [
      R("S", [Nt("A")]),
      R("S", [Nt("B")]),
      R("A", [T("a")]),
      R("B", [Nt("B")]),
    ], []);

    NonProductiveRuleAnalysis nonProductiveRuleAnalysis = new(grammar);
    Assert.Equal([R("S", [Nt("B")]), R("B", [Nt("B")])], nonProductiveRuleAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithMutuallyRecursiveRules_WhenAnalyzeCalled_thenRulesReturned() {
    Grammar grammar = new([],
    [
      R("S", [Nt("A")]),
      R("A", [Nt("B")]),
      R("B", [Nt("A")]),
    ], []);

    NonProductiveRuleAnalysis nonProductiveRuleAnalysis = new(grammar);
    Assert.Equal([R("S", [Nt("A")]), R("A", [Nt("B")]), R("B", [Nt("A")])],
                 nonProductiveRuleAnalysis.Analyze());
  }
}