namespace Giraffe.Frontend;
public record ParseTree(ParseNode[] Children) : ParseNode(0, 0, 0);