using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Token = Giraffe.Frontend.Token;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpNonterminalSourceGenerator : CSharpSourceGenerator {
  public required string ParseNodeRecordName { get; init; }
  public required string ParseNodeIndexPropertyName { get; init; }
  public required string ParseNodeRowPropertyName { get; init; }
  public required string ParseNodeColumnPropertyName { get; init; }
  public required string NonterminalRecordName { get; init; }
  public required string NonterminalKindEnumName { get; init; }
  public required string KindPropertyName { get; init; }
  public required string ChildrenPropertyName { get; init; }

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateNonterminalRecord()]))
      .NormalizeWhitespace();

  private RecordDeclarationSyntax GenerateNonterminalRecord() =>
    RecordDeclaration(SyntaxKind.RecordDeclaration, Token(SyntaxKind.RecordKeyword), Identifier(NonterminalRecordName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
        Parameter(Identifier(KindPropertyName))
          .WithType(IdentifierName(NonterminalKindEnumName)),
        Token(SyntaxKind.CommaToken),
        Parameter(Identifier(ChildrenPropertyName))
          .WithType(ArrayType(IdentifierName(ParseNodeRecordName))
                      .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
                                                                            OmittedArraySizeExpression()))))),
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