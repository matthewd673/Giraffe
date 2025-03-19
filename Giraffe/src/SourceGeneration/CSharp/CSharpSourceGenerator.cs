using Giraffe.GIR;
using Giraffe.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public abstract class CSharpSourceGenerator {
  public string FileNamespace { get; init; } = "Giraffe";

  public abstract CompilationUnitSyntax Generate();

  protected delegate TOutput SyntaxTransformer<in TInput, out TOutput>(TInput input) where TOutput : SyntaxNode;

  protected static ThrowStatementSyntax GenerateExceptionThrowStatement(string exceptionClassName, InterpolatedStringExpressionSyntax message) =>
    ThrowStatement(GenerateExceptionObjectCreation(exceptionClassName, message));

  protected static ThrowExpressionSyntax GenerateExceptionThrowExpression(string exceptionClassName, InterpolatedStringExpressionSyntax message) =>
    ThrowExpression(GenerateExceptionObjectCreation(exceptionClassName, message));

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

  protected static string StringToSafeUpperCamelCase(string name) =>
    StringUtils.Capitalize(StringUtils.SnakeCaseToCamelCase(StringUtils.SanitizeNonWordCharacters(name)));

  protected static string StringToSafeCamelCase(string name) =>
    StringUtils.SnakeCaseToCamelCase(StringUtils.SanitizeNonWordCharacters(name));

  protected static string GetDisplayName(Grammar grammar, Symbol symbol) =>
    grammar.DisplayNames.GetValueOrDefault(symbol.Value, symbol.Value);

  private static ObjectCreationExpressionSyntax GenerateExceptionObjectCreation(string exceptionClassName, string message) =>
    ObjectCreationExpression(IdentifierName(exceptionClassName))
      .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                                     Literal(message))))));
  private static ObjectCreationExpressionSyntax GenerateExceptionObjectCreation(string exceptionClassName, InterpolatedStringExpressionSyntax message) =>
    ObjectCreationExpression(IdentifierName(exceptionClassName))
      .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(message))));
}