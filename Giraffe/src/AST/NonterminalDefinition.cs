namespace Giraffe.AST;

public record NonterminalDefinition(string Name,
                                    List<RuleDefinition> Rules,
                                    bool Entry) : SymbolDefinition;