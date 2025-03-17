using Giraffe.Analyses;
using Giraffe.AST;
using Giraffe.Checks;
using Giraffe.Frontend;
using Giraffe.GIR;
using Giraffe.Passes;
using Giraffe.SourceGeneration;
using Giraffe.SourceGeneration.CSharp;
using static Giraffe.Utils.ConsoleUtils;

namespace Giraffe;

public class Program {
  public static void Main(string[] args) {
    if (args.Length != 3) {
      PrintInfo("usage: giraffe <grammar file> <output directory> <namespace>");
      return;
    }
    string grammarFilename = args[0];
    string outputDirectory = args[1];
    string @namespace = args[2];

    // Load the grammar file
    string grammarText;
    try {
      grammarText = File.ReadAllText(grammarFilename);
    }
    catch (Exception exception) {
      PrintError($"An error occurred while loading the grammar definition file: {exception.Message}");
      return;
    }

    // Create the Grammar
    Grammar grammar;
    try {
      grammar = CreateGrammar(grammarText);
    }
    catch (FrontendException exception) {
      PrintError($"A {exception.GetType().Name} occurred at ({exception.Row},{exception.Column}): {exception.Message}");
      return;
    }
    catch (Exception exception) {
      PrintError($"An internal error occurred while constructing the grammar: {exception.Message}");
      return;
    }

    // Run semantic checks on the Grammar
    if (!CheckGrammar(grammar)) {
      return;
    }

    // Compute the sets and generate the source code output
    try {
      ProcessGrammarAndGenerateSourceFiles(grammar, outputDirectory, @namespace);
    }
    catch (Exception e) {
      PrintError($"An error occurred while generating the parser: {e.Message}");
    }

    PrintInfo("Done");
  }

  private static Grammar CreateGrammar(string grammarText) {
    // Parse the grammar definition
    Scanner scanner = new(grammarText);
    Parser parser = new(scanner);

    ParseTree parseTree = parser.Parse();

    // Walk the definition
    GrammarVisitor visitor = new();
    GrammarDefinition grammarDefinition = visitor.Visit(parseTree);

    // Convert AST to Grammar
    return GrammarBuilder.GrammarOfAST(grammarDefinition);
  }

  private static bool CheckGrammar(Grammar grammar) {
    // Run checks to ensure grammar is semantically valid
    CheckResult undefinedSymbolsCheckResult = new UndefinedSymbolsCheck(grammar).Evaluate();
    if (!undefinedSymbolsCheckResult.Pass) {
      PrintError($"{nameof(UndefinedSymbolsCheck)} failed: {undefinedSymbolsCheckResult.Message}");
      return false;
    }

    // Run analyses to find warnings
    IgnoredTerminalUsageAnalysis ignoredTerminalUsageAnalysis = new(grammar);
    HashSet<Rule> rulesContainingIgnoredTerminals = ignoredTerminalUsageAnalysis.Analyze();
    if (rulesContainingIgnoredTerminals.Count > 0) {
      // TODO: Improve this message, it's very vague
      PrintWarning($"Warning: {rulesContainingIgnoredTerminals.Count} rule(s) contain ignored terminals");
    }

    UnreachableSymbolsAnalysis unreachableSymbolsAnalysis = new(grammar);
    HashSet<Symbol> unreachableSymbols = unreachableSymbolsAnalysis.Analyze().ToHashSet();
    // Don't report ignored terminals or EOF as unreachable
    unreachableSymbols.RemoveWhere(s => s is Terminal t && (t.Equals(Grammar.Eof) ||
                                                            grammar.TerminalDefinitions[t].Ignore));
    if (unreachableSymbols.Count > 0) {
      PrintWarning($"Warning: Grammar contains unreachable symbols: {string.Join(", ",
        unreachableSymbols.Select(s => $"\"{s.Value}\""))}");
    }

    NonProductiveRuleAnalysis nonProductiveRuleAnalysis = new(grammar);
    HashSet<Rule> nonProductiveRules = nonProductiveRuleAnalysis.Analyze();
    if (nonProductiveRules.Count > 0) {
      // TODO: Improve this message, it's very vague
      PrintWarning($"Warning: {nonProductiveRules.Count} rule(s) are non-productive");
    }

    return true;
  }

  private static void ProcessGrammarAndGenerateSourceFiles(Grammar grammar, string outputDirectory, string @namespace) {
    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets grammarSets = setsAnalysis.Analyze();

    CSharpSourceFilesGenerator sourceFilesGenerator = new(grammarSets) { Namespace = @namespace };
    List<CSharpSourceFile> sourceFiles = sourceFilesGenerator.GenerateSourceFiles();

    if (!Directory.Exists(outputDirectory)) {
      Directory.CreateDirectory(outputDirectory);
    }
    WriteSourceFiles(sourceFiles, outputDirectory);
  }

  private static void WriteSourceFiles<T>(IEnumerable<SourceFile<T>> sourceFiles,
                                          string outputDirectory) where T : notnull {
    foreach (SourceFile<T> file in sourceFiles) {
      string filePath = Path.Combine(outputDirectory, file.Filename);
      PrintInfo($"Writing \"{filePath}\"");

      using StreamWriter writer = new(filePath);
      writer.Write(file.Contents.ToString());
    }
  }
}
