using Giraffe.GIR;

namespace Giraffe.RDT;

public record Prediction(HashSet<Terminal> PredictSet,
                         List<Consumption> Consumptions,
                         List<Index> Output,
                         SemanticAction SemanticAction)
  : Node;