using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpTokenSourceGenerator : CSharpSourceGenerator {
  public string TokenStructName { get; set; } = "Token";

  private const string TypePropertyName = "Type";
  private const string ImagePropertyName = "Image";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithUsings(List<UsingDirectiveSyntax>())
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateTokenStruct()]))
      .NormalizeWhitespace();

  private StructDeclarationSyntax GenerateTokenStruct() {
    const string typeParamName = "type";
    const string imageParamName = "image";

    return StructDeclaration(TokenStructName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)))
      .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
        Parameter(Identifier(TriviaList(), SyntaxKind.TypeKeyword, typeParamName, typeParamName, TriviaList()))
          .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
        Token(SyntaxKind.CommaToken),
        Parameter(Identifier(imageParamName)).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
      })))
      .WithMembers(List(new MemberDeclarationSyntax[] {
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier(TypePropertyName))
          .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
          .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                         .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
          .WithInitializer(EqualsValueClause(IdentifierName(Identifier(TriviaList(), SyntaxKind.TypeKeyword, typeParamName,
                                                                       typeParamName, TriviaList()))))
          .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier(ImagePropertyName))
          .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
          .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                         .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
          .WithInitializer(EqualsValueClause(IdentifierName(imageParamName)))
          .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
      }));
  }
}