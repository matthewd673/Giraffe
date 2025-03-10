namespace Giraffe.AST;

public record NonterminalUsage(string Name, bool Expand) : SymbolUsage;