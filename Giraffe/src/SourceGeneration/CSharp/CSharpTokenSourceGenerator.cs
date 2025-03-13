using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpTokenSourceGenerator : CSharpSourceGenerator {
  public required string ParseNodeRecordName { get; init; }
  public required string ParseNodeIndexPropertyName { get; init; }
  public required string ParseNodeRowPropertyName { get; init; }
  public required string ParseNodeColumnPropertyName { get; init; }
  public required string TokenRecordName { get; init; }
  public required string TokenKindEnumName { get; init; }
  public required string KindPropertyName { get; init; }
  public required string ImagePropertyName { get; init; }

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateTokenStruct()]))
      .NormalizeWhitespace();

  private RecordDeclarationSyntax GenerateTokenStruct() =>
    RecordDeclaration(SyntaxKind.RecordDeclaration, Token(SyntaxKind.RecordKeyword), Identifier(TokenRecordName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
        Parameter(Identifier(KindPropertyName))
          .WithType(IdentifierName(TokenKindEnumName)),
        Token(SyntaxKind.CommaToken),
        Parameter(Identifier(ImagePropertyName))
          .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
          Token(SyntaxKind.CommaToken),
          Parameter(Identifier(ParseNodeIndexPropertyName))
            .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
          Token(SyntaxKind.CommaToken),
          Parameter(Identifier(ParseNodeRowPropertyName))
            .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
          Token(SyntaxKind.CommaToken),
          Parameter(Identifier(ParseNodeColumnPropertyName))
            .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
      })))
      .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(PrimaryConstructorBaseType(
                                                                       IdentifierName(ParseNodeRecordName))
                                                                      .WithArgumentList(ArgumentList(
                                                                       SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] {
                                                                         Argument(IdentifierName(ParseNodeIndexPropertyName)),
                                                                         Token(SyntaxKind.CommaToken),
                                                                         Argument(IdentifierName(ParseNodeRowPropertyName)),
                                                                         Token(SyntaxKind.CommaToken),
                                                                         Argument(IdentifierName(ParseNodeColumnPropertyName))}))))))
      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
}