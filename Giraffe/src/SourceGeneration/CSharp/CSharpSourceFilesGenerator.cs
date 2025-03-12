using Giraffe.Analyses;
using Giraffe.GIR;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpSourceFilesGenerator(GrammarSets grammarSets) {
  public string Namespace { get; init; } = "Giraffe";

  public string ScannerClassName { get; init; } = "Scanner";
  public string ScannerNameOfMethodName { get; init; } = "NameOf";
  public string ScannerPeekMethodName { get; init; } = "Peek";
  public string ScannerEatMethodName { get; init; } = "Eat";

  public string ParserClassName { get; init; } = "Parser";
  public string ParserEntryMethodName { get; init; } = "Parse";

  public string ScannerExceptionClassName { get; init; } = "ScannerException";
  public string ParserExceptionClassName { get; init; } = "ParserException";

  public string ParseNodeRecordName { get; init; } = "ParseNode";

  public string TokenRecordName { get; init; } = "Token";
  public string TokenKindPropertyName { get; init; } = "Kind";
  public string TokenImagePropertyName { get; init; } = "Image";
  public string TokenIndexPropertyName { get; init; } = "Index";

  public string NonterminalRecordName { get; init; } = "Nonterminal";
  public string NonterminalKindPropertyName { get; init; } = "Kind";
  public string NonterminalChildrenPropertyName { get; init; } = "Children";

  public string ParseTreeRecordName { get; init; } = "ParseTree";
  public string ParseTreeChildrenPropertyName { get; init; } = "Children";

  public string NonterminalKindEnumName { get; init; } = "NtKind";
  public string TokenKindEnumName { get; init; } = "TokenKind";

  public string VisitorClassName { get; init; } = "Visitor";
  public string VisitorVisitMethodName { get; init; } = "Visit";

  public List<CSharpSourceFile> GenerateSourceFiles() {
    List<Terminal> terminalsOrdering = grammarSets.Grammar.Terminals.ToList();
    List<Nonterminal> nonterminalsOrdering = grammarSets.Grammar.Nonterminals.ToList();

    List<CSharpSourceFile> sourceFiles = [];

    CSharpScannerSourceGenerator scannerSourceGenerator = new(grammarSets.Grammar) {
      FileNamespace = Namespace,
      ScannerClassName = ScannerClassName,
      ScannerExceptionClassName = ScannerExceptionClassName,
      TokenRecordName = TokenRecordName,
      TokenImagePropertyName = TokenImagePropertyName,
      TokenKindPropertyName = TokenKindPropertyName,
      NameOfMethodName = ScannerNameOfMethodName,
      PeekMethodName = ScannerPeekMethodName,
      EatMethodName = ScannerEatMethodName,
      TerminalsOrdering = terminalsOrdering,
      TokenKindEnumName = TokenKindEnumName,
    };
    sourceFiles.Add(new(GetFileName(ScannerClassName), scannerSourceGenerator.Generate()));

    CSharpParserSourceGenerator parserSourceGenerator = new(grammarSets) {
      FileNamespace = Namespace,
      ParserClassName = ParserClassName,
      ScannerClassName = ScannerClassName,
      ParserExceptionClassName = ParserExceptionClassName,
      TokenRecordName = TokenRecordName,
      TokenKindEnumName = TokenKindEnumName,
      NonterminalRecordName = NonterminalRecordName,
      NonterminalChildrenPropertyName = NonterminalChildrenPropertyName,
      TokenKindPropertyName = TokenKindPropertyName,
      ScannerPeekMethodName = ScannerPeekMethodName,
      ScannerEatMethodName = ScannerEatMethodName,
      ScannerNameOfMethodName = ScannerNameOfMethodName,
      EntryMethodName = ParserEntryMethodName,
      NonterminalKindEnumName = NonterminalKindEnumName,
      ParseTreeRecordName = ParseTreeRecordName,
    };
    sourceFiles.Add(new(GetFileName(ParserClassName), parserSourceGenerator.Generate()));

    CSharpExceptionSourceGenerator scannerExceptionSourceGenerator = new(ScannerExceptionClassName) {
      FileNamespace = Namespace,
    };
    sourceFiles.Add(new(GetFileName(ScannerExceptionClassName), scannerExceptionSourceGenerator.Generate()));

    CSharpExceptionSourceGenerator parserExceptionSourceGenerator = new(ParserExceptionClassName) {
      FileNamespace = Namespace,
    };
    sourceFiles.Add(new(GetFileName(ParserExceptionClassName), parserExceptionSourceGenerator.Generate()));

    CSharpParseNodeSourceGenerator parseNodeSourceGenerator = new() {
      FileNamespace = Namespace,
      ParseNodeRecordName = ParseNodeRecordName,
    };
    sourceFiles.Add(new(GetFileName(ParseNodeRecordName), parseNodeSourceGenerator.Generate()));

    CSharpTokenSourceGenerator tokenSourceGenerator = new() {
      FileNamespace = Namespace,
      ParseNodeRecordName = ParseNodeRecordName,
      TokenRecordName = TokenRecordName,
      TokenKindEnumName = TokenKindEnumName,
      KindPropertyName = TokenKindPropertyName,
      ImagePropertyName = TokenImagePropertyName,
      IndexPropertyName = TokenIndexPropertyName,
    };
    sourceFiles.Add(new(GetFileName(TokenRecordName), tokenSourceGenerator.Generate()));

    CSharpNonterminalSourceGenerator nonterminalSourceGenerator = new() {
      FileNamespace = Namespace,
      ParseNodeRecordName = ParseNodeRecordName,
      NonterminalRecordName = NonterminalRecordName,
      NonterminalKindEnumName = NonterminalKindEnumName,
      KindPropertyName = NonterminalKindPropertyName,
      ChildrenPropertyName = NonterminalChildrenPropertyName,
    };
    sourceFiles.Add(new(GetFileName(NonterminalRecordName), nonterminalSourceGenerator.Generate()));

    CSharpParseTreeSourceGenerator parseTreeSourceGenerator = new() {
      FileNamespace = Namespace,
      ParseNodeRecordName = ParseNodeRecordName,
      ParseTreeRecordName = ParseTreeRecordName,
      ChildrenPropertyName = ParseTreeChildrenPropertyName,
    };
    sourceFiles.Add(new(GetFileName(ParseTreeRecordName), parseTreeSourceGenerator.Generate()));

    CSharpEnumSourceGenerator nonterminalKindSourceGenerator = new() {
      FileNamespace = Namespace,
      EnumName = NonterminalKindEnumName,
      EnumMembers = nonterminalsOrdering.Select(nt => nt.Value).ToList(),
    };
    sourceFiles.Add(new(GetFileName(NonterminalKindEnumName), nonterminalKindSourceGenerator.Generate()));

    CSharpEnumSourceGenerator tokenKindSourceGenerator = new() {
      FileNamespace = Namespace,
      EnumName = TokenKindEnumName,
      EnumMembers = terminalsOrdering.Select(t => t.Value).ToList(),
    };
    sourceFiles.Add(new(GetFileName(TokenKindEnumName), tokenKindSourceGenerator.Generate()));

    // Filter out irrelevant terminals and nonterminals for the Visitor class
    DiscardedSymbolAnalysis discardedSymbolAnalysis = new(grammarSets.Grammar);
    HashSet<Symbol> discardedSymbols = discardedSymbolAnalysis.Analyze();
    ExpandedNonterminalAnalysis expandedNonterminalAnalysis = new(grammarSets.Grammar);
    HashSet<Nonterminal> expandedNonterminals = expandedNonterminalAnalysis.Analyze();

    List<Terminal> relevantTerminals = terminalsOrdering
                                       // Don't generate methods for ignored terminals
                                       .Where(t => !grammarSets.Grammar.TerminalDefinitions[t].Ignore)
                                       .Where(t => !discardedSymbols.Contains(t))
                                       .ToList();
    List<Nonterminal> relevantNonterminals = nonterminalsOrdering
                                             .Where(nt => !discardedSymbols.Contains(nt))
                                             .Where(nt => !expandedNonterminals.Contains(nt))
                                             // Entry nonterminals are always relevant for the Visitor class
                                             .Union(grammarSets.Grammar.EntryNonterminals)
                                             .ToList();

    CSharpVisitorSourceGenerator visitorSourceGenerator = new(relevantTerminals, relevantNonterminals) {
      FileNamespace = Namespace,
      VisitorClassName = VisitorClassName,
      VisitMethodName = VisitorVisitMethodName,
      ParseTreeRecordName = ParseTreeRecordName,
      ParseNodeRecordName = ParseNodeRecordName,
      NonterminalRecordName = NonterminalRecordName,
      NonterminalKindPropertyName = NonterminalKindPropertyName,
      NonterminalChildrenPropertyName = NonterminalChildrenPropertyName,
      TokenRecordName = TokenRecordName,
      TokenKindPropertyName = TokenKindPropertyName,
      NonterminalKindEnumName = NonterminalKindEnumName,
      TokenKindEnumName = TokenKindEnumName,
    };
    sourceFiles.Add(new(GetFileName(VisitorClassName), visitorSourceGenerator.Generate()));

    return sourceFiles;
  }

  private static string GetFileName(string className) => $"{className}.cs";
}