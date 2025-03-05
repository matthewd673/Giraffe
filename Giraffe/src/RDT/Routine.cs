using Giraffe.GIR;

namespace Giraffe.RDT;

public record Routine(Nonterminal Nonterminal, List<Prediction> Predictions) : Node;