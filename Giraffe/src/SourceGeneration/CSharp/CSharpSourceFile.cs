using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Giraffe.SourceGeneration.CSharp;

public record CSharpSourceFile(string Filename, CompilationUnitSyntax Contents)
  : SourceFile<CompilationUnitSyntax>(Filename, Contents);