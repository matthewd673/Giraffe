namespace ExampleRecognizer.Generated;
public record Token(int Kind, string Image) : ParseNode(Kind);