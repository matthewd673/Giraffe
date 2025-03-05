using Giraffe.GIR;

namespace Giraffe.RDT;

public record Prediction(HashSet<Terminal> PredictSet,
                         List<Consumption> Consumptions,
                         SemanticAction SemanticAction)
  : Node;