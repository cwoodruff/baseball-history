using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PaginationBoundaryTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Players_Htmx_PageZero_ClampsToFirstPage()
    {
        var html = await GetHtmxStringAsync("/Players?letter=A&page=0");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(1, currentPage);
        Assert.True(totalPages > 1);
    }

    [Fact]
    public async Task Players_Htmx_NegativePage_ClampsToFirstPage()
    {
        var html = await GetHtmxStringAsync("/Players?letter=B&page=-5");
        var (currentPage, _) = ParsePaginationSummary(html);

        Assert.Equal(1, currentPage);
    }

    [Fact]
    public async Task Players_Htmx_PageBeyondMax_ClampsToLastPage()
    {
        var html = await GetHtmxStringAsync("/Players?letter=A&page=999999");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(totalPages, currentPage);
    }

    [Fact]
    public async Task StatsBatting_Htmx_PageZero_ClampsToFirstPage()
    {
        var html = await GetHtmxStringAsync("/Stats/Batting?stat=hr&page=0");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(1, currentPage);
        Assert.True(totalPages > 1);
    }

    [Fact]
    public async Task StatsBatting_Htmx_NegativePage_ClampsToFirstPage()
    {
        var html = await GetHtmxStringAsync("/Stats/Batting?stat=avg&page=-10");
        var (currentPage, _) = ParsePaginationSummary(html);

        Assert.Equal(1, currentPage);
    }

    [Fact]
    public async Task StatsBatting_Htmx_PageBeyondMax_ClampsToLastPage()
    {
        var html = await GetHtmxStringAsync("/Stats/Batting?stat=hr&page=999999");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(totalPages, currentPage);
    }

    [Fact]
    public async Task ApiPlayers_PageZero_ClampsToFirstPage()
    {
        using var document = await GetJsonAsync("/api/players?letter=A&page=0");
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.True(root.GetProperty("totalPages").GetInt32() > 1);
    }

    [Fact]
    public async Task ApiPlayers_NegativePage_ClampsToFirstPage()
    {
        using var document = await GetJsonAsync("/api/players?letter=B&page=-5");

        Assert.Equal(1, document.RootElement.GetProperty("page").GetInt32());
    }

    [Fact]
    public async Task ApiPlayers_PageBeyondMax_ClampsToLastPage()
    {
        using var document = await GetJsonAsync("/api/players?letter=A&page=999999");
        var root = document.RootElement;
        var page = root.GetProperty("page").GetInt32();
        var totalPages = root.GetProperty("totalPages").GetInt32();

        Assert.Equal(totalPages, page);
        Assert.True(root.GetProperty("data").GetArrayLength() > 0);
    }

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
