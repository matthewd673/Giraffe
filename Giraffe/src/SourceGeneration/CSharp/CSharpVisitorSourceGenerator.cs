using Giraffe.GIR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpVisitorSourceGenerator(List<Terminal> relevantTerminals,
                                          List<Nonterminal> relevantNonterminals) : CSharpSourceGenerator {
  public required string VisitorClassName { get; init; }
  public required string VisitMethodName { get; init; }
  public required string ParseTreeRecordName { get; init; }
  public required string ParseNodeRecordName { get; init; }
  public required string NonterminalRecordName { get; init; }
  public required string NonterminalKindPropertyName { get; init; }
  public required string NonterminalChildrenPropertyName { get; init; }
  public required string TokenRecordName { get; init; }
  public required string TokenKindPropertyName { get; init; }
  public required string NonterminalKindEnumName { get; init; }
  public required string TokenKindEnumName { get; init; }

  private const string GenericName = "T";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateVisitorInterface()]))
      .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateVisitorInterface() =>
    ClassDeclaration(VisitorClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)))
      .WithTypeParameterList(TypeParameterList(SingletonSeparatedList(TypeParameter(Identifier(GenericName)))))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateVisitMethodParseTreeOverload(),
                                                  GenerateVisitMethodParseNodeOverload(),
                                                  GenerateVisitMethodNonterminalOverload(),
                                                  GenerateVisitMethodTokenOverload(),
                                                  ..relevantNonterminals.Select(GenerateVisitNonterminalMethodStub),
                                                  ..relevantTerminals.Select(GenerateVisitTokenMethodStub),
                                                 ]));

  private MethodDeclarationSyntax GenerateVisitMethodParseTreeOverload() =>
    GenerateVisitMethod(ParseTreeRecordName, "parseTree")
      .AddModifiers(Token(SyntaxKind.AbstractKeyword));

  private MethodDeclarationSyntax GenerateVisitMethodParseNodeOverload() =>
    GenerateVisitMethod(ParseNodeRecordName, "parseNode")
      .WithExpressionBody(ArrowExpressionClause(SwitchExpression(IdentifierName("parseNode"))
                                                  .WithArms(SeparatedList<SwitchExpressionArmSyntax>(
                                                               new SyntaxNodeOrToken[] {
                                                                 SwitchExpressionArm(DeclarationPattern(
                                                                  IdentifierName(NonterminalRecordName),
                                                                  SingleVariableDesignation(Identifier("nt"))),
                                                                    InvocationExpression(IdentifierName(VisitMethodName))
                                                                      .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("nt")))))),
                                                                 Token(SyntaxKind.CommaToken),
                                                                 SwitchExpressionArm(DeclarationPattern(
                                                                  IdentifierName(TokenRecordName),
                                                                  SingleVariableDesignation(Identifier("t"))),
                                                                    InvocationExpression(IdentifierName(VisitMethodName))
                                                                      .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("t")))))),
                                                                 Token(SyntaxKind.CommaToken),
                                                                 GenerateDefaultThrowSwitchExpressionArm(),
                                                                 Token(SyntaxKind.CommaToken),
                                                               }))));

  private MethodDeclarationSyntax GenerateVisitMethodNonterminalOverload() =>
    GenerateVisitMethod(NonterminalRecordName, "nonterminal")
      .WithExpressionBody(ArrowExpressionClause(SwitchExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                      IdentifierName("nonterminal"),
                                                                      IdentifierName(NonterminalKindPropertyName)))
                                                  .WithArms(SeparatedList<SwitchExpressionArmSyntax>(
                                                             [..GenerateCommaSeparatedList(relevantNonterminals,
                                                               nt => GenerateNonterminalSwitchExpressionArm(nt, "nonterminal")),
                                                              ..relevantNonterminals.Count == 0 // No comma before default arm if there are no other arms
                                                                  ? new List<SyntaxNodeOrToken>()
                                                                  : [Token(SyntaxKind.CommaToken)],
                                                              GenerateDefaultThrowSwitchExpressionArm(),
                                                              Token(SyntaxKind.CommaToken),
                                                               ]))));

  private MethodDeclarationSyntax GenerateVisitMethodTokenOverload() =>
    GenerateVisitMethod(TokenRecordName, "token")
      .WithExpressionBody(ArrowExpressionClause(SwitchExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                      IdentifierName("token"),
                                                                      IdentifierName(TokenKindPropertyName)))
                                                  .WithArms(SeparatedList<SwitchExpressionArmSyntax>(
                                                             [..GenerateCommaSeparatedList(relevantTerminals,
                                                               t => GenerateTokenSwitchExpressionArm(t, "token")),
                                                              ..relevantTerminals.Count == 0 // No comma before default arm if there are no other arms
                                                                  ? new List<SyntaxNodeOrToken>()
                                                                  : [Token(SyntaxKind.CommaToken)],
                                                              GenerateDefaultThrowSwitchExpressionArm(),
                                                              Token(SyntaxKind.CommaToken),
                                                               ]))));

  private SwitchExpressionArmSyntax GenerateNonterminalSwitchExpressionArm(Nonterminal nonterminal,
                                                                           string nonterminalParameterName) =>
    SwitchExpressionArm(ConstantPattern(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                               IdentifierName(NonterminalKindEnumName),
                                                               IdentifierName(StringToSafeUpperCamelCase(nonterminal.Value)))),
                                        InvocationExpression(IdentifierName(GetVisitMethodName(nonterminal)))
                                          .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                                                            Argument(
                                                                               IdentifierName(nonterminalParameterName))))));

  private SwitchExpressionArmSyntax GenerateTokenSwitchExpressionArm(Terminal token, string tokenParameterName) =>
    SwitchExpressionArm(ConstantPattern(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                               IdentifierName(TokenKindEnumName),
                                                               IdentifierName(StringToSafeUpperCamelCase(token.Value)))),
                                        InvocationExpression(IdentifierName(GetVisitMethodName(token)))
                                          .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                                                          Argument(IdentifierName(tokenParameterName))))));

  private MethodDeclarationSyntax GenerateVisitNonterminalMethodStub(Nonterminal nonterminal) =>
    MethodDeclaration(IdentifierName(GenericName), Identifier(GetVisitMethodName(nonterminal)))
      .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.AbstractKeyword)))
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(StringToSafeCamelCase(nonterminal.Value)))
                                                                .WithType(IdentifierName(NonterminalRecordName)))))
      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

  private MethodDeclarationSyntax GenerateVisitTokenMethodStub(Terminal token) =>
    MethodDeclaration(IdentifierName(GenericName), Identifier(GetVisitMethodName(token)))
      .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.AbstractKeyword)))
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("token"))
                                                                .WithType(IdentifierName(TokenRecordName)))))
      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

  private static string GetVisitMethodName(Symbol symbol) => $"Visit{StringToSafeUpperCamelCase(symbol.Value)}";

  private SwitchExpressionArmSyntax GenerateDefaultThrowSwitchExpressionArm() =>
    SwitchExpressionArm(DiscardPattern(), GenerateThrowArgumentOutOfRangeExceptionExpression());

  private ThrowExpressionSyntax GenerateThrowArgumentOutOfRangeExceptionExpression() =>
    ThrowExpression(ObjectCreationExpression(IdentifierName("ArgumentOutOfRangeException"))
                      .WithArgumentList(ArgumentList()));

  private MethodDeclarationSyntax GenerateVisitMethod(string parameterType, string parameterName) =>
    MethodDeclaration(IdentifierName(GenericName), Identifier(VisitMethodName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(parameterName))
                                                                .WithType(IdentifierName(parameterType)))))
      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
}