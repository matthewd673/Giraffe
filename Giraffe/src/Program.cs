using Giraffe.Analyses;
using Giraffe.AST;
using Giraffe.Frontend;
using Giraffe.GIR;
using Giraffe.Passes;
using Giraffe.SourceGeneration;
using Giraffe.SourceGeneration.CSharp;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe;

public class Program {
  public static void Main(string[] args) {
    if (args.Length != 3) {
      Console.WriteLine("usage: giraffe <grammar file> <output directory> <namespace>");
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
    catch (Exception e) {
      Console.WriteLine("Failed to read file \"{0}\"", grammarFilename);
      return;
    }

    // Parse the grammar definition
    Scanner scanner = new(grammarText);
    Parser parser = new(scanner);

    ParseTree parseTree;
    try {
      parseTree = parser.Parse();
    }
    catch (ScannerException e) {
      Console.WriteLine("A ScannerException occurred: {0}", e.Message);
      return;
    }
    catch (ParserException e) {
      Console.WriteLine("A ParserException occurred: {0}", e.Message);
      return;
    }

    Console.WriteLine("That is a valid grammar definition, walking...");

    // Walk the definition
    GrammarVisitor visitor = new();

    GrammarDefinition grammarDefinition;
    try {
      grammarDefinition = visitor.Visit(parseTree);
    }
    catch (VisitorException e) {
      Console.WriteLine("A VisitorException occurred: {0}", e.Message);
      return;
    }

    // Convert AST to Grammar
    GrammarBuilder builder = new(grammarDefinition);
    Grammar grammar = builder.GrammarOfAST();

    try {
      ProcessGrammarAndGenerateSourceFiles(grammar, outputDirectory, @namespace);
    }
    catch (Exception e) {
      Console.WriteLine("An error occurred while processing the grammar and generating the parser: {0}", e.Message);
    }

    Console.WriteLine("Done");
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
      Console.WriteLine("Writing \"{0}\"", filePath);

      using StreamWriter writer = new(filePath);
      writer.Write(file.Contents.ToString());
    }
  }
}
