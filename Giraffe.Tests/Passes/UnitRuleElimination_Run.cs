using Giraffe.Passes;

namespace Giraffe.Tests.Passes;

public class UnitRuleElimination_Run {
  [Fact]
  public void GivenGrammarWithSimpleUnitRule_WhenRunCalled_ThenUnitRuleRemovedProperly() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "aa", new("A") },
    },
    [
      new("S", ["A"]),
      new("S", ["A", "B"]),
      new("A", ["a"]),
      new("A", ["aa"]),
      new("B", []),
    ],
    ["S"]);

    UnitRuleEliminationPass unitRuleEliminationPass = new(grammar);
    unitRuleEliminationPass.Run();

    Assert.Equal(["a", "aa", "$$"], grammar.Terminals);
    Assert.Equal(["S", "A", "B"], grammar.Nonterminals);

    Assert.Equal(
      [
        new("S", ["a"]),
        new("S", ["aa"]),
        new("S", ["A", "B"]),
        new("A", ["a"]),
        new("A", ["aa"]),
        new("B", []),
      ],
      grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithMultipleUnitRulesForANonterminal_WhenRunCalled_ThenUnitRulesRemovedProperly() {
    Grammar grammar = new(new() {
      { "d", new("d") },
    },
    [
      new("S", ["A", "$$"]),
      new("A", ["B"]),
      new("A", ["C"]),
      new("B", ["D"]),
      new("C", ["D"]),
      new("D", ["d"]),
    ],
    ["S"]);

    UnitRuleEliminationPass unitRuleEliminationPass = new(grammar);
    unitRuleEliminationPass.Run();

    Assert.Equal(["d", "$$"], grammar.Terminals);
    Assert.Equal(["S", "A", "B", "C", "D"], grammar.Nonterminals);

    Assert.Equal(
      [
        new("S", ["A", "$$"]),
        new("A", ["d"]),
        new("B", ["d"]),
        new("C", ["d"]),
        new("D", ["d"]),
      ],
      grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithLeftRecursiveUnitRule_WhenRunCalled_ThenRuleRemoved() {
    Grammar grammar = new(
      [],
      [
        new("S", ["A", "$$"]),
        new("A", ["A"]),
      ],
      ["S"]);

    UnitRuleEliminationPass unitRuleEliminationPass = new(grammar);
    unitRuleEliminationPass.Run();

    Assert.Equal(["$$"], grammar.Terminals);
    Assert.Equal(["S", "A"], grammar.Nonterminals);

    Assert.Equal(
      [
        new("S", ["A", "$$"]),
      ],
      grammar.Rules);
  }
}