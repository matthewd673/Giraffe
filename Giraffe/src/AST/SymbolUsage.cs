namespace Giraffe.AST;

public abstract record SymbolUsage(string Name, bool Discard) : ASTNode;