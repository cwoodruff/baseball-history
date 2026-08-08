namespace BaseballHistory.Data.Querying;

public sealed record LeaderboardRequest(
    string Stat,
    int? FromYear = null,
    int? ToYear = null,
    string? League = null,
    bool SingleSeason = false,
    bool Qualified = true,
    int? MinAtBats = null,
    int? MinInningsPitched = null,
    int Page = 1,
    int PageSize = 25);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Rows,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record BattingLeaderRow(
    int Rank,
    string PlayerId,
    string PlayerName,
    bool IsHallOfFamer,
    short? YearId,
    string? TeamId,
    string? TeamName,
    int G,
    int AB,
    int R,
    int H,
    int Doubles,
    int Triples,
    int HR,
    int RBI,
    int SB,
    int BB,
    decimal? AVG,
    decimal? OBP,
    decimal? SLG,
    decimal? OPS);

public sealed record PitchingLeaderRow(
    int Rank,
    string PlayerId,
    string PlayerName,
    bool IsHallOfFamer,
    short? YearId,
    string? TeamId,
    string? TeamName,
    int G,
    int GS,
    int W,
    int L,
    int SV,
    int CG,
    int SHO,
    decimal IP,
    int H,
    int HR,
    int BB,
    int SO,
    decimal? ERA,
    decimal? WHIP,
    decimal? K9,
    decimal? BB9,
    decimal? WPCT);
