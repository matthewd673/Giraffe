namespace ExampleRecognizer.Generated;
public record Nonterminal(int Kind, ParseNode[] Children) : ParseNode(Kind);