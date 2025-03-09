using System.Text;

namespace Giraffe.Utils;

public static class StringUtils {
  /// <summary>
  /// Given a string containing non-word characters (i.e. characters not in <c>[A-Za-z0-9_]</c>), replace them with an
  /// underscore.
  /// </summary>
  /// <param name="str">The string to sanitize.</param>
  /// <returns>The string with all non-word characters replaced with underscores.</returns>
  public static string SanitizeNonWordCharacters(string str) {
    StringBuilder builder = new();

    foreach (char c in str) {
      if (char.IsNumber(c) || char.IsLetter(c)) {
        builder.Append(c);
      }
      else {
        builder.Append('_');
      }
    }

    return builder.ToString();
  }

  /// <summary>
  /// Convert a string from snake_case or SCREAMING_SNAKE_CASE to CamelCase. Non-word characters in the input will not
  /// be sanitized.
  /// </summary>
  /// <param name="str">The string to convert.</param>
  /// <returns>The string converted to CamelCase.</returns>
  public static string SnakeCaseToCamelCase(string str) {
    StringBuilder builder = new();
    bool capitalize = false;

    foreach (char c in str) {
      if (char.IsLetter(c) && capitalize) {
        builder.Append(char.ToUpper(c));
        capitalize = false;
      }
      else if (char.IsLetter(c) && !capitalize) {
        builder.Append(char.ToLower(c));
      }
      else if (char.IsNumber(c)) {
        capitalize = true;
        builder.Append(c);
      }
      else if (c == '_') {
        capitalize = true;
      }
      else {
        builder.Append(c);
      }
    }

    return builder.ToString();
  }

  /// <summary>
  /// Capitalize the first character of the string iff it is a letter.
  /// </summary>
  /// <param name="str">The string to capitalize.</param>
  /// <returns>The string, with the first character capitalized iff it is a letter.</returns>
  public static string Capitalize(string str) =>
    str.Length > 0 && char.IsLower(str[0])
      ? char.ToUpper(str[0]) + str.Remove(0, 1)
      : str;
}