using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe;

public class ParserSourceGenerator(Grammar grammar) : SourceGenerator {
  private const string ProductionsArrayFieldName = "productions";
  private const string ParseTableFieldName = "parseTable";

  public string FileNamespace { get; set; } = "Giraffe";
  public string ParserClassName { get; set; } = "Parser";

  private readonly ParseTable parseTable = grammar.BuildParseTable();

  public override CompilationUnitSyntax Generate() => GenerateParserFile();

  private CompilationUnitSyntax GenerateParserFile() =>
    CompilationUnit().WithUsings(List<UsingDirectiveSyntax>([
                                   UsingDirective(QualifiedName(QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Collections")),
                                                    IdentifierName("Generic")))]))
                     .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                                 GenerateParserClass()]))
                     .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateParserClass() =>
    ClassDeclaration(ParserClassName).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                     .WithMembers(List<MemberDeclarationSyntax>([
                                                    GenerateProductionsListDeclaration(),
                                                    GenerateTableDeclaration()]));

  private MemberDeclarationSyntax GenerateProductionsListDeclaration() =>
    FieldDeclaration(VariableDeclaration(ArrayType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                                           .WithRankSpecifiers(List([ArrayRankSpecifier(SingletonSeparatedList<
                                                                        ExpressionSyntax>(OmittedArraySizeExpression())),
                                                                      ArrayRankSpecifier(SingletonSeparatedList<
                                                                        ExpressionSyntax>(OmittedArraySizeExpression())),
                                                                    ])))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(ProductionsArrayFieldName))
                                                               .WithInitializer(EqualsValueClause(CollectionExpression(GenerateProductionsElements()))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

  private SeparatedSyntaxList<CollectionElementSyntax> GenerateProductionsElements() =>
     SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(grammar.Productions,
                                                                       GenerateProductionElement));

  private ExpressionElementSyntax GenerateProductionElement(Production production) =>
    ExpressionElement(CollectionExpression(SeparatedList<CollectionElementSyntax>(
                                             GenerateCommaSeparatedList(production,
                                                                        GenerateProductionElementLiteral))));

  private ExpressionElementSyntax GenerateProductionElementLiteral(string name) =>
    ExpressionElement(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                        Literal(Grammar.IsTerminal(name)
                                                  ? GetTerminalIndex(grammar
                                                      .Terminals
                                                      .IndexOf(name))
                                                  : GetNonterminalIndex(grammar
                                                      .Nonterminals
                                                      .IndexOf(name)))));

  private MemberDeclarationSyntax GenerateTableDeclaration() =>
    FieldDeclaration(VariableDeclaration(GenericName(Identifier("Dictionary"))
                                           .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] {
                                              TupleType(SeparatedList<TupleElementSyntax>(new SyntaxNodeOrToken[] {
                                                   TupleElement(PredefinedType(Token(SyntaxKind.IntKeyword))),
                                                   Token(SyntaxKind.CommaToken),
                                                   TupleElement(PredefinedType(Token(SyntaxKind.IntKeyword)))})),
                                              Token(SyntaxKind.CommaToken),
                                              PredefinedType(Token(SyntaxKind.IntKeyword))}))))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(ParseTableFieldName))
                                        .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                                          .WithInitializer(InitializerExpression(SyntaxKind.ComplexElementInitializerExpression,
                                            GenerateTableEntries())))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                               Token(SyntaxKind.ReadOnlyKeyword)));

  // TODO: This code assumes the grammar has look-ahead = 1
  // In table entries, we refer to nonterminals by (index + 1) and terminals
  // by -(index + 1). This way they can be differentiated with no ambiguity.
  private SeparatedSyntaxList<ExpressionSyntax> GenerateTableEntries() =>
    SeparatedList<ExpressionSyntax>(GenerateCommaSeparatedList(parseTable.Keys,
                                    key => GenerateTableEntry(grammar.Nonterminals.IndexOf(key.Nonterminal),
                                                              grammar.Terminals.IndexOf(key.Terminal),
                                                              parseTable[key][0])));

  private InitializerExpressionSyntax GenerateTableEntry(int nonterminal, int terminal, int state) =>
    InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                          SeparatedList<ExpressionSyntax>(new
                            SyntaxNodeOrToken[] {
                              TupleExpression(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] {
                                   Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                               Literal(nonterminal))),
                                   Token(SyntaxKind.CommaToken),
                                   Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                               Literal(terminal))),
                                 })),
                              Token(SyntaxKind.CommaToken),
                              LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                   Literal(state))}));

  private int GetNonterminalIndex(int index) =>
    index == -1 ? throw new("Index for nonterminal is -1") : index + 1;

  private int GetTerminalIndex(int index) =>
    index == -1 ? throw new("Index for terminal is -1") : -(index + 1);
}