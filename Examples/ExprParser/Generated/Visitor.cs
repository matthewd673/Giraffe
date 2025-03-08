namespace ExprParser.Generated;
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
        NtKind.EXPR => VisitEXPR(nonterminal.Children),
        NtKind.E1 => VisitE1(nonterminal.Children),
        NtKind.E1T => VisitE1T(nonterminal.Children),
        NtKind.E2 => VisitE2(nonterminal.Children),
        NtKind.E2T => VisitE2T(nonterminal.Children),
        NtKind.E3 => VisitE3(nonterminal.Children),
        NtKind.AO => VisitAO(nonterminal.Children),
        NtKind.MO => VisitMO(nonterminal.Children),
        _ => throw new ArgumentOutOfRangeException(),
    };
    public T Visit(Token token) => token.Kind switch
    {
        TokenKind.number => Visitnumber(token),
        TokenKind.add => Visitadd(token),
        TokenKind.sub => Visitsub(token),
        TokenKind.mul => Visitmul(token),
        TokenKind.div => Visitdiv(token),
        TokenKind.ws => Visitws(token),
        TokenKind.eof => Visiteof(token),
        _ => throw new ArgumentOutOfRangeException(),
    };
    protected abstract T VisitEXPR(ParseNode[] children);
    protected abstract T VisitE1(ParseNode[] children);
    protected abstract T VisitE1T(ParseNode[] children);
    protected abstract T VisitE2(ParseNode[] children);
    protected abstract T VisitE2T(ParseNode[] children);
    protected abstract T VisitE3(ParseNode[] children);
    protected abstract T VisitAO(ParseNode[] children);
    protected abstract T VisitMO(ParseNode[] children);
    protected abstract T Visitnumber(Token token);
    protected abstract T Visitadd(Token token);
    protected abstract T Visitsub(Token token);
    protected abstract T Visitmul(Token token);
    protected abstract T Visitdiv(Token token);
    protected abstract T Visitws(Token token);
    protected abstract T Visiteof(Token token);
}