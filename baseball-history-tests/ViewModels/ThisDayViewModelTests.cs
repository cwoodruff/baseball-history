using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class ThisDayViewModelTests
{
    private static readonly DateOnly Today = new(2026, 9, 6);

    [Fact]
    public void ResolveDay_ParsesValidOverride()
    {
        Assert.Equal((2, 6), ThisDayViewModel.ResolveDay("02-06", Today));
        Assert.Equal((12, 31), ThisDayViewModel.ResolveDay("12-31", Today));
        Assert.Equal((2, 29), ThisDayViewModel.ResolveDay("02-29", Today));
    }

    [Fact]
    public void ResolveDay_FallsBackOnInvalidInput()
    {
        Assert.Equal((9, 6), ThisDayViewModel.ResolveDay(null, Today));
        Assert.Equal((9, 6), ThisDayViewModel.ResolveDay("", Today));
        Assert.Equal((9, 6), ThisDayViewModel.ResolveDay("banana", Today));
        Assert.Equal((9, 6), ThisDayViewModel.ResolveDay("13-01", Today));
        Assert.Equal((9, 6), ThisDayViewModel.ResolveDay("02-30", Today));
        Assert.Equal((9, 6), ThisDayViewModel.ResolveDay("04-31", Today));
        Assert.Equal((9, 6), ThisDayViewModel.ResolveDay("2-6-1895", Today));
    }

    [Fact]
    public void DateLabel_FormatsMonthName()
    {
        var vm = new ThisDayViewModel { Month = 2, Day = 6 };
        Assert.Equal("February 6", vm.DateLabel);
    }

    [Fact]
    public void HasContent_FalseWhenAllEmpty()
    {
        Assert.False(new ThisDayViewModel().HasContent);
        Assert.True(new ThisDayViewModel
        {
            Birthdays = { new ThisDayEntry { PlayerId = "x", FullName = "X" } }
        }.HasContent);
    }
}
