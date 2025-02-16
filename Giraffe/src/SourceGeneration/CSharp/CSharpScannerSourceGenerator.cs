using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpScannerSourceGenerator(Grammar grammar) : CSharpSourceGenerator {
  private const string TokenDefArrayFieldName = "tokenDef";
  private const string NamesArrayFieldName = "names";
  private const string InputFieldName = "input";
  private const string ScanIndexFieldName = "scanIndex";
  private const string NextTokenFieldName = "nextToken";

  public string ScannerClassName { get; set; } = "Scanner";
  public string TokenStructName { get; set; } = "Token";
  public string ScannerExceptionClassName { get; set; } = "ScannerException";
  public string TokenStructTypePropertyName { get; set; } = "Type";
  public string TokenStructImagePropertyName { get; set; } = "Image";
  public string NameOfMethodName { get; set; } = "NameOf";
  public string PeekMethodName { get; set; } = "Peek";
  public string EatMethodName { get; set; } = "Eat";
  public string ScanNextMethodName { get; set; } = "ScanNext";

  public override CompilationUnitSyntax Generate() => GenerateScannerFile();

  private CompilationUnitSyntax GenerateScannerFile() =>
    CompilationUnit()
      .WithUsings(List(GenerateUsings()))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateScannerClass()]))
      .NormalizeWhitespace();

  private static SyntaxList<UsingDirectiveSyntax> GenerateUsings() =>
    List<UsingDirectiveSyntax>([
      UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"),
                                                 IdentifierName("Text")),
                                                 IdentifierName("RegularExpressions")))]);

  private ClassDeclarationSyntax GenerateScannerClass() =>
    ClassDeclaration(ScannerClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateTokenDefArrayDeclaration(),
                                                  GenerateNamesArrayDeclaration(),
                                                  // Boilerplate...
                                                  ..GenerateFieldBoilerplate(),
                                                  GenerateConstructorBoilerplate(),
                                                  GenerateNameOfMethodBoilerplate(),
                                                  GeneratePeekMethodBoilerplate(),
                                                  GenerateEatMethodBoilerplate(),
                                                  GenerateScanNextMethodBoilerplate(),
                                                 ]));

  private FieldDeclarationSyntax GenerateTokenDefArrayDeclaration() =>
    FieldDeclaration(VariableDeclaration(ArrayType(IdentifierName("Regex"))
                                           .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<
                                                                               ExpressionSyntax>(OmittedArraySizeExpression())))))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(TokenDefArrayFieldName))
                                                               .WithInitializer(EqualsValueClause(CollectionExpression(SeparatedList(GenerateTokenDefElements())))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));


  private SeparatedSyntaxList<CollectionElementSyntax> GenerateTokenDefElements() =>
    // Don't try to generate a rule for Eof, which has none
    SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(grammar.Terminals.Where(t => !t.Equals(Grammar.Eof)),
                                                                      terminal =>
                                                                          GenerateTokenDefElement(grammar
                                                                              .GetTerminalRule(terminal))));

  private ExpressionElementSyntax GenerateTokenDefElement(Regex regex) =>
    ExpressionElement(ImplicitObjectCreationExpression()
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                                                Literal(regex.ToString())))))));

  private FieldDeclarationSyntax GenerateNamesArrayDeclaration() =>
      FieldDeclaration(VariableDeclaration(ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                                               .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList
                                                                       <ExpressionSyntax>(OmittedArraySizeExpression())))))
                           .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(NamesArrayFieldName))
                                                                     .WithInitializer(EqualsValueClause(CollectionExpression(GenerateNamesElements()))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

  private SeparatedSyntaxList<CollectionElementSyntax> GenerateNamesElements() =>
    SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(grammar.Terminals, GenerateNamesElement));

  private ExpressionElementSyntax GenerateNamesElement(string name) =>
    ExpressionElement(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name)));

  private FieldDeclarationSyntax[] GenerateFieldBoilerplate() => [GenerateTextFieldBoilerplate(),
                                                                  GenerateScanIndexFieldBoilerplate(),
                                                                  GenerateNextTokenFieldBoilerplate(),
                                                                 ];

  private static FieldDeclarationSyntax GenerateTextFieldBoilerplate() =>
    FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(InputFieldName)))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

  private static FieldDeclarationSyntax GenerateScanIndexFieldBoilerplate() =>
    FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(ScanIndexFieldName))
                                                               .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                                                  Literal(0)))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

  private FieldDeclarationSyntax GenerateNextTokenFieldBoilerplate() =>
    FieldDeclaration(VariableDeclaration(NullableType(IdentifierName(TokenStructName)))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(NextTokenFieldName))
                                                               .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind
                                                                                  .NullLiteralExpression))))))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

  private GlobalStatementSyntax GenerateConstructorBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName(ScannerClassName), MissingToken(SyntaxKind.IdentifierToken))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(InputFieldName))
                                                                              .WithType(PredefinedType(Token(SyntaxKind
                                                                                .StringKeyword))))))
                    .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                             MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 ThisExpression(),
                                                                                 IdentifierName(InputFieldName)),
                                                                             IdentifierName(InputFieldName))))));

  private GlobalStatementSyntax GenerateNameOfMethodBoilerplate() =>
      GlobalStatement(LocalFunctionStatement(PredefinedType(Token(SyntaxKind.StringKeyword)),
                                             Identifier(NameOfMethodName))
                      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("terminal"))
                                                                                  .WithType(PredefinedType(Token(SyntaxKind
                                                                                      .IntKeyword))))))
                      .WithExpressionBody(ArrowExpressionClause(ElementAccessExpression(IdentifierName(NamesArrayFieldName))
                                                                    .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName("terminal")))))))
                      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

  private GlobalStatementSyntax GeneratePeekMethodBoilerplate() =>
      GlobalStatement(LocalFunctionStatement(IdentifierName(TokenStructName), Identifier(PeekMethodName))
                      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                      .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                                                               IdentifierName(NextTokenFieldName),
                                                                               InvocationExpression(IdentifierName(ScanNextMethodName)))),
                                      ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression,
                                                                                      IdentifierName(NextTokenFieldName)),
                                                                             IdentifierName("Value"))))));

  private GlobalStatementSyntax GenerateEatMethodBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName(TokenStructName), Identifier(EatMethodName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                                                             IdentifierName(NextTokenFieldName),
                                                                             InvocationExpression(IdentifierName(ScanNextMethodName)))),
                                    LocalDeclarationStatement(VariableDeclaration(IdentifierName(TokenStructName))
                                                                  .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("consumed"))
                                                                      .WithInitializer(EqualsValueClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, IdentifierName(NextTokenFieldName)), IdentifierName("Value"))))))),
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                             IdentifierName(NextTokenFieldName),
                                                                             InvocationExpression(IdentifierName(ScanNextMethodName)))),
                                    ReturnStatement(IdentifierName("consumed")))));

  private GlobalStatementSyntax GenerateScanNextMethodBoilerplate() =>
      GlobalStatement(LocalFunctionStatement(IdentifierName(TokenStructName), Identifier(ScanNextMethodName))
                      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                      .WithBody(Block(
                                      IfStatement(
                                                  BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression,
                                                                   IdentifierName(ScanIndexFieldName),
                                                                   MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                       IdentifierName(InputFieldName),
                                                                       IdentifierName("Length"))),
                                                  Block(SingletonList<
                                                            StatementSyntax>(ReturnStatement(ImplicitObjectCreationExpression()
                                                                                 .WithArgumentList(ArgumentList(SeparatedList
                                                                                 <
                                                                                     ArgumentSyntax>(new
                                                                                     SyntaxNodeOrToken
                                                                                     [] {
                                                                                         Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                             IdentifierName(TokenDefArrayFieldName),
                                                                                             IdentifierName("Length"))),
                                                                                         Token(SyntaxKind
                                                                                             .CommaToken),
                                                                                         Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                                                             Literal("")))
                                                                                     }))))))),
                                      LocalDeclarationStatement(VariableDeclaration(NullableType(IdentifierName(TokenStructName)))
                                                                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("best"))
                                                                        .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind
                                                                            .NullLiteralExpression)))))),
                                      ForStatement(
                                                   Block(
                                                         LocalDeclarationStatement(
                                                          VariableDeclaration(IdentifierName("Match"))
                                                              .WithVariables(SingletonSeparatedList(
                                                                              VariableDeclarator(Identifier("match"))
                                                                                  .WithInitializer(
                                                                                   EqualsValueClause(
                                                                                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                            ElementAccessExpression(IdentifierName(TokenDefArrayFieldName))
                                                                                                .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName("t"))))),
                                                                                            IdentifierName("Match")))
                                                                                        .WithArgumentList(ArgumentList(SeparatedList
                                                                                        <
                                                                                            ArgumentSyntax>(new
                                                                                            SyntaxNodeOrToken
                                                                                            [] {
                                                                                                Argument(IdentifierName(InputFieldName)),
                                                                                                Token(SyntaxKind
                                                                                                    .CommaToken),
                                                                                                Argument(IdentifierName(ScanIndexFieldName))
                                                                                            })))))))),
                                                         IfStatement(BinaryExpression(SyntaxKind.LogicalOrExpression,
                                                                     PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                                                                         MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             IdentifierName("match"),
                                                                             IdentifierName("Success"))),
                                                                     BinaryExpression(SyntaxKind.GreaterThanExpression,
                                                                         MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             IdentifierName("match"),
                                                                             IdentifierName("Index")),
                                                                         IdentifierName(ScanIndexFieldName))),
                                                                     Block(SingletonList<StatementSyntax>(ContinueStatement()))),
                                                         ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                                                                 IdentifierName("best"),
                                                                                 ImplicitObjectCreationExpression()
                                                                                     .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new
                                                                                         SyntaxNodeOrToken[] {
                                                                                             Argument(IdentifierName("t")),
                                                                                             Token(SyntaxKind
                                                                                                 .CommaToken),
                                                                                             Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                                 IdentifierName("match"),
                                                                                                 IdentifierName("Value")))
                                                                                         }))))),
                                                         IfStatement(BinaryExpression(SyntaxKind.GreaterThanExpression,
                                                                              MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                       IdentifierName("match"),
                                                                                       IdentifierName("Length")),
                                                                              MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                       MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                                         IdentifierName("best"),
                                                                                                         IdentifierName("Value")),
                                                                                                IdentifierName(TokenStructImagePropertyName)),
                                                                                       IdentifierName("Length"))),
                                                                     Block(SingletonList<StatementSyntax>(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                                                      IdentifierName("best"),
                                                                                                      ImplicitObjectCreationExpression()
                                                                                                          .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new
                                                                                                                                         SyntaxNodeOrToken[] {
                                                                                                                                             Argument(IdentifierName("t")),
                                                                                                                                             Token(SyntaxKind
                                                                                                                                                          .CommaToken),
                                                                                                                                             Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                                                                                               IdentifierName("match"),
                                                                                                                                                               IdentifierName("Value"))),
                                                                                                                                         }))))))))))
                                          .WithDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                                                               .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("t"))
                                                                                  .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                                                      Literal(0)))))))
                                          .WithCondition(BinaryExpression(SyntaxKind.LessThanExpression,
                                                                          IdentifierName("t"),
                                                                          MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                              IdentifierName(TokenDefArrayFieldName),
                                                                              IdentifierName("Length"))))
                                          .WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(PostfixUnaryExpression(SyntaxKind.PostIncrementExpression,
                                                                IdentifierName("t")))),
                                      IfStatement(PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                                                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 IdentifierName("best"),
                                                                                 IdentifierName("HasValue"))),
                                                  Block(SingletonList<StatementSyntax>(GenerateExceptionThrowStatement(ScannerExceptionClassName,
                                                            GetIllegalCharacterExceptionMessage(ElementAccessExpression(IdentifierName("input"))
                                                                .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName(ScanIndexFieldName))))),
                                                                 IdentifierName(ScanIndexFieldName)))))),
                                      ExpressionStatement(AssignmentExpression(SyntaxKind.AddAssignmentExpression,
                                                                               IdentifierName(ScanIndexFieldName),
                                                                               MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                   MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                       MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                           IdentifierName("best"),
                                                                                           IdentifierName("Value")),
                                                                                       IdentifierName(TokenStructImagePropertyName)),
                                                                                   IdentifierName("Length")))),
                                      ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             IdentifierName("best"),
                                                                             IdentifierName("Value"))))));

  private static InterpolatedStringExpressionSyntax GetIllegalCharacterExceptionMessage(ExpressionSyntax characterExpression, ExpressionSyntax indexExpression) =>
      InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken))
          .WithContents(List(new InterpolatedStringContentSyntax[] {
              InterpolatedStringText()
                  .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, "Illegal character '", "Illegal character '", TriviaList())),
              Interpolation(characterExpression),
              InterpolatedStringText()
                  .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, "' at index ", "' at index ", TriviaList())),
              Interpolation(indexExpression),
          }));
}