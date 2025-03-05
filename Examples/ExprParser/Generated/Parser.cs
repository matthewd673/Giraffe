namespace ExprParser.Generated;
public class Parser(Scanner scanner)
{
    public ParseTree Parse()
    {
        if (See(TokenKind.number))
        {
            Nonterminal s0 = ParseEXPR();
            Token s1 = Eat(TokenKind.eof);
            return new([s0, s1]);
        }

        throw new ParserException($"Cannot parse {{EXPR}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private bool See(params TokenKind[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(TokenKind terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'");
    private Nonterminal ParseEXPR()
    {
        if (See(TokenKind.number))
        {
            Nonterminal s0 = ParseE1();
            return new(NtKind.EXPR, [s0]);
        }

        throw new ParserException($"Cannot parse EXPR, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseE1()
    {
        if (See(TokenKind.number))
        {
            Nonterminal s0 = ParseE2();
            Nonterminal s1 = ParseE1T();
            return new(NtKind.E1, [s0, ..s1.Children]);
        }

        throw new ParserException($"Cannot parse E1, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseE1T()
    {
        if (See(TokenKind.add, TokenKind.sub))
        {
            Nonterminal s0 = ParseAO();
            Nonterminal s1 = ParseE2();
            Nonterminal s2 = ParseE1T();
            return new(NtKind.E1T, [s0, s1, ..s2.Children]);
        }

        if (See(TokenKind.eof))
        {
            return new(NtKind.E1T, []);
        }

        throw new ParserException($"Cannot parse E1T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{add, sub, <end of input>}}");
    }

    private Nonterminal ParseE2()
    {
        if (See(TokenKind.number))
        {
            Nonterminal s0 = ParseE3();
            Nonterminal s1 = ParseE2T();
            return new(NtKind.E2, [s0, ..s1.Children]);
        }

        throw new ParserException($"Cannot parse E2, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseE2T()
    {
        if (See(TokenKind.mul, TokenKind.div))
        {
            Nonterminal s0 = ParseMO();
            Nonterminal s1 = ParseE3();
            Nonterminal s2 = ParseE2T();
            return new(NtKind.E2T, [s0, s1, ..s2.Children]);
        }

        if (See(TokenKind.add, TokenKind.sub, TokenKind.eof))
        {
            return new(NtKind.E2T, []);
        }

        throw new ParserException($"Cannot parse E2T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{mul, div, add, sub, <end of input>}}");
    }

    private Nonterminal ParseE3()
    {
        if (See(TokenKind.number))
        {
            Token s0 = Eat(TokenKind.number);
            return new(NtKind.E3, [s0]);
        }

        throw new ParserException($"Cannot parse E3, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseAO()
    {
        if (See(TokenKind.add))
        {
            Token s0 = Eat(TokenKind.add);
            return new(NtKind.AO, [s0]);
        }

        if (See(TokenKind.sub))
        {
            Token s0 = Eat(TokenKind.sub);
            return new(NtKind.AO, [s0]);
        }

        throw new ParserException($"Cannot parse AO, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{add, sub}}");
    }

    private Nonterminal ParseMO()
    {
        if (See(TokenKind.mul))
        {
            Token s0 = Eat(TokenKind.mul);
            return new(NtKind.MO, [s0]);
        }

        if (See(TokenKind.div))
        {
            Token s0 = Eat(TokenKind.div);
            return new(NtKind.MO, [s0]);
        }

        throw new ParserException($"Cannot parse MO, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{mul, div}}");
    }
}