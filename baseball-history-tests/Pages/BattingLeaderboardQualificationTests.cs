using Microsoft.AspNetCore.Mvc.Testing;
using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace baseball_history_tests.Pages;

public class BattingLeaderboardQualificationTests(WebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task BattingHR_Career_BondsHas762()
    {
        using var context = TestDatabaseFactory.CreateContext();
        var bondsTotal = await context.Batting
            .Where(b => b.PlayerId == "bondsba01")
            .SumAsync(b => (int)(b.Hr ?? 0));
        
        Assert.Equal(762, bondsTotal);
    }
    
    [Fact]
    public async Task BattingHR_Career_AaronHas755()
    {
        using var context = TestDatabaseFactory.CreateContext();
        var aaronTotal = await context.Batting
            .Where(b => b.PlayerId == "aaronha01")
            .SumAsync(b => (int)(b.Hr ?? 0));
        
        Assert.Equal(755, aaronTotal);
    }
    
    [Fact]
    public async Task BattingHR_Career_RuthHas714()
    {
        using var context = TestDatabaseFactory.CreateContext();
        var ruthTotal = await context.Batting
            .Where(b => b.PlayerId == "ruthba01")
            .SumAsync(b => (int)(b.Hr ?? 0));
        
        Assert.Equal(714, ruthTotal);
    }
    
    [Fact]
    public async Task BattingH_Career_AaronHas3771()
    {
        using var context = TestDatabaseFactory.CreateContext();
        var aaronTotal = await context.Batting
            .Where(b => b.PlayerId == "aaronha01")
            .SumAsync(b => (int)(b.H ?? 0));
        
        Assert.Equal(3771, aaronTotal);
    }

    [Fact]
    public async Task BattingAVG_CareerLeaders_Top50IncludesHistoricalNames()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=avg&singleSeason=false&pageSize=50");
        
        var historicalNames = new[] { "Cobb", "Hornsby", "Williams", "Gwynn", "Carew" };
        var matchCount = historicalNames.Count(name => html.Contains(name, StringComparison.OrdinalIgnoreCase));
        
        Assert.True(matchCount >= 3, 
            $"Expected at least 3 historical AVG leaders in top 50, found {matchCount}.");
    }
    
    [Fact]
    public async Task BattingOBP_CareerLeaders_Top50IncludesHistoricalNames()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=obp&singleSeason=false&pageSize=50");
        
        var historicalNames = new[] { "Williams", "Ruth", "Bonds", "Gehrig" };
        var matchCount = historicalNames.Count(name => html.Contains(name, StringComparison.OrdinalIgnoreCase));
        
        Assert.True(matchCount >= 2, 
            $"Expected at least 2 historical OBP leaders in top 50, found {matchCount}.");
    }

    [Fact]
    public async Task BattingAVG_Career_IncludesNegroLeaguesPlayers()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=avg&singleSeason=false&pageSize=100");
        
        var negroLeaguesStars = new[] { "Gibson", "Charleston", "Stearnes", "Bell" };
        var matchCount = negroLeaguesStars.Count(name => html.Contains(name, StringComparison.OrdinalIgnoreCase));
        
        Assert.True(matchCount >= 1, 
            $"Expected at least 1 Negro Leagues player in top 100 AVG, found {matchCount}.");
    }

    [Fact]
    public async Task BattingAVG_CareerDefault_NoSmallSampleOutliersOnPageOne()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=avg&singleSeason=false");
        
        Assert.DoesNotContain("1.000", html); 
    }
    
    [Fact]
    public async Task BattingHR_Career_UnaffectedByQualification()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=hr&singleSeason=false&pageSize=10");
        
        Assert.Contains("Bonds", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aaron", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Ruth", html, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task BattingH_Career_UnaffectedByQualification()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=h&singleSeason=false&pageSize=10");
        
        Assert.Contains("table-baseball", html);
        Assert.Contains("Batting Leaders", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BattingAVG_ExplicitMinAb_OverridesSeasonRelative()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=avg&singleSeason=false&minAb=500");
        
        Assert.Contains("table-baseball", html);
        Assert.Contains("Batting Leaders", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BattingQualification_NullTeamG_DoesNotCrash()
    {
        using var context = TestDatabaseFactory.CreateContext();
        
        var nullTeamGCount = await context.Batting
            .Where(b => b.Team.G == null || b.Team.G == 0)
            .CountAsync();
        
        Assert.Equal(0, nullTeamGCount);
    }
    
    [Fact]
    public async Task BattingAVG_WithQualification_DoesNotReturnServerError()
    {
        var response = await Client.GetAsync("/Stats/Batting?stat=avg&singleSeason=false");
        
        Assert.True(response.IsSuccessStatusCode, 
            $"Expected 200 OK, got {response.StatusCode}.");
    }

    // Regression coverage for a defect found during independent verification of #63/#65:
    // the career leaderboard path computed a season-relative Threshold but never applied it,
    // and separately, even after fixing that, the computed Threshold alone could dip below a
    // sane floor for teams with anomalously low recorded Teams.G, letting degenerate
    // small-sample careers (e.g. 4 career AB) qualify. These tests assert the *service layer*
    // behavior directly (this branch predates #65's API/UI qualified param), so they can't
    // silently regress again.

    [Fact]
    public async Task BattingAVG_Career_QualifiedTrueVsFalse_ProducesDifferentTotalCounts()
    {
        using var scope = Factory.Services.CreateScope();
        var leaderboards = scope.ServiceProvider.GetRequiredService<ILeaderboardQueryService>();

        var qualified = await leaderboards.GetBattingLeadersAsync(new LeaderboardRequest(
            Stat: "avg", FromYear: null, ToYear: null, League: null, SingleSeason: false,
            Qualified: true, MinAtBats: null, MinInningsPitched: null, Page: 1, PageSize: 10));

        var unqualified = await leaderboards.GetBattingLeadersAsync(new LeaderboardRequest(
            Stat: "avg", FromYear: null, ToYear: null, League: null, SingleSeason: false,
            Qualified: false, MinAtBats: null, MinInningsPitched: null, Page: 1, PageSize: 10));

        Assert.True(unqualified.TotalCount > qualified.TotalCount,
            $"Expected qualified=false ({unqualified.TotalCount}) > qualified=true ({qualified.TotalCount}) " +
            "for career AVG - if these are equal, the 'qualified' flag is being ignored by the career path.");
    }

    [Fact]
    public async Task BattingAVG_Career_Qualified_ExcludesDegenerateSmallSampleCareers()
    {
        using var scope = Factory.Services.CreateScope();
        var leaderboards = scope.ServiceProvider.GetRequiredService<ILeaderboardQueryService>();

        var result = await leaderboards.GetBattingLeadersAsync(new LeaderboardRequest(
            Stat: "avg", FromYear: null, ToYear: null, League: null, SingleSeason: false,
            Qualified: true, MinAtBats: null, MinInningsPitched: null, Page: 1, PageSize: 200));

        Assert.All(result.Rows, row =>
            Assert.True(row.AB + row.BB >= 100,
                $"Player {row.PlayerName} qualified with only {row.AB} AB + {row.BB} BB " +
                "(< 100 combined) - the season-relative threshold likely dipped below the flat " +
                "sanity floor due to an anomalously low Teams.G value for one of their stints."));
    }

    [Fact]
    public async Task PitchingERA_Career_QualifiedTrueVsFalse_ProducesDifferentTotalCounts()
    {
        using var scope = Factory.Services.CreateScope();
        var leaderboards = scope.ServiceProvider.GetRequiredService<ILeaderboardQueryService>();

        var qualified = await leaderboards.GetPitchingLeadersAsync(new LeaderboardRequest(
            Stat: "era", FromYear: null, ToYear: null, League: null, SingleSeason: false,
            Qualified: true, MinAtBats: null, MinInningsPitched: null, Page: 1, PageSize: 10));

        var unqualified = await leaderboards.GetPitchingLeadersAsync(new LeaderboardRequest(
            Stat: "era", FromYear: null, ToYear: null, League: null, SingleSeason: false,
            Qualified: false, MinAtBats: null, MinInningsPitched: null, Page: 1, PageSize: 10));

        Assert.True(unqualified.TotalCount > qualified.TotalCount,
            $"Expected qualified=false ({unqualified.TotalCount}) > qualified=true ({qualified.TotalCount}) " +
            "for career ERA - if these are equal, the 'qualified' flag is being ignored by the career path.");
    }

    [Fact]
    public async Task PitchingERA_Career_Qualified_ExcludesDegenerateSmallSampleCareers()
    {
        using var scope = Factory.Services.CreateScope();
        var leaderboards = scope.ServiceProvider.GetRequiredService<ILeaderboardQueryService>();

        var result = await leaderboards.GetPitchingLeadersAsync(new LeaderboardRequest(
            Stat: "era", FromYear: null, ToYear: null, League: null, SingleSeason: false,
            Qualified: true, MinAtBats: null, MinInningsPitched: null, Page: 1, PageSize: 200));

        Assert.All(result.Rows, row =>
            Assert.True(row.IP >= 30,
                $"Pitcher {row.PlayerName} qualified with only {row.IP} IP (< 30) - " +
                "the season-relative threshold likely dipped below the flat sanity floor due to " +
                "an anomalously low Teams.G value for one of their stints."));
    }
}
