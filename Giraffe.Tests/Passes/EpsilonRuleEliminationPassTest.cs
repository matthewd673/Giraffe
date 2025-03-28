using Giraffe.GIR;
using Giraffe.Passes;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Passes;

public class EpsilonRuleEliminationPassTest {
  [Fact]
  // Test case from "Parsing Techniques" Fig.s 4.10-4.11
  public void GivenGrammarWithEpsilonRules_WhenRunCalled_ThenEpsilonRulesRemovedProperly() {
    Grammar grammar = new(new() {
        { T("a"), new(new("a")) },
      },
      [
        R("S", [Nt("L"), T("a"), Nt("M")]),
        R("L", [Nt("L"), Nt("M")]),
        R("L", []),
        R("M", [Nt("M"), Nt("M")]),
        R("M", []),
      ],
      [Nt("S")]);

    EpsilonRuleEliminationPass epsilonRuleEliminationPass = new(grammar);
    epsilonRuleEliminationPass.Run();

    Assert.Equal([T("a"), Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"), Nt("L"), Nt("M"), Nt("L'"), Nt("M'")], grammar.Nonterminals);

    Assert.Equal(
      [
        R("S", [Nt("L'"), T("a"), Nt("M'")]),
        R("S", [T("a"), Nt("M'")]),
        R("S", [Nt("L'"), T("a")]),
        R("S", [T("a")]),
        R("L", [Nt("L'"), Nt("M'")]),
        R("L", [Nt("L'")]),
        R("L", [Nt("M'")]),
        R("L", []),
        R("M", [Nt("M'"), Nt("M'")]),
        R("M", [Nt("M'")]),
        R("M", []),
        R("L'", [Nt("L'"), Nt("M'")]),
        R("L'", [Nt("L'")]),
        R("L'", [Nt("M'")]),
        R("M'", [Nt("M'"), Nt("M'")]),
        R("M'", [Nt("M'")]),
      ],
      grammar.Rules);
  }
}