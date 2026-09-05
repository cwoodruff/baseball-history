using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using BaseballHistory.Data.Models;

namespace baseball_history_tests.ViewModels;

public class PlayerRecordFactsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("W.")]
    [InlineData("W")]
    [InlineData("w.")]
    public void IsPartialName_SurnameOnlyOrInitial_IsPartial(string? nameFirst)
    {
        Assert.True(PlayerRecordFacts.IsPartialName(nameFirst));
    }

    [Theory]
    [InlineData("Walter")]
    [InlineData("Wm")]
    [InlineData("J.R.")]
    [InlineData(" Babe ")]
    public void IsPartialName_DocumentedFirstName_IsNotPartial(string nameFirst)
    {
        Assert.False(PlayerRecordFacts.IsPartialName(nameFirst));
    }

    [Fact]
    public void PlayerDetailViewModel_SurnameOnlyPlayer_SetsIsPartialRecord()
    {
        var person = new People { PlayerId = "smith01", NameFirst = "", NameLast = "Smith" };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.True(vm.IsPartialRecord);
        Assert.Equal("Smith", vm.FullName);
    }

    [Fact]
    public void PlayerDetailViewModel_DocumentedPlayer_DoesNotSetIsPartialRecord()
    {
        var person = new People { PlayerId = "ruthba01", NameFirst = "Babe", NameLast = "Ruth" };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.False(vm.IsPartialRecord);
    }

    [Fact]
    public void PlayerSummary_InitialOnlyPlayer_SetsIsPartialRecord()
    {
        var person = new People { PlayerId = "cobbw01", NameFirst = "W.", NameLast = "Cobb" };

        var summary = PlayerSummary.FromPeople(person);

        Assert.True(summary.IsPartialRecord);
        Assert.Equal("W. Cobb", summary.FullName);
    }
}
