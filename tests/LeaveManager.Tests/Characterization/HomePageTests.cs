using LeaveManager.Tests.Support;

namespace LeaveManager.Tests.Characterization;

// Karakterizasyon (spec 0001): gozlenen davranisi sabitler, "dogru" davranisi degil.
public class HomePageTests
{
    // PB-1 (BR-10, BR-9)
    [Fact]
    public async Task Home_FreshApp_ShowsSeedCounters()
    {
        using var app = new AppFixture();

        var html = await app.CreateClient().GetStringAsync("/");

        Assert.Equal(4, HtmlTable.Counter(html, "Bekleyen"));
        Assert.Equal(1, HtmlTable.Counter(html, "Onaylanan"));
        Assert.Equal(3, HtmlTable.Counter(html, "Reddedilen"));
    }

    // PB-2 (BR-1, BR-4, BR-5) — yalnizca kidemi 2035'e kadar sabit kalan personel; Zeynep ve Elif spec 0002'de.
    [Theory]
    [InlineData("Ayse Yilmaz", "IT", "26", "12", "14")]
    [InlineData("Mehmet Demir", "Muhasebe", "20", "0", "20")]
    [InlineData("Ahmet Celik", "IT", "20", "0", "20")]
    public async Task Home_FreshApp_ShowsStableBalances(
        string name, string department, string entitlement, string used, string remaining)
    {
        using var app = new AppFixture();

        var html = await app.CreateClient().GetStringAsync("/");

        var row = Assert.Single(HtmlTable.Rows(html), r => r[0] == name);
        Assert.Equal(new[] { name, department, entitlement, used, remaining }, row);
    }
}
