namespace Giraffe.AST;

public record TerminalUsage(string Name, bool Discard) : SymbolUsage(Name, Discard);