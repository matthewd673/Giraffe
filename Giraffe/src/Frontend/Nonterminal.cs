namespace Giraffe.Frontend;
public record Nonterminal(NtKind Kind, ParseNode[] Children) : ParseNode;