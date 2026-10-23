using System.Text.RegularExpressions;

namespace LeaveManager.Tests.Support;

// Sayfalar sabit string birlestirmesiyle uretildigi icin regex ile ayristirma yeterli (plan 0001, R3).
public static class HtmlTable
{
    private static readonly Regex RowPattern = new("<tr>(.*?)</tr>", RegexOptions.Singleline);
    private static readonly Regex CellPattern = new("<td>(.*?)</td>", RegexOptions.Singleline);
    private static readonly Regex TagPattern = new("<[^>]*>");
    private static readonly Regex OptionPattern = new("<option value='([^']*)'>(.*?)</option>");

    // Veri satirlari (<td> iceren); baslik satirlari (<th>) atlanir. Hucre icindeki etiketler soyulur.
    public static List<string[]> Rows(string html) =>
        RowPattern.Matches(html)
            .Select(row => CellPattern.Matches(row.Groups[1].Value)
                .Select(cell => TagPattern.Replace(cell.Groups[1].Value, "").Trim())
                .ToArray())
            .Where(cells => cells.Length > 0)
            .ToList();

    // Ana sayfadaki sayac kutusu: "<div class='kutu'>Bekleyen: <b>4</b></div>" -> 4
    public static int Counter(string html, string label)
    {
        var match = Regex.Match(html, $"<div class='kutu'>{Regex.Escape(label)}: <b>(-?\\d+)</b></div>");
        Assert.True(match.Success, $"'{label}' sayaci sayfada bulunamadi");
        return int.Parse(match.Groups[1].Value);
    }

    // Yeni Talep formundaki secenekler: (value, gorunen metin)
    public static List<(string Value, string Text)> Options(string html) =>
        OptionPattern.Matches(html)
            .Select(m => (m.Groups[1].Value, m.Groups[2].Value))
            .ToList();
}
