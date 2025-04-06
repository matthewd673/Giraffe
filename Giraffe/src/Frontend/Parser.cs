namespace Giraffe.Frontend;
public class Parser(Scanner scanner)
{
    public ParseTree Parse()
    {
        if (See(TokenKind.KwProperties))
        {
            Nonterminal s0 = ParseFile();
            Token s1 = Eat(TokenKind.Eof);
            return new([s0, s1]);
        }

        throw new ParserException($"Cannot parse {{FILE}}, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{kw_properties}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private bool See(params TokenKind[] terminals) => terminals.Contains(scanner.Peek().Kind);
    private Token Eat(TokenKind terminal) => See(terminal) ? scanner.Eat() : throw new ParserException($"Unexpected terminal, saw '{scanner.NameOf(scanner.Peek().Kind)}' but expected '{scanner.NameOf(terminal)}'", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    private Nonterminal ParseFile()
    {
        if (See(TokenKind.KwProperties))
        {
            Token s0 = Eat(TokenKind.KwProperties);
            Nonterminal s1 = ParseProperties();
            Token s2 = Eat(TokenKind.KwGrammar);
            Nonterminal s3 = ParseGrammar();
            return new(NtKind.File, [s0, s1, s2, s3], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse FILE, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{kw_properties}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseProperties()
    {
        if (See(TokenKind.TermName, TokenKind.KwGrammar))
        {
            Nonterminal s0 = ParsePropertyDefT();
            return new(NtKind.Properties, [..s0.Children], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse PROPERTIES, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, kw_grammar}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParsePropertyDef()
    {
        if (See(TokenKind.TermName))
        {
            Token s0 = Eat(TokenKind.TermName);
            Token s1 = Eat(TokenKind.Colon);
            Token s2 = Eat(TokenKind.String);
            return new(NtKind.PropertyDef, [s0, s1, s2], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse PROPERTY_DEF, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParsePropertyDefT()
    {
        if (See(TokenKind.TermName))
        {
            Nonterminal s0 = ParsePropertyDef();
            Nonterminal s1 = ParsePropertyDefT();
            return new(NtKind.PropertyDefT, [s0, ..s1.Children], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.KwGrammar))
        {
            return new(NtKind.PropertyDefT, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse PROPERTY_DEF_T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, kw_grammar}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseGrammar()
    {
        if (See(TokenKind.TermName, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseAnyDef();
            Nonterminal s1 = ParseAnyDefT();
            return new(NtKind.Grammar, [s0, ..s1.Children], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse GRAMMAR, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, nonterm_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseAnyDef()
    {
        if (See(TokenKind.TermName))
        {
            Nonterminal s0 = ParseTermDef();
            return new(NtKind.AnyDef, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.NontermName))
        {
            Nonterminal s0 = ParseNontermDef();
            return new(NtKind.AnyDef, [s0], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse ANY_DEF, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, nonterm_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseAnyDefT()
    {
        if (See(TokenKind.TermName, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseAnyDef();
            Nonterminal s1 = ParseAnyDefT();
            return new(NtKind.AnyDefT, [s0, ..s1.Children], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.Eof))
        {
            return new(NtKind.AnyDefT, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse ANY_DEF_T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, nonterm_name, eof}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseTermDef()
    {
        if (See(TokenKind.TermName))
        {
            Token s0 = Eat(TokenKind.TermName);
            Nonterminal s1 = ParseOptDiscard();
            Token s2 = Eat(TokenKind.Arrow);
            Nonterminal s3 = ParseTermRhs();
            Token s4 = Eat(TokenKind.End);
            return new(NtKind.TermDef, [s0, s1, s3], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse TERM_DEF, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseTermRhs()
    {
        if (See(TokenKind.Regex))
        {
            Token s0 = Eat(TokenKind.Regex);
            return new(NtKind.TermRhs, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.String))
        {
            Token s0 = Eat(TokenKind.String);
            return new(NtKind.TermRhs, [s0], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse TERM_RHS, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{regex, string}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseNontermDef()
    {
        if (See(TokenKind.NontermName))
        {
            Token s0 = Eat(TokenKind.NontermName);
            Nonterminal s1 = ParseOptKwEntry();
            Nonterminal s2 = ParseRule();
            Nonterminal s3 = ParseRuleT();
            Token s4 = Eat(TokenKind.End);
            return new(NtKind.NontermDef, [s0, s1, s2, ..s3.Children], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse NONTERM_DEF, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{nonterm_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseRule()
    {
        if (See(TokenKind.Arrow))
        {
            Token s0 = Eat(TokenKind.Arrow);
            Nonterminal s1 = ParseSymbols();
            return new(NtKind.Rule, [..s1.Children], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse RULE, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{arrow}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseRuleT()
    {
        if (See(TokenKind.Arrow))
        {
            Nonterminal s0 = ParseRule();
            Nonterminal s1 = ParseRuleT();
            return new(NtKind.RuleT, [s0, ..s1.Children], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.End))
        {
            return new(NtKind.RuleT, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse RULE_T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{arrow, end}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseSymbols()
    {
        if (See(TokenKind.Discard, TokenKind.TermName, TokenKind.Expand, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseSymbol();
            Nonterminal s1 = ParseSymbols();
            return new(NtKind.Symbols, [s0, ..s1.Children], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.Arrow, TokenKind.End))
        {
            return new(NtKind.Symbols, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse SYMBOLS, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{discard, term_name, expand, nonterm_name, arrow, end}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseSymbol()
    {
        if (See(TokenKind.Discard, TokenKind.TermName, TokenKind.Expand, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseOptDiscard();
            Nonterminal s1 = ParseSymbolT();
            return new(NtKind.Symbol, [s0, ..s1.Children], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse SYMBOL, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{discard, term_name, expand, nonterm_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseSymbolT()
    {
        if (See(TokenKind.TermName))
        {
            Token s0 = Eat(TokenKind.TermName);
            return new(NtKind.SymbolT, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.Expand, TokenKind.NontermName))
        {
            Nonterminal s0 = ParseOptExpand();
            Token s1 = Eat(TokenKind.NontermName);
            return new(NtKind.SymbolT, [s0, s1], s0.Index, s0.Row, s0.Column);
        }

        throw new ParserException($"Cannot parse SYMBOL_T, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{term_name, expand, nonterm_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseOptKwEntry()
    {
        if (See(TokenKind.KwEntry))
        {
            Token s0 = Eat(TokenKind.KwEntry);
            return new(NtKind.OptKwEntry, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.Arrow))
        {
            return new(NtKind.OptKwEntry, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse OPT_KW_ENTRY, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{kw_entry, arrow}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseOptExpand()
    {
        if (See(TokenKind.Expand))
        {
            Token s0 = Eat(TokenKind.Expand);
            return new(NtKind.OptExpand, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.NontermName))
        {
            return new(NtKind.OptExpand, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse OPT_EXPAND, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{expand, nonterm_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }

    private Nonterminal ParseOptDiscard()
    {
        if (See(TokenKind.Discard))
        {
            Token s0 = Eat(TokenKind.Discard);
            return new(NtKind.OptDiscard, [s0], s0.Index, s0.Row, s0.Column);
        }

        if (See(TokenKind.Arrow, TokenKind.TermName, TokenKind.Expand, TokenKind.NontermName))
        {
            return new(NtKind.OptDiscard, [], -1, -1, -1);
        }

        throw new ParserException($"Cannot parse OPT_DISCARD, saw {scanner.NameOf(scanner.Peek().Kind)} but expected one of {{discard, arrow, term_name, expand, nonterm_name}}", scanner.Peek().Index, scanner.Peek().Row, scanner.Peek().Column);
    }
}