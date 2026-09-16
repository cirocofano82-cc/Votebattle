using VoteBattle.Infrastructure.Security;
using Xunit;

namespace VoteBattle.UnitTests;

public class TokenHasherTests
{
    [Fact]
    public void GenerateToken_is_url_safe_and_random()
    {
        var a = TokenHasher.GenerateToken();
        var b = TokenHasher.GenerateToken();

        Assert.NotEqual(a, b);
        Assert.DoesNotContain('+', a);
        Assert.DoesNotContain('/', a);
        Assert.DoesNotContain('=', a);
        Assert.True(a.Length >= 40);
    }

    [Fact]
    public void Hash_is_deterministic_for_same_input()
    {
        const string token = "some-token-value";
        Assert.Equal(TokenHasher.Hash(token), TokenHasher.Hash(token));
    }

    [Fact]
    public void Hash_differs_for_different_input()
    {
        Assert.NotEqual(TokenHasher.Hash("a"), TokenHasher.Hash("b"));
    }

    [Fact]
    public void Hash_does_not_return_the_raw_token()
    {
        const string token = "raw-secret";
        Assert.NotEqual(token, TokenHasher.Hash(token));
    }
}
