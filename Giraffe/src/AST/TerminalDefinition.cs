namespace Giraffe.AST;

public record TerminalDefinition(string Name, TerminalRhs TerminalRhs, bool Ignore) : SymbolDefinition;