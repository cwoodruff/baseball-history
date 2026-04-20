using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
}
