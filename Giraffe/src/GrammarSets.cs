using System.Collections.Immutable;
using Giraffe.RDT;

namespace Giraffe;

public record GrammarSets(Grammar Grammar,
                          Dictionary<string, HashSet<string>> First,
                          Dictionary<string, HashSet<string>> Follow,
                          Dictionary<Rule, HashSet<string>> Predict) {
  public TopLevel BuildRdt() =>
    new(BuildEntryRoutine(), Grammar.Nonterminals.Select(BuildRoutine).ToList(), Grammar.MemberDeclarations);

  private EntryRoutine BuildEntryRoutine() =>
    new(Grammar.EntryNonterminals.Select(BuildEntryNonterminalPrediction).ToList());

  private Prediction BuildEntryNonterminalPrediction(string nonterminal) =>
    new(Grammar.GetAllRulesForNonterminal(nonterminal)
               .Aggregate(new HashSet<string>(), (acc, rule) => acc.Union(Predict[rule]).ToHashSet()),
        [new NonterminalConsumption(nonterminal, []), new TerminalConsumption(Grammar.Eof)],
        [new SymbolIndex(0)], // TODO: Support custom outputs in entry routine predictions
        new()); // TODO: Support semantic actions in entry routine predictions

  private Routine BuildRoutine(string nonterminal) =>
    new(nonterminal,
        Grammar.GetAllRulesForNonterminal(nonterminal).Select(BuildPrediction).ToList(),
        Grammar.NonterminalParameters.GetValueOrDefault(nonterminal, []));

  private Prediction BuildPrediction(Rule rule) =>
    new(Predict[rule],
        BuildConsumptions(rule),
        rule.Output
            .Select(o => GetSymbolOrParameterIndex(o,
                                                   rule.Symbols,
                                                   Grammar.NonterminalParameters
                                                          .GetValueOrDefault(rule.Name, [])))
            .ToList(),
        rule.SemanticAction);

  private List<Consumption> BuildConsumptions(Rule rule) =>
    rule.Symbols.Select((s, i) => SymbolToConsumption(s, i, rule)).ToList();

  private Consumption SymbolToConsumption(string symbol, int index, Rule sourceRule) =>
    Grammar.IsTerminal(symbol)
      ? new TerminalConsumption(symbol)
      : new NonterminalConsumption(symbol, sourceRule.SymbolArguments.GetValueOrDefault(index, [])
                                                     .Select(a => GetSymbolOrParameterIndex(a,
                                                               sourceRule.Symbols,
                                                               Grammar.NonterminalParameters
                                                                            .GetValueOrDefault(sourceRule.Name, [])))
                                                     .ToList());

  private static RDT.Index GetSymbolOrParameterIndex(string symbolOrArgument,
                                              ImmutableList<string> ruleSymbols,
                                              List<string> ntParameters) =>
    Grammar.IsParameter(symbolOrArgument)
      ? new ParameterIndex(ntParameters.IndexOf(symbolOrArgument))
      : new SymbolIndex(ruleSymbols.IndexOf(symbolOrArgument));
}