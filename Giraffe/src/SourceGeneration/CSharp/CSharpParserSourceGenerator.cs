using Giraffe.RDT;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpParserSourceGenerator(GrammarSets grammarSets) : CSharpSourceGenerator {
  public string ParserClassName { get; set; } = "Parser";
  public string ScannerClassName { get; set; } = "Scanner";
  public string ParserExceptionClassName { get; set; } = "ParserException";

  private const string SeeMethodName = "See";
  private const string EatMethodName = "Eat";
  private const string EntryMethodName = "Parse";
  private const string ScannerFieldName = "scanner";
  private const string ScannerPeekMethodName = "Peek";
  private const string ScannerEatMethodName = "Eat";
  private const string TokenClassName = "Token";

  private readonly List<string> terminalsOrdering = grammarSets.Grammar.Terminals.ToList();

  public override CompilationUnitSyntax Generate() {
    TopLevel topLevel = grammarSets.BuildRDT();

    return CompilationUnit()
      .WithUsings(List<UsingDirectiveSyntax>([]))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateParserClass(topLevel)]))
      .NormalizeWhitespace();
  }

  private ClassDeclarationSyntax GenerateParserClass(TopLevel topLevel) =>
    ClassDeclaration(ParserClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(ScannerFieldName))
                                                                .WithType(IdentifierName(ScannerClassName)))))
      .WithMembers(List([GenerateEntryRoutine(topLevel.EntryRoutine),
                         GenerateSeeMethod(),
                         GenerateEatMethod(),
                         ..GenerateRoutines(topLevel.Routines)]));

  private IEnumerable<MemberDeclarationSyntax> GenerateRoutines(IEnumerable<Routine> routines) =>
    routines.Select(GenerateRoutine);

  private MethodDeclarationSyntax GenerateEntryRoutine(EntryRoutine entryRoutine) =>
    MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),
                      Identifier(EntryMethodName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithBody(Block((IEnumerable<StatementSyntax>)[..GeneratePredictions(entryRoutine.Predictions),
                                                     GenerateExceptionThrowStatement()]));

  private MethodDeclarationSyntax GenerateRoutine(Routine routine) =>
    MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), // TODO: Return type
                      Identifier(GetParseMethodName(routine.Nonterminal)))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
      .WithBody(Block((IEnumerable<StatementSyntax>)[..GeneratePredictions(routine.Predictions),
                                                     GenerateExceptionThrowStatement()])); // TODO: Exception message

  private IEnumerable<IfStatementSyntax> GeneratePredictions(IEnumerable<Prediction> predictions) =>
    predictions.Select(GeneratePrediction);

  private IfStatementSyntax GeneratePrediction(Prediction prediction) =>
    IfStatement(GeneratePeekCall(prediction.PredictSet),
                Block((IEnumerable<StatementSyntax>)[..GenerateConsumptions(prediction.Consumptions),
                                                     ReturnStatement()])); // TODO: Return an object

  private InvocationExpressionSyntax GeneratePeekCall(HashSet<string> predictSet) =>
    InvocationExpression(IdentifierName(SeeMethodName))
      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(GenerateCommaSeparatedList(predictSet, TerminalToIntArgument))));

  private IEnumerable<StatementSyntax> GenerateConsumptions(IEnumerable<Consumption> consumptions) =>
    consumptions.Select(GenerateConsumption);

  private StatementSyntax GenerateConsumption(Consumption consumption) =>
    consumption switch {
      TerminalConsumption terminalConsumption => GenerateTerminalConsumption(terminalConsumption),
      NonterminalConsumption nonterminalConsumption => GenerateNonterminalConsumption(nonterminalConsumption),
      _ => throw new CSharpSourceGeneratorException($"Cannot generate source code for consumption type {consumption.GetType().FullName}"),
    };

  private ExpressionStatementSyntax GenerateTerminalConsumption(TerminalConsumption terminalConsumption) =>
    ExpressionStatement(InvocationExpression(IdentifierName(EatMethodName))
                          .WithArgumentList(ArgumentList(SingletonSeparatedList(TerminalToIntArgument(terminalConsumption.Terminal)))));

  private static ExpressionStatementSyntax GenerateNonterminalConsumption(NonterminalConsumption nonterminalConsumption) =>
    ExpressionStatement(InvocationExpression(IdentifierName(GetParseMethodName(nonterminalConsumption.Nonterminal)))
                          .WithArgumentList(ArgumentList()));

  private ArgumentSyntax TerminalToIntArgument(string terminal) =>
    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(GetTerminalIndex(terminal))));

  private ThrowStatementSyntax GenerateExceptionThrowStatement() =>
    ThrowStatement(ObjectCreationExpression(IdentifierName(ParserExceptionClassName))
                     .WithArgumentList(ArgumentList()));

  private ThrowExpressionSyntax GenerateExceptionThrowExpression() =>
    ThrowExpression(ObjectCreationExpression(IdentifierName(ParserExceptionClassName))
                     .WithArgumentList(ArgumentList()));

  private static string GetParseMethodName(string nonterminal) => $"Parse{SanitizeMethodName(nonterminal)}";

  private int GetTerminalIndex(string terminal) => terminalsOrdering.IndexOf(terminal);

  // Generates: `private bool See(params int[] terminals) => terminals.Contains(scanner.Peek().Type);`
  private static MethodDeclarationSyntax GenerateSeeMethod() {
    const string terminalsParamName = "terminals";
    return MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier(SeeMethodName))
           .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
           .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(terminalsParamName))
                                                                   .WithModifiers(TokenList(Token(SyntaxKind
                                                                     .ParamsKeyword)))
                                                                   .WithType(ArrayType(PredefinedType(Token(SyntaxKind
                                                                                 .IntKeyword)))
                                                                               .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList
                                                                                 <ExpressionSyntax>(OmittedArraySizeExpression()))))))))
           .WithExpressionBody(ArrowExpressionClause(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName(terminalsParamName),
                                                                            IdentifierName("Contains")))
                                                       .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                           InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             IdentifierName(ScannerFieldName),
                                                                             IdentifierName(ScannerPeekMethodName))),
                                                                           IdentifierName("Type"))))))))
           .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
  }

  // Generates: `private Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new Exception();`
  private MethodDeclarationSyntax GenerateEatMethod() {
    const string terminalParamName = "terminal";
    return MethodDeclaration(IdentifierName(TokenClassName), Identifier(EatMethodName))
           .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
           .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(terminalParamName))
                                                                     .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))))))
           .WithExpressionBody(ArrowExpressionClause(ConditionalExpression(InvocationExpression(IdentifierName(SeeMethodName))
                                                                             .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(terminalParamName))))),
                                                                           InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                              IdentifierName(ScannerFieldName),
                                                                              IdentifierName(ScannerEatMethodName))),
                                                                           GenerateExceptionThrowExpression())))
           .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
  }
}