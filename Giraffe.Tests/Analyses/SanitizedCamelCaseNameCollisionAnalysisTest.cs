using Giraffe.Analyses;
using Giraffe.GIR;
using static Giraffe.GIR.GrammarFactory;

namespace Giraffe.Tests.Analyses;

public class SanitizedCamelCaseNameCollisionAnalysisTest {
  [Fact]
  public void GivenNamesWithoutCollisions_WhenAnalyzeCalled_ThenNoCollisionsReported() {
    Grammar grammar = G([], [R("A", []), R("B", []), R("A_B", []), R("A_C", [])], []);
    SanitizedCamelCaseNameCollisionAnalysis sanitizedCamelCaseNameCollisionAnalysis = new(grammar);
    Assert.Empty(sanitizedCamelCaseNameCollisionAnalysis.Analyze());
  }

  [Fact]
  public void GivenNamesWithCollisions_WhenAnalyzeCalled_ThenExpectedCollisionsReported() {
    Grammar grammar = G([], [R("A__B", []), R("B1C", []), R("A_B", []), R("B_1_$C#", []), R("C", [])], []);
    SanitizedCamelCaseNameCollisionAnalysis sanitizedCamelCaseNameCollisionAnalysis = new(grammar);
    Dictionary<string, List<string>> collisions = sanitizedCamelCaseNameCollisionAnalysis.Analyze();

    Assert.Equal(2, collisions.Count);
    Assert.Equal(["A__B", "A_B"], collisions["aB"]);
    Assert.Equal(["B1C", "B_1_$C#"], collisions["b1C"]);
  }
}