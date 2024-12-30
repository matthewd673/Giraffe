using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Text.RegularExpressions;

namespace Giraffe;

public class ScannerSourceGenerator(Grammar grammar) : SourceGenerator(grammar) {
  private const string TokenDefDictFieldName = "tokenDef";
  private const string EofConstantName = "Eof";

  public string ScannerClassName { get; set; } = "Scanner";

  public override CompilationUnitSyntax Generate() => GenerateScannerFile();

  private CompilationUnitSyntax GenerateScannerFile() =>
    CompilationUnit().WithUsings(List<UsingDirectiveSyntax>([
                                   UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Collections")),
                                                    IdentifierName("Generic"))),
                                   UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Text")),
                                                    IdentifierName("RegularExpressions"))),
                                 ]))
                     .WithMembers(SingletonList<MemberDeclarationSyntax>(GenerateScannerClass()))
                     .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateScannerClass() =>
    ClassDeclaration(ScannerClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateTokenDefDictDeclaration(),
                                                  GenerateEofConstant()]));

  private MemberDeclarationSyntax GenerateTokenDefDictDeclaration() =>
    FieldDeclaration(VariableDeclaration(GenericName(Identifier("Dictionary"))
                                           .WithTypeArgumentList(
                                             TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] {
                                               PredefinedType(Token(SyntaxKind.IntKeyword)),
                                               Token(SyntaxKind.CommaToken),
                                               IdentifierName("Regex")}))))
                       .WithVariables(SingletonSeparatedList(
                                        VariableDeclarator(Identifier(TokenDefDictFieldName))
                                          .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                                          .WithInitializer(InitializerExpression(SyntaxKind.ComplexElementInitializerExpression,
                                             GenerateTokenDefEntries())))))))
       .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                                Token(SyntaxKind.ReadOnlyKeyword)));


  private SeparatedSyntaxList<ExpressionSyntax> GenerateTokenDefEntries() =>
    // Don't try to generate a rule for Eof, which has none
    SeparatedList<ExpressionSyntax>(GenerateCommaSeparatedList(grammar.Terminals.Where(t => !t.Equals(Grammar.Eof)),
                                    terminal => GenerateTokenDefEntry(grammar.Terminals.IndexOf(terminal), grammar.GetTerminalRule(terminal))));

  private InitializerExpressionSyntax GenerateTokenDefEntry(int terminal, Regex regex) =>
    InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                          SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[] {
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(terminal)),
                            Token(SyntaxKind.CommaToken),
                            ImplicitObjectCreationExpression()
                                 .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                                     Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(regex.ToString())))))),
                          }));

  private MemberDeclarationSyntax GenerateEofConstant() =>
    FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(EofConstantName))
                       .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(grammar.Terminals.Count - 1)))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                               Token(SyntaxKind.ConstKeyword)));
}