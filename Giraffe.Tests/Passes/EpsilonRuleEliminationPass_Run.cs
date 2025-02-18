using Giraffe.Passes;

namespace Giraffe.Tests.Passes;

public class EpsilonRuleEliminationPass_Run {
  [Fact]
  // Test case from "Parsing Techniques" Fig.s 4.10-4.11
  public void GivenGrammarWithEpsilonRules_WhenRunCalled_ThenEpsilonRulesRemovedProperly() {
    Grammar grammar = new(new() {
        { "a", new("a") },
      },
      [
        new("S", ["L", "a", "M"]),
        new("L", ["L", "M"]),
        new("L", []),
        new("M", ["M", "M"]),
        new("M", []),
      ],
      ["S"]);

    EpsilonRuleEliminationPass epsilonRuleEliminationPass = new(grammar);
    epsilonRuleEliminationPass.Run();

    Assert.Equal(["a", Grammar.Eof], grammar.Terminals);
    Assert.Equal(["S", "L", "M", "L'", "M'"], grammar.Nonterminals);

    Assert.Equal(
      [
        new("S", ["L'", "a", "M'"]),
        new("S", ["a", "M'"]),
        new("S", ["L'", "a"]),
        new("S", ["a"]),
        new("L", ["L'", "M'"]),
        new("L", ["L'"]),
        new("L", ["M'"]),
        new("L", []),
        new("M", ["M'", "M'"]),
        new("M", ["M'"]),
        new("M", []),
        new("L'", ["L'", "M'"]),
        new("L'", ["L'"]),
        new("L'", ["M'"]),
        new("M'", ["M'", "M'"]),
        new("M'", ["M'"]),
      ],
      grammar.Rules);
  }
}