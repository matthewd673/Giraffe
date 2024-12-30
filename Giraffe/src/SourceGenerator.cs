using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe;

public class SourceGenerator(Grammar grammar) {
   private const string TokenDefDictFieldName = "tokenDef";
   private const string TerminalTypeEnumName = "TokenType";
   private const string ProductionsListName = "productions";
   private const string ParseTableFieldName = "parseTable";

   public string ParserClassName { get; set; } = "Parser";

   private readonly ParseTable parseTable = grammar.BuildParseTable();

   public string Generate() =>
      GenerateParserFile().NormalizeWhitespace().ToString();

   private CompilationUnitSyntax GenerateParserFile() =>
      CompilationUnit().WithUsings(List<UsingDirectiveSyntax>([
                                      UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Collections")),
                                                        IdentifierName("Generic"))),
                                      UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Text")),
                                                        IdentifierName("RegularExpressions"))),
                                   ]))
                       .WithMembers(SingletonList<MemberDeclarationSyntax>(GenerateParserClass()))
                       .NormalizeWhitespace();

   private ClassDeclarationSyntax GenerateParserClass() =>
      ClassDeclaration(ParserClassName)
         .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
         .WithMembers(List<MemberDeclarationSyntax>([GenerateTokenTypeDeclaration(),
                                                     GenerateTokenDefDictDeclaration(),
                                                     GenerateProductionsListDeclaration(),
                                                     GenerateTableDeclaration(),
                                                    ]));

   /// <summary>
   /// This method generates the following:
   /// <code>public enum TokenType {members}</code>
   /// Where <c>members</c> is the members of the enum (the terminals in the grammar).
   /// </summary>
   /// <returns>An EnumDeclarationSyntax containing the terminals in the grammar as its members.</returns>
   private EnumDeclarationSyntax GenerateTokenTypeDeclaration() =>
      EnumDeclaration(TerminalTypeEnumName)
         .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
         .WithMembers(SeparatedList<
                         EnumMemberDeclarationSyntax>(GenerateTokenTypeMembers()));

   private IEnumerable<SyntaxNodeOrToken> GenerateTokenTypeMembers() =>
      GenerateCommaSeparatedList(grammar.Terminals, GenerateTokenTypeMember);

   /// <summary>
   /// This method generates an enum member for a given terminal name, e.g.
   /// <c>MyTerminalName,</c>
   /// </summary>
   /// <param name="terminal">The name of the terminal to use as the enum member name.</param>
   /// <returns>An EnumMember containing the terminal name as its identifier.</returns>
   private EnumMemberDeclarationSyntax GenerateTokenTypeMember(string terminal) =>
      EnumMemberDeclaration(Identifier(terminal));

   private MemberDeclarationSyntax GenerateTokenDefDictDeclaration() =>
      FieldDeclaration(VariableDeclaration(GenericName(Identifier("Dictionary"))
                                              .WithTypeArgumentList(TypeArgumentList(SeparatedList
                                                 <TypeSyntax>(new
                                                    SyntaxNodeOrToken
                                                    [] {
                                                       IdentifierName(TerminalTypeEnumName),
                                                       Token(SyntaxKind
                                                          .CommaToken),
                                                       IdentifierName("Regex")
                                                    }))))
                          .WithVariables(SingletonSeparatedList<
                                            VariableDeclaratorSyntax>(VariableDeclarator(Identifier(TokenDefDictFieldName))
                                            .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                                               .WithInitializer(InitializerExpression(SyntaxKind.ComplexElementInitializerExpression,
                                                  GenerateTokenDefEntries())))))))
         .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                                  Token(SyntaxKind.ReadOnlyKeyword)));

   private SeparatedSyntaxList<ExpressionSyntax> GenerateTokenDefEntries() =>
      // Don't try to generate a rule for Eof, which has none
      SeparatedList<ExpressionSyntax>(GenerateCommaSeparatedList(grammar.Terminals.Where(t => !t.Equals(Grammar.Eof)),
                                         terminal =>
                                            GenerateTokenDefEntry(terminal,
                                               grammar
                                                  .GetTerminalRule(terminal))));

   /// <summary>
   /// This method generates the following Dictionary entry:
   /// <code>
   ///   { terminal, new("pattern") }
   /// </code>
   /// Where <c>terminal</c> is the given terminal name and <c>pattern</c> is the
   /// pattern of the given Regex.
   /// </summary>
   /// <param name="terminal">The terminal name to use for the entry.</param>
   /// <param name="regex">The Regex from which to get the pattern used for the entry.</param>
   /// <returns>An InitializerExpressionSyntax representing the entry.</returns>
   private InitializerExpressionSyntax GenerateTokenDefEntry(string terminal, Regex regex) =>
      InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                            SeparatedList<ExpressionSyntax>(new
                               SyntaxNodeOrToken[] {
                                  MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                     IdentifierName(TerminalTypeEnumName),
                                     IdentifierName(terminal)),
                                  Token(SyntaxKind.CommaToken),
                                  ImplicitObjectCreationExpression()
                                     .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                        Literal(regex
                                                   .ToString())))))),
                               }));

   private MemberDeclarationSyntax GenerateProductionsListDeclaration() =>
      FieldDeclaration(VariableDeclaration(GenericName(Identifier("List"))
                                              .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList
                                              <
                                                 TypeSyntax>(GenericName(Identifier("List"))
                                                 .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList
                                                 <
                                                    TypeSyntax>(PredefinedType(Token(SyntaxKind
                                                    .IntKeyword)))))))))
                          .WithVariables(SingletonSeparatedList(
                                          VariableDeclarator(Identifier(ProductionsListName))
                                             .WithInitializer(EqualsValueClause(CollectionExpression(GenerateProductionsElements()))))))
         .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

   private SeparatedSyntaxList<CollectionElementSyntax> GenerateProductionsElements() =>
      SeparatedList<
         CollectionElementSyntax>(GenerateCommaSeparatedList(grammar.Productions,
                                     GenerateProductionElement));

   private ExpressionElementSyntax GenerateProductionElement(Production production) =>
      ExpressionElement(CollectionExpression(SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(production, GenerateProductionElementLiteral))));

   private ExpressionElementSyntax GenerateProductionElementLiteral(string name) =>
      ExpressionElement(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                          Literal(Grammar.IsTerminal(name)
                                                   ? GetTerminalIndex(grammar.Terminals.IndexOf(name))
                                                   : GetNonterminalIndex(grammar.Nonterminals.IndexOf(name)))));

   /// <summary>
   /// This method generates the following:
   /// <code>
   ///   private const Dictionary&lt;(int, int), int&gt; parseTable = new() {def}
   /// </code>
   /// Where <c>def</c> is the definition of the table. The key of the parse
   /// table is a tuple of the nonterminal index and the terminal index.
   /// </summary>
   /// <returns>A FieldDeclaration for the table.</returns>
   private MemberDeclarationSyntax GenerateTableDeclaration() =>
      FieldDeclaration(VariableDeclaration(GenericName(Identifier("Dictionary"))
                                              .WithTypeArgumentList(
                                               TypeArgumentList(SeparatedList
                                                  <TypeSyntax>(new
                                                     SyntaxNodeOrToken
                                                     [] {
                                                        TupleType(SeparatedList
                                                        <
                                                           TupleElementSyntax>(new
                                                           SyntaxNodeOrToken
                                                           [] {
                                                              TupleElement(PredefinedType(Token(SyntaxKind
                                                                 .IntKeyword))),
                                                              Token(SyntaxKind
                                                                 .CommaToken),
                                                              TupleElement(PredefinedType(Token(SyntaxKind
                                                                 .IntKeyword))),
                                                           })),
                                                        Token(SyntaxKind
                                                           .CommaToken),
                                                        PredefinedType(Token(SyntaxKind
                                                           .IntKeyword)),
                                                     }))))
                          .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(ParseTableFieldName))
                                            .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()
                                               .WithInitializer(InitializerExpression(SyntaxKind.ComplexElementInitializerExpression,
                                                  GenerateTableEntries())))))))
         .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                                  Token(SyntaxKind.ReadOnlyKeyword)));

   private SeparatedSyntaxList<ExpressionSyntax> GenerateTableEntries() =>
      // NOTE: This code assumes the grammar has look-ahead = 1
      // In table entries, we refer to nonterminals by (index + 1) and terminals
      // by -(index + 1). This way they can be differentiated with no ambiguity.
      SeparatedList<ExpressionSyntax>(GenerateCommaSeparatedList(parseTable.Keys,
                                         key =>
                                            GenerateTableEntry(grammar.Nonterminals.IndexOf(key.Nonterminal),
                                               grammar.Terminals.IndexOf(key.Terminal),
                                               parseTable[key][0])));

   /// <summary>
   /// The method generates a table entry of the following form:
   /// <code>
   ///   { (nonterminal, terminal), state }
   /// </code>
   /// Where <c>nonterminal</c>, <c>terminal</c>, and <c>state</c> are the
   /// arguments of the function.
   /// </summary>
   /// <param name="nonterminal">The nonterminal of the entry.</param>
   /// <param name="terminal">The terminal of the entry.</param>
   /// <param name="state">The state of the entry.</param>
   /// <returns>An InitializerExpression of the table entry.</returns>
   private InitializerExpressionSyntax GenerateTableEntry(int nonterminal,
                                                          int terminal,
                                                          int state) =>
      InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                            SeparatedList<ExpressionSyntax>(new
                               SyntaxNodeOrToken[] {
                                  TupleExpression(SeparatedList<
                                                     ArgumentSyntax>(new
                                                     SyntaxNodeOrToken[] {
                                                        Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                           Literal(nonterminal))),
                                                        Token(SyntaxKind
                                                           .CommaToken),
                                                        Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                           Literal(terminal))),
                                                     })),
                                  Token(SyntaxKind.CommaToken),
                                  LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                    Literal(state)),
                               }));


   private int GetNonterminalIndex(int index) =>
      index == -1
         ? throw new Exception("Index for nonterminal is -1")
         : index + 1;
   private int GetTerminalIndex(int index) =>
      index == -1
         ? throw new Exception("Index for terminal is -1")
         : -(index + 1);

   private delegate TOutput SyntaxTransformer<in TInput, out TOutput>(TInput input) where TOutput : SyntaxNode;

   private IEnumerable<SyntaxNodeOrToken> GenerateCommaSeparatedList<TInput, TOutput>(IEnumerable<TInput> collection,
                                                                                      SyntaxTransformer<TInput, TOutput> transformer)
      where TOutput : SyntaxNode =>
      collection.SelectMany<TInput, SyntaxNodeOrToken>(i => [transformer.Invoke(i), Token(SyntaxKind.CommaToken)]);
}