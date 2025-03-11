using Giraffe.GIR;

namespace Giraffe.Analyses;

/// <summary>
/// Find all rules which contain an ignored terminal and can therefore never be produced.
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class IgnoredTerminalUsageAnalysis(Grammar grammar) : Analysis<HashSet<Rule>>(grammar) {
  public override HashSet<Rule> Analyze() {
    List<Terminal> ignoredTerminals = Grammar.TerminalDefinitions.Keys
                                             .Where(k => Grammar.TerminalDefinitions[k].Ignore).ToList();

    return Grammar.Rules.Where(r => r.Symbols.Exists(s => ignoredTerminals.Contains(s))).ToHashSet();
  }
}