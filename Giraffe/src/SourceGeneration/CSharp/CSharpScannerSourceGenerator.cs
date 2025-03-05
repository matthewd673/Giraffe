using System.Text.RegularExpressions;
using Giraffe.GIR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpScannerSourceGenerator(Grammar grammar) : CSharpSourceGenerator {
  public required string ScannerClassName { get; init; }
  public required string ScannerExceptionClassName { get; init; }
  public required string TokenRecordName { get; init; }
  public required string TokenKindEnumName { get; init; }
  public required string TokenImagePropertyName { get; init; }
  public required string TokenKindPropertyName { get; init; }
  public required string NameOfMethodName { get; init; }
  public required string PeekMethodName { get; init; }
  public required string EatMethodName { get; init; }

  public required List<Terminal> TerminalsOrdering { get; init; }

  private const string TokenDefArrayFieldName = "tokenDef";
  private const string NamesArrayFieldName = "names";
  private const string IgnoredArrayFieldName = "ignored";
  private const string InputFieldName = "input";
  private const string ScanIndexFieldName = "scanIndex";
  private const string NextTokenFieldName = "nextToken";
  private const string SkipIgnoredMethodName = "SkipIgnored";
  private const string ScanNextMethodName = "ScanNext";

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
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(InputFieldName))
                                                                .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))))))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateTokenDefArrayDeclaration(),
                                                  GenerateNamesArrayDeclaration(),
                                                  GenerateIgnoredArrayDeclaration(),
                                                  // Boilerplate...
                                                  GenerateScanIndexFieldBoilerplate(),
                                                  GenerateNextTokenFieldBoilerplate(),
                                                  GenerateNameOfMethodBoilerplate(),
                                                  GeneratePeekMethodBoilerplate(),
                                                  GenerateEatMethodBoilerplate(),
                                                  GenerateSkipIgnoredMethodBoilerplate(),
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
    SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(TerminalsOrdering.Where(t => !t.Equals(Grammar.Eof)),
                                                                      terminal =>
                                                                          GenerateTokenDefElement(grammar
                                                                              .GetTerminalDefinition(terminal).Regex)));

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
    SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(TerminalsOrdering, GenerateNamesElement));

  private ExpressionElementSyntax GenerateNamesElement(Terminal name) =>
    ExpressionElement(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                        Literal(GetDisplayName(grammar, name))));

  private FieldDeclarationSyntax GenerateIgnoredArrayDeclaration() =>
      FieldDeclaration(VariableDeclaration(ArrayType(IdentifierName(TokenKindEnumName))
                                               .WithRankSpecifiers(SingletonList(
                                                                    ArrayRankSpecifier(
                                                                     SingletonSeparatedList<ExpressionSyntax>(
                                                                      OmittedArraySizeExpression())))))
                           .WithVariables(SingletonSeparatedList(VariableDeclarator(IgnoredArrayFieldName)
                                                                     .WithInitializer(EqualsValueClause(GenerateIgnoredCollection())))))
          .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

  private CollectionExpressionSyntax GenerateIgnoredCollection() =>
      CollectionExpression()
          .WithElements(SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(
                                                                grammar.Terminals
                                                                       .Where(t => grammar.GetTerminalDefinition(t).Ignore),
                                                                t => ExpressionElement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName(TokenKindEnumName),
                                                                        IdentifierName(t.Value))))));

  private static FieldDeclarationSyntax GenerateScanIndexFieldBoilerplate() =>
      FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                           .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(ScanIndexFieldName)))))
          .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

  private FieldDeclarationSyntax GenerateNextTokenFieldBoilerplate() =>
      FieldDeclaration(VariableDeclaration(NullableType(IdentifierName(TokenRecordName)))
                           .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(NextTokenFieldName)))))
          .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

  private MethodDeclarationSyntax GenerateNameOfMethodBoilerplate() =>
    MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier(NameOfMethodName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("terminal"))
                                                                              .WithType(IdentifierName(TokenKindEnumName)))))
                    .WithExpressionBody(ArrowExpressionClause(ElementAccessExpression(IdentifierName(NamesArrayFieldName))
                                                                .WithArgumentList(BracketedArgumentList(
                                                                 SingletonSeparatedList(Argument(CastExpression(PredefinedType(Token(SyntaxKind.IntKeyword)),
                                                                   IdentifierName("terminal"))))))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

  private GlobalStatementSyntax GeneratePeekMethodBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName(TokenRecordName), Identifier(PeekMethodName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                                                             IdentifierName(NextTokenFieldName),
                                                                             InvocationExpression(IdentifierName(SkipIgnoredMethodName)))),
                                    ReturnStatement(IdentifierName(NextTokenFieldName)))));

  private GlobalStatementSyntax GenerateEatMethodBoilerplate() =>
    GlobalStatement(LocalFunctionStatement(IdentifierName(TokenRecordName), Identifier(EatMethodName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                                                             IdentifierName(NextTokenFieldName),
                                                                             InvocationExpression(IdentifierName(SkipIgnoredMethodName)))),
                                    LocalDeclarationStatement(VariableDeclaration(IdentifierName(TokenRecordName))
                                                                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("consumed"))
                                                                                 .WithInitializer(EqualsValueClause(IdentifierName(NextTokenFieldName)))))),
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                             IdentifierName(NextTokenFieldName),
                                                                             InvocationExpression(IdentifierName(SkipIgnoredMethodName)))),
                                    ReturnStatement(IdentifierName("consumed")))));

  private MethodDeclarationSyntax GenerateSkipIgnoredMethodBoilerplate() =>
    MethodDeclaration(IdentifierName(TokenRecordName), Identifier(SkipIgnoredMethodName))
        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
        .WithBody(Block(LocalDeclarationStatement(VariableDeclaration(IdentifierName(TokenRecordName))
                                                      .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("next"))))),
                        DoStatement(Block(SingletonList<StatementSyntax>(ExpressionStatement(
                                                                          AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                              IdentifierName("next"),
                                                                              InvocationExpression(IdentifierName(ScanNextMethodName)))))),
                                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                             IdentifierName(IgnoredArrayFieldName),
                                                             IdentifierName("Contains")))
                                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                                                        Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName("next"),
                                                                            IdentifierName(TokenKindPropertyName))))))),
                        ReturnStatement(IdentifierName("next"))));

  private GlobalStatementSyntax GenerateScanNextMethodBoilerplate() =>
      GlobalStatement(LocalFunctionStatement(IdentifierName(TokenRecordName), Identifier(ScanNextMethodName))
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
                                                                                             IdentifierName(TokenKindEnumName),
                                                                                             IdentifierName(Grammar.Eof.Value))),
                                                                                         Token(SyntaxKind
                                                                                             .CommaToken),
                                                                                         Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                                                             Literal("")))
                                                                                     }))))))),
                                      LocalDeclarationStatement(VariableDeclaration(NullableType(IdentifierName(TokenRecordName)))
                                                                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("best"))
                                                                        .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind
                                                                            .NullLiteralExpression)))))),
                                      ForStatement(
                                                   Block(
                                                         LocalDeclarationStatement(
                                                          VariableDeclaration(IdentifierName("Match"))
                                                              .WithVariables(
                                                                             SingletonSeparatedList(
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
                                                         IfStatement(BinaryExpression(SyntaxKind.LogicalOrExpression, PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("match"), IdentifierName("Success"))), BinaryExpression(SyntaxKind.GreaterThanExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("match"), IdentifierName("Index")), IdentifierName(ScanIndexFieldName))),
                                                                     Block(SingletonList<
                                                                               StatementSyntax>(ContinueStatement()))),
                                                         ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                                                                                 IdentifierName("best"),
                                                                                 ImplicitObjectCreationExpression()
                                                                                     .WithArgumentList(ArgumentList(SeparatedList
                                                                                     <
                                                                                         ArgumentSyntax>(new
                                                                                         SyntaxNodeOrToken
                                                                                         [] {
                                                                                             Argument(CastExpression(IdentifierName(TokenKindEnumName),
                                                                                                 IdentifierName("t"))),
                                                                                             Token(SyntaxKind
                                                                                                 .CommaToken),
                                                                                             Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                                 IdentifierName("match"),
                                                                                                 IdentifierName("Value")))
                                                                                         }))))),
                                                         IfStatement(
                                                                     BinaryExpression(SyntaxKind.GreaterThanExpression,
                                                                         MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             IdentifierName("match"),
                                                                             IdentifierName("Length")),
                                                                         MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 IdentifierName("best"),
                                                                                 IdentifierName(TokenImagePropertyName)),
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
                                                                                           Argument(CastExpression(IdentifierName(TokenKindEnumName),
                                                                                               IdentifierName("t"))),
                                                                                           Token(SyntaxKind
                                                                                               .CommaToken),
                                                                                           Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                               IdentifierName("match"),
                                                                                               IdentifierName("Value")))
                                                                                       }))))))))))
                                          .WithDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind
                                                                                   .IntKeyword)))
                                                               .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("t"))
                                                                                  .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                                                      Literal(0)))))))
                                          .WithCondition(BinaryExpression(SyntaxKind.LessThanExpression,
                                                                          IdentifierName("t"),
                                                                          MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                              IdentifierName(TokenDefArrayFieldName),
                                                                              IdentifierName("Length"))))
                                          .WithIncrementors(SingletonSeparatedList<
                                                                ExpressionSyntax>(PostfixUnaryExpression(SyntaxKind.PostIncrementExpression,
                                                                IdentifierName("t")))),
                                      IfStatement(IsPatternExpression(IdentifierName("best"), ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                                                  Block(SingletonList<
                                                            StatementSyntax>(GenerateExceptionThrowStatement(ScannerExceptionClassName,
                                                                                 GetIllegalCharacterExceptionMessage(ElementAccessExpression(IdentifierName(InputFieldName)).WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName(ScanIndexFieldName))))),
                                                                                     IdentifierName(ScanIndexFieldName)))))),
                                      ExpressionStatement(AssignmentExpression(SyntaxKind.AddAssignmentExpression,
                                                                               IdentifierName(ScanIndexFieldName),
                                                                               MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                   MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                       IdentifierName("best"),
                                                                                       IdentifierName(TokenImagePropertyName)),
                                                                                   IdentifierName("Length")))),
                                      ReturnStatement(IdentifierName("best")))));


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