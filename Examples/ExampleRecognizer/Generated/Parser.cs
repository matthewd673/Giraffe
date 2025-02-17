namespace ExampleRecognizer.Generated;
public class Parser(Scanner scanner)
{
    public Nonterminal Parse()
    {
        if (See(0, 1, 2))
        {
            ParseNode[] c = [ParseS(), Eat(5)];
            return new(-1, c);
        }

        throw new ParserException($"Cannot parse {{Start}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private bool See(params int[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'");
    private Nonterminal ParseS()
    {
        if (See(0, 1, 2))
        {
            ParseNode[] c = [ParseA(), ParseB(), ParseC(), ParseD(), ParseE()];
            return new(0, c);
        }

        throw new ParserException($"Cannot parse Start, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private Nonterminal ParseA()
    {
        if (See(0))
        {
            ParseNode[] c = [Eat(0)];
            return new(1, c);
        }

        if (See(1, 2))
        {
            ParseNode[] c = [];
            return new(1, c);
        }

        throw new ParserException($"Cannot parse A, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private Nonterminal ParseB()
    {
        if (See(1))
        {
            ParseNode[] c = [Eat(1)];
            return new(2, c);
        }

        if (See(2))
        {
            ParseNode[] c = [];
            return new(2, c);
        }

        throw new ParserException($"Cannot parse B, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{b, c}}");
    }

    private Nonterminal ParseC()
    {
        if (See(2))
        {
            ParseNode[] c = [Eat(2)];
            return new(3, c);
        }

        throw new ParserException($"Cannot parse C, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{c}}");
    }

    private Nonterminal ParseD()
    {
        if (See(3))
        {
            ParseNode[] c = [Eat(3)];
            return new(4, c);
        }

        if (See(4, 5))
        {
            ParseNode[] c = [];
            return new(4, c);
        }

        throw new ParserException($"Cannot parse D, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{d, e, <end of input>}}");
    }

    private Nonterminal ParseE()
    {
        if (See(4))
        {
            ParseNode[] c = [Eat(4)];
            return new(5, c);
        }

        if (See(5))
        {
            ParseNode[] c = [];
            return new(5, c);
        }

        throw new ParserException($"Cannot parse E, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{e, <end of input>}}");
    }
}