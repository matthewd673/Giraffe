namespace Giraffe;

public class Production : List<string> {
  public string Name { get; }

  public Production(string name) {
    Name = name;
  }

  public Production(string name, IEnumerable<string> sequence) {
    Name = name;
    AddRange(sequence);
  }
}
