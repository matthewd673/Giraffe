namespace Giraffe.SourceGeneration.CSharp;

public class CSharpSourceFilesGenerator(GrammarSets grammarSets) {
  public string Namespace { get; init; } = "Giraffe";

  public string ScannerClassName { get; init; } = "Scanner";
  public string ScannerNameOfMethodName { get; init; } = "NameOf";
  public string ScannerPeekMethodName { get; init; } = "Peek";
  public string ScannerEatMethodName { get; init; } = "Eat";
  public string ScannerScanNextMethodName { get; init; } = "ScanNext";

  public string ParserClassName { get; init; } = "Parser";
  public string ParserEntryMethodName { get; init; } = "Parse";

  public string ScannerExceptionClassName { get; init; } = "ScannerException";
  public string ParserExceptionClassName { get; init; } = "ParserException";

  public string ParseNodeRecordName { get; init; } = "ParseNode";
  public string ParseNodeKindPropertyName { get; init; } = "Kind";

  public string TokenRecordName { get; init; } = "Token";
  public string TokenImagePropertyName { get; init; } = "Image";

  public string NonterminalRecordName { get; init; } = "Nonterminal";
  public string NonterminalChildrenPropertyName { get; init; } = "Children";

  public List<CSharpSourceFile> GenerateSourceFiles() {
    List<CSharpSourceFile> sourceFiles = [];

    CSharpScannerSourceGenerator scannerSourceGenerator = new(grammarSets.Grammar) {
      FileNamespace = Namespace,
      ScannerClassName = ScannerClassName,
      ScannerExceptionClassName = ScannerExceptionClassName,
      TokenRecordName = TokenRecordName,
      TokenRecordImagePropertyName = TokenImagePropertyName,
      NameOfMethodName = ScannerNameOfMethodName,
      PeekMethodName = ScannerPeekMethodName,
      EatMethodName = ScannerEatMethodName,
      ScanNextMethodName = ScannerScanNextMethodName,
    };
    sourceFiles.Add(new(GetFileName(ScannerClassName), scannerSourceGenerator.Generate()));

    CSharpParserSourceGenerator parserSourceGenerator = new(grammarSets) {
      FileNamespace = Namespace,
      ParserClassName = ParserClassName,
      ScannerClassName = ScannerClassName,
      ParserExceptionClassName = ParserExceptionClassName,
      TokenRecordName = TokenRecordName,
      ParseNodeKindPropertyName = ParseNodeKindPropertyName,
      ScannerPeekMethodName = ScannerPeekMethodName,
      ScannerEatMethodName = ScannerEatMethodName,
      ScannerNameOfMethodName = ScannerNameOfMethodName,
      EntryMethodName = ParserEntryMethodName,
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
      KindPropertyName = ParseNodeKindPropertyName,
    };
    sourceFiles.Add(new(GetFileName(ParseNodeRecordName), parseNodeSourceGenerator.Generate()));

    CSharpTokenSourceGenerator tokenSourceGenerator = new() {
      FileNamespace = Namespace,
      TokenRecordName = TokenRecordName,
      KindPropertyName = ParseNodeKindPropertyName,
      ParseNodeRecordName = ParseNodeRecordName,
      ImagePropertyName = TokenImagePropertyName,
    };
    sourceFiles.Add(new(GetFileName(TokenRecordName), tokenSourceGenerator.Generate()));

    CSharpNonterminalSourceGenerator nonterminalSourceGenerator = new() {
      FileNamespace = Namespace,
      ParseNodeRecordName = ParseNodeRecordName,
      NonterminalRecordName = NonterminalRecordName,
      KindPropertyName = ParseNodeKindPropertyName,
      ChildrenPropertyName = NonterminalChildrenPropertyName,
    };
    sourceFiles.Add(new(GetFileName(NonterminalRecordName), nonterminalSourceGenerator.Generate()));

    return sourceFiles;
  }

  private static string GetFileName(string className) => $"{className}.cs";
}