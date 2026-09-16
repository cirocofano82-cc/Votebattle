using VoteBattle.Infrastructure.Common;
using Xunit;

namespace VoteBattle.UnitTests;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Samsung Galaxy Fold 8 vs iPhone Duo", "samsung-galaxy-fold-8-vs-iphone-duo")]
    [InlineData("PS5 vs Xbox Series X", "ps5-vs-xbox-series-x")]
    [InlineData("Coca-Cola vs Pepsi", "coca-cola-vs-pepsi")]
    [InlineData("  Trimmed   spaces  ", "trimmed-spaces")]
    [InlineData("Special!@#$%^&*()Chars", "special-chars")]
    public void Generate_produces_expected_slug(string input, string expected)
    {
        Assert.Equal(expected, SlugGenerator.Generate(input));
    }

    [Fact]
    public void Generate_strips_diacritics()
    {
        Assert.Equal("cafe-uber", SlugGenerator.Generate("Café Über"));
    }

    [Fact]
    public void Generate_handles_empty_input()
    {
        Assert.Equal(string.Empty, SlugGenerator.Generate(""));
    }
}
