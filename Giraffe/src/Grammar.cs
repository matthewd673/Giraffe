using System.Text.RegularExpressions;

namespace Giraffe;

public record Grammar {
  public const string Eof = "_eof";

  public HashSet<string> Terminals { get; }

  public HashSet<string> Nonterminals => Rules.Select(p => p.Name).ToHashSet();

  public Dictionary<string, string> DisplayNames { get; } = new();
  public HashSet<Rule> Rules { get; init; }
  public HashSet<string> EntryNonterminals { get; init; }
  public SemanticAction MemberDeclarations { get; init; }

  private Dictionary<string, Regex> terminalDefs;

  public Grammar(Dictionary<string, Regex> terminalDefs,
                 HashSet<Rule> rules,
                 HashSet<string> entryNonterminals,
                 SemanticAction? memberDeclarations = null) {
    this.terminalDefs = terminalDefs;
    Rules = rules;
    EntryNonterminals = entryNonterminals;
    MemberDeclarations = memberDeclarations ?? new();

    Terminals = [..terminalDefs.Keys, Eof];
  }

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
  public void Deconstruct(out Dictionary<string, Regex> terminalDefs, out HashSet<Rule> Rules, out HashSet<string> EntryNonterminals) {
    terminalDefs = this.terminalDefs;
    Rules = this.Rules;
    EntryNonterminals = this.EntryNonterminals;
  }
}
