using System.Text.RegularExpressions;

namespace Giraffe.AST;

public record TerminalRhs(Regex Regex, bool Ignore) : ASTNode;