using System.Text.RegularExpressions;

namespace Giraffe.GIR;

public readonly struct TerminalDefinition(Regex regex, bool ignore = false) {
  public Regex Regex { get; } = regex;
  public bool Ignore { get; } = ignore;
}