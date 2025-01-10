using Giraffe.Checks;

namespace Giraffe.Tests.Checks;

public class UndefinedTerminalsCheck_Evaluate {
    [Fact]
    public void GivenGrammarWithNoUndefinedTerminals_WhenEvaluateCalled_ThenCheckPasses() {
        Grammar grammar = new(new() {
          { "a", new("a") },
          { "b", new("b") },
        },
        [
          new("S", ["A", "B"]),
          new("A", ["a"]),
          new("B", ["b"]),
       ],
       ["S"]);

        UndefinedTerminalsCheck undefinedTerminalsCheck = new(grammar);
        Assert.True(undefinedTerminalsCheck.Evaluate().Pass);
    }

    [Fact]
    public void GivenGrammarWithUndefinedTerminals_WhenEvaluateCalled_ThenCheckPasses() {
        Grammar grammar = new(new() {
            { "a", new("a") },
        }, [
          new("S", ["A", "B"]),
          new("A", ["a"]),
          new("B", ["b"]),
       ],
       ["S"]);

        UndefinedTerminalsCheck undefinedTerminalsCheck = new(grammar);
        Assert.False(undefinedTerminalsCheck.Evaluate().Pass);
    }
}