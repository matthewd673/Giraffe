namespace Giraffe.SourceGeneration.CSharp;

public class CSharpSourceFilesGenerator(GrammarSets grammarSets) {
  public string Namespace { get; set; } = "Giraffe";
  public string ScannerClassName { get; set; } = "Scanner";
  public string ParserClassName { get; set; } = "Parser";
  public string ScannerExceptionClassName { get; set; } = "ScannerException";
  public string ParserExceptionClassName { get; set; } = "ParserException";
  public string TokenStructName { get; set; } = "Token";

  public List<CSharpSourceFile> GenerateSourceFiles() {
    List<CSharpSourceFile> sourceFiles = [];

    CSharpScannerSourceGenerator scannerSourceGenerator = new(grammarSets.Grammar) {
      FileNamespace = Namespace,
      ScannerClassName = ScannerClassName,
      ScannerExceptionClassName = ScannerExceptionClassName,
      TokenStructName = TokenStructName,
    };
    sourceFiles.Add(new(GetFileName(ScannerClassName), scannerSourceGenerator.Generate()));

    CSharpParserSourceGenerator parserSourceGenerator = new(grammarSets) {
      FileNamespace = Namespace,
      ParserClassName = ParserClassName,
      ScannerClassName = ScannerClassName,
      ParserExceptionClassName = ParserExceptionClassName,
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

    CSharpTokenSourceGenerator tokenSourceGenerator = new() {
      FileNamespace = Namespace,
      TokenStructName = TokenStructName,
    };
    sourceFiles.Add(new(GetFileName(TokenStructName), tokenSourceGenerator.Generate()));

    return sourceFiles;
  }

  private static string GetFileName(string className) => $"{className}.cs";
}