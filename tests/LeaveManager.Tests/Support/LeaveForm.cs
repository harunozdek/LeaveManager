namespace LeaveManager.Tests.Support;

// POST /new form gonderimi; alan adlari Program.cs'teki formla ayni.
public static class LeaveForm
{
    public static Task<HttpResponseMessage> PostAsync(
        HttpClient client, string employeeId, string start, string end, string reason) =>
        PostFieldsAsync(client, new Dictionary<string, string>
        {
            ["employee_id"] = employeeId,
            ["start"] = start,
            ["end"] = end,
            ["reason"] = reason,
        });

    public static Task<HttpResponseMessage> PostFieldsAsync(HttpClient client, IDictionary<string, string> fields) =>
        client.PostAsync("/new", new FormUrlEncodedContent(fields));
}
