namespace Giraffe.AST;

public record RuleDefinition(List<SymbolUsage> SymbolUsages) : ASTNode;