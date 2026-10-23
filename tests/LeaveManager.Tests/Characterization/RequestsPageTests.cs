using LeaveManager.Tests.Support;

namespace LeaveManager.Tests.Characterization;

// Karakterizasyon (spec 0001): gozlenen davranisi sabitler, "dogru" davranisi degil.
public class RequestsPageTests
{
    // PB-1 (BR-10, BR-9)
    [Fact]
    public async Task Requests_FreshApp_ListsEightSeedRequestsInOrder()
    {
        using var app = new AppFixture();

        var html = await app.CreateClient().GetStringAsync("/requests");

        var expected = new[]
        {
            new[] { "Ayse Yilmaz", "2026-07-06", "2026-07-17", "12", "Yaz tatili", "Onaylandi" },
            new[] { "Mehmet Demir", "2026-11-02", "2026-11-04", "3", "Aile ziyareti", "Beklemede" },
            new[] { "Zeynep Kaya", "2026-11-09", "2026-11-09", "1", "Saglik", "Beklemede" },
            new[] { "Ahmet Celik", "2026-12-21", "2026-12-25", "5", "Yilbasi oncesi", "Beklemede" },
            new[] { "Ayse Yilmaz", "2026-12-28", "2026-12-30", "3", "Yil sonu", "Beklemede" },
            new[] { "Elif Sahin", "2026-10-05", "2026-10-07", "3", "Tasinma", "Reddedildi" },
            new[] { "Zeynep Kaya", "2026-09-14", "2026-09-25", "12", "Uzun tatil", "Reddedildi" },
            new[] { "Mehmet Demir", "2026-08-03", "2026-08-03", "1", "Ozel is", "Reddedildi" },
        };
        Assert.Equal(expected, HtmlTable.Rows(html));
    }

    // PB-3 (BR-3) — takvim gunu, hafta sonlari dahil, iki uc da sayilir
    [Theory]
    [InlineData("2026-07-06", "12")] // Pzt -> sonraki Cuma: iki hafta sonu dahil
    [InlineData("2026-11-09", "1")]  // tek gunluk talep
    public async Task Requests_DaysColumn_CountsCalendarDaysInclusive(string start, string expectedDays)
    {
        using var app = new AppFixture();

        var html = await app.CreateClient().GetStringAsync("/requests");

        var row = Assert.Single(HtmlTable.Rows(html), r => r[1] == start);
        Assert.Equal(expectedDays, row[3]);
    }
}
