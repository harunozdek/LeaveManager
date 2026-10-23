namespace LeaveManager.Tests;

// Altyapi kaniti: uygulama surec icinde (TestServer) ayaga kalkar ve ana sayfa render edilir.
// Kapsamli karakterizasyon testleri ilk is kaleminde (spec ile) gelir.
public class SmokeTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _app;

    public SmokeTests(AppFixture app) => _app = app;

    [Fact]
    public async Task Home_AppStarts_RendersBalanceTableWithSeededEmployees()
    {
        var client = _app.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("<h3>Personel Bakiyeleri</h3>", html);
        Assert.Contains("Ayse Yilmaz", html);
    }
}
