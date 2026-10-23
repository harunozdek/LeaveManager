using System.Net;
using LeaveManager.Tests.Support;

namespace LeaveManager.Tests.Characterization;

// Karakterizasyon (spec 0001): gozlenen davranisi sabitler, "dogru" davranisi degil.
// Kusurlar (OD-n) bilincli olarak oldugu gibi sabitlenir; degisiklikleri kendi spec'leriyle gelir.
// Her test kendi uygulamasini baslatir: Db statik, yeni ornek seed'e doner (plan 0001, R2).
public class CreateRequestTests
{
    private const int SeedRequestCount = 8;

    // PB-4 (BR-6, OD-5)
    [Fact]
    public async Task Create_SufficientBalance_SavedAsPendingAndBalanceUnchanged()
    {
        using var app = new AppFixture();
        var client = app.CreateClient(allowAutoRedirect: false);

        var response = await LeaveForm.PostAsync(client, "2", "2026-11-16", "2026-11-17", "Test");

        AssertRedirectsToRequests(response);
        var rows = await RequestRowsAsync(client);
        Assert.Equal(SeedRequestCount + 1, rows.Count);
        Assert.Equal(new[] { "Mehmet Demir", "2026-11-16", "2026-11-17", "2", "Test", "Beklemede" }, rows[^1]);
        var home = await client.GetStringAsync("/");
        Assert.Equal(5, HtmlTable.Counter(home, "Bekleyen"));
        var mehmet = Assert.Single(HtmlTable.Rows(home), r => r[0] == "Mehmet Demir");
        Assert.Equal(new[] { "20", "0", "20" }, mehmet[2..]);
    }

    // PB-5 (BR-6) — Ayse: kalan 14, talep 15 gun
    [Fact]
    public async Task Create_RemainingLessThanDays_ShowsErr412AndNotSaved()
    {
        using var app = new AppFixture();
        var client = app.CreateClient(allowAutoRedirect: false);

        var response = await LeaveForm.PostAsync(client, "1", "2026-12-01", "2026-12-15", "Uzun");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("HATA: ERR-412 - bakiye yetersiz. IT'ye bildirin: dahili 4127", html);
        Assert.Equal(SeedRequestCount, (await RequestRowsAsync(client)).Count);
    }

    // PB-6 (BR-6, OD-4) — kalan = talep gunu: hata mesaji yok, yonlendirme var, kayit yok (sessiz ret)
    [Fact]
    public async Task Create_RemainingEqualsDays_SilentlyNotSaved()
    {
        using var app = new AppFixture();
        var client = app.CreateClient(allowAutoRedirect: false);

        var response = await LeaveForm.PostAsync(client, "1", "2026-12-01", "2026-12-14", "Esit");

        AssertRedirectsToRequests(response);
        Assert.DoesNotContain("ERR-412", await response.Content.ReadAsStringAsync());
        Assert.Equal(SeedRequestCount, (await RequestRowsAsync(client)).Count);
    }

    // PB-6 (BR-6, OD-4) — sinirin bir gun altinda talep kaydedilir
    [Fact]
    public async Task Create_RemainingOneMoreThanDays_Saved()
    {
        using var app = new AppFixture();
        var client = app.CreateClient(allowAutoRedirect: false);

        var response = await LeaveForm.PostAsync(client, "1", "2026-12-01", "2026-12-13", "Sinir");

        AssertRedirectsToRequests(response);
        var rows = await RequestRowsAsync(client);
        Assert.Equal(SeedRequestCount + 1, rows.Count);
        Assert.Equal(new[] { "Ayse Yilmaz", "2026-12-01", "2026-12-13", "13", "Sinir", "Beklemede" }, rows[^1]);
    }

    // PB-7 (BR-7, OD-6) — bitis < baslangic reddedilmez, gun sayisi negatif gosterilir
    [Fact]
    public async Task Create_EndBeforeStart_SavedWithNegativeDays()
    {
        using var app = new AppFixture();
        var client = app.CreateClient(allowAutoRedirect: false);

        var response = await LeaveForm.PostAsync(client, "2", "2026-11-10", "2026-11-08", "Ters");

        AssertRedirectsToRequests(response);
        var rows = await RequestRowsAsync(client);
        Assert.Equal(SeedRequestCount + 1, rows.Count);
        Assert.Equal(new[] { "Mehmet Demir", "2026-11-10", "2026-11-08", "-1", "Ters", "Beklemede" }, rows[^1]);
    }

    // PB-7 (BR-7, OD-6) — gecmis tarihli talep reddedilmez
    [Fact]
    public async Task Create_PastDate_Saved()
    {
        using var app = new AppFixture();
        var client = app.CreateClient(allowAutoRedirect: false);

        var response = await LeaveForm.PostAsync(client, "2", "2020-01-01", "2020-01-01", "Gecmis");

        AssertRedirectsToRequests(response);
        var rows = await RequestRowsAsync(client);
        Assert.Equal(SeedRequestCount + 1, rows.Count);
        Assert.Equal(new[] { "Mehmet Demir", "2020-01-01", "2020-01-01", "1", "Gecmis", "Beklemede" }, rows[^1]);
    }

    // PB-8 (BR-8) — hatali girdi yutulur: hata mesaji yok, yonlendirme, kayit yok
    [Theory]
    [InlineData("bozuk-tarih")]
    [InlineData("bilinmeyen-personel")]
    [InlineData("bos-form")]
    public async Task Create_InvalidInput_RedirectsWithoutSaving(string scenario)
    {
        var fields = scenario switch
        {
            "bozuk-tarih" => new Dictionary<string, string>
            { ["employee_id"] = "2", ["start"] = "abc", ["end"] = "2026-11-08", ["reason"] = "x" },
            "bilinmeyen-personel" => new Dictionary<string, string>
            { ["employee_id"] = "99", ["start"] = "2026-11-10", ["end"] = "2026-11-11", ["reason"] = "x" },
            _ => new Dictionary<string, string>(),
        };
        using var app = new AppFixture();
        var client = app.CreateClient(allowAutoRedirect: false);

        var response = await LeaveForm.PostFieldsAsync(client, fields);

        AssertRedirectsToRequests(response);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        Assert.Equal(SeedRequestCount, (await RequestRowsAsync(client)).Count);
    }

    private static void AssertRedirectsToRequests(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/requests", response.Headers.Location?.OriginalString);
    }

    private static async Task<List<string[]>> RequestRowsAsync(HttpClient client) =>
        HtmlTable.Rows(await client.GetStringAsync("/requests"));
}
