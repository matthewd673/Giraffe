using Giraffe.GIR;
using Giraffe.Passes;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Passes;

public class UnitRuleElimination_Run {
  [Fact]
  public void GivenGrammarWithSimpleUnitRule_WhenRunCalled_ThenUnitRuleRemovedProperly() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "aa", new("A") },
    },
    [
      R("S", [Nt("A")]),
      R("S", [Nt("A"), Nt("B")]),
      R("A", [T("a")]),
      R("A", [T("aa")]),
      R("B", []),
    ],
    [Nt("S")]);

    UnitRuleEliminationPass unitRuleEliminationPass = new(grammar);
    unitRuleEliminationPass.Run();

    Assert.Equal([T("a"), T("aa"), Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"), Nt("A"), Nt("B")], grammar.Nonterminals);

    Assert.Equal(
      [
        R("S", [T("a")]),
        R("S", [T("aa")]),
        R("S", [Nt("A"), Nt("B")]),
        R("A", [T("a")]),
        R("A", [T("aa")]),
        R("B", []),
      ],
      grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithMultipleUnitRulesForANonterminal_WhenRunCalled_ThenUnitRulesRemovedProperly() {
    Grammar grammar = new(new() {
      { "d", new("d") },
    },
    [
      R("S", [Nt("A"), Grammar.Eof]),
      R("A", [Nt("B")]),
      R("A", [Nt("C")]),
      R("B", [Nt("D")]),
      R("C", [Nt("D")]),
      R("D", [T("d")]),
    ],
    [Nt("S")]);

    UnitRuleEliminationPass unitRuleEliminationPass = new(grammar);
    unitRuleEliminationPass.Run();

    Assert.Equal([T("d"), Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"), Nt("A"), Nt("B"), Nt("C"), Nt("D")], grammar.Nonterminals);

    Assert.Equal(
      [
        R("S", [Nt("A"), Grammar.Eof]),
        R("A", [T("d")]),
        R("B", [T("d")]),
        R("C", [T("d")]),
        R("D", [T("d")]),
      ],
      grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithLeftRecursiveUnitRule_WhenRunCalled_ThenRuleRemoved() {
    Grammar grammar = new(
      [],
      [
        R("S", [Nt("A"), Grammar.Eof]),
        R("A", [Nt("A")]),
      ],
      [Nt("S")]);

    UnitRuleEliminationPass unitRuleEliminationPass = new(grammar);
    unitRuleEliminationPass.Run();

    Assert.Equal([Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S")], grammar.Nonterminals);

    Assert.Equal(
      [
        R("S", [Nt("A"), Grammar.Eof]), // A is now an undefined non-terminal
      ],
      grammar.Rules);
  }
}