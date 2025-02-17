using Giraffe.Analyses;
using Giraffe.Passes;
using Giraffe.SourceGeneration;
using Giraffe.SourceGeneration.CSharp;

namespace Giraffe;

public class Program {
  public static void Main(string[] args) {
    Console.WriteLine("Giraffe");

    const string outputDirectory = "GiraffeOutput";

    // TEMP: Generate a Parser
    Grammar grammar = new(
      new() {
        {"a", new("a")},
        {"b", new("b")},
        {"c", new("c")},
        {"d", new("d")},
        {"e", new("e")},
      },
      [
        new("S", ["A", "B", "C", "D", "E"],
            semanticAction: new(Before: "Console.WriteLine(\"Semantic action!\");",
                                After: "Console.WriteLine(\"Done :D\");")),
        new("A", ["a"]), new("A", []),
        new("B", ["b"]), new("B", []),
        new("C", ["c"], semanticAction: new(Before: "Console.WriteLine(\"See C\");")),
        new("D", ["d"]), new("D", []),
        new("E", ["e"]), new("E", []),
      ],
      ["S"]
    );

    grammar.DisplayNames.Add(Grammar.Eof, "<end of input>");
    grammar.DisplayNames.Add("S", "Start");

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    CSharpSourceFilesGenerator sourceFilesGenerator = new(sets) { Namespace = "ExampleRecognizer.Generated" };
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
