using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class PaginationModelTests
{
    [Fact]
    public void GetPageUrl_ReturnsCorrectUrl()
    {
        var model = new PaginationModel
        {
            BaseUrl = "/Players",
            QueryParams = new Dictionary<string, string> { { "letter", "A" } }
        };

        var url = model.GetPageUrl(2);
        Assert.Contains("page=2", url);
        Assert.Contains("letter=A", url);
        Assert.StartsWith("/Players?", url);
    }

    [Fact]
    public void GetPageUrl_EscapesSpecialCharacters()
    {
        var model = new PaginationModel
        {
            BaseUrl = "/HallOfFame",
            QueryParams = new Dictionary<string, string> { { "category", "Pioneer/Executive" } }
        };

        var url = model.GetPageUrl(1);
        Assert.Contains("Pioneer%2FExecutive", url);
    }

    [Fact]
    public void GetPageUrl_WithNoQueryParams()
    {
        var model = new PaginationModel { BaseUrl = "/Players" };
        var url = model.GetPageUrl(3);
        Assert.Equal("/Players?page=3", url);
    }

    [Fact]
    public void Create_SetsCorrectProperties()
    {
        var model = PaginationModel.Create(2, 150, 50, "/Players");

        Assert.Equal(2, model.CurrentPage);
        Assert.Equal(150, model.TotalItems);
        Assert.Equal(50, model.PageSize);
        Assert.Equal(3, model.TotalPages);
        Assert.Equal("/Players", model.BaseUrl);
    }

    [Fact]
    public void Create_WithQueryParams()
    {
        var queryParams = new Dictionary<string, string> { { "letter", "B" } };
        var model = PaginationModel.Create(1, 100, 25, "/Players", queryParams);

        Assert.Contains("letter", model.QueryParams.Keys);
        Assert.Equal("B", model.QueryParams["letter"]);
    }

    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        var model1 = PaginationModel.Create(1, 100, 25, "/test");
        Assert.Equal(4, model1.TotalPages);

        var model2 = PaginationModel.Create(1, 101, 25, "/test");
        Assert.Equal(5, model2.TotalPages);

        var model3 = PaginationModel.Create(1, 99, 25, "/test");
        Assert.Equal(4, model3.TotalPages);
    }

    [Fact]
    public void GetVisiblePages_ShowsAllPagesWhenFew()
    {
        var model = new PaginationModel
        {
            CurrentPage = 1,
            TotalPages = 5,
            VisiblePageCount = 5
        };

        var pages = model.GetVisiblePages().ToList();
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, pages);
    }

    [Fact]
    public void GetVisiblePages_AtStart_ShowsEllipsisAtEnd()
    {
        var model = new PaginationModel
        {
            CurrentPage = 1,
            TotalPages = 20,
            VisiblePageCount = 5
        };

        var pages = model.GetVisiblePages().ToList();
        Assert.Equal(1, pages.First());
        Assert.Equal(20, pages.Last());
        Assert.Contains(-1, pages); // Ellipsis marker
    }

    [Fact]
    public void GetVisiblePages_AtEnd_ShowsEllipsisAtStart()
    {
        var model = new PaginationModel
        {
            CurrentPage = 20,
            TotalPages = 20,
            VisiblePageCount = 5
        };

        var pages = model.GetVisiblePages().ToList();
        Assert.Equal(1, pages.First());
        Assert.Equal(20, pages.Last());
        Assert.Contains(-1, pages); // Ellipsis marker
    }

    [Fact]
    public void GetVisiblePages_InMiddle_ShowsEllipsisOnBothSides()
    {
        var model = new PaginationModel
        {
            CurrentPage = 10,
            TotalPages = 20,
            VisiblePageCount = 5
        };

        var pages = model.GetVisiblePages().ToList();
        Assert.Equal(1, pages.First());
        Assert.Equal(20, pages.Last());

        // Should have ellipsis after 1 and before 20
        var ellipsisCount = pages.Count(p => p == -1);
        Assert.Equal(2, ellipsisCount);
    }

    [Fact]
    public void GetVisiblePages_IncludesCurrentPage()
    {
        var model = new PaginationModel
        {
            CurrentPage = 7,
            TotalPages = 15,
            VisiblePageCount = 5
        };

        var pages = model.GetVisiblePages().ToList();
        Assert.Contains(7, pages);
    }

    [Fact]
    public void GetVisiblePages_AlwaysIncludesFirstAndLastPage()
    {
        for (int currentPage = 1; currentPage <= 20; currentPage++)
        {
            var model = new PaginationModel
            {
                CurrentPage = currentPage,
                TotalPages = 20,
                VisiblePageCount = 5
            };

            var pages = model.GetVisiblePages().ToList();
            Assert.Contains(1, pages);
            Assert.Contains(20, pages);
        }
    }

    [Fact]
    public void GetVisiblePages_WithSinglePage()
    {
        var model = new PaginationModel
        {
            CurrentPage = 1,
            TotalPages = 1,
            VisiblePageCount = 5
        };

        var pages = model.GetVisiblePages().ToList();
        Assert.Single(pages);
        Assert.Equal(1, pages[0]);
    }

    [Fact]
    public void GetVisiblePages_WithTwoPages()
    {
        var model = new PaginationModel
        {
            CurrentPage = 1,
            TotalPages = 2,
            VisiblePageCount = 5
        };

        var pages = model.GetVisiblePages().ToList();
        Assert.Equal(new[] { 1, 2 }, pages);
    }
}