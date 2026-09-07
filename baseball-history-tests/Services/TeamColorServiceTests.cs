using baseball_history_web.Services;

namespace baseball_history_tests.Services;

public class TeamColorServiceTests
{
    private readonly TeamColorService _service = new();

    [Theory]
    [InlineData("NYA")]
    [InlineData("nya")]
    [InlineData("NYY")]
    public void GetTeamColors_ForKnownAlias_ReturnsExpectedTeamIdentity(string teamId)
    {
        var colors = _service.GetTeamColors(teamId);

        Assert.Equal("New York Yankees", colors.TeamName);
        Assert.Equal("#003087", colors.Primary);
        Assert.Equal("#E4002B", colors.Secondary);
        Assert.Equal("#FFFFFF", colors.Accent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("XYZ")]
    public void GetTeamColors_ForUnknownTeam_ReturnsDefaultColors(string? teamId)
    {
        var colors = _service.GetTeamColors(teamId);

        Assert.Equal("Unknown Team", colors.TeamName);
        Assert.Equal("#002D72", colors.Primary);
        Assert.Equal("#D50032", colors.Secondary);
        Assert.Equal("#FFFFFF", colors.Accent);
    }

    [Fact]
    public void DataAttributeHelpers_RenderExpectedMarkup()
    {
        Assert.Equal("data-team=\"SEA\"", _service.GetDataTeamAttribute("SEA"));
        Assert.Equal("", _service.GetDataTeamAttribute(null));
        Assert.Equal("data-franchise=\"NYY\"", _service.GetDataFranchiseAttribute("NYY"));
        Assert.Equal("", _service.GetDataFranchiseAttribute(string.Empty));
    }

    [Fact]
    public void TeamColors_ComputedStylesUseConfiguredColors()
    {
        var colors = _service.GetTeamColors("BOS");

        Assert.Equal("background-color: #BD3039; color: #FFFFFF;", colors.PrimaryBackgroundStyle);
        Assert.Equal("background-color: #0C2340; color: #FFFFFF;", colors.SecondaryBackgroundStyle);
        Assert.Equal("background: linear-gradient(135deg, #BD3039 0%, #0C2340 100%); color: #FFFFFF;", colors.GradientStyle);
        Assert.True(_service.HasTeamColors("bos"));
        Assert.False(_service.HasTeamColors("XYZ"));
        Assert.Contains("BOS", _service.GetAllTeamIds());
    }
}
