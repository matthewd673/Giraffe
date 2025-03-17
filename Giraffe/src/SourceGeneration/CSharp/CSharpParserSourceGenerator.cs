using Giraffe.GIR;
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
  public required string TokenKindEnumName { get; init; }
  public required string NonterminalKindEnumName { get; init; }
  public required string ParseNodeIndexPropertyName { get; init; }
  public required string ParseNodeRowPropertyName { get; init; }
  public required string ParseNodeColumnPropertyName { get; init; }
  public required string TokenRecordName { get; init; }
  public required string TokenKindPropertyName { get; init; }
  public required string NonterminalRecordName { get; init; }
  public required string NonterminalChildrenPropertyName { get; init; }
  public required string ScannerPeekMethodName { get; init; }
  public required string ScannerEatMethodName { get; init; }
  public required string ScannerNameOfMethodName { get; init; }
  public required string EntryMethodName { get; init; }

  private const string SeeMethodName = "See";
  private const string EatMethodName = "Eat";
  private const string ScannerFieldName = "scanner";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateParserClass(grammarSets.BuildRdt())]))
      .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateParserClass(TopLevel topLevel) =>
    ClassDeclaration(ParserClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(ScannerFieldName))
                                                                .WithType(IdentifierName(ScannerClassName)))))
      .WithMembers(List([..GenerateMemberDeclarations(topLevel.MemberDeclarations.Before),
                         GenerateEntryRoutine(topLevel.EntryRoutine),
                         GenerateSeeMethod(),
                         GenerateEatMethod(),
                         ..GenerateRoutines(topLevel.Routines),
                         ..GenerateMemberDeclarations(topLevel.MemberDeclarations.After),
                        ]));

  private IEnumerable<MemberDeclarationSyntax> GenerateRoutines(IEnumerable<Routine> routines) =>
    routines.Select(GenerateRoutine);

  private MethodDeclarationSyntax GenerateEntryRoutine(EntryRoutine entryRoutine) =>
    MethodDeclaration(IdentifierName(ParseTreeRecordName), Identifier(EntryMethodName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithBody(Block((IEnumerable<StatementSyntax>)[..GenerateEntryPredictions(entryRoutine.Predictions),
                                                       GenerateParserExceptionThrowStatement(GetParseEntryRoutineExceptionMessage(entryRoutine)),
                                                    ]));

  private IEnumerable<IfStatementSyntax> GenerateEntryPredictions(IEnumerable<Prediction> predictions) =>
    predictions.Select(GenerateEntryPrediction);

  private IfStatementSyntax GenerateEntryPrediction(Prediction prediction) =>
    IfStatement(GeneratePeekCall(prediction.PredictSet),
                Block((IEnumerable<StatementSyntax>)[..GenerateSemanticAction(prediction.SemanticAction.Before),
                                                     ..GenerateConsumptions(prediction.Consumptions),
                                                     ..GenerateSemanticAction(prediction.SemanticAction.After),
                                                     GenerateParseTreeReturnStatement(prediction.Consumptions)]));

  private MethodDeclarationSyntax GenerateRoutine(Routine routine) =>
    MethodDeclaration(IdentifierName(NonterminalRecordName),
                      Identifier(GetParseMethodName(routine.Nonterminal)))
      .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
      .WithBody(Block((IEnumerable<StatementSyntax>)[..GeneratePredictions(routine.Predictions, routine.Nonterminal),
                                                       GenerateParserExceptionThrowStatement(GetParseRoutineExceptionMessage(routine)),
                                                    ]));

  private IEnumerable<IfStatementSyntax> GeneratePredictions(IEnumerable<Prediction> predictions, Nonterminal nt) =>
    predictions.Select(p => GeneratePrediction(p, nt));

  private IfStatementSyntax GeneratePrediction(Prediction prediction, Nonterminal nt) =>
    IfStatement(GeneratePeekCall(prediction.PredictSet),
                Block((IEnumerable<StatementSyntax>)[..GenerateSemanticAction(prediction.SemanticAction.Before),
                                                     ..GenerateConsumptions(prediction.Consumptions),
                                                     ..GenerateSemanticAction(prediction.SemanticAction.After),
                                                     GeneratePredictionOutputReturnStatement(nt, prediction.Consumptions)]));

  private InvocationExpressionSyntax GeneratePeekCall(HashSet<Terminal> predictSet) =>
    InvocationExpression(IdentifierName(SeeMethodName))
      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(GenerateCommaSeparatedList(predictSet, TerminalToTokenKindArgument))));

  private IEnumerable<LocalDeclarationStatementSyntax> GenerateConsumptions(IEnumerable<Consumption> consumptions) =>
    consumptions.Select(GenerateConsumption);

  private LocalDeclarationStatementSyntax GenerateConsumption(Consumption consumption, int index) =>
    consumption switch {
      TerminalConsumption terminalConsumption => GenerateTerminalConsumption(terminalConsumption, index),
      NonterminalConsumption nonterminalConsumption => GenerateNonterminalConsumption(nonterminalConsumption, index),
      _ => throw new CSharpSourceGeneratorException($"Cannot generate source code for consumption type {consumption.GetType().FullName}"),
    };

  private LocalDeclarationStatementSyntax GenerateTerminalConsumption(TerminalConsumption terminalConsumption,
                                                                      int index) =>
    LocalDeclarationStatement(VariableDeclaration(IdentifierName(TokenRecordName))
                                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(GetSymbolIdFromIndex(index)))
                                                                        .WithInitializer(EqualsValueClause(InvocationExpression(IdentifierName(EatMethodName))
                                                                          .WithArgumentList(ArgumentList(SingletonSeparatedList(TerminalToTokenKindArgument(terminalConsumption.Terminal)))))))));

  private LocalDeclarationStatementSyntax GenerateNonterminalConsumption(NonterminalConsumption nonterminalConsumption,
                                                                         int index) =>
    LocalDeclarationStatement(VariableDeclaration(IdentifierName(NonterminalRecordName))
                                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(GetSymbolIdFromIndex(index)))
                                                                        .WithInitializer(EqualsValueClause(
                                                                         InvocationExpression(IdentifierName(GetParseMethodName(nonterminalConsumption.Nonterminal)))
                                                                         )))));

  private static List<StatementSyntax> GenerateSemanticAction(string? semanticAction) =>
    semanticAction is null ? [] : [ParseStatement(semanticAction)];

  private static List<MemberDeclarationSyntax> GenerateMemberDeclarations(string? memberDeclarations) =>
    memberDeclarations is null ? [] : [ParseMemberDeclaration(memberDeclarations)
                                       ?? throw new CSharpSourceGeneratorException("Cannot parse member declarations")];

  private ReturnStatementSyntax GeneratePredictionOutputReturnStatement(Nonterminal nt,
                                                                        List<Consumption> consumptions) =>
    ReturnStatement(ImplicitObjectCreationExpression()
                      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] {
                        NonterminalToNonterminalKindArgument(nt),
                        Token(SyntaxKind.CommaToken),
                        Argument(GeneratePredictionOutputCollection(consumptions)),
                        Token(SyntaxKind.CommaToken),
                        GeneratePredictionOutputPositionPropertyArgument(ParseNodeIndexPropertyName,
                                                                         consumptions.Count == 0),
                        Token(SyntaxKind.CommaToken),
                        GeneratePredictionOutputPositionPropertyArgument(ParseNodeRowPropertyName,
                                                                         consumptions.Count == 0),
                        Token(SyntaxKind.CommaToken),
                        GeneratePredictionOutputPositionPropertyArgument(ParseNodeColumnPropertyName,
                                                                         consumptions.Count == 0),
                      }))));

  private ReturnStatementSyntax GenerateParseTreeReturnStatement(IEnumerable<Consumption> consumptions) =>
    ReturnStatement(ImplicitObjectCreationExpression()
                      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] {
                        Argument(GeneratePredictionOutputCollection(consumptions)),
                      }))));

  private CollectionExpressionSyntax GeneratePredictionOutputCollection(IEnumerable<Consumption> consumptions) =>
    CollectionExpression()
      .WithElements(SeparatedList<CollectionElementSyntax>(GenerateCommaSeparatedList(
                                                            consumptions.Select(GeneratePredictionOutputElement)
                                                                        .Where(e => e is not null),
                                                            e => e!)));

  private CollectionElementSyntax? GeneratePredictionOutputElement(Consumption consumption, int index) {
    if (consumption.SymbolTransformation.Discard) {
      return null;
    }

    string idName = GetSymbolIdFromIndex(index);

    if (consumption.SymbolTransformation.Expand) {
      return SpreadElement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                  IdentifierName(idName),
                                                  IdentifierName(NonterminalChildrenPropertyName)));
    }

    return ExpressionElement(IdentifierName(idName));
  }

  private ArgumentSyntax GeneratePredictionOutputPositionPropertyArgument(string positionPropertyName,
                                                                          bool consumptionsEmpty) =>
    Argument(consumptionsEmpty
      ? LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(-1))
      : MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                               IdentifierName(GetSymbolIdFromIndex(0)),
                               IdentifierName(positionPropertyName)));

  private ArgumentSyntax TerminalToTokenKindArgument(Terminal terminal) =>
    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(TokenKindEnumName),
                                    IdentifierName(StringToSafeUpperCamelCase(terminal.Value))));

  private ArgumentSyntax NonterminalToNonterminalKindArgument(Nonterminal nt) =>
    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(NonterminalKindEnumName),
                                    IdentifierName(StringToSafeUpperCamelCase(nt.Value))));

  private static string GetSymbolIdFromIndex(int id) => $"s{id}";

  private static string GetParseMethodName(Nonterminal nt) => $"Parse{StringToSafeUpperCamelCase(nt.Value)}";

  private ThrowStatementSyntax GenerateParserExceptionThrowStatement(InterpolatedStringExpressionSyntax message) =>
    ThrowStatement(GenerateParserExceptionObjectCreation(message));

  private ThrowExpressionSyntax GenerateParserExceptionThrowExpression(InterpolatedStringExpressionSyntax message) =>
    ThrowExpression(GenerateParserExceptionObjectCreation(message));

  private ObjectCreationExpressionSyntax GenerateParserExceptionObjectCreation(InterpolatedStringExpressionSyntax message) {
    InvocationExpressionSyntax scannerPeekCall = InvocationExpression(MemberAccessExpression(
                                                                       SyntaxKind.SimpleMemberAccessExpression,
                                                                       IdentifierName(ScannerFieldName),
                                                                       IdentifierName(ScannerPeekMethodName)));

    return ObjectCreationExpression(IdentifierName(ParserExceptionClassName))
      .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] {
        Argument(message),
        Token(SyntaxKind.CommaToken),
        Argument(MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        scannerPeekCall,
                                        IdentifierName(ParseNodeIndexPropertyName))),
        Token(SyntaxKind.CommaToken),
        Argument(MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        scannerPeekCall,
                                        IdentifierName(ParseNodeRowPropertyName))),
        Token(SyntaxKind.CommaToken),
        Argument(MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        scannerPeekCall,
                                        IdentifierName(ParseNodeColumnPropertyName))),
      })));
  }

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
                                                                           GenerateParserExceptionThrowExpression(
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