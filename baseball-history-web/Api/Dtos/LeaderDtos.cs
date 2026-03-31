namespace baseball_history_web.Api.Dtos;

public sealed record BattingLeaderDto(
    int Rank, string PlayerId, string PlayerName, short? Year,
    string? TeamId, string? TeamName, bool IsHallOfFamer,
    int Games, int AtBats, int Runs, int Hits, int Doubles, int Triples,
    int HomeRuns, int Rbi, int StolenBases, int Walks,
    double BattingAverage, double Obp, double Slg, double Ops);

public sealed record PitchingLeaderDto(
    int Rank, string PlayerId, string PlayerName, short? Year,
    string? TeamId, string? TeamName, bool IsHallOfFamer,
    int Games, int GamesStarted, int Wins, int Losses, int Saves,
    int CompleteGames, int Shutouts, double InningsPitched,
    int Hits, int EarnedRuns, int HomeRuns, int Walks, int Strikeouts,
    double Era, double Whip);
