using BaseballHistory.Data.Querying;

namespace baseball_history_tests.Data;

public class QualificationRulesTests
{
    [Fact]
    public void CalculatePlateAppearances_WithAllInputs_PlusNullOptionals_UsesZeroForMissingValues()
    {
        var plateAppearances = QualificationRules.CalculatePlateAppearances(
            ab: 500,
            bb: 80,
            hbp: null,
            sh: 5,
            sf: null);

        Assert.Equal(585m, plateAppearances);
    }

    [Fact]
    public void CalculateSeasonBattingThreshold_UsesStandardPlateAppearancesPerGameRate()
    {
        var threshold = QualificationRules.CalculateSeasonBattingThreshold(teamGames: 162);

        Assert.Equal(502.2m, threshold);
    }

    [Fact]
    public void CalculateSeasonPitchingThreshold_UsesThreeOutsPerGame()
    {
        var threshold = QualificationRules.CalculateSeasonPitchingThreshold(teamGames: 162);

        Assert.Equal(486m, threshold);
    }
}
