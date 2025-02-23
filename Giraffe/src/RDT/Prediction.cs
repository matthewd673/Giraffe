namespace Giraffe.RDT;

public record Prediction(HashSet<string> PredictSet,
                         List<Consumption> Consumptions,
                         List<Index> Output,
                         SemanticAction SemanticAction)
  : Node;