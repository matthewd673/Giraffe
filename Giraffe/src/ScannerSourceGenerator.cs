using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Text.RegularExpressions;

namespace Giraffe;

public class ScannerSourceGenerator(Grammar grammar) : SourceGenerator {
  private const string TokenDefDictFieldName = "tokenDef";
  private const string EofConstantName = "Eof";

  public string ScannerClassName { get; set; } = "Scanner";

  public override CompilationUnitSyntax Generate() => GenerateScannerFile();

  private CompilationUnitSyntax GenerateScannerFile() =>
    CompilationUnit()
      .WithUsings(List(GenerateUsings()))
      .WithMembers(SingletonList<MemberDeclarationSyntax>(GenerateScannerClass()))
      .NormalizeWhitespace();

  private static SyntaxList<UsingDirectiveSyntax> GenerateUsings() =>
    List<UsingDirectiveSyntax>([
      UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"),
                                                 IdentifierName("Text")),
                                                 IdentifierName("RegularExpressions")))]);

  private ClassDeclarationSyntax GenerateScannerClass() =>
    ClassDeclaration(ScannerClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateTokenDefDictDeclaration(),
                                                  GenerateEofConstant(),
                                                  // Boilerplate...
                                                  GenerateTokenStructBoilerplate(),
                                                  GenerateScannerExceptionBoilerplate(),
                                                  ..GenerateFieldBoilerplate(),
                                                  GenerateConstructorBoilerplate(),
                                                  GeneratePeekMethodBoilerplate(),
                                                  GenerateEatMethodBoilerplate(),
                                                  GenerateScanNextMethodBoilerplate(),
                                                 ]));

  private FieldDeclarationSyntax GenerateTokenDefDictDeclaration() =>
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

  private FieldDeclarationSyntax GenerateEofConstant() =>
    FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(EofConstantName))
                       .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(grammar.Terminals.Count - 1)))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                               Token(SyntaxKind.ConstKeyword)));

  private static StructDeclarationSyntax GenerateTokenStructBoilerplate() =>
    StructDeclaration("Token")
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)))
      .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
        Parameter(Identifier(TriviaList(), SyntaxKind.TypeKeyword, "type", "type", TriviaList()))
          .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
        Token(SyntaxKind.CommaToken),
        Parameter(Identifier("image")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
      })))
      .WithMembers(List(new MemberDeclarationSyntax[] {
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("Type"))
          .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
          .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                         .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
          .WithInitializer(EqualsValueClause(IdentifierName(Identifier(TriviaList(), SyntaxKind.TypeKeyword, "type",
                                                                       "type", TriviaList()))))
          .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("Image"))
          .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
          .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                         .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
          .WithInitializer(EqualsValueClause(IdentifierName("image")))
          .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
      }));

  private static ClassDeclarationSyntax GenerateScannerExceptionBoilerplate() =>
    ClassDeclaration("ScannerException")
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
        Parameter(Identifier("index")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
        Token(SyntaxKind.CommaToken),
        Parameter(Identifier("message")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
      })))
      .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(PrimaryConstructorBaseType(IdentifierName("Exception"))
                                                                      .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("message"))))))))
      .WithMembers(SingletonList<MemberDeclarationSyntax>(PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)),
                                                                              Identifier("Index"))
                                                          .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                                          .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind
                                                                                .GetAccessorDeclaration)
                                                                              .WithSemicolonToken(Token(SyntaxKind
                                                                                .SemicolonToken)))))
                                                          .WithInitializer(EqualsValueClause(IdentifierName("index")))
                                                          .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))));

  private static FieldDeclarationSyntax[] GenerateFieldBoilerplate() => [
                                                                          GenerateTextFieldBoilerplate(),
                                                                          GenerateScanIndexFieldBoilerplate(),
                                                                          GenerateNextTokenFieldBoilerplate(),
                                                                        ];

  private static FieldDeclarationSyntax GenerateTextFieldBoilerplate() =>
    FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("text")))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

  private static FieldDeclarationSyntax GenerateScanIndexFieldBoilerplate() =>
    FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("scanIndex"))
                                                               .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                                                  Literal(0)))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

  private static FieldDeclarationSyntax GenerateNextTokenFieldBoilerplate() =>
    FieldDeclaration(VariableDeclaration(NullableType(IdentifierName("Token")))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("nextToken"))
                                                               .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind
                                                                                  .NullLiteralExpression))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

  private static GlobalStatementSyntax GenerateConstructorBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName("ManualScanner"), MissingToken(SyntaxKind.IdentifierToken))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("text"))
                                                                              .WithType(PredefinedType(Token(SyntaxKind
                                                                                .StringKeyword))))))
                    .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName("text")), IdentifierName("text"))),
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                             IdentifierName("nextToken"),
                                                                             InvocationExpression(IdentifierName("ScanNext")))))));

  private static GlobalStatementSyntax GeneratePeekMethodBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName("Token"), Identifier("Peek"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression,
                                                                  IdentifierName("nextToken")),
                                                                IdentifierName("Value"))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

  private static GlobalStatementSyntax GenerateEatMethodBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName("Token"), Identifier("Eat"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(LocalDeclarationStatement(VariableDeclaration(IdentifierName("Token")).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("consumed")).WithInitializer(EqualsValueClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, IdentifierName("nextToken")), IdentifierName("Value"))))))),
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                             IdentifierName("nextToken"),
                                                                             InvocationExpression(IdentifierName("ScanNext")))),
                                    ReturnStatement(IdentifierName("consumed")))));

  private static GlobalStatementSyntax GenerateScanNextMethodBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName("Token"), Identifier("ScanNext"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                    .WithBody(Block(
                                    IfStatement(BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, IdentifierName("scanIndex"), MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("text"), IdentifierName("Length"))),
                                                Block(SingletonList<
                                                        StatementSyntax>(ReturnStatement(ImplicitObjectCreationExpression()
                                                                           .WithArgumentList(ArgumentList(SeparatedList
                                                                           <
                                                                             ArgumentSyntax>(new
                                                                             SyntaxNodeOrToken
                                                                             [] {
                                                                               Argument(IdentifierName("Eof")),
                                                                               Token(SyntaxKind
                                                                                 .CommaToken),
                                                                               Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                                                 Literal("")))
                                                                             }))))))),
                                    LocalDeclarationStatement(VariableDeclaration(NullableType(IdentifierName("Token")))
                                                                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("best"))
                                                                                 .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind
                                                                                   .NullLiteralExpression)))))),
                                    ForEachStatement(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("t"),
                                                     MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName("tokenDef"),
                                                                            IdentifierName("Keys")),
                                                     Block(
                                                           LocalDeclarationStatement(
                                                            VariableDeclaration(IdentifierName("Match"))
                                                              .WithVariables(SingletonSeparatedList(
                                                                              VariableDeclarator(Identifier("match"))
                                                                                .WithInitializer(EqualsValueClause(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                    ElementAccessExpression(IdentifierName("tokenDef"))
                                                                                      .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName("t"))))),
                                                                                    IdentifierName("Match")))
                                                                                  .WithArgumentList(ArgumentList(SeparatedList
                                                                                  <
                                                                                    ArgumentSyntax>(new
                                                                                    SyntaxNodeOrToken
                                                                                    [] {
                                                                                      Argument(IdentifierName("text")),
                                                                                      Token(SyntaxKind
                                                                                        .CommaToken),
                                                                                      Argument(IdentifierName("scanIndex"))
                                                                                    })))))))),
                                                           IfStatement(BinaryExpression(SyntaxKind.LogicalOrExpression, PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("match"), IdentifierName("Success"))), BinaryExpression(SyntaxKind.GreaterThanExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("match"), IdentifierName("Index")), IdentifierName("scanIndex"))),
                                                                       Block(SingletonList<
                                                                               StatementSyntax>(ContinueStatement()))),
                                                           IfStatement(
                                                                       PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                                                                         MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                           IdentifierName("best"),
                                                                           IdentifierName("HasValue"))),
                                                                       Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("best"), ImplicitObjectCreationExpression().WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] { Argument(IdentifierName("t")), Token(SyntaxKind.CommaToken), Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("match"), IdentifierName("Value"))) }))))),
                                                                             ContinueStatement())),
                                                           IfStatement(
                                                                       BinaryExpression(SyntaxKind.GreaterThanExpression,
                                                                         MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                           IdentifierName("match"),
                                                                           IdentifierName("Length")),
                                                                         MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                           MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                               IdentifierName("best"),
                                                                               IdentifierName("Value")),
                                                                             IdentifierName("Image")),
                                                                           IdentifierName("Length"))),
                                                                       Block(SingletonList<
                                                                               StatementSyntax>(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                               IdentifierName("best"),
                                                                               ImplicitObjectCreationExpression()
                                                                                 .WithArgumentList(ArgumentList(SeparatedList
                                                                                 <
                                                                                   ArgumentSyntax>(new
                                                                                   SyntaxNodeOrToken
                                                                                   [] {
                                                                                     Argument(IdentifierName("t")),
                                                                                     Token(SyntaxKind
                                                                                       .CommaToken),
                                                                                     Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                       IdentifierName("match"),
                                                                                       IdentifierName("Value")))
                                                                                   })))))))))),
                                    IfStatement(
                                                PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                                                                      MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName("best"),
                                                                        IdentifierName("HasValue"))),
                                                Block(
                                                      SingletonList<StatementSyntax>(
                                                       ThrowStatement(
                                                                      ObjectCreationExpression(IdentifierName("ScannerException"))
                                                                        .WithArgumentList(ArgumentList(SeparatedList<
                                                                          ArgumentSyntax>(new
                                                                          SyntaxNodeOrToken
                                                                          [] {
                                                                            Argument(IdentifierName("scanIndex")),
                                                                            Token(SyntaxKind
                                                                              .CommaToken),
                                                                            Argument(
                                                                             InterpolatedStringExpression(Token(SyntaxKind
                                                                                 .InterpolatedStringStartToken))
                                                                               .WithContents(List<
                                                                                 InterpolatedStringContentSyntax>([
                                                                                 InterpolatedStringText()
                                                                                   .WithTextToken(Token(TriviaList(),
                                                                                     SyntaxKind
                                                                                       .InterpolatedStringTextToken,
                                                                                     "Illegal character: '",
                                                                                     "Illegal character: '",
                                                                                     TriviaList())),
                                                                                 Interpolation(ElementAccessExpression(IdentifierName("text"))
                                                                                   .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName("scanIndex")))))),
                                                                                 InterpolatedStringText()
                                                                                   .WithTextToken(Token(TriviaList(),
                                                                                     SyntaxKind
                                                                                       .InterpolatedStringTextToken,
                                                                                     "'",
                                                                                     "'",
                                                                                     TriviaList())),
                                                                               ])))
                                                                          }))))))),
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.AddAssignmentExpression,
                                                                             IdentifierName("scanIndex"),
                                                                             MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                               MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                   IdentifierName("best"),
                                                                                   IdentifierName("Value")),
                                                                                 IdentifierName("Image")),
                                                                               IdentifierName("Length")))),
                                    ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                           IdentifierName("best"),
                                                                           IdentifierName("Value"))))));
}