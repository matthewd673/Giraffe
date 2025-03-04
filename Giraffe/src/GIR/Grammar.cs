using System.Text.RegularExpressions;

namespace Giraffe.GIR;

public record Grammar {
  public static Terminal Eof { get; } = new("eof");

  public HashSet<Terminal> Terminals { get; }
  public HashSet<Nonterminal> Nonterminals => Rules.Select(p => p.Nonterminal).ToHashSet();
  public HashSet<Rule> Rules { get; }
  public HashSet<Nonterminal> EntryNonterminals { get; }

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
  public Dictionary<Nonterminal, List<string>> NonterminalParameters { get; init; }

  private Dictionary<string, Regex> terminalDefs;

  public Grammar(Dictionary<string, Regex> terminalDefs,
                 HashSet<Rule> rules,
                 HashSet<Nonterminal> entryNonterminals,
                 Dictionary<string, string>? displayNames = null,
                 SemanticAction? memberDeclarations = null,
                 Dictionary<Nonterminal, List<string>>? nonterminalParameters = null) {
    this.terminalDefs = terminalDefs;
    Rules = rules;
    EntryNonterminals = entryNonterminals;
    DisplayNames = displayNames ?? new();
    MemberDeclarations = memberDeclarations ?? new();
    NonterminalParameters = nonterminalParameters ?? new();

    Terminals = [..terminalDefs.Keys.Select(t => new Terminal(t)), Eof];
  }

  public Regex GetTerminalRule(Terminal terminal) => terminalDefs[terminal.Value];

  public IEnumerable<Rule> GetAllRulesForNonterminal(Nonterminal nt) =>
    Rules.Where(rule => rule.Nonterminal.Equals(nt));

  /// <summary>
  /// Remove a nonterminal from the grammar. Also remove all rules associated with the nonterminal and all rules
  /// where the nonterminal appears on the right-hand side.
  /// </summary>
  /// <param name="nonterminal">The nonterminal to remove.</param>
  public void RemoveAllOccurrencesOfNonterminal(Nonterminal nonterminal) {
    Nonterminals.Remove(nonterminal);
    Rules.RemoveWhere(r => r.Nonterminal.Equals(nonterminal));
    Rules.RemoveWhere(r => r.Symbols.Contains(nonterminal));
  }

  public static bool IsParameter(string name) => name.StartsWith('$');
}
