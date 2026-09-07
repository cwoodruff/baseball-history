using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class BreadcrumbModelTests
{
    [Fact]
    public void Build_PreservesSuppliedTrailOrder()
    {
        var model = BreadcrumbModel.Build(
            new BreadcrumbItem("Players", "/Players"),
            new BreadcrumbItem("Babe Ruth", "/Players/Details/ruthba01"),
            new BreadcrumbItem("Postseason"));

        Assert.Collection(model.Items,
            item =>
            {
                Assert.Equal("Players", item.Label);
                Assert.Equal("/Players", item.Url);
            },
            item =>
            {
                Assert.Equal("Babe Ruth", item.Label);
                Assert.Equal("/Players/Details/ruthba01", item.Url);
            },
            item =>
            {
                Assert.Equal("Postseason", item.Label);
                Assert.Null(item.Url);
            });
    }
}
