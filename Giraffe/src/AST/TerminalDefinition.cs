namespace Giraffe.AST;

public record TerminalDefinition(string Name, TerminalRhs TerminalRhs) : SymbolDefinition;