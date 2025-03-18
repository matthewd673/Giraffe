namespace ExampleRecognizer.Generated;
public record Nonterminal(NtKind Kind, ParseNode[] Children, int Index, int Row, int Column) : ParseNode(Index, Row, Column);