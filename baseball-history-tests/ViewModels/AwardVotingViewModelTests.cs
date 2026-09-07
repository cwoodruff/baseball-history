using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class AwardVotingViewModelTests
{
    [Fact]
    public void IsManagersScope_WhenScopeIsManagers_ReturnsTrue()
    {
        var model = new AwardVotingViewModel { Scope = "managers" };

        Assert.True(model.IsManagersScope);
    }

    [Fact]
    public void IsManagersScope_WhenScopeIsPlayers_ReturnsFalse()
    {
        var model = new AwardVotingViewModel { Scope = "players" };

        Assert.False(model.IsManagersScope);
    }

    [Fact]
    public void AwardVoteEntry_FormatsVoteShareAsPercent()
    {
        var vote = new AwardVoteEntry { VoteShare = 62.34 };

        Assert.Equal("62.3%", vote.FormattedVoteShare);
    }
}
