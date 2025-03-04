using Giraffe.Analyses;
using static Giraffe.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class SetsAnalysis_Analyze {
  [Fact]
  public void GivenSimpleGrammar_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      R("S", [Nt("A"), Nt("B")]),
      R("A", [T("a")]),
      R("A", []),
      R("B", [T("b")]),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, rules.ToHashSet(), [Nt("S")]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { Nt("S"), [T("a"), T("b")] },
      { Nt("A"), [T("a")] },
      { Nt("B"), [T("b")] },
    }, sets.First);
    Assert.Equal(new() {
      { Nt("S"), [Grammar.Eof] },
      { Nt("A"), [T("b")] },
      { Nt("B"), [Grammar.Eof] },
    }, sets.Follow);
    Assert.Equal(new() {
      { rules[0], [T("a"), T("b")] },
      { rules[1], [T("a")] },
      { rules[2], [T("b")] },
      { rules[3], [T("b")] },
    }, sets.Predict);
  }

  [Fact]
  public void GivenGrammarWithConsecutiveEpsilons_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      R("S", [Nt("A"), Nt("B"), Nt("C"), Nt("D"), Nt("E")]),
      R("A", [T("a")]),
      R("A", []),
      R("B", [T("b")]),
      R("B", []),
      R("C", [T("c")]),
      R("D", [T("d")]),
      R("D", []),
      R("E", [T("e")]),
      R("E", []),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
      { "c", new("c") },
      { "d", new("d") },
      { "e", new("e") },
    }, rules.ToHashSet(), [Nt("S")]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { Nt("S"), [T("a"), T("b"), T("c")] },
      { Nt("A"), [T("a")] },
      { Nt("B"), [T("b")] },
      { Nt("C"), [T("c")] },
      { Nt("D"), [T("d")] },
      { Nt("E"), [T("e")] },
    }, sets.First);

    Assert.Equal(new() {
      { Nt("S"), [Grammar.Eof] },
      { Nt("A"), [T("b"), T("c")] },
      { Nt("B"), [T("c")] },
      { Nt("C"), [T("d"), T("e"), Grammar.Eof] },
      { Nt("D"), [T("e"), Grammar.Eof] },
      { Nt("E"), [Grammar.Eof] },
    }, sets.Follow);

    Assert.Equal(new() {
      { rules[0], [T("a"), T("b"), T("c")] },
      { rules[1], [T("a")] },
      { rules[2], [T("b"), T("c")] },
      { rules[3], [T("b")] },
      { rules[4], [T("c")] },
      { rules[5], [T("c")] },
      { rules[6], [T("d")] },
      { rules[7], [T("e"), Grammar.Eof] },
      { rules[8], [T("e")] },
      { rules[9], [Grammar.Eof] },
    }, sets.Predict);
  }

  [Fact]
  public void GivenGrammarWithRightRecursion_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      R("E", [Nt("T"), Nt("TT")]),
      R("T", [Nt("F"), Nt("FT")]),
      R("TT", [T("add"), Nt("T"), Nt("TT")]),
      R("TT", []),
      R("F", [T("number")]),
      R("FT", []),
    ];
    Grammar grammar = new(new() {
      { "add", new(@"\+") },
      { "number", new("[0-9]+") },
    }, rules.ToHashSet(), [Nt("E")]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { Nt("E"), [T("number")] },
      { Nt("T"), [T("number")] },
      { Nt("TT"), [T("add")] },
      { Nt("F"), [T("number")] },
      { Nt("FT"), [] },
    }, sets.First);

    Assert.Equal(new() {
      { Nt("E"), [Grammar.Eof] },
      { Nt("T"), [T("add"), Grammar.Eof] },
      { Nt("TT"), [Grammar.Eof] },
      { Nt("F"), [T("add"), Grammar.Eof] },
      { Nt("FT"), [T("add"), Grammar.Eof] },
    }, sets.Follow);

    Assert.Equal(new() {
      { rules[0], [T("number")] },
      { rules[1], [T("number")] },
      { rules[2], [T("add")] },
      { rules[3], [Grammar.Eof] },
      { rules[4], [T("number")] },
      { rules[5], [T("add"), Grammar.Eof] },
    }, sets.Predict);
  }

  [Fact]
  public void GivenGrammarWithIndirectRightRecursion_WhenAnalyzeCalled_ThenExpectedSetsReturned() {
    List<Rule> rules = [
      R("S", [Nt("A"), Nt("B")]),
      R("A", [T("a")]),
      R("B", [T("b"), Nt("S")]),
      R("B", [T("b")]),
    ];
    Grammar grammar = new(new() {
      { "a", new("a") },
      { "b", new("b") },
    }, rules.ToHashSet(), [Nt("S")]);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    Assert.Equal(new() {
      { Nt("S"), [T("a")] },
      { Nt("A"), [T("a")] },
      { Nt("B"), [T("b")] },
    }, sets.First);

    Assert.Equal(new() {
      { Nt("S"), [Grammar.Eof] },
      { Nt("A"), [T("b")] },
      { Nt("B"), [Grammar.Eof] },
    }, sets.Follow);

    Assert.Equal(new() {
      { rules[0], [T("a")] },
      { rules[1], [T("a")] },
      { rules[2], [T("b")] },
      { rules[3], [T("b")] },
    }, sets.Predict);
  }
}