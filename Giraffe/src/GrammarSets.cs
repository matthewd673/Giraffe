using Giraffe.RDT;

namespace Giraffe;

public record GrammarSets(Grammar Grammar,
                          Dictionary<string, HashSet<string>> First,
                          Dictionary<string, HashSet<string>> Follow,
                          Dictionary<Rule, HashSet<string>> Predict) {
  public TopLevel BuildRDT() =>
    new(BuildEntryRoutine(), Grammar.Nonterminals.Select(BuildRoutine).ToList());

  private EntryRoutine BuildEntryRoutine() =>
    new(Grammar.EntryNonterminals.Select(BuildEntryNonterminalPrediction).ToList());

  private Prediction BuildEntryNonterminalPrediction(string nonterminal) =>
    new(Grammar.GetAllRulesForNonterminal(nonterminal)
               .Aggregate(new HashSet<string>(), (acc, rule) => acc.Union(Predict[rule]).ToHashSet()),
        [new NonterminalConsumption(nonterminal), new TerminalConsumption(Grammar.Eof)]);

  private Routine BuildRoutine(string nonterminal) =>
    new(nonterminal, Grammar.GetAllRulesForNonterminal(nonterminal).Select(BuildPrediction).ToList());

  private Prediction BuildPrediction(Rule rule) =>
    new(Predict[rule], BuildConsumptions(rule));

  private static List<Consumption> BuildConsumptions(Rule rule) =>
    rule.Symbols.Select(SymbolToConsumption).ToList();

  private static Consumption SymbolToConsumption(string symbol) =>
    Grammar.IsTerminal(symbol)
      ? new TerminalConsumption(symbol)
      : new NonterminalConsumption(symbol);
}