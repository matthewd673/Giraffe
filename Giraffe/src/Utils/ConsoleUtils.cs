namespace Giraffe.Utils;

public static class ConsoleUtils {
  /// <summary>
  /// Print a string with "info" styling.
  /// </summary>
  /// <param name="str">The string to print.</param>
  public static void PrintInfo(string str) {
    Console.WriteLine(str);
  }

  /// <summary>
  /// Print a string with "warning" styling.
  /// </summary>
  /// <param name="str">The string to print.</param>
  public static void PrintWarning(string str) {
    PrintWithForegroundColor(str, ConsoleColor.DarkYellow);
  }

  /// <summary>
  /// Print a string with "error" styling.
  /// </summary>
  /// <param name="str">The string to print.</param>
  public static void PrintError(string str) {
    PrintWithForegroundColor(str, ConsoleColor.DarkRed);
  }

  private static void PrintWithForegroundColor(string str, ConsoleColor foregroundColor) {
    ConsoleColor defaultColor = Console.ForegroundColor;
    Console.ForegroundColor = foregroundColor;
    Console.WriteLine(str);
    Console.ForegroundColor = defaultColor;
  }
}