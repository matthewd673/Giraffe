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
        new("S", ["A", "B", "C", "D", "E"]),
        new("A", ["a"]), new("A", []),
        new("B", ["b"]), new("B", []),
        new("C", ["c"]),
        new("D", ["d"]), new("D", []),
        new("E", ["e"]), new("E", []),
      ],
      ["S"]
    );

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    CSharpSourceFilesGenerator sourceFilesGenerator = new(sets) { Namespace = "ExampleRecognizer.Generated" };
    List<CSharpSourceFile> sourceFiles = sourceFilesGenerator.GenerateSourceFiles();

    Console.WriteLine("Writing to {0}", outputDirectory);
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
