using Giraffe.GIR;

namespace Giraffe.RDT;

public abstract record Consumption(Symbol Symbol, SymbolTransformation SymbolTransformation) : Node;