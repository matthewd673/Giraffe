using Giraffe.Analyses;
using Giraffe.GIR;
using Giraffe.SourceGeneration.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Giraffe.Tests.SourceGeneration.CSharp;

public class CSharpSourceFilesGeneratorTest {
  [Fact]
  public void GivenCustomNamespace_WhenGenerateSourceFilesCalled_ThenEveryFileHasTheCorrectNamespace() {
    const string testNamespace = "TestNamespace";

    Grammar grammar = new([], [], []);

    SetsAnalysis setsAnalysis = new(grammar);
    GrammarSets sets = setsAnalysis.Analyze();

    CSharpSourceFilesGenerator sourceFilesGenerator = new(sets) { Namespace = testNamespace };

    // NOTE: This assertion will likely break if the custom namespace has multiple parts
    Assert.All(sourceFilesGenerator.GenerateSourceFiles(),
               f => Assert.Contains(f.Contents.Members,
                                    m => m is FileScopedNamespaceDeclarationSyntax s &&
                                         testNamespace.Equals(s.Name.GetText().ToString())));
  }
}