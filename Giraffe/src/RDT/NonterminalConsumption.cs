using Giraffe.GIR;

namespace Giraffe.RDT;

public record NonterminalConsumption(Nonterminal Nonterminal,
                                     SymbolTransformation SymbolTransformation)
  : Consumption(Nonterminal, SymbolTransformation);