namespace Giraffe.Tests;

public class GrammarTest {
    [Fact]
    public void GrammarSetCalculation() {
      Grammar grammar = new(
        new() {
          {"a", new("a")},
          {"b", new("b")},
          {"c", new("c")},
          {"d", new("d")},
          {"e", new("e")},
        },
        [
          new("S", ["A", "B", "C", "D", "E", Grammar.Eof]),
          new("A", ["a"]), new("A", []),
          new("B", ["b"]), new("B", []),
          new("C", ["c"]),
          new("D", ["d"]), new("D", []),
          new("E", ["e"]), new("E", []),
        ]
      );
      grammar.ComputeSets();

      // Epsilon
      Assert.True(grammar.HasEpsilon("A"));
      Assert.True(grammar.HasEpsilon("B"));
      Assert.False(grammar.HasEpsilon("C"));
      Assert.True(grammar.HasEpsilon("D"));
      Assert.True(grammar.HasEpsilon("E"));

      // First
      Assert.Equal(["a", "b", "c"], grammar.First("S"));
      Assert.Equal(["a"], grammar.First("A"));
      Assert.Equal(["b"], grammar.First("B"));
      Assert.Equal(["c"], grammar.First("C"));
      Assert.Equal(["d"], grammar.First("D"));
      Assert.Equal(["e"], grammar.First("E"));

      // Follow
      Assert.Equal([], grammar.Follow("S"));
      Assert.Equal(["b", "c"], grammar.Follow("A"));
      Assert.Equal(["c"], grammar.Follow("B"));
      Assert.Equal(["d", "e", Grammar.Eof], grammar.Follow("C"));
      Assert.Equal(["e", Grammar.Eof], grammar.Follow("D"));
      Assert.Equal([Grammar.Eof], grammar.Follow("E"));

      // Predict
      Assert.Equal(["a", "b", "c"], grammar.Predict(0));
      Assert.Equal(["a"], grammar.Predict(1));
      Assert.Equal(["b", "c"], grammar.Predict(2));
      Assert.Equal(["b"], grammar.Predict(3));
      Assert.Equal(["c"], grammar.Predict(4));
      Assert.Equal(["c"], grammar.Predict(5));
      Assert.Equal(["d"], grammar.Predict(6));
      Assert.Equal(["e", Grammar.Eof], grammar.Predict(7));
      Assert.Equal(["e"], grammar.Predict(8));
      Assert.Equal([Grammar.Eof], grammar.Predict(9));
    }
}
