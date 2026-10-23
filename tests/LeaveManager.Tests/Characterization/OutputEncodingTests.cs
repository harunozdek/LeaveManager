using LeaveManager.Tests.Support;

namespace LeaveManager.Tests.Characterization;

// Karakterizasyon (spec 0001): gozlenen davranisi sabitler, "dogru" davranisi degil.
public class OutputEncodingTests
{
    // PB-10 (SEC-1) — KUSUR OLARAK SABITLENIR: Neden alani HTML'e encode edilmeden basilir.
    // SEC-1'i duzelten /fix-bug bu testi kendi onayli spec'iyle degistirecek (spec 0001, Out of scope).
    [Fact]
    public async Task Requests_ReasonWithScriptTag_RenderedUnencoded()
    {
        using var app = new AppFixture();
        var client = app.CreateClient();

        await LeaveForm.PostAsync(client, "2", "2026-11-20", "2026-11-20", "<script>alert(1)</script>");

        var html = await client.GetStringAsync("/requests");
        Assert.Contains("<td><script>alert(1)</script></td>", html);
    }
}
