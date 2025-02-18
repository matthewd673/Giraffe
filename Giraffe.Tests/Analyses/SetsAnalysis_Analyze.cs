using Giraffe.Analyses;

namespace Giraffe.Tests.Analyses;

public class SetsAnalysis_Analyze {
  [Fact]
  public void GivenSimpleGrammar_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
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

  [Fact]
  public void GivenGrammarWithConsecutiveEpsilons_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      new("S", ["A", "B", "C", "D", "E"]),
      new("A", ["a"]),
      new("A", []),
      new("B", ["b"]),
      new("B", []),
      new("C", ["c"]),
      new("D", ["d"]),
      new("D", []),
      new("E", ["e"]),
      new("E", []),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
      { "c", new("c") },
      { "d", new("d") },
      { "e", new("e") },
    }, rules.ToHashSet(), ["S"]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { "S", ["a", "b", "c"] },
      { "A", ["a"] },
      { "B", ["b"] },
      { "C", ["c"] },
      { "D", ["d"] },
      { "E", ["e"] },
    }, sets.First);

    Assert.Equal(new() {
      { "S", [Grammar.Eof] },
      { "A", ["b", "c"] },
      { "B", ["c"] },
      { "C", ["d", "e", Grammar.Eof] },
      { "D", ["e", Grammar.Eof] },
      { "E", [Grammar.Eof] },
    }, sets.Follow);

    Assert.Equal(new() {
      { rules[0], ["a", "b", "c"] },
      { rules[1], ["a"] },
      { rules[2], ["b", "c"] },
      { rules[3], ["b"] },
      { rules[4], ["c"] },
      { rules[5], ["c"] },
      { rules[6], ["d"] },
      { rules[7], ["e", Grammar.Eof] },
      { rules[8], ["e"] },
      { rules[9], [Grammar.Eof] },
    }, sets.Predict);
  }

  [Fact]
  public void GivenGrammarWithRightRecursion_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      new("E", ["T", "TT"]),
      new("T", ["F", "FT"]),
      new("TT", ["add", "T", "TT"]),
      new("TT", []),
      new("F", ["number"]),
      new("FT", []),
    ];
    Grammar grammar = new(new() {
      { "add", new(@"\+") },
      { "number", new("[0-9]+") },
    }, rules.ToHashSet(), ["E"]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { "E", ["number"] },
      { "T", ["number"] },
      { "TT", ["add"] },
      { "F", ["number"] },
      { "FT", [] },
    }, sets.First);

    Assert.Equal(new() {
      { "E", [Grammar.Eof] },
      { "T", ["add", Grammar.Eof] },
      { "TT", [Grammar.Eof] },
      { "F", ["add", Grammar.Eof] },
      { "FT", ["add", Grammar.Eof] },
    }, sets.Follow);

    Assert.Equal(new() {
      { rules[0], ["number"] },
      { rules[1], ["number"] },
      { rules[2], ["add"] },
      { rules[3], [Grammar.Eof] },
      { rules[4], ["number"] },
      { rules[5], ["add", Grammar.Eof] },
    }, sets.Predict);
  }

  [Fact]
  public void GivenGrammarWithIndirectRightRecursion_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      new("S", ["A", "B"]),
      new("A", ["a"]),
      new("B", ["b", "S"]),
      new("B", ["b"]),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, rules.ToHashSet(), ["S"]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { "S", ["a"] },
      { "A", ["a"] },
      { "B", ["b"] },
    }, sets.First);

    Assert.Equal(new() {
      { "S", [Grammar.Eof] },
      { "A", ["b"] },
      { "B", [Grammar.Eof] },
    }, sets.Follow);

    Assert.Equal(new() {
      { rules[0], ["a"] },
      { rules[1], ["a"] },
      { rules[2], ["b"] },
      { rules[3], ["b"] },
    }, sets.Predict);
  }
}