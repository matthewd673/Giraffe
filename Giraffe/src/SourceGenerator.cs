using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe;

public class SourceGenerator(ParseTable parseTable) {
   private readonly ParseTable parseTable = parseTable;

   public string Generate() {
      return GenerateTable().NormalizeWhitespace().ToString();
   }

   private CompilationUnitSyntax GenerateTable() {
      return CompilationUnit()
         .WithMembers(
                      SingletonList<MemberDeclarationSyntax>(
                       FieldDeclaration(
                                        VariableDeclaration(
                                            GenericName(
                                                Identifier("Dictionary"))
                                               .WithTypeArgumentList(
                                                TypeArgumentList(
                                                 SeparatedList<
                                                    TypeSyntax>(
                                                  new
                                                     SyntaxNodeOrToken
                                                     [] {
                                                        TupleType(
                                                         SeparatedList
                                                         <
                                                            TupleElementSyntax>(
                                                          new
                                                             SyntaxNodeOrToken
                                                             [] {
                                                                TupleElement(
                                                                 PredefinedType(
                                                                  Token(SyntaxKind
                                                                     .StringKeyword))),
                                                                Token(SyntaxKind
                                                                   .CommaToken),
                                                                TupleElement(
                                                                 PredefinedType(
                                                                  Token(SyntaxKind
                                                                     .StringKeyword))),
                                                             })),
                                                        Token(SyntaxKind
                                                           .CommaToken),
                                                        PredefinedType(
                                                         Token(SyntaxKind
                                                            .IntKeyword)),
                                                     }))))
                                           .WithVariables(
                                            SingletonSeparatedList(
                                             VariableDeclarator(
                                                 Identifier("parseTable"))
                                                .WithInitializer(
                                                 EqualsValueClause(
                                                  GenerateTableEntries())))))
                          .WithModifiers(
                                         TokenList(Token(SyntaxKind
                                                      .PrivateKeyword),
                                                   Token(SyntaxKind
                                                      .ConstKeyword)))));
   }

   private ExpressionSyntax GenerateTableEntries() {
      List<SyntaxNodeOrToken> entries = [];
      foreach ((string, string) key in parseTable.Keys) {
         // NOTE: This assumes the grammar is LR(1)
         entries.AddRange(GenerateTableEntry(key.Item1, key.Item2, parseTable[key][0]));
      }

      return InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression,
                            SeparatedList<ExpressionSyntax>(entries));
   }

   private List<SyntaxNodeOrToken> GenerateTableEntry(string nonterminal,
                                                      string terminal,
                                                      int state) => [
      InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression,
                            SeparatedList<ExpressionSyntax>(
                             new SyntaxNodeOrToken[] {
                                TupleExpression(
                                                SeparatedList<
                                                   ArgumentSyntax>(
                                                 new SyntaxNodeOrToken[] {
                                                    Argument(
                                                     LiteralExpression(
                                                      SyntaxKind
                                                         .StringLiteralExpression,
                                                      Literal(nonterminal))),
                                                    Token(SyntaxKind
                                                       .CommaToken),
                                                    Argument(
                                                     LiteralExpression(
                                                      SyntaxKind
                                                         .StringLiteralExpression,
                                                      Literal(terminal)))
                                                 })),
                                Token(SyntaxKind.CommaToken),
                                LiteralExpression(
                                                  SyntaxKind
                                                     .NumericLiteralExpression,
                                                  Literal(state))
                             })),
      Token(SyntaxKind.CommaToken),
   ];
}