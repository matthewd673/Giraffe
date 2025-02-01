using Giraffe.Passes;

namespace Giraffe.Tests.Passes;

public class LeftFactoringPass_Run {
  [Fact]
  public void GivenGrammarWithNoCommonLeftFactors_WhenRunCalled_ThenGrammarNotChanged() {
    HashSet<Rule> rules = [
      new("S", ["a", "A"]),
      new("S", ["b", "B"]),
      new("A", []),
      new("B", []),
    ];
    Grammar grammar = new(new() {
        { "a", new("a") },
        { "b", new("b") },
      }, rules, ["S"]);

    LeftFactoringPass leftFactoringPass = new(grammar);
    leftFactoringPass.Run();

    Assert.Equal(["a", "b", "$$"], grammar.Terminals);
    Assert.Equal(["S", "A", "B"], grammar.Nonterminals);

    Assert.Equal(rules, grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithRulesWithCommonLeftFactor_WhenRunCalled_ThenRuleLeftFactored() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
      { "c", new("c") },
    },
    [
      new("S", ["a", "A"]),
      new("S", ["a", "B", "c"]),
      new("S", ["b", "A"]),
      new("A", ["c"]),
      new("B", ["c"]),
    ], ["S"]);

    LeftFactoringPass leftFactoringPass = new(grammar);
    leftFactoringPass.Run();

    Assert.Equal(["a", "b", "c", "$$"], grammar.Terminals);
    Assert.Equal(["S", "S-a#tail", "A", "B"], grammar.Nonterminals);

    Assert.Equal(
      [
        new("S", ["a", "S-a#tail"]),
        new("S", ["b", "A"]),
        new("S-a#tail", ["A"]),
        new("S-a#tail", ["B", "c"]),
        new("A", ["c"]),
        new("B", ["c"]),
      ], grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithRulesWithMultipleCommonLeftFactors_WhenRunCalled_ThenRuleLeftFactoredRecursively() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
      { "c", new("c") },
    },
    [
      new("S", ["a", "A"]),
      new("S", ["b", "A"]),
      new("A", ["a", "B", "c"]),
      new("A", ["a", "B", "c", "b", "a"]),
      new("B", ["c"]),
    ], ["S"]);

    LeftFactoringPass leftFactoringPass = new(grammar);
    leftFactoringPass.Run();

    Assert.Equal(["a", "b", "c", "$$"], grammar.Terminals);
    Assert.Equal(["S", "A", "A-a#tail", "A-a#tail-B#tail", "A-a#tail-B#tail-c#tail", "B"], grammar.Nonterminals);

    Assert.Equal(
      [
        new("S", ["a", "A"]),
        new("S", ["b", "A"]),
        new("A", ["a", "A-a#tail"]),
        new("A-a#tail", ["B", "A-a#tail-B#tail"]),
        new("A-a#tail-B#tail", ["c", "A-a#tail-B#tail-c#tail"]),
        new("A-a#tail-B#tail-c#tail", ["b", "a"]),
        new("A-a#tail-B#tail-c#tail", []),
        new("B", ["c"]),
      ], grammar.Rules);
  }
}