namespace Giraffe.Utils;

public static class CollectionUtils {

  /// <summary>
  /// Get a hash code representing the current state of a collection. If the collection is mutated, its hash code will
  /// likely change.
  /// Adapted from https://stackoverflow.com/a/30758270
  /// </summary>
  /// <param name="collection">The collection to generate hash code of.</param>
  /// <typeparam name="T">The type of the objects in the collection.</typeparam>
  /// <returns>A hash code representing the collection.</returns>
  public static int GetHashCode<T>(IEnumerable<T> collection) where T : notnull {
    const int seed = 487;
    const int modifier = 31;

    unchecked {
      return collection.Aggregate(seed, (a, b) => a * modifier + b.GetHashCode());
    }
  }

  /// <summary>
  /// Get the index of the first occurrence of a given object in a collection after a given start index.
  /// </summary>
  /// <param name="collection">The collection to search.</param>
  /// <param name="value">The object to find.</param>
  /// <param name="startIndex">The index to begin searching at (inclusive).</param>
  /// <typeparam name="T">The type of the objects in the collection.</typeparam>
  /// <returns>
  ///   The index of the first occurrence of the object after the start index. If the object does not exist after the
  ///   start index, the return value is <c>-1</c>.
  /// </returns>
  public static int IndexOf<T>(IList<T> collection, T value, int startIndex) where T : notnull {
    for (int i = startIndex; i < collection.Count; i++) {
      if (value.Equals(collection[i])) {
        return i;
      }
    }

    return -1;
  }
}