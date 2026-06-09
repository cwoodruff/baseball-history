using baseball_history_mcp.Configuration;
using baseball_history_mcp.Querying;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModelContextProtocol;

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
    public async Task SearchPlayersAsync_WithFullNameQuery_MatchesBothNameTokens()
    {
        var service = CreatePlayerReadService();

        var result = await service.SearchPlayersAsync(new PlayerLookupRequest(Query: "Babe Ruth", PageSize: 10));

        Assert.Contains(result.Items, player => player.PlayerId == "ruthba01");
    }

    [Fact]
    public async Task SearchPlayersAsync_WithConflictingFilters_ThrowsInvalidParams()
    {
        var service = CreatePlayerReadService();

        var exception = await Assert.ThrowsAsync<McpProtocolException>(() =>
            service.SearchPlayersAsync(new PlayerLookupRequest(Query: "Ruth", LastNameStartsWith: "R")));

        Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
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
    public async Task GetPlayerAsync_WithMixedCaseId_NormalizesLookup()
    {
        var service = CreatePlayerReadService();

        var player = await service.GetPlayerAsync("RUTHBA01");

        Assert.NotNull(player);
        Assert.Equal("ruthba01", player.PlayerId);
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
    public async Task GetFranchiseAsync_WithLowerCaseId_NormalizesLookup()
    {
        var service = CreateFranchiseReadService();

        var franchise = await service.GetFranchiseAsync("nyy");

        Assert.NotNull(franchise);
        Assert.Equal("NYY", franchise.FranchiseId);
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
    public async Task GetBattingLeadersAsync_WithInvalidStat_ThrowsInvalidParams()
    {
        var service = CreateLeaderboardReadService();

        var exception = await Assert.ThrowsAsync<McpProtocolException>(() =>
            service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "totally-not-a-stat")));

        Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
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
    public async Task GetPitchingLeadersAsync_WithInvalidYearRange_ThrowsInvalidParams()
    {
        var service = CreateLeaderboardReadService();

        var exception = await Assert.ThrowsAsync<McpProtocolException>(() =>
            service.GetPitchingLeadersAsync(new PitchingLeaderboardQuery(FromYear: 2001, ToYear: 1999)));

        Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
    }

    [Fact]
    public async Task GetTeamSeasonAsync_WithNormalizedInputs_ReturnsExactSeason()
    {
        using var context = TestDatabaseFactory.CreateContext();
        var knownSeason = await context.Teams
            .Where(team => team.YearId >= 2000)
            .OrderBy(team => team.YearId)
            .Select(team => new { team.TeamId, team.LgId, Year = (int)team.YearId })
            .FirstAsync();

        var service = CreateTeamReadService();
        var season = await service.GetTeamSeasonAsync(knownSeason.TeamId.ToLowerInvariant(), knownSeason.LgId.ToLowerInvariant(), knownSeason.Year);

        Assert.NotNull(season);
        Assert.Equal(knownSeason.TeamId, season.TeamId);
        Assert.Equal(knownSeason.LgId, season.LeagueId);
        Assert.Equal(knownSeason.Year, season.Year);
    }

    [Fact]
    public async Task ListHallOfFameInducteesAsync_WhenPageSizeExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateHallOfFameReadService(hallOfFamePageSizeMax: 2);

        var result = await service.GetInducteesAsync(new HallOfFameInducteeRequest(PageSize: 10));

        Assert.Equal(2, result.PageSize);
        Assert.Equal(10, result.RequestedPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetHallOfFameVotingHistoryAsync_ReturnsBoundedChronologicalHistory()
    {
        var service = CreateHallOfFameReadService(hallOfFameVotingHistoryYearsMax: 2);

        var history = await service.GetVotingHistoryAsync("ruthba01");

        Assert.NotNull(history);
        Assert.True(history.TotalYearCount >= history.ReturnedYearCount);
        Assert.True(history.ReturnedYearCount <= 2);
        Assert.True(history.VotingHistory.SequenceEqual(history.VotingHistory.OrderBy(row => row.Year).ThenBy(row => row.Category).ThenBy(row => row.VotedBy)));
    }

    [Fact]
    public async Task GetPlayerSalaryHistoryAsync_ReturnsBoundedSeasonList()
    {
        using var context = TestDatabaseFactory.CreateContext();
        var playerId = await context.Salaries
            .OrderBy(salary => salary.PlayerId)
            .Select(salary => salary.PlayerId)
            .FirstAsync();

        var service = CreateSalaryReadService(salaryHistorySeasonsMax: 1);
        var history = await service.GetPlayerSalaryHistoryAsync(playerId);

        Assert.NotNull(history);
        Assert.Equal(playerId, history.PlayerId);
        Assert.True(history.TotalSeasonCount >= history.ReturnedSeasonCount);
        Assert.True(history.ReturnedSeasonCount <= 1);
    }

    [Fact]
    public async Task GetSalaryLeadersAsync_WhenPageSizeExceedsConfiguredCap_ReportsAppliedLimit()
    {
        var service = CreateSalaryReadService(salaryLeaderboardPageSizeMax: 3);

        var result = await service.GetSalaryLeadersAsync(new SalaryLeaderRequest(PageSize: 10));

        Assert.Equal(3, result.PageSize);
        Assert.Equal(10, result.RequestedPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal(3, result.Items.Count);
    }

    private static IPlayerReadService CreatePlayerReadService(int playerSearchPageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var hallOfFame = new HallOfFameReadService(factory, cache, CreateOptions(playerSearchPageSizeMax: playerSearchPageSizeMax));
        return new PlayerReadService(factory, hallOfFame, CreateOptions(playerSearchPageSizeMax: playerSearchPageSizeMax));
    }

    private static IFranchiseReadService CreateFranchiseReadService(int franchiseListPageSizeMax = 50) =>
        new FranchiseReadService(new TestDbContextFactory(), CreateOptions(franchiseListPageSizeMax: franchiseListPageSizeMax));

    private static ILeaderboardReadService CreateLeaderboardReadService(int leaderboardPageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var hallOfFame = new HallOfFameReadService(factory, cache, CreateOptions(leaderboardPageSizeMax: leaderboardPageSizeMax));
        return new LeaderboardReadService(factory, hallOfFame, CreateOptions(leaderboardPageSizeMax: leaderboardPageSizeMax));
    }

    private static IHallOfFameReadService CreateHallOfFameReadService(
        int hallOfFamePageSizeMax = 50,
        int hallOfFameVotingHistoryYearsMax = 25)
    {
        var factory = new TestDbContextFactory();
        return new HallOfFameReadService(
            factory,
            new MemoryCache(new MemoryCacheOptions()),
            CreateOptions(
                hallOfFamePageSizeMax: hallOfFamePageSizeMax,
                hallOfFameVotingHistoryYearsMax: hallOfFameVotingHistoryYearsMax));
    }

    private static ITeamReadService CreateTeamReadService()
    {
        var factory = new TestDbContextFactory();
        var hallOfFame = new HallOfFameReadService(factory, new MemoryCache(new MemoryCacheOptions()), CreateOptions());
        return new TeamReadService(factory, hallOfFame);
    }

    private static ISalaryReadService CreateSalaryReadService(
        int salaryHistorySeasonsMax = 40,
        int salaryLeaderboardPageSizeMax = 50) =>
        new SalaryReadService(
            new TestDbContextFactory(),
            CreateOptions(
                salaryHistorySeasonsMax: salaryHistorySeasonsMax,
                salaryLeaderboardPageSizeMax: salaryLeaderboardPageSizeMax));

    private static IOptions<BaseballMcpOptions> CreateOptions(
        int playerSearchPageSizeMax = 100,
        int franchiseListPageSizeMax = 50,
        int leaderboardPageSizeMax = 100,
        int hallOfFamePageSizeMax = 50,
        int hallOfFameVotingHistoryYearsMax = 25,
        int salaryHistorySeasonsMax = 40,
        int salaryLeaderboardPageSizeMax = 50,
        int queryTimeoutSeconds = 30) =>
        Options.Create(new BaseballMcpOptions
        {
            QueryTimeoutSeconds = queryTimeoutSeconds,
            Limits = new BaseballMcpLimitOptions
            {
                PlayerSearchPageSizeMax = playerSearchPageSizeMax,
                FranchiseListPageSizeMax = franchiseListPageSizeMax,
                LeaderboardPageSizeMax = leaderboardPageSizeMax,
                HallOfFamePageSizeMax = hallOfFamePageSizeMax,
                HallOfFameVotingHistoryYearsMax = hallOfFameVotingHistoryYearsMax,
                SalaryHistorySeasonsMax = salaryHistorySeasonsMax,
                SalaryLeaderboardPageSizeMax = salaryLeaderboardPageSizeMax
            }
        });

    private sealed class TestDbContextFactory : IDbContextFactory<BaseballDbContext>
    {
        public BaseballDbContext CreateDbContext() => TestDatabaseFactory.CreateContext();

        public Task<BaseballDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
