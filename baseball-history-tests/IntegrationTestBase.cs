using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Regex PaginationSummaryRegex = new(@"Page\s+(?<current>\d+)\s+of\s+(?<total>\d+)", RegexOptions.Compiled);

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

    protected async Task<string> GetStringAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    protected async Task<string> GetHtmxStringAsync(string url, bool boosted = false)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("HX-Request", "true");

        if (boosted)
            request.Headers.Add("HX-Boosted", "true");

        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    protected static (int CurrentPage, int TotalPages) ParsePaginationSummary(string html)
    {
        var match = PaginationSummaryRegex.Match(html);
        Assert.True(match.Success, "Expected a pagination summary like 'Page X of Y' in the response.");

        return (
            int.Parse(match.Groups["current"].Value),
            int.Parse(match.Groups["total"].Value));
    }
}
