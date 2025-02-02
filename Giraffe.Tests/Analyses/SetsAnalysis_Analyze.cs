using Giraffe.Analyses;

namespace Giraffe.Tests.Analyses;

public class SetsAnalysis_Analyze {
  [Fact]
  public void GivenGrammar_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      new("S", ["A", "B"]),
      new("A", ["a"]),
      new("A", []),
      new("B", ["b"]),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, rules.ToHashSet(), ["S"]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { "S", ["a", "b"] },
      { "A", ["a"] },
      { "B", ["b"] },
      { "a", ["a"] },
      { "b", ["b"] },
    }, sets.First);
    Assert.Equal(new() {
      { "S", [Grammar.Eof] },
      { "A", ["b"] },
      { "B", [Grammar.Eof] },
    }, sets.Follow);
    Assert.Equal(new() {
      { rules[0], ["a", "b"] },
      { rules[1], ["a"] },
      { rules[2], ["b"] },
      { rules[3], ["b"] },
    }, sets.Predict); // TODO
  }
}