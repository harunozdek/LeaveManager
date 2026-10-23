using LeaveManager.Tests.Support;

namespace LeaveManager.Tests.Characterization;

// Karakterizasyon (spec 0001): gozlenen davranisi sabitler, "dogru" davranisi degil.
public class NewRequestFormTests
{
    // PB-1 (BR-10)
    [Fact]
    public async Task NewForm_FreshApp_ListsFiveEmployees()
    {
        using var app = new AppFixture();

        var html = await app.CreateClient().GetStringAsync("/new");

        var expected = new List<(string, string)>
        {
            ("1", "Ayse Yilmaz"),
            ("2", "Mehmet Demir"),
            ("3", "Zeynep Kaya"),
            ("4", "Ahmet Celik"),
            ("5", "Elif Sahin"),
        };
        Assert.Equal(expected, HtmlTable.Options(html));
    }
}
