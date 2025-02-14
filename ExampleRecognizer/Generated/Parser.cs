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

        throw new ParserException();
    }

    private bool See(params int[] terminals) => terminals.Contains(scanner.Peek().Type);
    private Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new ParserException();
    private void ParseS()
    {
        if (See(0, 1, 2))
        {
            ParseA();
            ParseB();
            ParseC();
            ParseD();
            ParseE();
            return;
        }

        throw new ParserException();
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

        throw new ParserException();
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

        throw new ParserException();
    }

    private void ParseC()
    {
        if (See(2))
        {
            Eat(2);
            return;
        }

        throw new ParserException();
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

        throw new ParserException();
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

        throw new ParserException();
    }
}