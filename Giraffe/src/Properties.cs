using System.Collections.Immutable;

namespace Giraffe;

public static class Properties {
  public const string Namespace = "namespace";

  public static ImmutableList<string> RequiredProperties => [Namespace];
}
