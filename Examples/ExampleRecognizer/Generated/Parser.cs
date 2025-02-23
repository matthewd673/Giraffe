namespace ExampleRecognizer.Generated;
public class Parser(Scanner scanner)
{
    public ParseTree Parse()
    {
        if (See(TokenKind.a, TokenKind.b, TokenKind.c))
        {
            Nonterminal s0 = ParseS();
            Token s1 = Eat(TokenKind._eof);
            return new([s0]);
        }

        throw new ParserException($"Cannot parse {{Start}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private bool See(params TokenKind[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(TokenKind terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'");
    private Nonterminal ParseS()
    {
        if (See(TokenKind.a, TokenKind.b, TokenKind.c))
        {
            Console.WriteLine("Semantic action!");
            Nonterminal s0 = ParseA();
            Nonterminal s1 = ParseB();
            Nonterminal s2 = ParseC(s0, s1);
            Nonterminal s3 = ParseD();
            Nonterminal s4 = ParseE();
            Console.WriteLine("Done :D");
            return new(NtKind.S, [s0, s1, s2, s3, s4]);
        }

        throw new ParserException($"Cannot parse Start, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private Nonterminal ParseA()
    {
        if (See(TokenKind.a))
        {
            Token s0 = Eat(TokenKind.a);
            return new(NtKind.A, [s0]);
        }

        if (See(TokenKind.b, TokenKind.c))
        {
            return new(NtKind.A, []);
        }

        throw new ParserException($"Cannot parse A, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}");
    }

    private Nonterminal ParseB()
    {
        if (See(TokenKind.b))
        {
            Token s0 = Eat(TokenKind.b);
            return new(NtKind.B, [s0]);
        }

        if (See(TokenKind.c))
        {
            return new(NtKind.B, []);
        }

        throw new ParserException($"Cannot parse B, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{b, c}}");
    }

    private Nonterminal ParseC(ParseNode a0, ParseNode a1)
    {
        if (See(TokenKind.c))
        {
            Console.WriteLine("See C");
            Token s0 = Eat(TokenKind.c);
            return new(NtKind.C, [a0, s0, a1]);
        }

        throw new ParserException($"Cannot parse C, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{c}}");
    }

    private Nonterminal ParseD()
    {
        if (See(TokenKind.d))
        {
            Token s0 = Eat(TokenKind.d);
            return new(NtKind.D, [s0]);
        }

        if (See(TokenKind.e, TokenKind._eof))
        {
            return new(NtKind.D, []);
        }

        throw new ParserException($"Cannot parse D, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{d, e, <end of input>}}");
    }

    private Nonterminal ParseE()
    {
        if (See(TokenKind.e))
        {
            Token s0 = Eat(TokenKind.e);
            return new(NtKind.E, [s0]);
        }

        if (See(TokenKind._eof))
        {
            return new(NtKind.E, []);
        }

        throw new ParserException($"Cannot parse E, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{e, <end of input>}}");
    }
}