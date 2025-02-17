using Giraffe.RDT;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpParserSourceGenerator(GrammarSets grammarSets) : CSharpSourceGenerator {
  public required string ParserClassName { get; init; }
  public required string ScannerClassName { get; init; }
  public required string ParserExceptionClassName { get; init; }
  public required string TokenRecordName { get; init; }
  public required string ParseNodeKindPropertyName { get; init; }
  public required string ScannerPeekMethodName { get; init; }
  public required string ScannerEatMethodName { get; init; }
  public required string ScannerNameOfMethodName { get; init; }
  public required string EntryMethodName { get; init; }

  private const string SeeMethodName = "See";
  private const string EatMethodName = "Eat";
  private const string ScannerFieldName = "scanner";

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
                                                     GenerateExceptionThrowStatement(ParserExceptionClassName, GetParseEntryRoutineExceptionMessage(entryRoutine))]));

  private MethodDeclarationSyntax GenerateRoutine(Routine routine) =>
    MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), // TODO: Return type
                      Identifier(GetParseMethodName(routine.Nonterminal)))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
      .WithBody(Block((IEnumerable<StatementSyntax>)[..GeneratePredictions(routine.Predictions),
                                                     GenerateExceptionThrowStatement(ParserExceptionClassName, GetParseRoutineExceptionMessage(routine))]));

  private IEnumerable<IfStatementSyntax> GeneratePredictions(IEnumerable<Prediction> predictions) =>
    predictions.Select(GeneratePrediction);

  private IfStatementSyntax GeneratePrediction(Prediction prediction) =>
    IfStatement(GeneratePeekCall(prediction.PredictSet),
                Block((IEnumerable<StatementSyntax>)[..GenerateSemanticAction(prediction.SemanticAction.Before),
                                                     ..GenerateConsumptions(prediction.Consumptions),
                                                     ..GenerateSemanticAction(prediction.SemanticAction.After),
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

  private static List<StatementSyntax> GenerateSemanticAction(string? semanticAction) =>
    semanticAction is null ? [] : [ParseStatement(semanticAction)];

  private ArgumentSyntax TerminalToIntArgument(string terminal) =>
    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(GetTerminalIndex(terminal))));


  private static string GetParseMethodName(string nonterminal) => $"Parse{SanitizeMethodName(nonterminal)}";

  private int GetTerminalIndex(string terminal) => terminalsOrdering.IndexOf(terminal);

  private InterpolatedStringExpressionSyntax GetParseEntryRoutineExceptionMessage(EntryRoutine entryRoutine) {
    string[] textTokens = ["Cannot parse {{" +
                            string.Join(", ", grammarSets.Grammar.EntryNonterminals
                                                         .Select(nt => GetDisplayName(grammarSets.Grammar, nt))) +
                            "}}, saw ",
                           " but expected one of {{" +
                            string.Join(", ", entryRoutine.Predictions
                                                          .SelectMany(p => p.PredictSet
                                                                            .Select(t => GetDisplayName(grammarSets.Grammar, t)))) +
                            "}}",
                          ];
    return InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken))
      .WithContents(List(new InterpolatedStringContentSyntax[] {
        InterpolatedStringText()
          .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, textTokens[0], textTokens[0],
                               TriviaList())),
        Interpolation(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                  IdentifierName(ScannerFieldName),
                                                                  IdentifierName(ScannerNameOfMethodName)))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(
                                                                               MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                   IdentifierName(ScannerFieldName),
                                                                                   IdentifierName(ScannerPeekMethodName))),
                                                                                 IdentifierName(ParseNodeKindPropertyName))))))),
        InterpolatedStringText()
          .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, textTokens[1], textTokens[1],
                               TriviaList())),
      }));
  }

  private InterpolatedStringExpressionSyntax GetParseRoutineExceptionMessage(Routine routine) {
    string[] textTokens = [$"Cannot parse {GetDisplayName(grammarSets.Grammar, routine.Nonterminal)}, saw ",
                           " but expected one of {{" +
                            string.Join(", ", routine.Predictions
                                                     .SelectMany(p => p.PredictSet
                                                                       .Select(t => GetDisplayName(grammarSets.Grammar, t)))) +
                            "}}",
                          ];
    return InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken))
      .WithContents(List(new InterpolatedStringContentSyntax[] {
        InterpolatedStringText()
          .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken,textTokens[0], textTokens[0], TriviaList())),
        Interpolation(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                  IdentifierName(ScannerFieldName),
                                                                  IdentifierName(ScannerNameOfMethodName)))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(
                                                                                 MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                      InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                                IdentifierName(ScannerFieldName),
                                                                                                IdentifierName(ScannerPeekMethodName))),
                                                                                      IdentifierName(ParseNodeKindPropertyName))))))),
        InterpolatedStringText()
          .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, textTokens[1], textTokens[1], TriviaList())),
      }));
  }

  // Generates: `private bool See(params int[] terminals) => terminals.Contains(scanner.Peek().Type);`
  private MethodDeclarationSyntax GenerateSeeMethod() {
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
                                                                           IdentifierName(ParseNodeKindPropertyName))))))))
           .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
  }

  // Generates: `private Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new Exception();`
  private MethodDeclarationSyntax GenerateEatMethod() {
    const string terminalParamName = "terminal";
    return MethodDeclaration(IdentifierName(TokenRecordName), Identifier(EatMethodName))
           .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
           .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(terminalParamName))
                                                                     .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))))))
           .WithExpressionBody(ArrowExpressionClause(ConditionalExpression(InvocationExpression(IdentifierName(SeeMethodName))
                                                                             .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(terminalParamName))))),
                                                                           InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             IdentifierName(ScannerFieldName),
                                                                             IdentifierName(ScannerEatMethodName))),
                                                                           GenerateExceptionThrowExpression(
                                                                            ParserExceptionClassName,
                                                                            GetUnexpectedTerminalExceptionMessage(
                                                                             InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 IdentifierName(ScannerFieldName),
                                                                                 IdentifierName(ScannerNameOfMethodName)))
                                                                               .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                   IdentifierName(ScannerFieldName),
                                                                                   IdentifierName(ScannerPeekMethodName))),
                                                                                 IdentifierName(ParseNodeKindPropertyName)))))),
                                                                             InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                 IdentifierName(ScannerFieldName),
                                                                                 IdentifierName(ScannerNameOfMethodName)))
                                                                               .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(terminalParamName))))))))))
           .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
  }

  private static InterpolatedStringExpressionSyntax GetUnexpectedTerminalExceptionMessage(ExpressionSyntax sawExpression, ExpressionSyntax expectedExpression) =>
    InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken))
      .WithContents(List(new InterpolatedStringContentSyntax[] {
        InterpolatedStringText()
          .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, "Unexpected terminal, saw '", "Unexpected terminal, saw '", TriviaList())),
        Interpolation(sawExpression),
        InterpolatedStringText()
          .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, "' but expected '", "' but expected '", TriviaList())),
        Interpolation(expectedExpression),
        InterpolatedStringText().WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, "'", "'", TriviaList()))}));
}