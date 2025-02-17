namespace ExampleRecognizer.Generated;
public class Parser(Scanner scanner)
{
    public void Parse()
    {
        if (See(0, 1, 2))
        {
            ParseS();
            Eat(5);
            return;
        }

        throw new ParserException($"Cannot parse {{Start}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private bool See(params int[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'");
    private void ParseS()
    {
        if (See(0, 1, 2))
        {
            Console.WriteLine("Semantic action!");
            ParseA();
            ParseB();
            ParseC();
            ParseD();
            ParseE();
            Console.WriteLine("Done :D");
            return;
        }

        throw new ParserException($"Cannot parse Start, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private void ParseA()
    {
        if (See(0))
        {
            Eat(0);
            return;
        }

        if (See(1, 2))
        {
            return;
        }

        throw new ParserException($"Cannot parse A, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private void ParseB()
    {
        if (See(1))
        {
            Eat(1);
            return;
        }

        if (See(2))
        {
            return;
        }

        throw new ParserException($"Cannot parse B, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{b, c}}");
    }

    private void ParseC()
    {
        if (See(2))
        {
            Console.WriteLine("See C");
            Eat(2);
            return;
        }

        throw new ParserException($"Cannot parse C, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{c}}");
    }

    private void ParseD()
    {
        if (See(3))
        {
            Eat(3);
            return;
        }

        if (See(4, 5))
        {
            return;
        }

        throw new ParserException($"Cannot parse D, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{d, e, <end of input>}}");
    }

    private void ParseE()
    {
        if (See(4))
        {
            Eat(4);
            return;
        }

        if (See(5))
        {
            return;
        }

        throw new ParserException($"Cannot parse E, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{e, <end of input>}}");
    }
}