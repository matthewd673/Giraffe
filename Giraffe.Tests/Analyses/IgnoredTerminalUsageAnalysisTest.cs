using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class IgnoredTerminalUsageAnalysisTest {
  [Fact]
  public void GivenGrammarWithNoIgnoredTerminals_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = G(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b")) },
    },
    [R("S", [T("a"), T("b")])],
    [Nt("S")]);

    IgnoredTerminalUsageAnalysis ignoredTerminalUsageAnalysis = new(grammar);
    Assert.Empty(ignoredTerminalUsageAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithUnusedIgnoredTerminals_WhenAnalyzeCalled_ThenEmptySetReturned() {
    Grammar grammar = G(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b"), ignore: true) },
      { T("c"), new(new("c")) },
    },
    [R("S", [T("a")]), R("S", [T("c")])],
    [Nt("S")]);

    IgnoredTerminalUsageAnalysis ignoredTerminalUsageAnalysis = new(grammar);
    Assert.Empty(ignoredTerminalUsageAnalysis.Analyze());
  }

  [Fact]
  public void GivenGrammarWithUsedIgnoredTerminals_WhenAnalyzeCalled_ThenAffectedRulesReturned() {
    Grammar grammar = G(new() {
      { T("a"), new(new("a")) },
      { T("b"), new(new("b"), ignore: true) },
      { T("c"), new(new("c")) },
      { T("d"), new(new("d"), ignore: true) },
    },
    [R("S", [T("a")]), R("S", [T("a"), T("b")]), R("P", [T("c")]), R("P", [T("d")])],
    [Nt("S")]);

    IgnoredTerminalUsageAnalysis ignoredTerminalUsageAnalysis = new(grammar);
    Assert.Equal([R("S", [T("a"), T("b")]), R("P", [T("d")])], ignoredTerminalUsageAnalysis.Analyze());
  }
}