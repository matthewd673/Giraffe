namespace Giraffe.Tests;

public class GrammarTest {
  [Fact]
  public void GivenSimpleLL1Grammar_WhenComputeSetsCalled_ThenCorrectSetsComputed() {
    Grammar grammar = new(
      new() {
        { "a", new("a") },
        { "b", new("b") },
        { "c", new("c") },
        { "d", new("d") },
        { "e", new("e") },
      },
      [
        new("S", ["A", "B", "C", "D", "E", Grammar.Eof]),
        new("A", ["a"]), new("A", []),
        new("B", ["b"]), new("B", []),
        new("C", ["c"]),
        new("D", ["d"]), new("D", []),
        new("E", ["e"]), new("E", []),
      ]
    );
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
    Assert.Equal(["a", "b", "c"], grammar.Predict(0));
    Assert.Equal(["a"], grammar.Predict(1));
    Assert.Equal(["b", "c"], grammar.Predict(2));
    Assert.Equal(["b"], grammar.Predict(3));
    Assert.Equal(["c"], grammar.Predict(4));
    Assert.Equal(["c"], grammar.Predict(5));
    Assert.Equal(["d"], grammar.Predict(6));
    Assert.Equal(["e", Grammar.Eof], grammar.Predict(7));
    Assert.Equal(["e"], grammar.Predict(8));
    Assert.Equal([Grammar.Eof], grammar.Predict(9));

    // Table
    ParseTable table = grammar.BuildParseTable();
    AssertParseTableEntries(new() {
      { ("S", "a"), [0] },
      { ("S", "b"), [0] },
      { ("S", "c"), [0] },
      { ("A", "a"), [1] },
      { ("A", "b"), [2] },
      { ("A", "c"), [2] },
      { ("B", "b"), [3] },
      { ("B", "c"), [4] },
      { ("C", "c"), [5] },
      { ("D", "d"), [6] },
      { ("D", "e"), [7] },
      { ("D", Grammar.Eof), [7] },
      { ("E", "e"), [8] },
      { ("E", Grammar.Eof), [9] },
    }, table);
    Assert.True(table.IsLl1());
  }

  [Fact]
  public void GivenRightRecursiveLL1Grammar_WhenComputeSetsCalled_ThenCorrectSetsComputed() {
    Grammar grammar = new(
      new() {
        {"a", new("a")},
      },
      [
        new("S", ["a", "T", Grammar.Eof]),
        new("T", ["a", "T"]), new("T", []),
      ]
    );
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
    Assert.Equal(["a"], grammar.Predict(0));
    Assert.Equal(["a"], grammar.Predict(1));
    Assert.Equal([Grammar.Eof], grammar.Predict(2));

    // Table
    ParseTable table = grammar.BuildParseTable();
    AssertParseTableEntries(new() {
        { ("S", "a"), [0] },
        { ("T", "a"), [1] },
        { ("T", Grammar.Eof), [2] },
      }, table);
    Assert.True(table.IsLl1());
  }

  [Fact]
  public void GivenLL1GrammarWithNtThatFollowsItself_WhenComputeSetsCalled_ThenCorrectSetsComputed() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    },
    [
      new("S", ["A", "A", Grammar.Eof]),
      new("A", ["a"]), new("A", ["b"]),
    ]);
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
    Assert.Equal(["a", "b"], grammar.Predict(0));
    Assert.Equal(["a"], grammar.Predict(1));
    Assert.Equal(["b"], grammar.Predict(2));

    // Table
    ParseTable table = grammar.BuildParseTable();
    AssertParseTableEntries(new() {
      { ("S", "a"), [0] },
      { ("S", "b"), [0] },
      { ("A", "a"), [1] },
      { ("A", "b"), [2] },
    }, table);
    Assert.True(table.IsLl1());
  }

  private void AssertParseTableEntries(Dictionary<(string, string), List<int>> expected, ParseTable parseTable) {
    foreach (string nonterminal in parseTable.Keys.Select(k => k.Nonterminal).ToHashSet()) {
      foreach (string terminal in parseTable.Keys.Select(k => k.Terminal).ToHashSet()) {
        Assert.Equal(expected.TryGetValue((nonterminal, terminal), out List<int>? expectedProduction)
                       ? expectedProduction
                       : [],
                     parseTable.Get(nonterminal, terminal));
      }
    }
  }
}
