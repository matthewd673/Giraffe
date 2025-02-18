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
  public required string ParseTreeRecordName { get; init; }
  public required string ParseNodeRecordName { get; init; }
  public required string TokenKindEnumName { get; init; }
  public required string NonterminalKindEnumName { get; init; }
  public required string TokenRecordName { get; init; }
  public required string TokenKindPropertyName { get; init; }
  public required string NonterminalRecordName { get; init; }
  public required string ScannerPeekMethodName { get; init; }
  public required string ScannerEatMethodName { get; init; }
  public required string ScannerNameOfMethodName { get; init; }
  public required string EntryMethodName { get; init; }

  private const string SeeMethodName = "See";
  private const string EatMethodName = "Eat";
  private const string ScannerFieldName = "scanner";
  private const string ChildrenVariableName = "c";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateParserClass(grammarSets.BuildRDT())]))
      .NormalizeWhitespace();

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
    MethodDeclaration(IdentifierName(ParseTreeRecordName), Identifier(EntryMethodName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithBody(Block((IEnumerable<StatementSyntax>)[..GenerateEntryPredictions(entryRoutine.Predictions),
                                                     GenerateExceptionThrowStatement(ParserExceptionClassName, GetParseEntryRoutineExceptionMessage(entryRoutine))]));

  private IEnumerable<IfStatementSyntax> GenerateEntryPredictions(IEnumerable<Prediction> predictions) =>
    predictions.Select(GenerateEntryPrediction);

  private IfStatementSyntax GenerateEntryPrediction(Prediction prediction) =>
    IfStatement(GeneratePeekCall(prediction.PredictSet),
                Block((IEnumerable<StatementSyntax>)[..GenerateSemanticAction(prediction.SemanticAction.Before),
                                                     GenerateConsumptions(prediction.Consumptions),
                                                     ..GenerateSemanticAction(prediction.SemanticAction.After),
                                                     GenerateParseTreeReturnStatement()]));

  private MethodDeclarationSyntax GenerateRoutine(Routine routine) =>
    MethodDeclaration(IdentifierName(NonterminalRecordName),
                      Identifier(GetParseMethodName(routine.Nonterminal)))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
      .WithBody(Block((IEnumerable<StatementSyntax>)[..GeneratePredictions(routine.Predictions, routine.Nonterminal),
                                                     GenerateExceptionThrowStatement(ParserExceptionClassName, GetParseRoutineExceptionMessage(routine))]));

  private IEnumerable<IfStatementSyntax> GeneratePredictions(IEnumerable<Prediction> predictions, string nonterminal) =>
    predictions.Select(p => GeneratePrediction(p, nonterminal));

  private IfStatementSyntax GeneratePrediction(Prediction prediction, string nonterminal) =>
    IfStatement(GeneratePeekCall(prediction.PredictSet),
                Block((IEnumerable<StatementSyntax>)[..GenerateSemanticAction(prediction.SemanticAction.Before),
                                                     GenerateConsumptions(prediction.Consumptions),
                                                     ..GenerateSemanticAction(prediction.SemanticAction.After),
                                                     GenerateNonterminalReturnStatement(nonterminal)]));

  private InvocationExpressionSyntax GeneratePeekCall(HashSet<string> predictSet) =>
    InvocationExpression(IdentifierName(SeeMethodName))
      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(GenerateCommaSeparatedList(predictSet, TerminalToTokenKindArgument))));

  private LocalDeclarationStatementSyntax GenerateConsumptions(IEnumerable<Consumption> consumptions) =>
    LocalDeclarationStatement(VariableDeclaration(ArrayType(IdentifierName(ParseNodeRecordName))
                                                    .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(ChildrenVariableName))
                                                                        .WithInitializer(EqualsValueClause(CollectionExpression(SeparatedList<CollectionElementSyntax>(consumptions.Select(c => ExpressionElement(GenerateConsumption(c))))))))));

  private InvocationExpressionSyntax GenerateConsumption(Consumption consumption) =>
    consumption switch {
      TerminalConsumption terminalConsumption => GenerateTerminalConsumption(terminalConsumption),
      NonterminalConsumption nonterminalConsumption => GenerateNonterminalConsumption(nonterminalConsumption),
      _ => throw new CSharpSourceGeneratorException($"Cannot generate source code for consumption type {consumption.GetType().FullName}"),
    };

  private InvocationExpressionSyntax GenerateTerminalConsumption(TerminalConsumption terminalConsumption) =>
    InvocationExpression(IdentifierName(EatMethodName))
                          .WithArgumentList(ArgumentList(SingletonSeparatedList(TerminalToTokenKindArgument(terminalConsumption.Terminal))));

  private static InvocationExpressionSyntax GenerateNonterminalConsumption(NonterminalConsumption nonterminalConsumption) =>
    InvocationExpression(IdentifierName(GetParseMethodName(nonterminalConsumption.Nonterminal)))
                           .WithArgumentList(ArgumentList());

  private static List<StatementSyntax> GenerateSemanticAction(string? semanticAction) =>
    semanticAction is null ? [] : [ParseStatement(semanticAction)];

  private ReturnStatementSyntax GenerateNonterminalReturnStatement(string nonterminal) =>
    ReturnStatement(ImplicitObjectCreationExpression()
                      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] {
                        NonterminalToNonterminalKindArgument(nonterminal),
                        Token(SyntaxKind.CommaToken),
                        Argument(IdentifierName(ChildrenVariableName)),
                      }))));

  private ReturnStatementSyntax GenerateParseTreeReturnStatement() =>
    ReturnStatement(ImplicitObjectCreationExpression()
                      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] {
                        Argument(IdentifierName(ChildrenVariableName)),
                      }))));

  private ArgumentSyntax TerminalToTokenKindArgument(string terminal) =>
    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(TokenKindEnumName),
                                    IdentifierName(terminal)));

  private ArgumentSyntax NonterminalToNonterminalKindArgument(string nonterminal) =>
    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(NonterminalKindEnumName),
                                    IdentifierName(nonterminal)));

  private static string GetParseMethodName(string nonterminal) => $"Parse{SanitizeMethodName(nonterminal)}";

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
                                                                                 IdentifierName(TokenKindPropertyName))))))),
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
                                                                                      IdentifierName(TokenKindPropertyName))))))),
        InterpolatedStringText()
          .WithTextToken(Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, textTokens[1], textTokens[1], TriviaList())),
      }));
  }

  // Generates: `private bool See(params TokenKind[] terminals) => terminals.Contains(scanner.Peek().Type);`
  private MethodDeclarationSyntax GenerateSeeMethod() {
    const string terminalsParamName = "terminals";
    return MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier(SeeMethodName))
           .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
           .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(terminalsParamName))
                                                                   .WithModifiers(TokenList(Token(SyntaxKind.ParamsKeyword)))
                                                                   .WithType(ArrayType(IdentifierName(TokenKindEnumName))
                                                                               .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(
                                                                                SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))))))
           .WithExpressionBody(ArrowExpressionClause(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName(terminalsParamName),
                                                                            IdentifierName("Contains")))
                                                       .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                           InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                             IdentifierName(ScannerFieldName),
                                                                             IdentifierName(ScannerPeekMethodName))),
                                                                           IdentifierName(TokenKindPropertyName))))))))
           .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
  }

  // Generates: `private Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new Exception();`
  private MethodDeclarationSyntax GenerateEatMethod() {
    const string terminalParamName = "terminal";
    return MethodDeclaration(IdentifierName(TokenRecordName), Identifier(EatMethodName))
           .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
           .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(terminalParamName))
                                                                     .WithType(IdentifierName(TokenKindEnumName)))))
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
                                                                                 IdentifierName(TokenKindPropertyName)))))),
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