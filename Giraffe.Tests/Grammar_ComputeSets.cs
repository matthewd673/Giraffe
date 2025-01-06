namespace Giraffe.Tests;

public class Grammar_ComputeSets {
  [Fact]
  public void GivenSimpleLL1Grammar_WhenComputeSetsCalled_ThenCorrectSetsComputed() {
    Rule[] rules = [
      new("S", ["A", "B", "C", "D", "E", Grammar.Eof]),
      new("A", ["a"]), new("A", []),
      new("B", ["b"]), new("B", []),
      new("C", ["c"]),
      new("D", ["d"]), new("D", []),
      new("E", ["e"]), new("E", []),
    ];
    Grammar grammar = new(
      new() {
        { "a", new("a") },
        { "b", new("b") },
        { "c", new("c") },
        { "d", new("d") },
        { "e", new("e") },
      }, rules.ToHashSet());
    grammar.ComputeSets();

    // Epsilon
    Assert.False(grammar.HasEpsilon("S"));
    Assert.True(grammar.HasEpsilon("A"));
    Assert.True(grammar.HasEpsilon("B"));
    Assert.False(grammar.HasEpsilon("C"));
    Assert.True(grammar.HasEpsilon("D"));
    Assert.True(grammar.HasEpsilon("E"));

    // First
    Assert.Equal(["a", "b", "c"], grammar.First("S"));
    Assert.Equal(["a"], grammar.First("A"));
    Assert.Equal(["b"], grammar.First("B"));
    Assert.Equal(["c"], grammar.First("C"));
    Assert.Equal(["d"], grammar.First("D"));
    Assert.Equal(["e"], grammar.First("E"));

    // Follow
    Assert.Equal([], grammar.Follow("S"));
    Assert.Equal(["b", "c"], grammar.Follow("A"));
    Assert.Equal(["c"], grammar.Follow("B"));
    Assert.Equal(["d", "e", Grammar.Eof], grammar.Follow("C"));
    Assert.Equal(["e", Grammar.Eof], grammar.Follow("D"));
    Assert.Equal([Grammar.Eof], grammar.Follow("E"));

    // Predict
    Assert.Equal(["a", "b", "c"], grammar.Predict(rules[0]));
    Assert.Equal(["a"], grammar.Predict(rules[1]));
    Assert.Equal(["b", "c"], grammar.Predict(rules[2]));
    Assert.Equal(["b"], grammar.Predict(rules[3]));
    Assert.Equal(["c"], grammar.Predict(rules[4]));
    Assert.Equal(["c"], grammar.Predict(rules[5]));
    Assert.Equal(["d"], grammar.Predict(rules[6]));
    Assert.Equal(["e", Grammar.Eof], grammar.Predict(rules[7]));
    Assert.Equal(["e"], grammar.Predict(rules[8]));
    Assert.Equal([Grammar.Eof], grammar.Predict(rules[9]));
  }

  [Fact]
  public void GivenRightRecursiveLL1Grammar_WhenComputeSetsCalled_ThenCorrectSetsComputed() {
    Rule[] rules = [
      new("S", ["a", "T", Grammar.Eof]),
      new("T", ["a", "T"]), new("T", []),
    ];
    Grammar grammar = new(
      new() {
        {"a", new("a")},
      }, rules.ToHashSet());
    grammar.ComputeSets();

    // Epsilon
    Assert.False(grammar.HasEpsilon("S"));
    Assert.True(grammar.HasEpsilon("T"));

    // First
    Assert.Equal(["a"], grammar.First("S"));
    Assert.Equal(["a"], grammar.First("T"));

    // Follow
    Assert.Equal([], grammar.Follow("S"));
    Assert.Equal([Grammar.Eof], grammar.Follow("T"));

    // Predict
    Assert.Equal(["a"], grammar.Predict(rules[0]));
    Assert.Equal(["a"], grammar.Predict(rules[1]));
    Assert.Equal([Grammar.Eof], grammar.Predict(rules[2]));
  }

  [Fact]
  public void GivenLL1GrammarWithNtThatFollowsItself_WhenComputeSetsCalled_ThenCorrectSetsComputed() {
    Rule[] rules = [
      new("S", ["A", "A", Grammar.Eof]),
      new("A", ["a"]), new("A", ["b"]),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, rules.ToHashSet());
    grammar.ComputeSets();

    // Epsilon
    Assert.False(grammar.HasEpsilon("S"));
    Assert.False(grammar.HasEpsilon("A"));

    // First
    Assert.Equal(["a", "b"], grammar.First("S"));
    Assert.Equal(["a", "b"], grammar.First("A"));

    // Follow
    Assert.Equal([], grammar.Follow("S"));
    Assert.Equal(["a", "b", Grammar.Eof], grammar.Follow("A"));

    // Predict
    Assert.Equal(["a", "b"], grammar.Predict(rules[0]));
    Assert.Equal(["a"], grammar.Predict(rules[1]));
    Assert.Equal(["b"], grammar.Predict(rules[2]));
  }
}
