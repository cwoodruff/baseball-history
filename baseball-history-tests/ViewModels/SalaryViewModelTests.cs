using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class SalaryViewModelTests
{
    [Fact]
    public void FormattedTeamPayroll_WithValue_FormatsUsd()
    {
        var model = new SalaryViewModel { TeamPayroll = 123456789 };

        Assert.Equal("$123,456,789", model.FormattedTeamPayroll);
    }

    [Fact]
    public void FormattedTeamPayroll_WithoutValue_ReturnsNull()
    {
        var model = new SalaryViewModel();

        Assert.Null(model.FormattedTeamPayroll);
    }

    [Fact]
    public void SalaryEntry_FormatsSalaryAsUsd()
    {
        var entry = new SalaryEntry { Salary = 27500000 };

        Assert.Equal("$27,500,000", entry.FormattedSalary);
    }
}
