namespace Giraffe.Frontend;
public record Token(TokenKind Kind, string Image, int Index) : ParseNode;