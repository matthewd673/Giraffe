namespace Giraffe;

public static class Utils {
  // Adapted from https://stackoverflow.com/a/30758270
  public static int GetCollectionHashCode<T>(IEnumerable<T> collection) where T : notnull {
    const int seed = 487;
    const int modifier = 31;

    unchecked {
      return collection.Aggregate(seed, (a, b) => a * modifier + b.GetHashCode());
    }
  }

  public static int IndexOf<T>(IList<T> collection, T value, int startIndex) where T : notnull {
    for (int i = startIndex; i < collection.Count; i++) {
      if (value.Equals(collection[i])) {
        return i;
      }
    }

    return -1;
  }
}