namespace ExampleRecognizer.Generated;
public class Parser(Scanner scanner)
{
    public ParseTree Parse()
    {
        if (See(TokenKind.A, TokenKind.B, TokenKind.C))
        {
            Nonterminal s0 = ParseS();
            Token s1 = Eat(TokenKind.Eof);
            return new([s0, s1]);
        }

        throw new ParserException($"Cannot parse {{S}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private bool See(params TokenKind[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(TokenKind terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    private Nonterminal ParseS()
    {
        if (See(TokenKind.A, TokenKind.B, TokenKind.C))
        {
            Nonterminal s0 = ParseOptA();
            Nonterminal s1 = ParseOptB();
            Nonterminal s2 = ParseReqC();
            Nonterminal s3 = ParseOptD();
            Nonterminal s4 = ParseOptE();
            return new(NtKind.S, [s0, s1, s2, s3, s4], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse S, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseOptA()
    {
        if (See(TokenKind.A))
        {
            Token s0 = Eat(TokenKind.A);
            return new(NtKind.OptA, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.B, TokenKind.C))
        {
            return new(NtKind.OptA, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse OPT_A, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{a, b, c}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseOptB()
    {
        if (See(TokenKind.B))
        {
            Token s0 = Eat(TokenKind.B);
            return new(NtKind.OptB, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.C))
        {
            return new(NtKind.OptB, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse OPT_B, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{b, c}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseReqC()
    {
        if (See(TokenKind.C))
        {
            Token s0 = Eat(TokenKind.C);
            return new(NtKind.ReqC, [s0], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse REQ_C, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{c}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseOptD()
    {
        if (See(TokenKind.D))
        {
            Token s0 = Eat(TokenKind.D);
            return new(NtKind.OptD, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.E, TokenKind.Eof))
        {
            return new(NtKind.OptD, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse OPT_D, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{d, e, eof}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseOptE()
    {
        if (See(TokenKind.E))
        {
            Token s0 = Eat(TokenKind.E);
            return new(NtKind.OptE, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.Eof))
        {
            return new(NtKind.OptE, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse OPT_E, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{e, eof}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }
}