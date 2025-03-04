using Giraffe.GIR;
using Giraffe.Passes;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Passes;

public class LeftFactoringPass_Run {
  [Fact]
  public void GivenGrammarWithNoCommonLeftFactors_WhenRunCalled_ThenGrammarNotChanged() {
    HashSet<Rule> rules = [
      R("S", [T("a"), Nt("A")]),
      R("S", [T("b"), Nt("B")]),
      R("A", []),
      R("B", []),
    ];
    Grammar grammar = new(new() {
        { "a", new("a") },
        { "b", new("b") },
      }, rules, [Nt("S")]);

    LeftFactoringPass leftFactoringPass = new(grammar);
    leftFactoringPass.Run();

    Assert.Equal([T("a"), T("b"), Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"), Nt("A"), Nt("B")], grammar.Nonterminals);

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
      R("S", [T("a"), Nt("A")]),
      R("S", [T("a"), Nt("B"), T("c")]),
      R("S", [T("b"), Nt("A")]),
      R("A", [T("c")]),
      R("B", [T("c")]),
    ], [Nt("S")]);

    LeftFactoringPass leftFactoringPass = new(grammar);
    leftFactoringPass.Run();

    Assert.Equal([T("a"), T("b"), T("c"), Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"), Nt("S-a#tail"), Nt("A"), Nt("B")], grammar.Nonterminals);

    Assert.Equal(
      [
        R("S", [T("a"), Nt("S-a#tail")]),
        R("S", [T("b"), Nt("A")]),
        R("S-a#tail", [Nt("A")]),
        R("S-a#tail", [Nt("B"), T("c")]),
        R("A", [T("c")]),
        R("B", [T("c")]),
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
      R("S", [T("a"), Nt("A")]),
      R("S", [T("b"), Nt("A")]),
      R("A", [T("a"), Nt("B"), T("c")]),
      R("A", [T("a"), Nt("B"), T("c"), T("b"), T("a")]),
      R("B", [T("c")]),
    ], [Nt("S")]);

    LeftFactoringPass leftFactoringPass = new(grammar);
    leftFactoringPass.Run();

    Assert.Equal([T("a"), T("b"), T("c"), Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"),
                  Nt("A"),
                  Nt("A-a#tail"),
                  Nt("A-a#tail-B#tail"),
                  Nt("A-a#tail-B#tail-c#tail"),
                  Nt("B")],
                 grammar.Nonterminals);

    Assert.Equal(
      [
        R("S", [T("a"), Nt("A")]),
        R("S", [T("b"), Nt("A")]),
        R("A", [T("a"), Nt("A-a#tail")]),
        R("A-a#tail", [Nt("B"), Nt("A-a#tail-B#tail")]),
        R("A-a#tail-B#tail", [T("c"), Nt("A-a#tail-B#tail-c#tail")]),
        R("A-a#tail-B#tail-c#tail", [T("b"), T("a")]),
        R("A-a#tail-B#tail-c#tail", []),
        R("B", [T("c")]),
      ], grammar.Rules);
  }
}