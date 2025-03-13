using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpFrontendExceptionSourceGenerator : CSharpSourceGenerator {
  public required string FrontendExceptionClassName { get; init; }
  public required string IndexPropertyName { get; init; }
  public required string RowPropertyName { get; init; }
  public required string ColumnPropertyName { get; init; }

  private const string MessageParameterName = "message";
  private const string IndexParameterName = "index";
  private const string RowParameterName = "row";
  private const string ColumnParameterName = "column";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateFrontendExceptionClass()]))
      .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateFrontendExceptionClass() =>
    ClassDeclaration(FrontendExceptionClassName)
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
            Parameter(Identifier("column"))
              .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
       })))
       .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(PrimaryConstructorBaseType(IdentifierName("Exception"))
                                                                       .WithArgumentList(ArgumentList(
                                                                        SingletonSeparatedList(Argument(
                                                                         IdentifierName(MessageParameterName))))))))
       .WithMembers(List(new MemberDeclarationSyntax[] {
         GenerateIndexProperty(),
         GenerateRowProperty(),
         GenerateColumnProperty(),
       }));

  private PropertyDeclarationSyntax GenerateIndexProperty() =>
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)),
                            Identifier(IndexPropertyName))
              .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
              .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                                 .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
              .WithInitializer(EqualsValueClause(IdentifierName(IndexParameterName)))
              .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

  private PropertyDeclarationSyntax GenerateRowProperty() =>
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)),
                            Identifier(RowPropertyName))
              .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
              .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                                 .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
              .WithInitializer(EqualsValueClause(IdentifierName(RowParameterName)))
              .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

  private PropertyDeclarationSyntax GenerateColumnProperty() =>
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)),
                            Identifier(ColumnPropertyName))
              .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
              .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                                 .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
              .WithInitializer(EqualsValueClause(IdentifierName(ColumnParameterName)))
              .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
}