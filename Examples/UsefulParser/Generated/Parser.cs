namespace UsefulParser.Generated;
public class Parser(Scanner scanner)
{
    public Nonterminal Parse()
    {
        throw new ParserException($"Cannot parse {{}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{}}");
    }

    private bool See(params int[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'");
}