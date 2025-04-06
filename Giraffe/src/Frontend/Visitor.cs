namespace Giraffe.Frontend;
public abstract class Visitor<T>
{
    public abstract T Visit(ParseTree parseTree);
    public T Visit(ParseNode parseNode) => parseNode switch
    {
        Nonterminal nt => Visit(nt),
        Token t => Visit(t),
        _ => throw new ArgumentOutOfRangeException(),
    };
    public T Visit(Nonterminal nonterminal) => nonterminal.Kind switch
    {
        NtKind.File => VisitFile(nonterminal),
        NtKind.Properties => VisitProperties(nonterminal),
        NtKind.PropertyDef => VisitPropertyDef(nonterminal),
        NtKind.Grammar => VisitGrammar(nonterminal),
        NtKind.AnyDef => VisitAnyDef(nonterminal),
        NtKind.TermDef => VisitTermDef(nonterminal),
        NtKind.TermRhs => VisitTermRhs(nonterminal),
        NtKind.NontermDef => VisitNontermDef(nonterminal),
        NtKind.Rule => VisitRule(nonterminal),
        NtKind.Symbol => VisitSymbol(nonterminal),
        NtKind.OptKwEntry => VisitOptKwEntry(nonterminal),
        NtKind.OptExpand => VisitOptExpand(nonterminal),
        NtKind.OptDiscard => VisitOptDiscard(nonterminal),
        _ => throw new ArgumentOutOfRangeException(),
    };
    public T Visit(Token token) => token.Kind switch
    {
        TokenKind.KwProperties => VisitKwProperties(token),
        TokenKind.KwGrammar => VisitKwGrammar(token),
        TokenKind.KwEntry => VisitKwEntry(token),
        TokenKind.TermName => VisitTermName(token),
        TokenKind.NontermName => VisitNontermName(token),
        TokenKind.Regex => VisitRegex(token),
        TokenKind.String => VisitString(token),
        TokenKind.Expand => VisitExpand(token),
        TokenKind.Discard => VisitDiscard(token),
        TokenKind.Colon => VisitColon(token),
        TokenKind.Eof => VisitEof(token),
        _ => throw new ArgumentOutOfRangeException(),
    };
    protected abstract T VisitFile(Nonterminal file);
    protected abstract T VisitProperties(Nonterminal properties);
    protected abstract T VisitPropertyDef(Nonterminal propertyDef);
    protected abstract T VisitGrammar(Nonterminal grammar);
    protected abstract T VisitAnyDef(Nonterminal anyDef);
    protected abstract T VisitTermDef(Nonterminal termDef);
    protected abstract T VisitTermRhs(Nonterminal termRhs);
    protected abstract T VisitNontermDef(Nonterminal nontermDef);
    protected abstract T VisitRule(Nonterminal rule);
    protected abstract T VisitSymbol(Nonterminal symbol);
    protected abstract T VisitOptKwEntry(Nonterminal optKwEntry);
    protected abstract T VisitOptExpand(Nonterminal optExpand);
    protected abstract T VisitOptDiscard(Nonterminal optDiscard);
    protected abstract T VisitKwProperties(Token token);
    protected abstract T VisitKwGrammar(Token token);
    protected abstract T VisitKwEntry(Token token);
    protected abstract T VisitTermName(Token token);
    protected abstract T VisitNontermName(Token token);
    protected abstract T VisitRegex(Token token);
    protected abstract T VisitString(Token token);
    protected abstract T VisitExpand(Token token);
    protected abstract T VisitDiscard(Token token);
    protected abstract T VisitColon(Token token);
    protected abstract T VisitEof(Token token);
}