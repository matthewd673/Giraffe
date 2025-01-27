using System.Text.RegularExpressions;

namespace Giraffe;

public record Grammar(Dictionary<string, Regex> terminalDefs,
                     HashSet<Rule> Rules,
                     HashSet<string> EntryNonterminals) {
  public const string Eof = "$$";

  public HashSet<string> Terminals { get; } = [..terminalDefs.Keys, Eof];

  public HashSet<string> Nonterminals => Rules.Select(p => p.Name).ToHashSet();

  public Regex GetTerminalRule(string terminal) => terminalDefs[terminal];

  public IEnumerable<Rule> GetAllRulesForNonterminal(string nonterminal) =>
    Rules.Where(rule => rule.Name.Equals(nonterminal));

  /// <summary>
  /// Remove a nonterminal from the grammar. Also remove all rules associated with the nonterminal and all rules
  /// where the nonterminal appears on the right-hand side.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to remove.</param>
  public void RemoveAllOccurrencesOfNonterminal(string nonterminal) {
    Nonterminals.Remove(nonterminal);
    Rules.RemoveWhere(r => r.Name.Equals(nonterminal));
    Rules.RemoveWhere(r => r.Symbols.Contains(nonterminal));
  }

  public static bool IsTerminal(string name) => char.IsLower(name[0]) || name.Equals(Eof);
}
