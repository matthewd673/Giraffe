using ExprParser.Generated;

namespace ExprParser;

public class ExprVisitor : Visitor<int> {
  public override int Visit(ParseTree parseTree) => Visit(parseTree.Children[0]);

  protected override int VisitExpr(ParseNode[] children) => children switch {
    [Nonterminal { Kind: NtKind.E1 } e1] => Visit(e1),
    _ => throw new("Unexpected children in EXPR"),
  };

  protected override int VisitE1(ParseNode[] children) {
    int sum = 0;
    char nextOp = '+'; // When we see the first operand, add it to the total of 0

    foreach (ParseNode n in children) {
      if (n is Nonterminal { Kind: NtKind.E2 } operand) {
        sum = ApplyOperator(sum, Visit(operand), nextOp);
      }
      else if (n is Nonterminal { Kind: NtKind.Ao } op) {
        nextOp = (char)Visit(op);
      }
      else {
        throw new($"Unexpected child in E1: {n}");
      }
    }

    return sum;
  }
  protected override int VisitE1T(ParseNode[] children) {
    throw new NotImplementedException();
  }
  protected override int VisitE2(ParseNode[] children) {
    int product = 1;
    char nextOp = '*'; // When we see the first operand, multiply it by 1

    foreach (ParseNode n in children) {
      if (n is Nonterminal { Kind: NtKind.E3 } operand) {
        product = ApplyOperator(product, Visit(operand), nextOp);
      }
      else if (n is Nonterminal { Kind: NtKind.Mo } op) {
        nextOp = (char)Visit(op);
      }
      else {
        throw new($"Unexpected child in E2: {n}");
      }
    }

    return product;
  }
  protected override int VisitE2T(ParseNode[] children) {
    throw new NotImplementedException();
  }
  protected override int VisitE3(ParseNode[] children) => children switch {
    [Token {Kind: TokenKind.Number} t] => Visit(t),
    _ => throw new("Unexpected children in E3"),
  };

  protected override int VisitAo(ParseNode[] children) => Visit(children[0]);
  protected override int VisitMo(ParseNode[] children) => Visit(children[0]);
  protected override int VisitNumber(Token token) => Convert.ToInt32(token.Image);
  protected override int VisitAdd(Token token) => '+';
  protected override int VisitSub(Token token) => '-';
  protected override int VisitMul(Token token) => '*';
  protected override int VisitDiv(Token token) => '/';

  protected override int VisitWs(Token token) {
    throw new NotImplementedException();
  }
  protected override int VisitEof(Token token) {
    throw new NotImplementedException();
  }

  private static int ApplyOperator(int left, int right, char op) => op switch {
    '+' => left + right,
    '-' => left - right,
    '*' => left * right,
    '/' => left / right,
    _ => throw new($"Invalid operator '{op}'"),
  };
}