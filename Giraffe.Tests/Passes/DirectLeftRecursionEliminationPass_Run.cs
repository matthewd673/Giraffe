using Giraffe.Passes;

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
        new("S", ["A"]),
        new("A", ["A", "a"]),
        new("A", ["A", "b"]),
        new("A", ["c"]),
      ], ["S"]);

    DirectLeftRecursionEliminationPass directLeftRecursionEliminationPass = new(grammar);
    directLeftRecursionEliminationPass.Run();

    Assert.Equal(["a", "b", "c", Grammar.Eof], grammar.Terminals);
    Assert.Equal(["S", "A", "A#tail", "A#tails", "A#head"], grammar.Nonterminals);

    Assert.Equal([
      new("S", ["A"]),
      new("A", ["A#head", "A#tails"]),
      new("A", ["A#head"]),
      new("A#head", ["c"]),
      new("A#tails", ["A#tail", "A#tails"]),
      new("A#tails", ["A#tail"]),
      new("A#tail", ["a"]),
      new("A#tail", ["b"]),
    ], grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithIndirectLeftRecursion_WhenRunCalled_ThenGrammarNotChanged() {
    HashSet<Rule> rules = [new("S", ["A", Grammar.Eof]),
                           new("A", ["B"]),
                           new("B", ["A"])];
    Grammar grammar = new(new(), rules, ["S"]);

    DirectLeftRecursionEliminationPass directLeftRecursionEliminationPass = new(grammar);
    directLeftRecursionEliminationPass.Run();

    Assert.Equal([Grammar.Eof], grammar.Terminals);
    Assert.Equal(["S", "A", "B"], grammar.Nonterminals);

    Assert.Equal(rules, grammar.Rules);
  }

  [Fact]
  public void GivenGrammarWithLoop_WhenRunCalled_ThenExceptionThrown() {
    Grammar grammar = new(new() {
        { "a", new("a") },
        { "b", new("b") },
      },
      [
        new("S", ["A"]),
        new("A", ["A", "a"]),
        new("A", ["A", "b"]),
      ], ["S"]);

    DirectLeftRecursionEliminationPass directLeftRecursionEliminationPass = new(grammar);
    Assert.Throws<Exception>(directLeftRecursionEliminationPass.Run);
  }
}