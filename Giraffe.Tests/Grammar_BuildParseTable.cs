namespace Giraffe.Tests;

public class Grammar_BuildParseTable {
  [Fact]
  public void GivenSimpleLL1Grammar_WhenBuildParseTableCalled_ThenExpectedParseTableBuilt() {
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
    ParseTable parseTable = grammar.BuildParseTable();

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
    }, parseTable);
    Assert.True(parseTable.IsLl1());
  }

  [Fact]
  public void GivenRightRecursiveLL1Grammar_WhenBuildParseTableCalled_ThenExpectedParseTableBuilt() {
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
    ParseTable parseTable = grammar.BuildParseTable();

    AssertParseTableEntries(new() {
        { ("S", "a"), [0] },
        { ("T", "a"), [1] },
        { ("T", Grammar.Eof), [2] },
      }, parseTable);
    Assert.True(parseTable.IsLl1());
  }

  [Fact]
  public void GivenLL1GrammarWithNtThatFollowsItself_WhenBuildParseTableCalled_ThenExpectedParseTableBuilt() {
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    },
    [
      new("S", ["A", "A", Grammar.Eof]),
      new("A", ["a"]), new("A", ["b"]),
    ]);
    grammar.ComputeSets();
    ParseTable parseTable = grammar.BuildParseTable();

    AssertParseTableEntries(new() {
      { ("S", "a"), [0] },
      { ("S", "b"), [0] },
      { ("A", "a"), [1] },
      { ("A", "b"), [2] },
    }, parseTable);
    Assert.True(parseTable.IsLl1());
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