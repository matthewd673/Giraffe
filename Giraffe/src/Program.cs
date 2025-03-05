using Giraffe.Analyses;
using Giraffe.GIR;
using Giraffe.Passes;
using Giraffe.SourceGeneration;
using Giraffe.SourceGeneration.CSharp;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe;

public class Program {
  public static void Main(string[] args) {
    Console.WriteLine("Giraffe");

    const string outputDirectory = "/Users/matth/Documents/cs/Giraffe/Examples/ExprParser/Generated";

    Rule[] rules = [
      R("EXPR", [Nt("E1")]),
      R("E1", [Nt("E2"), Nt("E1T") with { Transformation = new(Expand: true) }]),
      R("E1T", [Nt("AO"), Nt("E2"), Nt("E1T") with { Transformation = new(Expand: true) }]),
      R("E1T", []),
      R("E2", [Nt("E3"), Nt("E2T") with { Transformation = new(Expand: true) }]),
      R("E2T", [Nt("MO"), Nt("E3"), Nt("E2T") with { Transformation = new(Expand: true) }]),
      R("E2T", []),
      R("E3", [T("number")]),
      R("AO", [T("add")]),
      R("AO", [T("sub")]),
      R("MO", [T("mul")]),
      R("MO", [T("div")]),
    ];

    // TEMP: Generate a Parser
    Grammar grammar = new(
      new() {
        { T("number"), new(new("[0-9]+"), false) },
        { T("add"), new(new(@"\+"), false) },
        { T("sub"), new(new("-"), false) },
        { T("mul"), new(new(@"\*"), false) },
        { T("div"), new(new("/"), false) },
        { T("ws"), new(new(" +"), true)}
      },
      rules.ToHashSet(),
      [new("EXPR")],
      displayNames: new() { {Grammar.Eof.Value, "<end of input>" } }
    );

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    CSharpSourceFilesGenerator sourceFilesGenerator = new(sets) { Namespace = "ExprParser.Generated" };
    List<CSharpSourceFile> sourceFiles = sourceFilesGenerator.GenerateSourceFiles();

    if (!Directory.Exists(outputDirectory)) {
      Directory.CreateDirectory(outputDirectory);
    }
    WriteSourceFiles(sourceFiles, outputDirectory);
  }

  private static void WriteSourceFiles<T>(IEnumerable<SourceFile<T>> sourceFiles, string outputDirectory) where T : notnull {
    foreach (SourceFile<T> file in sourceFiles) {
      string filePath = Path.Combine(outputDirectory, file.Filename);
      Console.WriteLine("Writing \"{0}\"", filePath);

      using StreamWriter writer = new(filePath);
      writer.Write(file.Contents.ToString());
    }
  }
}
