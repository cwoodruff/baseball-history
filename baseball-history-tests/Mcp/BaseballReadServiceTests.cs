using baseball_history_mcp;
using baseball_history_mcp.Configuration;
using baseball_history_mcp.Metadata;
using baseball_history_mcp.Querying;
using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
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
    public async Task SearchPlayersAsync_WithFullNameQuery_MatchesBothNameTokens()
    {
        var service = CreatePlayerReadService();

        var result = await service.SearchPlayersAsync(new PlayerLookupRequest(Query: "Babe Ruth", PageSize: 10));

        Assert.Contains(result.Items, player => player.PlayerId == "ruthba01");
    }

    [Fact]
    public async Task SearchPlayersAsync_WithConflictingFilters_ThrowsUsageError()
    {
        var service = CreatePlayerReadService();

        var exception = await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.SearchPlayersAsync(new PlayerLookupRequest(Query: "Ruth", LastNameStartsWith: "R")));

        Assert.Contains("either query or lastNameStartsWith", exception.Message, StringComparison.OrdinalIgnoreCase);
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

    [Theory]
    [InlineData("avg")]
    [InlineData("obp")]
    [InlineData("slg")]
    [InlineData("ops")]
    public async Task GetBattingLeadersAsync_WithRateStat_ReturnsResults(string stat)
    {
        var service = CreateLeaderboardReadService();

        var result = await service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: stat, MinAtBats: 1000, PageSize: 5));

        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public async Task GetBattingLeadersAsync_WithOps_ReturnsDescendingResults()
    {
        var service = CreateLeaderboardReadService();

        var result = await service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "ops", MinAtBats: 1000, PageSize: 5));

        Assert.Equal(5, result.Items.Count);
        Assert.True(result.Items[0].Ops >= result.Items[4].Ops);
    }

    [Fact]
    public async Task GetPitchingLeadersAsync_WithWinningPercentage_ReturnsDescendingResults()
    {
        var service = CreateLeaderboardReadService();

        var result = await service.GetPitchingLeadersAsync(new PitchingLeaderboardQuery(Stat: "wpct", MinInningsPitched: 100, PageSize: 5));

        Assert.Equal(5, result.Items.Count);
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
    public async Task GetBattingLeadersAsync_WithCareerStrikeouts_ReturnsDescendingResults()
    {
        var service = CreateLeaderboardReadService();

        var result = await service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "so", PageSize: 5));

        Assert.Equal(5, result.Items.Count);
        Assert.True(result.Items[0].Strikeouts > 0);
        Assert.True(result.Items[0].Strikeouts >= result.Items[4].Strikeouts);
    }

    [Fact]
    public async Task GetBattingLeadersAsync_WithSingleSeasonStrikeoutsAlias_ReturnsDescendingResults()
    {
        var service = CreateLeaderboardReadService();

        var result = await service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "strikeouts", SingleSeason: true, PageSize: 5));

        Assert.Equal(5, result.Items.Count);
        Assert.All(result.Items, item => Assert.NotNull(item.Year));
        Assert.True(result.Items[0].Strikeouts >= result.Items[4].Strikeouts);
    }

    [Fact]
    public async Task GetBattingLeadersAsync_WithInvalidStat_ThrowsUsageError()
    {
        var service = CreateLeaderboardReadService();

        var exception = await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.GetBattingLeadersAsync(new BattingLeaderboardQuery(Stat: "totally-not-a-stat")));

        Assert.Contains("Unsupported batting stat", exception.Message);
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
    public async Task GetPitchingLeadersAsync_WithInvalidYearRange_ThrowsUsageError()
    {
        var service = CreateLeaderboardReadService();

        var exception = await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.GetPitchingLeadersAsync(new PitchingLeaderboardQuery(FromYear: 2001, ToYear: 1999)));

        Assert.Equal("fromYear must be less than or equal to toYear.", exception.Message);
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

        var result = await service.ListInducteesAsync(new HallOfFameLookupRequest(PageSize: 10));

        Assert.Equal(2, result.PageSize);
        Assert.Equal(10, result.RequestedPageSize);
        Assert.Equal(2, result.MaxPageSize);
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
        Assert.Equal(3, result.MaxPageSize);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal(3, result.Items.Count);
    }

    private static IPlayerReadService CreatePlayerReadService(int playerSearchPageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = CreateOptions(playerSearchPageSizeMax: playerSearchPageSizeMax);
        var requestPolicy = CreateRequestPolicy(options);
        var hallOfFame = new HallOfFameReadService(factory, cache, requestPolicy, options);
        return new PlayerReadService(factory, hallOfFame, requestPolicy);
    }

    private static IFranchiseReadService CreateFranchiseReadService(int franchiseListPageSizeMax = 50) =>
        new FranchiseReadService(
            new TestDbContextFactory(),
            CreateRequestPolicy(CreateOptions(franchiseListPageSizeMax: franchiseListPageSizeMax)));

    private static ILeaderboardReadService CreateLeaderboardReadService(int leaderboardPageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var requestPolicy = CreateRequestPolicy(CreateOptions(leaderboardPageSizeMax: leaderboardPageSizeMax));
        var context = factory.CreateDbContext();
        var queryService = new LeaderboardQueryService(context);
        return new LeaderboardReadService(queryService, requestPolicy);
    }

    private static IHallOfFameReadService CreateHallOfFameReadService(
        int hallOfFamePageSizeMax = 50,
        int hallOfFameVotingHistoryYearsMax = 25,
        int playerSearchPageSizeMax = 100)
    {
        var factory = new TestDbContextFactory();
        var options = CreateOptions(
            playerSearchPageSizeMax: playerSearchPageSizeMax,
            hallOfFamePageSizeMax: hallOfFamePageSizeMax,
            hallOfFameVotingHistoryYearsMax: hallOfFameVotingHistoryYearsMax);

        return new HallOfFameReadService(
            factory,
            new MemoryCache(new MemoryCacheOptions()),
            CreateRequestPolicy(options),
            options);
    }

    private static ITeamReadService CreateTeamReadService()
    {
        var factory = new TestDbContextFactory();
        var options = CreateOptions();
        var hallOfFame = new HallOfFameReadService(
            factory,
            new MemoryCache(new MemoryCacheOptions()),
            CreateRequestPolicy(options),
            options);
        return new TeamReadService(factory, hallOfFame);
    }

    [Fact]
    public async Task GetPlayerPostseasonAsync_WithJeter_ReturnsBattingLines()
    {
        var service = CreatePostseasonReadService();

        var postseason = await service.GetPlayerPostseasonAsync("jeterde01");

        Assert.NotNull(postseason);
        Assert.Equal("Derek Jeter", postseason.FullName);
        Assert.True(postseason.Batting.Count > 20);
        // 2000 World Series MVP year is present with the WS round
        Assert.Contains(postseason.Batting, b => b.Year == 2000 && b.Round == "WS");
        // Rounds within a year sort chronologically, not alphabetically:
        // the 2000 run reads ALDS -> ALCS -> WS
        var run2000 = postseason.Batting.Where(b => b.Year == 2000).Select(b => b.Round).ToList();
        Assert.Equal(run2000.Count - 1, run2000.IndexOf("WS"));
    }

    [Fact]
    public async Task GetPlayerPostseasonAsync_CapsRows()
    {
        var service = CreatePostseasonReadService(postseasonRowsPerCategoryMax: 5);

        var postseason = await service.GetPlayerPostseasonAsync("jeterde01");

        Assert.NotNull(postseason);
        Assert.Equal(5, postseason.Batting.Count);
        Assert.True(postseason.WasBattingCapped);
        Assert.True(postseason.TotalBattingRowCount > 5);
    }

    [Fact]
    public async Task GetPlayerPostseasonAsync_WithUnknownPlayer_ReturnsNull()
    {
        var service = CreatePostseasonReadService();

        Assert.Null(await service.GetPlayerPostseasonAsync("nosuchplayer99"));
    }

    [Fact]
    public async Task GetPlayerFieldingAsync_WithRuth_ReturnsPositions()
    {
        var service = CreateFieldingReadService();

        var fielding = await service.GetPlayerFieldingAsync("ruthba01");

        Assert.NotNull(fielding);
        Assert.Equal("Babe Ruth", fielding.FullName);
        // Ruth pitched and played the outfield
        Assert.Contains(fielding.CareerByPosition, p => p.Position == "P");
        Assert.Contains(fielding.CareerByPosition, p => p.Position == "OF" || p.Position == "RF" || p.Position == "LF");
        Assert.All(fielding.CareerByPosition.Where(p => p.FieldingPercentage.HasValue),
            p => Assert.InRange(p.FieldingPercentage!.Value, 0, 1));
    }

    [Fact]
    public async Task GetPlayerFieldingAsync_WithUnknownPlayer_ReturnsNull()
    {
        var service = CreateFieldingReadService();

        Assert.Null(await service.GetPlayerFieldingAsync("nosuchplayer99"));
    }

    [Fact]
    public async Task GetPlayerFieldingAsync_WithCappedSeasonRows_CareerTotalsCoverFullHistory()
    {
        // Ruth's fielding history spans more rows than a small cap allows; career
        // totals must still reflect every season, not just the capped rows returned.
        var cappedService = CreateFieldingReadService(fieldingSeasonRowsMax: 3);
        var uncappedService = CreateFieldingReadService(fieldingSeasonRowsMax: 200);

        var capped = await cappedService.GetPlayerFieldingAsync("ruthba01");
        var uncapped = await uncappedService.GetPlayerFieldingAsync("ruthba01");

        Assert.NotNull(capped);
        Assert.NotNull(uncapped);
        Assert.True(capped.WasSeasonListCapped);
        Assert.Equal(3, capped.Seasons.Count);

        var cappedTotalGames = capped.CareerByPosition.Sum(p => p.Games);
        var uncappedTotalGames = uncapped.CareerByPosition.Sum(p => p.Games);

        Assert.Equal(uncappedTotalGames, cappedTotalGames);
        Assert.True(cappedTotalGames > capped.Seasons.Sum(s => s.Games));
    }

    [Fact]
    public async Task SearchParksAsync_ByName_FindsFenway()
    {
        var service = CreateParkReadService();

        var result = await service.SearchParksAsync(new ParkSearchRequest(Query: "fenway"));

        Assert.Contains(result.Items, p => p.ParkKey == "BOS07" && p.Name == "Fenway Park");
    }

    [Fact]
    public async Task SearchParksAsync_ByState_FiltersResults()
    {
        var service = CreateParkReadService();

        var result = await service.SearchParksAsync(new ParkSearchRequest(State: "ma"));

        Assert.True(result.TotalCount > 0);
        Assert.All(result.Items, p => Assert.Equal("MA", p.State));
    }

    [Fact]
    public async Task GetParkAsync_WithFenway_ReturnsTenantsAndSeasons()
    {
        var service = CreateParkReadService();

        var park = await service.GetParkAsync("bos07");

        Assert.NotNull(park);
        Assert.Equal("Fenway Park", park.Name);
        Assert.Equal((short)1912, park.FirstYear);
        // The Braves borrowed Fenway in 1913-14, so it has at least two tenants
        Assert.True(park.Tenants.Count >= 2);
        Assert.Contains(park.Tenants, t => t.TeamId == "BOS" && t.LeagueId == "AL");
        Assert.True(park.TotalAttendance > 100_000_000);
    }

    [Fact]
    public async Task GetParkAsync_CapsSeasonRows()
    {
        var service = CreateParkReadService(parkSeasonRowsMax: 10);

        var park = await service.GetParkAsync("BOS07");

        Assert.NotNull(park);
        Assert.Equal(10, park.Seasons.Count);
        Assert.True(park.WasSeasonListCapped);
        Assert.True(park.TotalSeasonRowCount > 10);
    }

    [Fact]
    public async Task GetParkAsync_WithCappedSeasonRows_AggregatesCoverFullHistory()
    {
        var cappedService = CreateParkReadService(parkSeasonRowsMax: 3);
        var uncappedService = CreateParkReadService(parkSeasonRowsMax: 1000);

        var capped = await cappedService.GetParkAsync("BOS07");
        var uncapped = await uncappedService.GetParkAsync("BOS07");

        Assert.NotNull(capped);
        Assert.NotNull(uncapped);
        Assert.True(capped.WasSeasonListCapped);
        Assert.False(uncapped.WasSeasonListCapped);
        Assert.Equal(3, capped.Seasons.Count);
        Assert.True(uncapped.Seasons.Count > capped.Seasons.Count);

        Assert.Equal(uncapped.ParkKey, capped.ParkKey);
        Assert.Equal(uncapped.Name, capped.Name);
        Assert.Equal(uncapped.Aliases, capped.Aliases);
        Assert.Equal(uncapped.City, capped.City);
        Assert.Equal(uncapped.State, capped.State);
        Assert.Equal(uncapped.Country, capped.Country);
        Assert.Equal(uncapped.FirstYear, capped.FirstYear);
        Assert.Equal(uncapped.LastYear, capped.LastYear);
        Assert.Equal(uncapped.TotalGames, capped.TotalGames);
        Assert.Equal(uncapped.TotalAttendance, capped.TotalAttendance);
        Assert.Equal(uncapped.TotalSeasonRowCount, capped.TotalSeasonRowCount);
        Assert.Equal(uncapped.Tenants, capped.Tenants);
        Assert.Equal(uncapped.Seasons.Take(capped.Seasons.Count), capped.Seasons);
    }

    [Fact]
    public async Task GetParkAsync_WithUnknownKey_ReturnsNull()
    {
        var service = CreateParkReadService();

        Assert.Null(await service.GetParkAsync("NOPE99"));
    }

    [Fact]
    public async Task GetManagerAsync_WithConnieMack_ReturnsCareer()
    {
        var service = CreateManagerReadService();

        var manager = await service.GetManagerAsync("mackco01");

        Assert.NotNull(manager);
        Assert.Equal("Connie Mack", manager.FullName);
        Assert.Equal(3731, manager.Wins);
        Assert.Equal(9, manager.Pennants);
        Assert.Equal(5, manager.WorldSeriesTitles);
        Assert.True(manager.IsInHallOfFame);
        Assert.True(manager.WasPlayerManager);
        Assert.Equal((short)1894, manager.FirstYear);
        Assert.Equal((short)1950, manager.LastYear);
    }

    [Fact]
    public async Task GetManagerAsync_CapsSeasonRows()
    {
        var service = CreateManagerReadService(managerSeasonRowsMax: 10);

        var manager = await service.GetManagerAsync("mackco01");

        Assert.NotNull(manager);
        Assert.Equal(10, manager.Seasons.Count);
        Assert.True(manager.WasSeasonListCapped);
        // Career totals still cover the full record despite the capped list
        Assert.Equal(3731, manager.Wins);
    }

    [Fact]
    public async Task GetManagerAsync_WithNonManager_ReturnsNull()
    {
        var service = CreateManagerReadService();

        // Jeter played but never managed
        Assert.Null(await service.GetManagerAsync("jeterde01"));
    }

    [Fact]
    public async Task SearchPlayersAsync_WithPositionEraAndSort_MatchesWebFilters()
    {
        var service = CreatePlayerReadService();

        var result = await service.SearchPlayersAsync(
            new PlayerLookupRequest(LastNameStartsWith: "R", PageSize: 10, Position: "ss", Era: 1925, Sort: "HR"));

        Assert.True(result.Items.Count > 1);
        var homeRuns = result.Items.Select(p => p.TotalHomeRuns ?? 0).ToList();
        Assert.Equal(homeRuns.OrderByDescending(h => h), homeRuns);
        Assert.All(result.Items, p =>
        {
            Assert.True(p.DebutYear <= 1929);
            Assert.True(p.FinalYear >= 1920);
        });
    }

    [Theory]
    [InlineData("XX", null, null)]
    [InlineData(null, 1700, null)]
    [InlineData(null, null, "salary")]
    public async Task SearchPlayersAsync_WithInvalidFilter_ThrowsUsageError(string? position, int? era, string? sort)
    {
        var service = CreatePlayerReadService();

        await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.SearchPlayersAsync(new PlayerLookupRequest(Position: position, Era: era, Sort: sort)));
    }

    [Fact]
    public async Task GetPlayerAsync_IncludesOpsIndexAllStarYearsAndManagerFlag()
    {
        var service = CreatePlayerReadService();

        var player = await service.GetPlayerAsync("torrejo01");

        Assert.NotNull(player);
        Assert.True(player.CareerOpsIndex > 100);
        Assert.Equal(9, player.AllStarSelectionCount);
        Assert.Equal(player.AllStarSelectionCount, player.AllStarYears!.Count);
        Assert.True(player.HasManagerialCareer);
    }

    [Fact]
    public async Task GetPlayerAdvancedStatsAsync_ReturnsBattingAndPitchingRates()
    {
        var service = CreatePlayerReadService();

        var stats = await service.GetPlayerAdvancedStatsAsync("ruthba01");

        Assert.NotNull(stats);
        var batting1927 = Assert.Single(stats.BattingSeasons, s => s.Year == 1927);
        Assert.True(batting1927.OpsIndex > 200);
        Assert.True(batting1927.Qualified);
        var pitching1916 = Assert.Single(stats.PitchingSeasons, s => s.Year == 1916);
        Assert.InRange(pitching1916.Era!.Value, 1.5, 2.0);
        Assert.Null(await service.GetPlayerAdvancedStatsAsync("zzzzz99"));
    }

    [Fact]
    public async Task ListFranchisesAsync_WithNameAndEra_FiltersRows()
    {
        var service = CreateFranchiseReadService();

        var result = await service.ListFranchisesAsync(new FranchiseLookupRequest(Query: "giants", Era: 1935));

        Assert.Contains(result.Items, f => f.FranchiseId == "SFG");
        Assert.All(result.Items, f =>
        {
            Assert.Contains("giants", f.FranchiseName, StringComparison.OrdinalIgnoreCase);
            Assert.True(f.FirstYear <= 1939 && f.LastYear >= 1930);
        });
    }

    [Fact]
    public async Task ListManagersAsync_DefaultSort_LeadsWithConnieMackAndClampsPageSize()
    {
        var service = CreateManagerReadService(managerListPageSizeMax: 5);

        var result = await service.ListManagersAsync(new ManagerLookupRequest(PageSize: 20));

        Assert.Equal(5, result.Items.Count);
        Assert.True(result.WasPageSizeClamped);
        Assert.Equal("mackco01", result.Items[0].PlayerId);
        Assert.Equal(3731, result.Items[0].Wins);
        Assert.True(result.Items[0].IsInHallOfFame);
    }

    [Fact]
    public async Task ListManagersAsync_WithNameQuery_SortsByName()
    {
        var service = CreateManagerReadService();

        var result = await service.ListManagersAsync(new ManagerLookupRequest(Query: "mack", Sort: "name"));

        Assert.Contains(result.Items, m => m.PlayerId == "mackco01");
        var names = result.Items.Select(m => m.FullName).ToList();
        Assert.Equal(names.Order(StringComparer.Ordinal), names);
        await Assert.ThrowsAsync<BaseballMcpUsageException>(() =>
            service.ListManagersAsync(new ManagerLookupRequest(Sort: "losses")));
    }

    [Fact]
    public async Task AllStarReadService_ListsYearsAndRosters()
    {
        var service = CreateAllStarReadService();

        var years = await service.ListAllStarYearsAsync();
        Assert.Contains(years, y => y.Year == 1960 && y.GameCount == 2);

        var season = await service.GetAllStarYearAsync(1943);
        Assert.NotNull(season);
        Assert.Equal("AllStar", season.Games[0].GameType);
        Assert.Contains(season.Games, g => g.GameType == "EastWest");
        Assert.True(season.Games[0].Rosters[0].Players[0].IsStarter);
        Assert.Null(await service.GetAllStarYearAsync(1800));
    }

    [Fact]
    public async Task NegroLeagueReadService_ReturnsLeaguesAndSeason()
    {
        var service = CreateNegroLeagueReadService();

        var leagues = await service.ListLeaguesAsync();
        Assert.Equal(7, leagues.Count);

        var league = await service.GetLeagueAsync("nn2");
        Assert.NotNull(league);
        Assert.Contains(league.Clubs, c => c.TeamId == "HG");

        var season = await service.GetLeagueSeasonAsync("NN2", 1943);
        Assert.NotNull(season);
        Assert.Equal("HG", season.Standings[0].TeamId);
        Assert.Equal(0, season.Standings[0].GamesBehind);
        Assert.NotEmpty(season.BattingAverageLeaders);
        Assert.Null(await service.GetLeagueAsync("XYZ"));
        Assert.Null(await service.GetLeagueSeasonAsync("NN2", 1960));
    }

    [Fact]
    public async Task ThisDayReadService_RanksHallOfFamersFirstAndValidatesInput()
    {
        var service = CreateThisDayReadService();

        var day = await service.GetThisDayAsync("02-06", 3);
        Assert.Equal("February 6", day.Label);
        Assert.Equal(3, day.Birthdays.Count);
        Assert.Equal("ruthba01", day.Birthdays[0].PlayerId);

        await Assert.ThrowsAsync<ModelContextProtocol.McpProtocolException>(() => service.GetThisDayAsync("02-30", 5));
        await Assert.ThrowsAsync<ModelContextProtocol.McpProtocolException>(() => service.GetThisDayAsync("02-06", 26));
    }

    [Fact]
    public async Task ThisDayReadService_AcceptsMonthAndDayAsAlternativeToDate()
    {
        var service = CreateThisDayReadService();

        var day = await service.GetThisDayAsync(null, 3, month: 2, day: 6);
        Assert.Equal("February 6", day.Label);
        Assert.Equal("ruthba01", day.Birthdays[0].PlayerId);

        await Assert.ThrowsAsync<ModelContextProtocol.McpProtocolException>(() => service.GetThisDayAsync(null, 5, month: 2));
        await Assert.ThrowsAsync<ModelContextProtocol.McpProtocolException>(() => service.GetThisDayAsync(null, 5, month: 2, day: 30));
        await Assert.ThrowsAsync<ModelContextProtocol.McpProtocolException>(() => service.GetThisDayAsync("02-06", 5, month: 2, day: 6));
    }

    private static ISalaryReadService CreateSalaryReadService(
        int salaryHistorySeasonsMax = 40,
        int salaryLeaderboardPageSizeMax = 50) =>
        new SalaryReadService(
            new TestDbContextFactory(),
            CreateOptions(
                salaryHistorySeasonsMax: salaryHistorySeasonsMax,
                salaryLeaderboardPageSizeMax: salaryLeaderboardPageSizeMax));

    private static IPostseasonReadService CreatePostseasonReadService(int postseasonRowsPerCategoryMax = 200) =>
        new PostseasonReadService(
            new TestDbContextFactory(),
            CreateOptions(postseasonRowsPerCategoryMax: postseasonRowsPerCategoryMax));

    private static IFieldingReadService CreateFieldingReadService(int fieldingSeasonRowsMax = 200) =>
        new FieldingReadService(
            new TestDbContextFactory(),
            CreateOptions(fieldingSeasonRowsMax: fieldingSeasonRowsMax));

    private static IParkReadService CreateParkReadService(
        int parkSearchPageSizeMax = 50,
        int parkSeasonRowsMax = 160) =>
        new ParkReadService(
            new TestDbContextFactory(),
            CreateOptions(parkSearchPageSizeMax: parkSearchPageSizeMax, parkSeasonRowsMax: parkSeasonRowsMax));

    private static IManagerReadService CreateManagerReadService(
        int managerSeasonRowsMax = 80,
        int managerListPageSizeMax = 50)
    {
        var factory = new TestDbContextFactory();
        var options = CreateOptions(managerSeasonRowsMax: managerSeasonRowsMax, managerListPageSizeMax: managerListPageSizeMax);
        var requestPolicy = CreateRequestPolicy(options);
        return new ManagerReadService(factory, options, CreateHallOfFameReadService(factory, options), requestPolicy);
    }

    private static IAllStarReadService CreateAllStarReadService()
    {
        var factory = new TestDbContextFactory();
        return new AllStarReadService(factory, CreateHallOfFameReadService(factory, CreateOptions()));
    }

    private static INegroLeagueReadService CreateNegroLeagueReadService()
    {
        var factory = new TestDbContextFactory();
        return new NegroLeagueReadService(factory, new LeaderboardQueryService(factory.CreateDbContext()));
    }

    private static IThisDayReadService CreateThisDayReadService()
    {
        var factory = new TestDbContextFactory();
        return new ThisDayReadService(
            factory,
            CreateHallOfFameReadService(factory, CreateOptions()),
            new MemoryCache(new MemoryCacheOptions()));
    }

    private static IHallOfFameReadService CreateHallOfFameReadService(
        IDbContextFactory<BaseballDbContext> factory,
        IOptions<BaseballMcpOptions> options) =>
        new HallOfFameReadService(factory, new MemoryCache(new MemoryCacheOptions()), CreateRequestPolicy(options), options);

    private static BaseballMcpRequestPolicy CreateRequestPolicy(IOptions<BaseballMcpOptions>? options = null) =>
        new(options ?? CreateOptions());

    private static IOptions<BaseballMcpOptions> CreateOptions(
        int playerSearchPageSizeMax = 100,
        int franchiseListPageSizeMax = 50,
        int leaderboardPageSizeMax = 100,
        int hallOfFamePageSizeMax = 50,
        int hallOfFameVotingHistoryYearsMax = 25,
        int salaryHistorySeasonsMax = 40,
        int salaryLeaderboardPageSizeMax = 50,
        int postseasonRowsPerCategoryMax = 200,
        int fieldingSeasonRowsMax = 200,
        int parkSearchPageSizeMax = 50,
        int parkSeasonRowsMax = 160,
        int managerSeasonRowsMax = 80,
        int managerListPageSizeMax = 50,
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
                SalaryLeaderboardPageSizeMax = salaryLeaderboardPageSizeMax,
                PostseasonRowsPerCategoryMax = postseasonRowsPerCategoryMax,
                FieldingSeasonRowsMax = fieldingSeasonRowsMax,
                ParkSearchPageSizeMax = parkSearchPageSizeMax,
                ParkSeasonRowsMax = parkSeasonRowsMax,
                ManagerSeasonRowsMax = managerSeasonRowsMax,
                ManagerListPageSizeMax = managerListPageSizeMax
            }
        });

    private sealed class TestDbContextFactory : IDbContextFactory<BaseballDbContext>
    {
        public BaseballDbContext CreateDbContext() => TestDatabaseFactory.CreateContext();

        public Task<BaseballDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
