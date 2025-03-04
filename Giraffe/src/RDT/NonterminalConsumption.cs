using Giraffe.GIR;

namespace Giraffe.RDT;

public record NonterminalConsumption(Nonterminal Nonterminal, List<Index> ArgumentIndices) : Consumption;