using Giraffe;

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
          new("S", ["A", "B", "C", "D", "E"]),
          new("A", ["a"]), new("A", []),
          new("B", ["b"]), new("B", []),
          new("C", ["c"]),
          new("D", ["d"]), new("D", []),
          new("E", ["e"]), new("E", []),
        ]
      );
      grammar.ComputeSets();

      Assert.True(grammar.HasEpsilon("A"));
      Assert.True(grammar.HasEpsilon("B"));
      Assert.False(grammar.HasEpsilon("C"));
      Assert.True(grammar.HasEpsilon("D"));
      Assert.True(grammar.HasEpsilon("E"));

      Assert.Equal(["a", "b", "c"], grammar.First("S"));
      Assert.Equal(["a"], grammar.First("A"));
      Assert.Equal(["b"], grammar.First("B"));
      Assert.Equal(["c"], grammar.First("C"));
      Assert.Equal(["d"], grammar.First("D"));
      Assert.Equal(["e"], grammar.First("E"));

      // TODO: Follow and Predict
    }
}
