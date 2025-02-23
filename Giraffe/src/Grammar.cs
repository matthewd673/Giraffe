using System.Text.RegularExpressions;

namespace Giraffe;

public record Grammar {
  public const string Eof = "_eof";

  public HashSet<string> Terminals { get; }
  public HashSet<string> Nonterminals => Rules.Select(p => p.Name).ToHashSet();
  public HashSet<Rule> Rules { get; }
  public HashSet<string> EntryNonterminals { get; }

  /// <summary>
  /// A mapping of the name of a symbol to its display name.
  /// </summary>
  public Dictionary<string, string> DisplayNames { get; } = new();
  /// <summary>
  /// A SemanticAction containing the member declarations to be added to the generated parser class.
  /// </summary>
  public SemanticAction MemberDeclarations { get; init; }
  /// <summary>
  /// A mapping of the name of a nonterminal to the names of its parameters. If no mapping exists for a nonterminal,
  /// it is assumed to have no parameters.
  /// </summary>
  public Dictionary<string, List<string>> NonterminalParameters { get; init; }

  private Dictionary<string, Regex> terminalDefs;

  public Grammar(Dictionary<string, Regex> terminalDefs,
                 HashSet<Rule> rules,
                 HashSet<string> entryNonterminals,
                 SemanticAction? memberDeclarations = null,
                 Dictionary<string, List<string>>? nonterminalParameters = null) {
    this.terminalDefs = terminalDefs;
    Rules = rules;
    EntryNonterminals = entryNonterminals;
    MemberDeclarations = memberDeclarations ?? new();
    NonterminalParameters = nonterminalParameters ?? new();

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

  public static bool IsParameter(string name) => name.StartsWith('$');
}
