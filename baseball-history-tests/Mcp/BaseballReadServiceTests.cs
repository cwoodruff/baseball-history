using baseball_history_mcp.Configuration;
using baseball_history_mcp;
using baseball_history_mcp.Metadata;
using baseball_history_mcp.Querying;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace baseball_history_tests.Mcp;

public class BaseballReadServiceTests
{
    [Fact]
    public async Task SearchPlayersAsync_WithPrefix_ReturnsPagedResults()
    {
        var service = CreatePlayerReadService();

        var result = await service.SearchPlayersAsync(new PlayerLookupRequest(LastNameStartsWith: "R", Page: 1, PageSize: 10));

        Assert.True(result.TotalCount > 0);
        Assert.True(result.Items.Count > 0);
        Assert.All(result.Items, player => Assert.StartsWith("R", player.FullName.Split(' ').Last(), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchPlayersAsync_WhenPageSizeExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreatePlayerReadService(playerSearchPageSizeMax: 3);

        var result = await service.SearchPlayersAsync(new PlayerLookupRequest(LastNameStartsWith: "R", Page: 1, PageSize: 10));

        Assert.Equal(3, result.PageSize);
        Assert.Equal(10, result.RequestedPageSize);
        Assert.Equal(3, result.MaxPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetPlayerAsync_WithKnownPlayer_ReturnsCareerSummaries()
    {
        var service = CreatePlayerReadService();

        var player = await service.GetPlayerAsync("ruthba01");

        Assert.NotNull(player);
        Assert.Equal("ruthba01", player.PlayerId);
        Assert.NotNull(player.CareerBatting);
        Assert.True(player.Teams.Count > 0);
    }

    [Fact]
    public async Task ListFranchisesAsync_WithActiveOnly_ReturnsActiveFranchises()
    {
        var service = CreateFranchiseReadService();

        var franchises = await service.ListFranchisesAsync(new FranchiseLookupRequest(ActiveOnly: true));

        Assert.True(franchises.Items.Count > 0);
        Assert.All(franchises.Items, franchise => Assert.True(franchise.IsActive));
    }

    [Fact]
    public async Task ListFranchisesAsync_WhenPageSizeExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateFranchiseReadService(franchiseListPageSizeMax: 2);

        var result = await service.ListFranchisesAsync(new FranchiseLookupRequest(ActiveOnly: true, PageSize: 10));

        Assert.Equal(2, result.PageSize);
        Assert.Equal(10, result.RequestedPageSize);
        Assert.Equal(2, result.MaxPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetBattingLeadersAsync_WithCareerHomeRuns_ReturnsDescendingResults()
    {
        var service = CreateLeaderboardReadService();

        var result = await service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "hr", PageSize: 5));

        Assert.Equal(5, result.Items.Count);
        Assert.True(result.Items[0].HomeRuns >= result.Items[1].HomeRuns);
    }

    [Fact]
    public async Task GetBattingLeadersAsync_WhenPageSizeExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateLeaderboardReadService(leaderboardPageSizeMax: 4);

        var result = await service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "hr", PageSize: 12));

        Assert.Equal(4, result.PageSize);
        Assert.Equal(12, result.RequestedPageSize);
        Assert.Equal(4, result.MaxPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal(4, result.Items.Count);
    }

    [Fact]
    public async Task ListHallOfFameInducteesAsync_WhenPageSizeExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateHallOfFameReadService(hallOfFamePageSizeMax: 2);

        var result = await service.ListInducteesAsync(new HallOfFameLookupRequest(PageSize: 10));

        Assert.Equal(2, result.PageSize);
        Assert.Equal(10, result.RequestedPageSize);
        Assert.Equal(2, result.MaxPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetPitchingLeadersAsync_WithSingleSeasonEra_ReturnsAscendingResults()
    {
        var service = CreateLeaderboardReadService();

        var result = await service.GetPitchingLeadersAsync(new PitchingLeaderboardQuery(Stat: "era", SingleSeason: true, MinInningsPitched: 100, PageSize: 5));

        Assert.Equal(5, result.Items.Count);
        Assert.True(result.Items[0].Era <= result.Items[1].Era);
    }

    [Fact]
    public async Task GetSalaryLeadersAsync_WhenPageSizeExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateSalaryReadService(leaderboardPageSizeMax: 3);

        var result = await service.GetSalaryLeadersAsync(new SalaryLeaderQuery(Year: 2016, PageSize: 10));

        Assert.Equal(3, result.PageSize);
        Assert.Equal(10, result.RequestedPageSize);
        Assert.Equal(3, result.MaxPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetPlayerSalaryHistoryAsync_WhenRequestedItemCountExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateSalaryReadService(salaryHistorySeasonCountMax: 3);

        var result = await service.GetPlayerSalaryHistoryAsync("bondsba01", itemCount: 10);

        Assert.NotNull(result);
        Assert.Equal(3, result.Seasons.Count);
        Assert.Equal(10, result.RequestedItemCount);
        Assert.Equal(3, result.MaxItemCount);
        Assert.True(result.WasItemCountClamped);
    }

    [Fact]
    public async Task GetTeamPayrollAsync_WhenRequestedItemCountExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateSalaryReadService(teamPayrollPlayerCountMax: 5);

        var result = await service.GetTeamPayrollAsync("NYA", 2016, itemCount: 12);

        Assert.NotNull(result);
        Assert.Equal(5, result.Players.Count);
        Assert.Equal(12, result.RequestedItemCount);
        Assert.Equal(5, result.MaxItemCount);
        Assert.True(result.WasItemCountClamped);
    }

    [Fact]
    public async Task GetBattingLeadersAsync_WithUnsupportedStat_ThrowsUsageException()
    {
        var service = CreateLeaderboardReadService();

        var exception = await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "war")));

        Assert.Contains("Unsupported batting stat", exception.Message);
    }

    [Fact]
    public async Task GetPitchingLeadersAsync_WithNegativeMinimum_ThrowsUsageException()
    {
        var service = CreateLeaderboardReadService();

        var exception = await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.GetPitchingLeadersAsync(new PitchingLeaderboardQuery(MinInningsPitched: -1)));

        Assert.Equal("minInningsPitched must be zero or greater.", exception.Message);
    }

    [Fact]
    public async Task GetPlayerAsync_WithBlankPlayerId_ThrowsUsageException()
    {
        var service = CreatePlayerReadService();

        var exception = await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.GetPlayerAsync("   "));

        Assert.Equal("playerId is required.", exception.Message);
    }

    private static IPlayerReadService CreatePlayerReadService(int playerSearchPageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var requestPolicy = CreateRequestPolicy(playerSearchPageSizeMax: playerSearchPageSizeMax);
        var hallOfFame = new HallOfFameReadService(factory, cache, requestPolicy);
        return new PlayerReadService(factory, hallOfFame, requestPolicy);
    }

    private static IFranchiseReadService CreateFranchiseReadService(int franchiseListPageSizeMax = 50) =>
        new FranchiseReadService(new TestDbContextFactory(), CreateRequestPolicy(franchiseListPageSizeMax: franchiseListPageSizeMax));

    private static IHallOfFameReadService CreateHallOfFameReadService(int hallOfFamePageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new HallOfFameReadService(factory, cache, CreateRequestPolicy(hallOfFamePageSizeMax: hallOfFamePageSizeMax));
    }

    private static ILeaderboardReadService CreateLeaderboardReadService(int leaderboardPageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var requestPolicy = CreateRequestPolicy(leaderboardPageSizeMax: leaderboardPageSizeMax);
        var hallOfFame = new HallOfFameReadService(factory, cache, requestPolicy);
        return new LeaderboardReadService(factory, hallOfFame, requestPolicy);
    }

    private static ISalaryReadService CreateSalaryReadService(
        int leaderboardPageSizeMax = 100,
        int salaryHistorySeasonCountMax = 20,
        int teamPayrollPlayerCountMax = 25) =>
        new SalaryReadService(
            new TestDbContextFactory(),
            CreateRequestPolicy(
                leaderboardPageSizeMax: leaderboardPageSizeMax,
                salaryHistorySeasonCountMax: salaryHistorySeasonCountMax,
                teamPayrollPlayerCountMax: teamPayrollPlayerCountMax));

    private static BaseballMcpRequestPolicy CreateRequestPolicy(
        int playerSearchPageSizeMax = 100,
        int franchiseListPageSizeMax = 50,
        int leaderboardPageSizeMax = 100,
        int hallOfFamePageSizeMax = 100,
        int salaryHistorySeasonCountMax = 20,
        int teamPayrollPlayerCountMax = 25,
        int queryTimeoutSeconds = 30) =>
        new(CreateOptions(
            playerSearchPageSizeMax,
            franchiseListPageSizeMax,
            leaderboardPageSizeMax,
            hallOfFamePageSizeMax,
            salaryHistorySeasonCountMax,
            teamPayrollPlayerCountMax,
            queryTimeoutSeconds));

    private static IOptions<BaseballMcpOptions> CreateOptions(
        int playerSearchPageSizeMax = 100,
        int franchiseListPageSizeMax = 50,
        int leaderboardPageSizeMax = 100,
        int hallOfFamePageSizeMax = 100,
        int salaryHistorySeasonCountMax = 20,
        int teamPayrollPlayerCountMax = 25,
        int queryTimeoutSeconds = 30) =>
        Options.Create(new BaseballMcpOptions
        {
            QueryTimeoutSeconds = queryTimeoutSeconds,
            Limits = new BaseballMcpLimitOptions
            {
                PlayerSearchPageSizeMax = playerSearchPageSizeMax,
                FranchiseListPageSizeMax = franchiseListPageSizeMax,
                HallOfFamePageSizeMax = hallOfFamePageSizeMax,
                LeaderboardPageSizeMax = leaderboardPageSizeMax,
                SalaryHistorySeasonCountMax = salaryHistorySeasonCountMax,
                TeamPayrollPlayerCountMax = teamPayrollPlayerCountMax
            }
        });

    private sealed class TestDbContextFactory : IDbContextFactory<BaseballDbContext>
    {
        public BaseballDbContext CreateDbContext() => TestDatabaseFactory.CreateContext();

        public Task<BaseballDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
