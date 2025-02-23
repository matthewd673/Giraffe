namespace Giraffe.RDT;

public record Routine(string Nonterminal, List<Prediction> Predictions, List<string> Parameters) : Node;