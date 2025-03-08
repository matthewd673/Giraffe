namespace ExprParser.Generated;
public record Token(TokenKind Kind, string Image, int Index) : ParseNode;