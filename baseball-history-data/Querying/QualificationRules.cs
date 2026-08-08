namespace BaseballHistory.Data.Querying;

public static class QualificationRules
{
    public const decimal BattingPlateAppearancesPerGame = 3.1m;
    public const decimal PitchingOutsPerGame = 3.0m;

    public static decimal CalculatePlateAppearances(int ab, int bb, int? hbp, int? sh, int? sf)
    {
        return ab + bb + (hbp ?? 0) + (sh ?? 0) + (sf ?? 0);
    }

    public static decimal CalculateSeasonBattingThreshold(int teamGames)
    {
        return BattingPlateAppearancesPerGame * teamGames;
    }

    public static decimal CalculateSeasonPitchingThreshold(int teamGames)
    {
        return PitchingOutsPerGame * teamGames;
    }
}
