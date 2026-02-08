using baseball_history_web.Models;
using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class PlayerDetailViewModelTests
{
    [Fact]
    public void FromPeople_SetsBasicInfo()
    {
        var person = new People
        {
            PlayerId = "ruthba01",
            NameFirst = "Babe",
            NameLast = "Ruth",
            NameGiven = "George Herman Ruth"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Equal("ruthba01", vm.PlayerId);
        Assert.Equal("Babe", vm.FirstName);
        Assert.Equal("Ruth", vm.LastName);
        Assert.Equal("Babe Ruth", vm.FullName);
        Assert.Equal("George Herman Ruth", vm.GivenName);
    }

    [Fact]
    public void FromPeople_SetsPhysicalAttributes()
    {
        var person = new People
        {
            PlayerId = "test",
            Height = "74",
            Weight = "215",
            Bats = "L",
            Throws = "L"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Equal("74", vm.Height);
        Assert.Equal("215", vm.Weight);
        Assert.Equal("L", vm.Bats);
        Assert.Equal("L", vm.Throws);
    }

    [Fact]
    public void FromPeople_FormatsDebutDate()
    {
        var person = new People
        {
            PlayerId = "test",
            Debut = new DateOnly(1914, 7, 11)
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.NotNull(vm.Debut);
        Assert.Contains("1914", vm.Debut);
    }

    [Fact]
    public void FromPeople_FormatsFinalGameDate()
    {
        var person = new People
        {
            PlayerId = "test",
            FinalGame = new DateOnly(1935, 5, 30)
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.NotNull(vm.FinalGame);
        Assert.Contains("1935", vm.FinalGame);
    }

    [Fact]
    public void FromPeople_CalculatesCareerYears()
    {
        var person = new People
        {
            PlayerId = "test",
            Debut = new DateOnly(1914, 7, 11),
            FinalGame = new DateOnly(1935, 5, 30)
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Equal(22, vm.CareerYears);
    }

    [Fact]
    public void FromPeople_WithMissingDates_NoCareerYears()
    {
        var person = new People
        {
            PlayerId = "test",
            Debut = new DateOnly(1914, 7, 11),
            FinalGame = null
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Null(vm.CareerYears);
    }

    [Fact]
    public void FromPeople_FormatsBirthDate_WithFullDate()
    {
        var person = new People
        {
            PlayerId = "test",
            BirthYear = "1895",
            BirthMonth = "2",
            BirthDay = "6"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.NotNull(vm.BirthDate);
        Assert.Contains("1895", vm.BirthDate);
    }

    [Fact]
    public void FromPeople_FormatsBirthDate_WithOnlyYear()
    {
        var person = new People
        {
            PlayerId = "test",
            BirthYear = "1895",
            BirthMonth = null,
            BirthDay = null
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Equal("1895", vm.BirthDate);
    }

    [Fact]
    public void FromPeople_BuildsBirthPlace()
    {
        var person = new People
        {
            PlayerId = "test",
            BirthCity = "Baltimore",
            BirthState = "MD",
            BirthCountry = "USA"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Contains("Baltimore", vm.BirthPlace!);
        Assert.Contains("MD", vm.BirthPlace!);
        Assert.Contains("USA", vm.BirthPlace!);
    }

    [Fact]
    public void FromPeople_WithPartialBirthPlace()
    {
        var person = new People
        {
            PlayerId = "test",
            BirthCity = "Tokyo",
            BirthCountry = "Japan"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Contains("Tokyo", vm.BirthPlace!);
        Assert.Contains("Japan", vm.BirthPlace!);
    }

    [Fact]
    public void FromPeople_WithNoBirthPlace()
    {
        var person = new People
        {
            PlayerId = "test"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Null(vm.BirthPlace);
    }

    [Fact]
    public void FromPeople_FormatsDeathDate_WithFullDate()
    {
        var person = new People
        {
            PlayerId = "test",
            DeathYear = "1948",
            DeathMonth = "8",
            DeathDay = "16"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.NotNull(vm.DeathDate);
        Assert.Contains("1948", vm.DeathDate);
    }

    [Fact]
    public void FromPeople_FormatsDeathDate_WithOnlyYear()
    {
        var person = new People
        {
            PlayerId = "test",
            DeathYear = "1948"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Equal("1948", vm.DeathDate);
    }

    [Fact]
    public void FromPeople_WithNullNameFirst_HandlesTrim()
    {
        var person = new People
        {
            PlayerId = "test",
            NameFirst = null,
            NameLast = "Smith"
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Equal("Smith", vm.FullName);
    }

    [Fact]
    public void FromPeople_HandlesInvalidBirthDate()
    {
        var person = new People
        {
            PlayerId = "test",
            BirthYear = "1900",
            BirthMonth = "2",
            BirthDay = "30" // Invalid date
        };

        var vm = PlayerDetailViewModel.FromPeople(person);

        Assert.Equal("1900", vm.BirthDate);
    }

    [Fact]
    public void DefaultLists_AreInitialized()
    {
        var vm = new PlayerDetailViewModel();

        Assert.NotNull(vm.BattingSeasons);
        Assert.Empty(vm.BattingSeasons);
        Assert.NotNull(vm.PitchingSeasons);
        Assert.Empty(vm.PitchingSeasons);
        Assert.NotNull(vm.Awards);
        Assert.Empty(vm.Awards);
        Assert.NotNull(vm.AllStarAppearances);
        Assert.Empty(vm.AllStarAppearances);
        Assert.NotNull(vm.Teams);
        Assert.Empty(vm.Teams);
    }
}