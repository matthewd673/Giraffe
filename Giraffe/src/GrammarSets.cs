using Giraffe.RDT;

namespace Giraffe;

public record GrammarSets(Grammar Grammar,
                          Dictionary<string, HashSet<string>> First,
                          Dictionary<string, HashSet<string>> Follow,
                          Dictionary<Rule, HashSet<string>> Predict) {
  public TopLevel BuildRDT() =>
    new(Grammar.Nonterminals.Select(BuildRoutine).ToList());

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