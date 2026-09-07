using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class CompareViewModelTests
{
    [Fact]
    public void SinglePlayerSelection_KeepsComparisonDisabledUntilSecondPlayerIsChosen()
    {
        var viewModel = new CompareViewModel
        {
            Player1 = new ComparePlayer
            {
                PlayerId = "ruthba01",
                FullName = "Babe Ruth",
                BattingStats = new CompareCareerBattingStats()
            }
        };

        Assert.Single(viewModel.SelectedPlayers);
        Assert.Equal(1, viewModel.SelectedCount);
        Assert.False(viewModel.BothSelected);
        Assert.True(viewModel.HasBatters);
        Assert.False(viewModel.HasPitchers);
        Assert.False(viewModel.HasPostseasonBatting);
        Assert.False(viewModel.HasPostseasonPitching);
        Assert.False(viewModel.HasFielding);
    }

    [Fact]
    public void ThreeAndFourPlayerSelections_AreIncludedInOrder()
    {
        var viewModel = new CompareViewModel
        {
            Player1 = new ComparePlayer { PlayerId = "one", FullName = "Player One" },
            Player2 = new ComparePlayer { PlayerId = "two", FullName = "Player Two" },
            Player3 = new ComparePlayer { PlayerId = "three", FullName = "Player Three" },
            Player4 = new ComparePlayer { PlayerId = "four", FullName = "Player Four" }
        };

        Assert.Equal(4, viewModel.SelectedCount);
        Assert.True(viewModel.BothSelected);
        Assert.Equal(["one", "two", "three", "four"], viewModel.SelectedPlayers.Select(player => player.PlayerId));
    }

    [Fact]
    public void VisibilityFlags_AggregateAcrossMixedPlayerStatProfiles()
    {
        var viewModel = new CompareViewModel
        {
            Player1 = new ComparePlayer
            {
                PlayerId = "batter",
                FullName = "Pure Batter",
                BattingStats = new CompareCareerBattingStats()
            },
            Player2 = new ComparePlayer
            {
                PlayerId = "pitcher",
                FullName = "Pure Pitcher",
                PitchingStats = new CompareCareerPitchingStats()
            },
            Player3 = new ComparePlayer
            {
                PlayerId = "october-bat",
                FullName = "October Batter",
                PostseasonBattingStats = new ComparePostseasonBattingStats()
            },
            Player4 = new ComparePlayer
            {
                PlayerId = "october-arm",
                FullName = "October Arm",
                PostseasonPitchingStats = new ComparePostseasonPitchingStats(),
                FieldingStats = new CompareFieldingStats()
            }
        };

        Assert.True(viewModel.HasBatters);
        Assert.True(viewModel.HasPitchers);
        Assert.True(viewModel.HasPostseasonBatting);
        Assert.True(viewModel.HasPostseasonPitching);
        Assert.True(viewModel.HasFielding);
    }

    [Theory]
    [InlineData("Babe Ruth", "BR")]
    [InlineData("Ichiro", "IC")]
    [InlineData("Q", "Q")]
    public void Initials_FormatFromAvailableNameParts(string fullName, string expectedInitials)
    {
        var player = new ComparePlayer { PlayerId = "player", FullName = fullName };

        Assert.Equal(expectedInitials, player.Initials);
    }

    [Fact]
    public void PartialRecordFlag_RemainsAvailableForDisplaySemantics()
    {
        var player = new ComparePlayer
        {
            PlayerId = "partial01",
            FullName = "Smith",
            IsPartialRecord = true
        };

        Assert.True(player.IsPartialRecord);
        Assert.Equal("SM", player.Initials);
    }
}
