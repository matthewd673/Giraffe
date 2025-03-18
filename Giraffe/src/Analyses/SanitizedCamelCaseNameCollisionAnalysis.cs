using Giraffe.GIR;
using Giraffe.Utils;

namespace Giraffe.Analyses;

/// <summary>
/// Identify which symbol names will collide when sanitized and converted to camelCase form.
/// </summary>
/// <param name="grammar">The Grammar to analyze.</param>
public class SanitizedCamelCaseNameCollisionAnalysis(Grammar grammar)
  : Analysis<Dictionary<string, List<string>>>(grammar) {
  public override Dictionary<string, List<string>> Analyze() {
    List<string> symbolNames = Grammar.Terminals.Select(t => t.Value)
                                      .Union(Grammar.Nonterminals.Select(nt => nt.Value))
                                      .ToList();

    Dictionary<string, List<string>> camelCaseMap = new();
    foreach (string s in symbolNames) {
      string converted = StringUtils.SnakeCaseToCamelCase(StringUtils.SanitizeNonWordCharacters(s));

      if (camelCaseMap.TryGetValue(converted, out List<string>? colliding)) {
        colliding.Add(s);
      }
      else {
        camelCaseMap.Add(converted, [s]);
      }
    }

    foreach (string key in camelCaseMap.Keys.Where(key => camelCaseMap[key].Count == 1).ToList()) {
      camelCaseMap.Remove(key);
    }

    return camelCaseMap;
  }
}