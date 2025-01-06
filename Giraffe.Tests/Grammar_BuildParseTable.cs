namespace Giraffe.Tests;

public class Grammar_BuildParseTable {
  [Fact]
  public void GivenSimpleLL1Grammar_WhenBuildParseTableCalled_ThenExpectedParseTableBuilt() {
    Rule[] rules = [
      new("S", ["A", "B", "C", "D", "E", Grammar.Eof]),
      new("A", ["a"]), new("A", []),
      new("B", ["b"]), new("B", []),
      new("C", ["c"]),
      new("D", ["d"]), new("D", []),
      new("E", ["e"]), new("E", []),
    ];
    Grammar grammar = new(
      new() {
        { "a", new("a") },
        { "b", new("b") },
        { "c", new("c") },
        { "d", new("d") },
        { "e", new("e") },
      }, rules.ToHashSet());
    grammar.ComputeSets();
    ParseTable parseTable = grammar.BuildParseTable();

    AssertParseTableEntries(new() {
      { ("S", "a"), [rules[0]] },
      { ("S", "b"), [rules[0]] },
      { ("S", "c"), [rules[0]] },
      { ("A", "a"), [rules[1]] },
      { ("A", "b"), [rules[2]] },
      { ("A", "c"), [rules[2]] },
      { ("B", "b"), [rules[3]] },
      { ("B", "c"), [rules[4]] },
      { ("C", "c"), [rules[5]] },
      { ("D", "d"), [rules[6]] },
      { ("D", "e"), [rules[7]] },
      { ("D", Grammar.Eof), [rules[7]] },
      { ("E", "e"), [rules[8]] },
      { ("E", Grammar.Eof), [rules[9]] },
    }, parseTable);
    Assert.True(parseTable.IsLl1());
  }

  [Fact]
  public void GivenRightRecursiveLL1Grammar_WhenBuildParseTableCalled_ThenExpectedParseTableBuilt() {
    Rule[] rules = [
      new("S", ["a", "T", Grammar.Eof]),
      new("T", ["a", "T"]), new("T", []),
    ];
    Grammar grammar = new(
      new() {
        {"a", new("a")},
      }, rules.ToHashSet());
    grammar.ComputeSets();
    ParseTable parseTable = grammar.BuildParseTable();

    AssertParseTableEntries(new() {
        { ("S", "a"), [rules[0]] },
        { ("T", "a"), [rules[1]] },
        { ("T", Grammar.Eof), [rules[2]] },
      }, parseTable);
    Assert.True(parseTable.IsLl1());
  }

  [Fact]
  public void GivenLL1GrammarWithNtThatFollowsItself_WhenBuildParseTableCalled_ThenExpectedParseTableBuilt() {
    Rule[] rules = [
      new("S", ["A", "A", Grammar.Eof]),
      new("A", ["a"]), new("A", ["b"]),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, rules.ToHashSet());
    grammar.ComputeSets();
    ParseTable parseTable = grammar.BuildParseTable();

    AssertParseTableEntries(new() {
      { ("S", "a"), [rules[0]] },
      { ("S", "b"), [rules[0]] },
      { ("A", "a"), [rules[1]] },
      { ("A", "b"), [rules[2]] },
    }, parseTable);
    Assert.True(parseTable.IsLl1());
  }

  private void AssertParseTableEntries(Dictionary<(string, string), HashSet<Rule>> expected, ParseTable parseTable) {
    foreach (string nonterminal in parseTable.Keys.Select(k => k.Nonterminal).ToHashSet()) {
      foreach (string terminal in parseTable.Keys.Select(k => k.Terminal).ToHashSet()) {
        Assert.Equal(expected.TryGetValue((nonterminal, terminal), out HashSet<Rule>? expectedRuleSet)
                       ? expectedRuleSet
                       : [],
                     parseTable.Get(nonterminal, terminal));
      }
    }
  }
}