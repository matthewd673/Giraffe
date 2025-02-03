namespace Giraffe.RDT;

public record Routine(string Nonterminal, List<Prediction> Predictions) : Node {
  // Empty
}