using Giraffe.GIR;

namespace Giraffe.RDT;

public record TerminalConsumption(Terminal Terminal,
                                  SymbolTransformation SymbolTransformation)
  : Consumption(Terminal, SymbolTransformation);