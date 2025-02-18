using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpParseTreeSourceGenerator : CSharpSourceGenerator {
  public required string ParseNodeRecordName { get; init; }
  public required string ParseTreeRecordName { get; init; }
  public required string ChildrenPropertyName { get; init; }

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateParseTreeRecord()]))
      .NormalizeWhitespace();

  private RecordDeclarationSyntax GenerateParseTreeRecord() =>
    RecordDeclaration(SyntaxKind.RecordDeclaration, Token(SyntaxKind.RecordKeyword), Identifier(ParseTreeRecordName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(ChildrenPropertyName))
                                                                .WithType(ArrayType(IdentifierName(ParseNodeRecordName))
                                                                            .WithRankSpecifiers(SingletonList(
                                                                             ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
                                                                              OmittedArraySizeExpression()))))))))
      .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(ParseNodeRecordName)))))
      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
}