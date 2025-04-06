using Giraffe.AST;
using Giraffe.Frontend;
using Giraffe.Utils;
using Nonterminal = Giraffe.Frontend.Nonterminal;
using TerminalDefinition = Giraffe.AST.TerminalDefinition;

namespace Giraffe;

public sealed class GrammarVisitor : Visitor<ASTNode> {
  public override FileDefinition Visit(ParseTree parseTree) {
    return parseTree.Children switch {
      [Nonterminal { Kind: NtKind.File } file, Token { Kind: TokenKind.Eof }] =>
        (FileDefinition)Visit(file),
      _ => throw new VisitorException("Cannot visit ParseTree, unexpected children"),
    };
  }

  protected override FileDefinition VisitFile(Nonterminal file) => file.Children switch {
    [Token { Kind: TokenKind.KwProperties },
     Nonterminal { Kind: NtKind.Properties } properties,
     Token { Kind: TokenKind.KwGrammar },
     Nonterminal { Kind: NtKind.Grammar } grammar] =>
      new((PropertiesGroup)Visit(properties), (GrammarGroup)Visit(grammar)),
    _ => throw new VisitorException("Cannot visit File, unexpected children"),
  };

  protected override PropertiesGroup VisitProperties(Nonterminal properties) =>
    new(properties.Children.Select(c => (PropertyDefinition)Visit(c)).ToList());

  protected override ASTNode VisitPropertyDef(Nonterminal propertyDef) => propertyDef.Children switch {
    [Token { Kind: TokenKind.TermName } name,
     Token { Kind: TokenKind.Colon },
     Token { Kind: TokenKind.String } value] =>
      new PropertyDefinition(name.Image, StringUtils.CleanStringLiteral(value.Image)),
    _ => throw new VisitorException("Cannot visit PropertyDef, unexpected children"),
  };

  protected override GrammarGroup VisitGrammar(Nonterminal grammar) {
    List<SymbolDefinition> symbolDefinitions = [];
    foreach (ParseNode child in grammar.Children) {
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

  protected override SymbolDefinition VisitAnyDef(Nonterminal anyDef) => anyDef.Children switch {
    [Nonterminal { Kind: NtKind.TermDef } termDef] => (SymbolDefinition)Visit(termDef),
    [Nonterminal { Kind: NtKind.NontermDef } nontermDef] => (SymbolDefinition)Visit(nontermDef),
    _ => throw new VisitorException("Cannot visit AnyDef, unexpected children"),
  };

  protected override TerminalDefinition VisitTermDef(Nonterminal termDef) {
    if (termDef.Children is [Token { Kind: TokenKind.TermName } termName,
                              Nonterminal { Kind: NtKind.OptDiscard } optDiscard,
                              Nonterminal { Kind: NtKind.TermRhs } termRhs,
                            ]) {
      return new(termName.Image, (TerminalRhs)Visit(termRhs), optDiscard.Children.Length > 0);
    }

    throw new VisitorException("Cannot visit TermDef, unexpected children");
  }

  protected override TerminalRhs VisitTermRhs(Nonterminal termRhs) => termRhs.Children switch {
    [Token { Kind: TokenKind.Regex } regex] => new(new(StringUtils.CleanRegexLiteral(regex.Image))),
    [Token { Kind: TokenKind.String } str] => new(new(StringUtils.CleanStringLiteralToRegex(str.Image))),
    _ => throw new VisitorException("Cannot visit TermRhs, unexpected children"),
  };

  protected override NonterminalDefinition VisitNontermDef(Nonterminal nontermDef) {
    List<RuleDefinition> ruleDefinitions = [];

    if (nontermDef.Children[0] is not Token { Kind: TokenKind.NontermName } nontermName) {
      throw new VisitorException("Cannot visit NonterminalDefinition, unexpected children");
    }

    if (nontermDef.Children[1] is not Nonterminal { Kind: NtKind.OptKwEntry } optKwEntry) {
      throw new VisitorException("Cannot visit NonterminalDefinition, unexpected children");
    }

    for (int i = 2; i < nontermDef.Children.Length; i++) {
      ruleDefinitions.Add((RuleDefinition)Visit(nontermDef.Children[i]));
    }

    return new(nontermName.Image, ruleDefinitions, optKwEntry.Children.Length > 0);
  }

  protected override RuleDefinition VisitRule(Nonterminal rule) =>
    new(rule.Children.Select(s => (SymbolUsage)Visit(s)).ToList());

  protected override SymbolUsage VisitSymbol(Nonterminal symbol) => symbol.Children switch {
    [Nonterminal { Kind: NtKind.OptDiscard } optDiscard,
     Nonterminal { Kind: NtKind.OptExpand } optExpand,
     Token { Kind: TokenKind.NontermName } nontermName] =>
      new NonterminalUsage(nontermName.Image, optDiscard.Children.Length > 0, optExpand.Children.Length > 0),
    [Nonterminal { Kind: NtKind.OptDiscard } optDiscard,
     Token { Kind: TokenKind.TermName } termName] =>
      new TerminalUsage(termName.Image, optDiscard.Children.Length > 0),
    _ => throw new VisitorException("Cannot visit Symbol, unexpected children"),
  };

  protected override ASTNode VisitOptKwEntry(Nonterminal optKwEntry) => throw new NotImplementedException();
  protected override ASTNode VisitOptExpand(Nonterminal optExpand) => throw new NotImplementedException();
  protected override ASTNode VisitOptDiscard(Nonterminal optDiscard) => throw new NotImplementedException();
  protected override ASTNode VisitKwProperties(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitKwGrammar(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitKwEntry(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitTermName(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitNontermName(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitRegex(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitString(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitExpand(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitDiscard(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitColon(Token token) => throw new NotImplementedException();
  protected override ASTNode VisitEof(Token token) => throw new NotImplementedException();
}
