namespace Giraffe.Utils;

public static class GrammarUtils {
  public static IEnumerable<Symbol> StringShorthandToSymbols(IEnumerable<string> strings) =>
    strings.Select(StringToSymbol);

  private static Symbol StringToSymbol(string str) {
    if (str.Length == 0) {
      throw new ArgumentException("Cannot convert empty string to symbol");
    }

    if (char.IsLower(str[0])) {
      return new Terminal(str);
    }

    if (char.IsUpper(str[0])) {
      return new Nonterminal(str);
    }

    throw new ArgumentException($"Cannot convert string \"{str}\" to symbol");
  }
}