namespace ExampleRecognizer.Generated;
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
        NtKind.S => VisitS(nonterminal),
        NtKind.OptA => VisitOptA(nonterminal),
        NtKind.OptB => VisitOptB(nonterminal),
        NtKind.ReqC => VisitReqC(nonterminal),
        NtKind.OptD => VisitOptD(nonterminal),
        NtKind.OptE => VisitOptE(nonterminal),
        _ => throw new ArgumentOutOfRangeException(),
    };
    public T Visit(Token token) => token.Kind switch
    {
        TokenKind.A => VisitA(token),
        TokenKind.B => VisitB(token),
        TokenKind.C => VisitC(token),
        TokenKind.D => VisitD(token),
        TokenKind.E => VisitE(token),
        TokenKind.Eof => VisitEof(token),
        _ => throw new ArgumentOutOfRangeException(),
    };
    protected abstract T VisitS(Nonterminal s);
    protected abstract T VisitOptA(Nonterminal optA);
    protected abstract T VisitOptB(Nonterminal optB);
    protected abstract T VisitReqC(Nonterminal reqC);
    protected abstract T VisitOptD(Nonterminal optD);
    protected abstract T VisitOptE(Nonterminal optE);
    protected abstract T VisitA(Token token);
    protected abstract T VisitB(Token token);
    protected abstract T VisitC(Token token);
    protected abstract T VisitD(Token token);
    protected abstract T VisitE(Token token);
    protected abstract T VisitEof(Token token);
}