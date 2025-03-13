using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpParseNodeSourceGenerator : CSharpSourceGenerator {
  public required string ParseNodeRecordName { get; init; }
  public required string IndexPropertyName { get; init; }
  public required string RowPropertyName { get; init; }
  public required string ColumnPropertyName { get; init; }

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([
                                                   GenerateNamespaceDeclaration(FileNamespace),
                                                   GenerateParseNodeRecord()
                                                 ]))
      .NormalizeWhitespace();

  private RecordDeclarationSyntax GenerateParseNodeRecord() =>
    RecordDeclaration(SyntaxKind.RecordDeclaration, Token(SyntaxKind.RecordKeyword), Identifier(ParseNodeRecordName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
        Parameter(Identifier(IndexPropertyName))
          .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
        Token(SyntaxKind.CommaToken),
        Parameter(Identifier(RowPropertyName))
          .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
        Token(SyntaxKind.CommaToken),
        Parameter(Identifier(ColumnPropertyName))
          .WithType(PredefinedType(Token(SyntaxKind.IntKeyword)))
      })))
      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
}