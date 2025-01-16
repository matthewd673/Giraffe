using Giraffe.Analyses;

namespace Giraffe.Tests.Analyses;

public class NonProductiveRuleAnalysis_Analyze {
  [Fact]
  public void GivenGrammarWithNoNonProductiveRules_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = new(new() {
      { "a", new("a") }
    },
    [
      new("S", []),
      new("S", ["A"]),
      new("A", ["a"]),
    ], ["S"]);

    NonProductiveRuleAnalysis nonProductiveRuleAnalysis = new(grammar);
    Assert.Equal([], nonProductiveRuleAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithInfinitelyAmbiguousRule_WhenAnalyzeCalled_ThenRulesReturned() {
    Grammar grammar = new(new() {
      { "a", new("a") },
    },
    [
      new("S", ["A"]),
      new("S", ["B"]),
      new("A", ["a"]),
      new("B", ["B"]),
    ], []);

    NonProductiveRuleAnalysis nonProductiveRuleAnalysis = new(grammar);
    Assert.Equal([new("S", ["B"]), new("B", ["B"])], nonProductiveRuleAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithMutuallyRecursiveRules_WhenAnalyzeCalled_thenRulesReturned() {
    Grammar grammar = new([],
    [
      new("S", ["A"]),
      new("A", ["B"]),
      new("B", ["A"]),
    ], []);

    NonProductiveRuleAnalysis nonProductiveRuleAnalysis = new(grammar);
    Assert.Equal([new("S", ["A"]), new("A", ["B"]), new("B", ["A"])],
                 nonProductiveRuleAnalysis.Analyze());
  }
}