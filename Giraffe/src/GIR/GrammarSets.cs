using System.Collections.Immutable;
using Giraffe.RDT;

namespace Giraffe.GIR;

public record GrammarSets(Grammar Grammar,
                          Dictionary<Nonterminal, HashSet<Terminal>> First,
                          Dictionary<Nonterminal, HashSet<Terminal>> Follow,
                          Dictionary<Rule, HashSet<Terminal>> Predict) {
  public TopLevel BuildRdt() =>
    new(BuildEntryRoutine(), Grammar.Nonterminals.Select(BuildRoutine).ToList(), Grammar.MemberDeclarations);

  private EntryRoutine BuildEntryRoutine() =>
    new(Grammar.EntryNonterminals.Select(BuildEntryNonterminalPrediction).ToList());

  private Prediction BuildEntryNonterminalPrediction(Nonterminal nonterminal) =>
    new(Grammar.GetAllRulesForNonterminal(nonterminal)
               .Aggregate(new HashSet<Terminal>(), (acc, rule) => acc.Union(Predict[rule]).ToHashSet()),
        [new NonterminalConsumption(nonterminal, []), new TerminalConsumption(Grammar.Eof)],
        [new SymbolIndex(0)], // TODO: Support custom outputs in entry routine predictions
        new()); // TODO: Support semantic actions in entry routine predictions

  private Routine BuildRoutine(Nonterminal nonterminal) =>
    new(nonterminal,
        Grammar.GetAllRulesForNonterminal(nonterminal).Select(BuildPrediction).ToList(),
        Grammar.NonterminalParameters.GetValueOrDefault(nonterminal, []));

  private Prediction BuildPrediction(Rule rule) =>
    new(Predict[rule],
        BuildConsumptions(rule),
        [], // TODO: Output
        // rule.Output
        //     .Select(o => GetSymbolOrParameterIndex(o,
        //                                            rule.Symbols,
        //                                            Grammar.NonterminalParameters
        //                                                   .GetValueOrDefault(rule.Name, [])))
        //     .ToList(),
        rule.SemanticAction);

  private List<Consumption> BuildConsumptions(Rule rule) =>
    rule.Symbols.Select((s, i) => SymbolToConsumption(s, i, rule)).ToList();

  private Consumption SymbolToConsumption(Symbol symbol, int index, Rule sourceRule) => symbol switch {
    Terminal t => new TerminalConsumption(t),
    Nonterminal nt => new NonterminalConsumption(nt,
                                                 sourceRule.SymbolArguments.GetValueOrDefault(index, [])
                                                           .Select(a => GetSymbolOrParameterIndex(a,
                                                                     ImmutableList.CreateRange(sourceRule.Symbols.Select(s => s.Value)),
                                                                     Grammar.NonterminalParameters
                                                                            .GetValueOrDefault(sourceRule.Nonterminal, [])))
                                                           .ToList()),
    _ => throw new ArgumentOutOfRangeException($"Cannot convert symbol of type {symbol.GetType()} to consumption"),
  };

  private static RDT.Index GetSymbolOrParameterIndex(string symbolOrArgument,
                                              ImmutableList<string> ruleSymbols,
                                              List<string> ntParameters) =>
    Grammar.IsParameter(symbolOrArgument)
      ? new ParameterIndex(ntParameters.IndexOf(symbolOrArgument))
      : new SymbolIndex(ruleSymbols.IndexOf(symbolOrArgument));
}