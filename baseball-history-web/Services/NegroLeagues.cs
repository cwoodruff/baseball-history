namespace baseball_history_web.Services;

/// <summary>
/// One of the seven Negro Leagues recognized by MLB as major leagues.
/// </summary>
public sealed record NegroLeagueInfo(
    string Id,
    string Name,
    short FirstYear,
    short LastYear,
    string Summary)
{
    public string YearsLabel => FirstYear == LastYear ? FirstYear.ToString() : $"{FirstYear}–{LastYear}";
}

/// <summary>
/// Registry of the seven Negro Leagues (1920–1948) that MLB recognized as major
/// leagues in December 2020. This is the scope of the Negro Leagues hub; earlier
/// independent Black baseball (lgIDs IND, EAS, WES, NAC in the data) predates the
/// organized leagues and is covered narratively on /SurvivingRecords instead.
/// </summary>
public static class NegroLeagues
{
    public static readonly IReadOnlyList<NegroLeagueInfo> All =
    [
        new("NNL", "Negro National League (I)", 1920, 1931,
            "Founded by Rube Foster in 1920, the first stable Black professional league, " +
            "anchored by his Chicago American Giants and the St. Louis Stars."),
        new("ECL", "Eastern Colored League", 1923, 1928,
            "The eastern rival to the Negro National League, home of Hilldale and the " +
            "Bacharach Giants; its champions met the NNL's in the first Colored World Series."),
        new("ANL", "American Negro League", 1929, 1929,
            "A one-season successor to the Eastern Colored League after its 1928 collapse."),
        new("EWL", "East-West League", 1932, 1932,
            "Cum Posey's Depression-era league, which folded before finishing its only season."),
        new("NSL", "Negro Southern League", 1932, 1932,
            "A long-running minor circuit whose 1932 season — with many displaced stars — " +
            "is counted as major league play."),
        new("NN2", "Negro National League (II)", 1933, 1948,
            "Rebuilt by Gus Greenlee in 1933; the Homestead Grays and Pittsburgh Crawfords " +
            "defined its era."),
        new("NAL", "Negro American League", 1937, 1948,
            "The midwestern and southern circuit whose Kansas City Monarchs sent " +
            "Jackie Robinson into organized baseball.")
    ];

    private static readonly Dictionary<string, NegroLeagueInfo> ById =
        All.ToDictionary(l => l.Id, StringComparer.OrdinalIgnoreCase);

    public static NegroLeagueInfo? Find(string? lgId) =>
        lgId != null && ById.TryGetValue(lgId.Trim(), out var league) ? league : null;

    public static bool IsNegroLeague(string? lgId) => Find(lgId) != null;
}
