namespace Giraffe.AST;

public record NonterminalUsage(string Name, bool Discard, bool Expand) : SymbolUsage(Name, Discard);