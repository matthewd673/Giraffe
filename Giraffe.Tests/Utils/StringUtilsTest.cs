using Giraffe.Utils;

namespace Giraffe.Tests.Utils;

public class StringUtilsTest {
  [Theory]
  [InlineData("", "")]
  [InlineData("LeGaL_StRiNg", "LeGaL_StRiNg")]
  [InlineData("#,illegal!@characters+.5_", "__illegal__characters__5_")]
  public void GivenString_WhenSanitizeNonWordCharactersCalled_ThenSanitizedStringReturned(string input, string expected) {
    Assert.Equal(expected, StringUtils.SanitizeNonWordCharacters(input));
  }

  [Theory]
  [InlineData("", "")]
  [InlineData("snake_case", "snakeCase")]
  [InlineData("ALL_CAPS", "allCaps")]
  [InlineData("with_1_number", "with1Number")]
  [InlineData("with_1number", "with1Number")]
  [InlineData("many___consecutive________underscores", "manyConsecutiveUnderscores")]
  [InlineData("random#other.characters", "random#other.characters")]
  [InlineData("separated1by2numbers", "separated1By2Numbers")]
  public void GivenStringInSnakeCase_WhenStringToCamelCaseCalled_ThenCamelCaseStringReturned(string input, string expected) {
    Assert.Equal(expected, StringUtils.SnakeCaseToCamelCase(input));
  }

  [Theory]
  [InlineData("", "")]
  [InlineData("lowercase", "Lowercase")]
  [InlineData("1number", "1number")]
  [InlineData("CAPS", "CAPS")]
  public void GivenString_WhenCapitalizeCalled_ThenFirstCharCapitalizedIfLetter(string input, string expected) {
    Assert.Equal(expected, StringUtils.Capitalize(input));
  }
}