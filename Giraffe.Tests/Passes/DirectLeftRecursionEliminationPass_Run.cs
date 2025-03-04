using Giraffe.GIR;
using Giraffe.Passes;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Passes;

public class DirectLeftRecursionEliminationPass_Run {
  [Fact]
  public void GivenGrammarWithDirectLeftRecursion_WhenRunCalled_ThenDirectLeftRecursionEliminated() {
    Grammar grammar = new(new() {
        { "a", new("a") },
        { "b", new("b") },
        { "c", new("c") },
      },
      [
        R("S", [Nt("A")]),
        R("A", [Nt("A"), T("a")]),
        R("A", [Nt("A"), T("b")]),
        R("A", [T("c")]),
      ], [Nt("S")]);

    DirectLeftRecursionEliminationPass directLeftRecursionEliminationPass = new(grammar);
    directLeftRecursionEliminationPass.Run();

    Assert.Equal([T("a"), T("b"), T("c"), Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"), Nt("A"), Nt("A#tail"), Nt("A#tails"), Nt("A#head")], grammar.Nonterminals);

    Assert.Equal([
      R("S", [Nt("A")]),
      R("A", [Nt("A#head"), Nt("A#tails")]),
      R("A", [Nt("A#head")]),
      R("A#head", [T("c")]),
      R("A#tails", [Nt("A#tail"), Nt("A#tails")]),
      R("A#tails", [Nt("A#tail")]),
      R("A#tail", [T("a")]),
      R("A#tail", [T("b")]),
    ], grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithIndirectLeftRecursion_WhenRunCalled_ThenGrammarNotChanged() {
    HashSet<Rule> rules = [R("S", [Nt("A"), Grammar.Eof]),
                           R("A", [Nt("B")]),
                           R("B", [Nt("A")])];
    Grammar grammar = new(new(), rules, [Nt("S")]);

    DirectLeftRecursionEliminationPass directLeftRecursionEliminationPass = new(grammar);
    directLeftRecursionEliminationPass.Run();

    Assert.Equal([Grammar.Eof], grammar.Terminals);
    Assert.Equal([Nt("S"), Nt("A"), Nt("B")], grammar.Nonterminals);

    Assert.Equal(rules, grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithLoop_WhenRunCalled_ThenExceptionThrown() {
    Grammar grammar = new(new() {
        { "a", new("a") },
        { "b", new("b") },
      },
      [
        R("S", [Nt("A")]),
        R("A", [Nt("A"), T("a")]),
        R("A", [Nt("A"), T("b")]),
      ], [Nt("S")]);

    DirectLeftRecursionEliminationPass directLeftRecursionEliminationPass = new(grammar);
    Assert.Throws<Exception>(directLeftRecursionEliminationPass.Run);
  }
}