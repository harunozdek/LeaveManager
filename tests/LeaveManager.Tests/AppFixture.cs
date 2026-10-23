using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveManager.Tests;

// Uygulamanin top-level Program tipi internal; public test siniflarinin imzasinda gorunmesin diye
// factory bu fixture'in internal uyesi olarak tutulur. Dispose, TestServer'i ve host'u kapatir.
// Her yeni ornek uygulamayi bastan baslatir; Program.cs Db.Seed() cagirdigi icin veri seed'e doner.
public sealed class AppFixture : IDisposable
{
    internal WebApplicationFactory<Program> Factory { get; } = new();

    public HttpClient CreateClient() => Factory.CreateClient();

    // Yonlendirmeyi takip etmeyen istemci: 302 + Location dogrulamak icin.
    public HttpClient CreateClient(bool allowAutoRedirect) =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = allowAutoRedirect });

    public void Dispose() => Factory.Dispose();
}
