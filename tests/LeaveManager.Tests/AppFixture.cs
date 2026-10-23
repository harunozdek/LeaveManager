using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveManager.Tests;

// Uygulamanin top-level Program tipi internal; public test siniflarinin imzasinda gorunmesin diye
// factory bu fixture'in internal uyesi olarak tutulur. Dispose, TestServer'i ve host'u kapatir.
public sealed class AppFixture : IDisposable
{
    internal WebApplicationFactory<Program> Factory { get; } = new();

    public HttpClient CreateClient() => Factory.CreateClient();

    public void Dispose() => Factory.Dispose();
}
