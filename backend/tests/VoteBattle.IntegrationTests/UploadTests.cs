using System.Net;
using System.Net.Http.Headers;
using VoteBattle.IntegrationTests.Infrastructure;
using Xunit;

namespace VoteBattle.IntegrationTests;

[Collection("integration")]
public class UploadTests
{
    private readonly VoteBattleWebAppFactory _factory;

    public UploadTests(VoteBattleWebAppFactory factory) => _factory = factory;

    // Minimal PNG signature (enough for the magic-byte check).
    private static readonly byte[] PngBytes =
        { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x00 };

    [Fact]
    public async Task Admin_can_upload_an_image()
    {
        var client = _factory.CreateClient();
        await client.LoginAsAdminAsync();

        var resp = await client.PostAsync("/api/uploads/image", BuildImageForm(PngBytes, "image/png", "logo.png"));
        resp.EnsureSuccessStatusCode();

        var url = (await resp.ReadDataAsync()).GetProperty("url").GetString();
        Assert.False(string.IsNullOrWhiteSpace(url));
        Assert.Contains("/uploads/", url);
        Assert.EndsWith(".png", url);
    }

    [Fact]
    public async Task Non_admin_cannot_upload()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);

        var resp = await client.PostAsync("/api/uploads/image", BuildImageForm(PngBytes, "image/png", "logo.png"));
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Non_image_file_is_rejected()
    {
        var client = _factory.CreateClient();
        await client.LoginAsAdminAsync();

        var notAnImage = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"
        var resp = await client.PostAsync("/api/uploads/image", BuildImageForm(notAnImage, "image/png", "fake.png"));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private static MultipartFormDataContent BuildImageForm(byte[] bytes, string contentType, string fileName)
    {
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { content, "file", fileName } };
    }
}
