namespace Giraffe;

public record GrammarSets(Dictionary<string, HashSet<string>> First,
                          Dictionary<string, HashSet<string>> Follow,
                          Dictionary<Rule, HashSet<string>> Predict) {
  // Empty
}