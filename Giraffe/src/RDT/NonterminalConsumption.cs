namespace Giraffe.RDT;

public record NonterminalConsumption(string Nonterminal, List<Index> ArgumentIndices) : Consumption;