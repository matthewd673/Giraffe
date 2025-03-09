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
        NtKind.Expr => VisitExpr(nonterminal.Children),
        NtKind.E1 => VisitE1(nonterminal.Children),
        NtKind.E1t => VisitE1t(nonterminal.Children),
        NtKind.E2 => VisitE2(nonterminal.Children),
        NtKind.E2t => VisitE2t(nonterminal.Children),
        NtKind.E3 => VisitE3(nonterminal.Children),
        NtKind.Ao => VisitAo(nonterminal.Children),
        NtKind.Mo => VisitMo(nonterminal.Children),
        _ => throw new ArgumentOutOfRangeException(),
    };
    public T Visit(Token token) => token.Kind switch
    {
        TokenKind.Number => VisitNumber(token),
        TokenKind.Add => VisitAdd(token),
        TokenKind.Sub => VisitSub(token),
        TokenKind.Mul => VisitMul(token),
        TokenKind.Div => VisitDiv(token),
        TokenKind.Ws => VisitWs(token),
        TokenKind.Eof => VisitEof(token),
        _ => throw new ArgumentOutOfRangeException(),
    };
    protected abstract T VisitExpr(ParseNode[] children);
    protected abstract T VisitE1(ParseNode[] children);
    protected abstract T VisitE1t(ParseNode[] children);
    protected abstract T VisitE2(ParseNode[] children);
    protected abstract T VisitE2t(ParseNode[] children);
    protected abstract T VisitE3(ParseNode[] children);
    protected abstract T VisitAo(ParseNode[] children);
    protected abstract T VisitMo(ParseNode[] children);
    protected abstract T VisitNumber(Token token);
    protected abstract T VisitAdd(Token token);
    protected abstract T VisitSub(Token token);
    protected abstract T VisitMul(Token token);
    protected abstract T VisitDiv(Token token);
    protected abstract T VisitWs(Token token);
    protected abstract T VisitEof(Token token);
}