namespace UsefulParser.Generated;
public record Token(int Kind, string Image) : ParseNode(Kind);