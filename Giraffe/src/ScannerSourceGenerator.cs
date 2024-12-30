using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Text.RegularExpressions;

namespace Giraffe;

public class ScannerSourceGenerator(Grammar grammar) : SourceGenerator(grammar) {
  private const string TokenDefDictFieldName = "tokenDef";
  private const string TerminalTypeEnumName = "TokenType";

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
      .WithMembers(List<MemberDeclarationSyntax>([GenerateTokenTypeDeclaration(),
                                                  GenerateTokenDefDictDeclaration()]));

  private EnumDeclarationSyntax GenerateTokenTypeDeclaration() =>
    EnumDeclaration(TerminalTypeEnumName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithMembers(SeparatedList<EnumMemberDeclarationSyntax>(GenerateTokenTypeMembers()));

  private IEnumerable<SyntaxNodeOrToken> GenerateTokenTypeMembers() =>
    GenerateCommaSeparatedList(grammar.Terminals, GenerateTokenTypeMember);

  private EnumMemberDeclarationSyntax GenerateTokenTypeMember(string terminal) =>
    EnumMemberDeclaration(Identifier(terminal));

  private MemberDeclarationSyntax GenerateTokenDefDictDeclaration() =>
    FieldDeclaration(VariableDeclaration(GenericName(Identifier("Dictionary"))
                                           .WithTypeArgumentList(
                                             TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] {
                                               IdentifierName(TerminalTypeEnumName),
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
                                    terminal => GenerateTokenDefEntry(terminal, grammar.GetTerminalRule(terminal))));

  private InitializerExpressionSyntax GenerateTokenDefEntry(string terminal, Regex regex) =>
    InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                          SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[] {
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                      IdentifierName(TerminalTypeEnumName),
                                                      IdentifierName(terminal)),
                            Token(SyntaxKind.CommaToken),
                            ImplicitObjectCreationExpression()
                                 .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                                     Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(regex.ToString())))))),
                          }));
}