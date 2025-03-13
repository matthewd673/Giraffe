using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpFrontendExceptionSubClassSourceGenerator(string exceptionClassName) : CSharpSourceGenerator {
  public required string FrontendExceptionClassName { get; init; }

  private const string MessageParameterName = "message";
  private const string IndexParameterName = "index";
  private const string RowParameterName = "row";
  private const string ColumnParameterName = "column";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateExceptionClass()]))
      .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateExceptionClass() =>
    ClassDeclaration(exceptionClassName)
       .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
       .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
          Parameter(Identifier(MessageParameterName))
             .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
          Token(SyntaxKind.CommaToken),
          Parameter(Identifier(IndexParameterName))
             .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
          Token(SyntaxKind.CommaToken),
          Parameter(Identifier(RowParameterName))
             .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
          Token(SyntaxKind.CommaToken),
          Parameter(Identifier(ColumnParameterName))
             .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
       })))
       .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(PrimaryConstructorBaseType(
                                                                         IdentifierName(FrontendExceptionClassName))
                                                                        .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(
                                                                         new SyntaxNodeOrToken[] {
                                                                            Argument(IdentifierName(MessageParameterName)),
                                                                            Token(SyntaxKind.CommaToken),
                                                                            Argument(IdentifierName(IndexParameterName)),
                                                                            Token(SyntaxKind.CommaToken),
                                                                            Argument(IdentifierName(RowParameterName)),
                                                                            Token(SyntaxKind.CommaToken),
                                                                            Argument(IdentifierName(ColumnParameterName)),
                                                                         }))))))
       .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
}