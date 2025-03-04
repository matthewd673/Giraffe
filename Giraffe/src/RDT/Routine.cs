namespace Giraffe.RDT;

public record Routine(Nonterminal Nonterminal, List<Prediction> Predictions, List<string> Parameters) : Node;