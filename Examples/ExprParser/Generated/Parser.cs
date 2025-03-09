namespace ExprParser.Generated;
public class Parser(Scanner scanner)
{
    public ParseTree Parse()
    {
        if (See(TokenKind.Number))
        {
            Nonterminal s0 = ParseExpr();
            Token s1 = Eat(TokenKind.Eof);
            return new([s0, s1]);
        }

        throw new ParserException($"Cannot parse {{EXPR}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private bool See(params TokenKind[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(TokenKind terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'");
    private Nonterminal ParseExpr()
    {
        if (See(TokenKind.Number))
        {
            Nonterminal s0 = ParseE1();
            return new(NtKind.Expr, [s0]);
        }

        throw new ParserException($"Cannot parse EXPR, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseE1()
    {
        if (See(TokenKind.Number))
        {
            Nonterminal s0 = ParseE2();
            Nonterminal s1 = ParseE1t();
            return new(NtKind.E1, [s0, ..s1.Children]);
        }

        throw new ParserException($"Cannot parse E1, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseE1t()
    {
        if (See(TokenKind.Add, TokenKind.Sub))
        {
            Nonterminal s0 = ParseAo();
            Nonterminal s1 = ParseE2();
            Nonterminal s2 = ParseE1t();
            return new(NtKind.E1t, [s0, s1, ..s2.Children]);
        }

        if (See(TokenKind.Eof))
        {
            return new(NtKind.E1t, []);
        }

        throw new ParserException($"Cannot parse E1T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{add, sub, <end of input>}}");
    }

    private Nonterminal ParseE2()
    {
        if (See(TokenKind.Number))
        {
            Nonterminal s0 = ParseE3();
            Nonterminal s1 = ParseE2t();
            return new(NtKind.E2, [s0, ..s1.Children]);
        }

        throw new ParserException($"Cannot parse E2, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseE2t()
    {
        if (See(TokenKind.Mul, TokenKind.Div))
        {
            Nonterminal s0 = ParseMo();
            Nonterminal s1 = ParseE3();
            Nonterminal s2 = ParseE2t();
            return new(NtKind.E2t, [s0, s1, ..s2.Children]);
        }

        if (See(TokenKind.Add, TokenKind.Sub, TokenKind.Eof))
        {
            return new(NtKind.E2t, []);
        }

        throw new ParserException($"Cannot parse E2T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{mul, div, add, sub, <end of input>}}");
    }

    private Nonterminal ParseE3()
    {
        if (See(TokenKind.Number))
        {
            Token s0 = Eat(TokenKind.Number);
            return new(NtKind.E3, [s0]);
        }

        throw new ParserException($"Cannot parse E3, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{number}}");
    }

    private Nonterminal ParseAo()
    {
        if (See(TokenKind.Add))
        {
            Token s0 = Eat(TokenKind.Add);
            return new(NtKind.Ao, [s0]);
        }

        if (See(TokenKind.Sub))
        {
            Token s0 = Eat(TokenKind.Sub);
            return new(NtKind.Ao, [s0]);
        }

        throw new ParserException($"Cannot parse AO, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{add, sub}}");
    }

    private Nonterminal ParseMo()
    {
        if (See(TokenKind.Mul))
        {
            Token s0 = Eat(TokenKind.Mul);
            return new(NtKind.Mo, [s0]);
        }

        if (See(TokenKind.Div))
        {
            Token s0 = Eat(TokenKind.Div);
            return new(NtKind.Mo, [s0]);
        }

        throw new ParserException($"Cannot parse MO, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{mul, div}}");
    }
}