namespace Giraffe;

public class Production(string name) : List<string> {
  public string Name { get; } = name;
}
