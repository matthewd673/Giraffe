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
        NtKind.Grammar => VisitGrammar(nonterminal.Children),
        NtKind.AnyDef => VisitAnyDef(nonterminal.Children),
        NtKind.TermDef => VisitTermDef(nonterminal.Children),
        NtKind.TermRhs => VisitTermRhs(nonterminal.Children),
        NtKind.NontermDef => VisitNontermDef(nonterminal.Children),
        NtKind.Rule => VisitRule(nonterminal.Children),
        NtKind.Symbols => VisitSymbols(nonterminal.Children),
        NtKind.Symbol => VisitSymbol(nonterminal.Children),
        NtKind.OptStar => VisitOptStar(nonterminal.Children),
        NtKind.OptExpand => VisitOptExpand(nonterminal.Children),
        NtKind.OptDiscard => VisitOptDiscard(nonterminal.Children),
        _ => throw new ArgumentOutOfRangeException(),
    };
    public T Visit(Token token) => token.Kind switch
    {
        TokenKind.TermName => VisitTermName(token),
        TokenKind.NontermName => VisitNontermName(token),
        TokenKind.Regex => VisitRegex(token),
        TokenKind.Star => VisitStar(token),
        TokenKind.Expand => VisitExpand(token),
        TokenKind.Discard => VisitDiscard(token),
        TokenKind.Eof => VisitEof(token),
        _ => throw new ArgumentOutOfRangeException(),
    };
    protected abstract T VisitGrammar(ParseNode[] children);
    protected abstract T VisitAnyDef(ParseNode[] children);
    protected abstract T VisitTermDef(ParseNode[] children);
    protected abstract T VisitTermRhs(ParseNode[] children);
    protected abstract T VisitNontermDef(ParseNode[] children);
    protected abstract T VisitRule(ParseNode[] children);
    protected abstract T VisitSymbols(ParseNode[] children);
    protected abstract T VisitSymbol(ParseNode[] children);
    protected abstract T VisitOptStar(ParseNode[] children);
    protected abstract T VisitOptExpand(ParseNode[] children);
    protected abstract T VisitOptDiscard(ParseNode[] children);
    protected abstract T VisitTermName(Token token);
    protected abstract T VisitNontermName(Token token);
    protected abstract T VisitRegex(Token token);
    protected abstract T VisitStar(Token token);
    protected abstract T VisitExpand(Token token);
    protected abstract T VisitDiscard(Token token);
    protected abstract T VisitEof(Token token);
}