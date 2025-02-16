using System.Collections;

namespace Giraffe.Tests;

public class Utils_IndexOf {
  [Theory]
  [ClassData(typeof(IndexOfTestData))]
  public void GivenCollectionAndValue_WhenIndexOfCalled_ThenExpectedIndexReturned<T>(IList<T> collection,
                                                                                     T value,
                                                                                     int startIndex,
                                                                                     int expectedIndex) where T : notnull =>
    Assert.Equal(expectedIndex, Utils.IndexOf(collection, value, startIndex));

  private class IndexOfTestData : IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() {
      yield return [(List<int>)[], 0, 0, -1];
      yield return [(List<int>)[1, 2, 3, 4, 5], 3, 0, 2];
      yield return [(List<int>)[1, 2, 1, 2, 1], 1, 1, 2];
      yield return [(IList<int>)[0, 1, 2, 3, 4, 5], 0, 4, -1];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}