namespace UsefulParser.Generated;
public record Nonterminal(int Kind, ParseNode[] Children) : ParseNode(Kind);