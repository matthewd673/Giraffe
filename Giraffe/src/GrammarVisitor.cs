using Giraffe.AST;
using Giraffe.Frontend;
using Nonterminal = Giraffe.Frontend.Nonterminal;
using TerminalDefinition = Giraffe.AST.TerminalDefinition;

namespace Giraffe;

public sealed class GrammarVisitor : Visitor<ASTNode> {
  public override GrammarDefinition Visit(ParseTree parseTree) {
    return parseTree.Children switch {
      [Nonterminal { Kind: NtKind.Grammar } grammar, Token { Kind: TokenKind.Eof } _] =>
        (GrammarDefinition)Visit(grammar),
      _ => throw new VisitorException("Cannot visit ParseTree, unexpected children"),
    };
  }

  protected override GrammarDefinition VisitGrammar(ParseNode[] children) {
    List<SymbolDefinition> symbolDefinitions = [];
    foreach (ParseNode child in children) {
      if (child is not Nonterminal nt) {
        throw new VisitorException($"Unexpected child in Grammar, child has type {child.GetType()}");
      }

      if (nt.Kind != NtKind.AnyDef) {
        throw new VisitorException($"Unexpected child in Grammar, child has kind {nt.Kind}");
      }

      symbolDefinitions.Add((SymbolDefinition)Visit(nt));
    }

    return new(symbolDefinitions);
  }

  protected override SymbolDefinition VisitAnyDef(ParseNode[] children) => children switch {
    [Nonterminal { Kind: NtKind.TermDef } termDef] => (SymbolDefinition)Visit(termDef),
    [Nonterminal { Kind: NtKind.NontermDef } nontermDef] => (SymbolDefinition)Visit(nontermDef),
    _ => throw new VisitorException("Cannot visit AnyDef, unexpected children"),
  };

  // NOTE: This nonterminal is always extended so will never be produced
  protected override ASTNode VisitAnyDefT(ParseNode[] children) {
    throw new NotImplementedException();
  }

  protected override TerminalDefinition VisitTermDef(ParseNode[] children) {
    if (children is [Token { Kind: TokenKind.TermName } termName,
                     Nonterminal { Kind: NtKind.OptDiscard } optDiscard,
                     Token { Kind: TokenKind.Arrow } _,
                     Nonterminal { Kind: NtKind.TermRhs } termRhs,
                     Token { Kind: TokenKind.End } _,
                    ]) {
      return new(termName.Image, (TerminalRhs)Visit(termRhs), optDiscard.Children.Length > 0);
    }

    throw new VisitorException("Cannot visit TermDef, unexpected children");
  }

  protected override TerminalRhs VisitTermRhs(ParseNode[] children) => children switch {
    [Token { Kind: TokenKind.Regex } regex] => new(new(CleanRegex(regex.Image))),
    _ => throw new VisitorException("Cannot visit TermRhs, unexpected children"),
  };

  protected override NonterminalDefinition VisitNontermDef(ParseNode[] children) {
    List<RuleDefinition> ruleDefinitions = [];

    if (children[0] is not Token { Kind: TokenKind.NontermName } nontermName) {
      throw new VisitorException("Cannot visit NonterminalDefinition, unexpected children");
    }

    if (children[1] is not Nonterminal { Kind: NtKind.OptStar } optStar) {
      throw new VisitorException("Cannot visit NonterminalDefinition, unexpected children");
    }

    for (int i = 2; i < children.Length; i++) {
      if (children[i] is Token { Kind: TokenKind.End }) {
        break;
      }

      ruleDefinitions.Add((RuleDefinition)Visit(children[i]));
    }

    return new(nontermName.Image, ruleDefinitions, optStar.Children.Length > 0);
  }

  protected override RuleDefinition VisitRule(ParseNode[] children) => children switch {
    [Token { Kind: TokenKind.Arrow} _, Nonterminal { Kind: NtKind.Symbols } symbols] =>
      new(symbols.Children.Select(s => (SymbolUsage)Visit(s)).ToList()),
    _ => throw new VisitorException("Cannot visit Rule, unexpected children"),
  };

  protected override ASTNode VisitRuleT(ParseNode[] children) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitSymbols(ParseNode[] children) {
    throw new NotImplementedException();
  }

  protected override SymbolUsage VisitSymbol(ParseNode[] children) => children switch {
    [Token { Kind: TokenKind.Expand } _, Token { Kind: TokenKind.NontermName } nontermName] =>
      new NonterminalUsage(nontermName.Image, true),
    [Token { Kind: TokenKind.NontermName } nontermName] => new NonterminalUsage(nontermName.Image, false),
    [Token { Kind: TokenKind.TermName } termName] => new TerminalUsage(termName.Image),
    _ => throw new VisitorException("Cannot visit Symbol, unexpected children"),
  };

  protected override ASTNode VisitOptStar(ParseNode[] children) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitOptExpand(ParseNode[] children) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitOptDiscard(ParseNode[] children) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitTermName(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitNontermName(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitArrow(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitEnd(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitRegex(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitStar(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitExpand(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitDiscard(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitWs(Token token) {
    throw new NotImplementedException();
  }

  protected override ASTNode VisitEof(Token token) {
    throw new NotImplementedException();
  }

  private static string CleanRegex(string input) {
    string trimmed = input[1..^1]; // Trim '/' at start and end
    string unEscaped = trimmed.Replace("\\/", "/"); // Un-sanitize escaped '/'
    return unEscaped;
  }
}