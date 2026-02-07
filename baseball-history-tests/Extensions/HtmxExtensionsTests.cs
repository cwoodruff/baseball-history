using baseball_history_web.Extensions;
using Microsoft.AspNetCore.Http;

namespace baseball_history_tests.Extensions;

public class HtmxExtensionsTests
{
    [Fact]
    public void IsHtmxRequest_WithHeader_ReturnsTrue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Request"] = "true";

        Assert.True(context.Request.IsHtmxRequest());
    }

    [Fact]
    public void IsHtmxRequest_WithoutHeader_ReturnsFalse()
    {
        var context = new DefaultHttpContext();

        Assert.False(context.Request.IsHtmxRequest());
    }

    [Fact]
    public void IsHtmxBoosted_WithHeader_ReturnsTrue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Boosted"] = "true";

        Assert.True(context.Request.IsHtmxBoosted());
    }

    [Fact]
    public void IsHtmxBoosted_WithoutHeader_ReturnsFalse()
    {
        var context = new DefaultHttpContext();

        Assert.False(context.Request.IsHtmxBoosted());
    }

    [Fact]
    public void IsHtmxNonBoostedRequest_WithOnlyHxRequest_ReturnsTrue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Request"] = "true";

        Assert.True(context.Request.IsHtmxNonBoostedRequest());
    }

    [Fact]
    public void IsHtmxNonBoostedRequest_WithBothHeaders_ReturnsFalse()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Request"] = "true";
        context.Request.Headers["HX-Boosted"] = "true";

        Assert.False(context.Request.IsHtmxNonBoostedRequest());
    }

    [Fact]
    public void IsHtmxNonBoostedRequest_WithNoHeaders_ReturnsFalse()
    {
        var context = new DefaultHttpContext();

        Assert.False(context.Request.IsHtmxNonBoostedRequest());
    }

    [Fact]
    public void GetHtmxTarget_WithHeader_ReturnsValue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Target"] = "#modal-container";

        Assert.Equal("#modal-container", context.Request.GetHtmxTarget());
    }

    [Fact]
    public void GetHtmxTarget_WithoutHeader_ReturnsNull()
    {
        var context = new DefaultHttpContext();

        Assert.Null(context.Request.GetHtmxTarget());
    }

    [Fact]
    public void GetHtmxTrigger_WithHeader_ReturnsValue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Trigger"] = "btn-submit";

        Assert.Equal("btn-submit", context.Request.GetHtmxTrigger());
    }

    [Fact]
    public void GetHtmxTrigger_WithoutHeader_ReturnsNull()
    {
        var context = new DefaultHttpContext();

        Assert.Null(context.Request.GetHtmxTrigger());
    }

    [Fact]
    public void GetHtmxTriggerName_WithHeader_ReturnsValue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Trigger-Name"] = "search";

        Assert.Equal("search", context.Request.GetHtmxTriggerName());
    }

    [Fact]
    public void GetHtmxCurrentUrl_WithHeader_ReturnsValue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Current-URL"] = "/Players?letter=A";

        Assert.Equal("/Players?letter=A", context.Request.GetHtmxCurrentUrl());
    }

    [Fact]
    public void GetHtmxPrompt_WithHeader_ReturnsValue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["HX-Prompt"] = "user input";

        Assert.Equal("user input", context.Request.GetHtmxPrompt());
    }

    [Fact]
    public void HtmxRedirect_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxRedirect("/new-page");

        Assert.Equal("/new-page", context.Response.Headers["HX-Redirect"]);
    }

    [Fact]
    public void HtmxRefresh_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxRefresh();

        Assert.Equal("true", context.Response.Headers["HX-Refresh"]);
    }

    [Fact]
    public void HtmxPushUrl_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxPushUrl("/Players?page=2");

        Assert.Equal("/Players?page=2", context.Response.Headers["HX-Push-Url"]);
    }

    [Fact]
    public void HtmxReplaceUrl_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxReplaceUrl("/new-url");

        Assert.Equal("/new-url", context.Response.Headers["HX-Replace-Url"]);
    }

    [Fact]
    public void HtmxReswap_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxReswap("outerHTML");

        Assert.Equal("outerHTML", context.Response.Headers["HX-Reswap"]);
    }

    [Fact]
    public void HtmxRetarget_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxRetarget("#content");

        Assert.Equal("#content", context.Response.Headers["HX-Retarget"]);
    }

    [Fact]
    public void HtmxTrigger_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxTrigger("dataLoaded");

        Assert.Equal("dataLoaded", context.Response.Headers["HX-Trigger"]);
    }

    [Fact]
    public void HtmxTriggerAfterSwap_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxTriggerAfterSwap("swapComplete");

        Assert.Equal("swapComplete", context.Response.Headers["HX-Trigger-After-Swap"]);
    }

    [Fact]
    public void HtmxTriggerAfterSettle_SetsHeader()
    {
        var context = new DefaultHttpContext();
        context.Response.HtmxTriggerAfterSettle("settleComplete");

        Assert.Equal("settleComplete", context.Response.Headers["HX-Trigger-After-Settle"]);
    }
}
