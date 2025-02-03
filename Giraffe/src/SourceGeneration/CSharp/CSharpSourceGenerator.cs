using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public abstract class CSharpSourceGenerator {
  public string FileNamespace { get; set; } = "Giraffe";

  public abstract CompilationUnitSyntax Generate();

  protected delegate TOutput SyntaxTransformer<in TInput, out TOutput>(TInput input) where TOutput : SyntaxNode;

  protected static IEnumerable<SyntaxNodeOrToken> GenerateCommaSeparatedList<TInput, TOutput>(IEnumerable<TInput> collection,
                                                                                              SyntaxTransformer<TInput, TOutput> transformer)
    where TOutput : SyntaxNode =>
    collection.SelectMany<TInput, SyntaxNodeOrToken>(i => [transformer.Invoke(i), Token(SyntaxKind.CommaToken)]).SkipLast(1);

  protected static FileScopedNamespaceDeclarationSyntax GenerateNamespaceDeclaration(string @namespace) {
    string[] identifierNames = @namespace.Split('.');
    NameSyntax nameSyntax = identifierNames.Skip(1).Aggregate((NameSyntax)IdentifierName(identifierNames[0]),
                                                              (acc, id) => QualifiedName(acc, IdentifierName(id)));
    return FileScopedNamespaceDeclaration(nameSyntax);
  }

  protected static string SanitizeMethodName(string name) => name; // TODO
}