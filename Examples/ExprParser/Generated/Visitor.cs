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
        TokenKind.number => Visitnumber(token.Image),
        TokenKind.add => Visitadd(token.Image),
        TokenKind.sub => Visitsub(token.Image),
        TokenKind.mul => Visitmul(token.Image),
        TokenKind.div => Visitdiv(token.Image),
        TokenKind.ws => Visitws(token.Image),
        TokenKind.eof => Visiteof(token.Image),
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
    protected abstract T Visitnumber(string image);
    protected abstract T Visitadd(string image);
    protected abstract T Visitsub(string image);
    protected abstract T Visitmul(string image);
    protected abstract T Visitdiv(string image);
    protected abstract T Visitws(string image);
    protected abstract T Visiteof(string image);
}