using System.Text.RegularExpressions;

namespace Giraffe.AST;

public record TerminalRhs(Regex Regex) : ASTNode;