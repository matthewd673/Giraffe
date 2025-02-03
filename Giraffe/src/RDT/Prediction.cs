namespace Giraffe.RDT;

public record Prediction(HashSet<string> PredictSet,
                         List<Consumption> Consumptions) : Node {
  // Empty
}