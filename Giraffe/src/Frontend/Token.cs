namespace Giraffe.Frontend;
public record Token(TokenKind Kind, string Image, int Index, int Row, int Column) : ParseNode(Index, Row, Column);