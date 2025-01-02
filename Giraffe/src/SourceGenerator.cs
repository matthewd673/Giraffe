using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe;

public abstract class SourceGenerator {
  public abstract CompilationUnitSyntax Generate();

   protected delegate TOutput SyntaxTransformer<in TInput, out TOutput>(TInput input) where TOutput : SyntaxNode;

   protected IEnumerable<SyntaxNodeOrToken> GenerateCommaSeparatedList<TInput, TOutput>(IEnumerable<TInput> collection,
                                                                                        SyntaxTransformer<TInput, TOutput> transformer)
     where TOutput : SyntaxNode =>
     collection.SelectMany<TInput, SyntaxNodeOrToken>(i => [transformer.Invoke(i), Token(SyntaxKind.CommaToken)]);

   protected static FileScopedNamespaceDeclarationSyntax GenerateNamespaceDeclaration(string @namespace) {
     string[] identifierNames = @namespace.Split('.');
     NameSyntax? nameSyntax = IdentifierName(identifierNames[0]);

     foreach (string i in identifierNames.Skip(1)) {
       nameSyntax = QualifiedName(nameSyntax, IdentifierName(i));
     }

     return FileScopedNamespaceDeclaration(nameSyntax);
   }
}