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
        // TODO: Support symbol transformations instead of just using the default
        [new NonterminalConsumption(nonterminal, new()), new TerminalConsumption(Grammar.Eof, new())],
        // TODO: Support semantic actions in entry routine predictions
        new());

  private Routine BuildRoutine(Nonterminal nonterminal) =>
    new(nonterminal, Grammar.GetAllRulesForNonterminal(nonterminal).Select(BuildPrediction).ToList());

  private Prediction BuildPrediction(Rule rule) =>
    new(Predict[rule], BuildConsumptions(rule), rule.SemanticAction);

  private List<Consumption> BuildConsumptions(Rule rule) =>
    rule.Symbols.Select((s, i) => SymbolToConsumption(s)).ToList();

  private Consumption SymbolToConsumption(Symbol symbol) => symbol switch {
    Terminal t => new TerminalConsumption(t, t.Transformation),
    Nonterminal nt => new NonterminalConsumption(nt, nt.Transformation),
    _ => throw new ArgumentOutOfRangeException($"Cannot convert symbol of type {symbol.GetType()} to consumption"),
  };
}