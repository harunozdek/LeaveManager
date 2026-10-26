using LeaveManager.Tests.Support;

namespace LeaveManager.Tests.Characterization;

// Karakterizasyon (spec 0001): gozlenen davranisi sabitler, "dogru" davranisi degil.
public class RestartTests
{
    // PB-9 (BR-10, OD-8) — kalicilik yok: yeniden baslatma eklenen talepleri siler
    [Fact]
    public async Task Restart_AfterCreatingRequest_ListResetsToSeed()
    {
        using (var first = new AppFixture())
        {
            var client = first.CreateClient();
            await LeaveForm.PostAsync(client, "2", "2026-11-16", "2026-11-17", "Kaybolacak");
            Assert.Equal(9, HtmlTable.Rows(await client.GetStringAsync("/requests")).Count);
        }

        using var restarted = new AppFixture();

        var rows = HtmlTable.Rows(await restarted.CreateClient().GetStringAsync("/requests"));
        Assert.Equal(8, rows.Count);
        Assert.DoesNotContain(rows, r => r[4] == "Kaybolacak");
    }
}
