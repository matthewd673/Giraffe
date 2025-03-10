namespace Giraffe.Frontend;
public class Parser(Scanner scanner)
{
    public ParseTree Parse()
    {
        if (See(TokenKind.TermName, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseGrammar();
            Token s1 = Eat(TokenKind.Eof);
            return new([s0, s1]);
        }

        throw new ParserException($"Cannot parse {{GRAMMAR}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, nonterm_name}}");
    }

    private bool See(params TokenKind[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(TokenKind terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'");
    private Nonterminal ParseGrammar()
    {
        if (See(TokenKind.TermName, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseAnyDef();
            Nonterminal s1 = ParseAnyDefT();
            return new(NtKind.Grammar, [s0, ..s1.Children]);
        }

        throw new ParserException($"Cannot parse GRAMMAR, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, nonterm_name}}");
    }

    private Nonterminal ParseAnyDef()
    {
        if (See(TokenKind.TermName))
        {
            Nonterminal s0 = ParseTermDef();
            return new(NtKind.AnyDef, [s0]);
        }

        if (See(TokenKind.NontermName))
        {
            Nonterminal s0 = ParseNontermDef();
            return new(NtKind.AnyDef, [s0]);
        }

        throw new ParserException($"Cannot parse ANY_DEF, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, nonterm_name}}");
    }

    private Nonterminal ParseAnyDefT()
    {
        if (See(TokenKind.TermName, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseAnyDef();
            Nonterminal s1 = ParseAnyDefT();
            return new(NtKind.AnyDefT, [s0, ..s1.Children]);
        }

        if (See(TokenKind.Eof))
        {
            return new(NtKind.AnyDefT, []);
        }

        throw new ParserException($"Cannot parse ANY_DEF_T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, nonterm_name, <end of input>}}");
    }

    private Nonterminal ParseTermDef()
    {
        if (See(TokenKind.TermName))
        {
            Token s0 = Eat(TokenKind.TermName);
            Token s1 = Eat(TokenKind.Arrow);
            Nonterminal s2 = ParseTermRhs();
            Token s3 = Eat(TokenKind.End);
            return new(NtKind.TermDef, [s0, s1, s2, s3]);
        }

        throw new ParserException($"Cannot parse TERM_DEF, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name}}");
    }

    private Nonterminal ParseTermRhs()
    {
        if (See(TokenKind.Discard, TokenKind.Regex))
        {
            Nonterminal s0 = ParseOptDiscard();
            Token s1 = Eat(TokenKind.Regex);
            return new(NtKind.TermRhs, [..s0.Children, s1]);
        }

        throw new ParserException($"Cannot parse TERM_RHS, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{discard, regex}}");
    }

    private Nonterminal ParseNontermDef()
    {
        if (See(TokenKind.NontermName))
        {
            Token s0 = Eat(TokenKind.NontermName);
            Nonterminal s1 = ParseOptStar();
            Nonterminal s2 = ParseRule();
            Nonterminal s3 = ParseRuleT();
            Token s4 = Eat(TokenKind.End);
            return new(NtKind.NontermDef, [s0, s1, s2, ..s3.Children, s4]);
        }

        throw new ParserException($"Cannot parse NONTERM_DEF, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{nonterm_name}}");
    }

    private Nonterminal ParseRule()
    {
        if (See(TokenKind.Arrow))
        {
            Token s0 = Eat(TokenKind.Arrow);
            Nonterminal s1 = ParseSymbols();
            return new(NtKind.Rule, [s0, s1]);
        }

        throw new ParserException($"Cannot parse RULE, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{arrow}}");
    }

    private Nonterminal ParseRuleT()
    {
        if (See(TokenKind.Arrow))
        {
            Nonterminal s0 = ParseRule();
            Nonterminal s1 = ParseRuleT();
            return new(NtKind.RuleT, [s0, ..s1.Children]);
        }

        if (See(TokenKind.End))
        {
            return new(NtKind.RuleT, []);
        }

        throw new ParserException($"Cannot parse RULE_T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{arrow, end}}");
    }

    private Nonterminal ParseSymbols()
    {
        if (See(TokenKind.TermName, TokenKind.Expand, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseSymbol();
            Nonterminal s1 = ParseSymbols();
            return new(NtKind.Symbols, [s0, ..s1.Children]);
        }

        if (See(TokenKind.Arrow, TokenKind.End))
        {
            return new(NtKind.Symbols, []);
        }

        throw new ParserException($"Cannot parse SYMBOLS, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, expand, nonterm_name, arrow, end}}");
    }

    private Nonterminal ParseSymbol()
    {
        if (See(TokenKind.TermName))
        {
            Token s0 = Eat(TokenKind.TermName);
            return new(NtKind.Symbol, [s0]);
        }

        if (See(TokenKind.Expand, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseOptExpand();
            Token s1 = Eat(TokenKind.NontermName);
            return new(NtKind.Symbol, [..s0.Children, s1]);
        }

        throw new ParserException($"Cannot parse SYMBOL, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, expand, nonterm_name}}");
    }

    private Nonterminal ParseOptStar()
    {
        if (See(TokenKind.Star))
        {
            Token s0 = Eat(TokenKind.Star);
            return new(NtKind.OptStar, [s0]);
        }

        if (See(TokenKind.Arrow))
        {
            return new(NtKind.OptStar, []);
        }

        throw new ParserException($"Cannot parse OPT_STAR, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{star, arrow}}");
    }

    private Nonterminal ParseOptExpand()
    {
        if (See(TokenKind.Expand))
        {
            Token s0 = Eat(TokenKind.Expand);
            return new(NtKind.OptExpand, [s0]);
        }

        if (See(TokenKind.NontermName))
        {
            return new(NtKind.OptExpand, []);
        }

        throw new ParserException($"Cannot parse OPT_EXPAND, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{expand, nonterm_name}}");
    }

    private Nonterminal ParseOptDiscard()
    {
        if (See(TokenKind.Discard))
        {
            Token s0 = Eat(TokenKind.Discard);
            return new(NtKind.OptDiscard, [s0]);
        }

        if (See(TokenKind.Regex))
        {
            return new(NtKind.OptDiscard, []);
        }

        throw new ParserException($"Cannot parse OPT_DISCARD, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{discard, regex}}");
    }
}