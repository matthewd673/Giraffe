namespace Giraffe.AST;

public record GrammarDefinition(List<SymbolDefinition> SymbolDefinitions) : ASTNode;