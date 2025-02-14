using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Giraffe.SourceGeneration.CSharp;

public readonly struct CSharpSourceFile(string filename, CompilationUnitSyntax contents) {
  public string Filename { get; } = filename;
  public CompilationUnitSyntax Contents { get; } = contents;
}