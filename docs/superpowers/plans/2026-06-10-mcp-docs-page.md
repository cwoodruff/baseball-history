# MCP Server Documentation Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a static `/McpDocs` Razor Page to `baseball-history-web` that documents the `baseball-history-mcp` server (tools, resources, connection config, example usage), mirroring the existing `/ApiDocs` page.

**Architecture:** A static Razor Page (`McpDocs.cshtml` + empty-`OnGet` code-behind) styled identically to `ApiDocs.cshtml`, plus a nav link in `_ShellHeader.cshtml`. No runtime introspection of the MCP assembly. Verified by an HTTP rendering smoke test using the existing `WebApplicationFactory<Program>` pattern.

**Tech Stack:** ASP.NET Core Razor Pages (.NET 10), Bootstrap, xUnit + `Microsoft.AspNetCore.Mvc.Testing`.

**Source of truth for content (do not paraphrase values):**
- Tools: `baseball-history-mcp/Tools/BaseballReferenceTools.cs`, `BaseballServerDiagnosticsTools.cs`
- Resources: `ResourceLinks` in `baseball-history-mcp/Metadata/BaseballMcpMetadataService.cs`
- Limits: `BaseballMcpLimitOptions` in `baseball-history-mcp/Configuration/BaseballMcpOptions.cs`
- Stat keys: `baseball-history-mcp/Querying/LeaderboardStatCatalog.cs`

---

## File Structure

- **Create** `baseball-history-web/Pages/McpDocs.cshtml.cs` — empty `OnGet` PageModel (mirrors `ApiDocsModel`).
- **Create** `baseball-history-web/Pages/McpDocs.cshtml` — the documentation page.
- **Modify** `baseball-history-web/Pages/Shared/_ShellHeader.cshtml` — add "MCP" nav link after the "API" link.
- **Create** `baseball-history-tests/Pages/McpDocsPageTests.cs` — rendering + nav smoke tests.

---

## Task 1: Rendering smoke test (red)

**Files:**
- Test: `baseball-history-tests/Pages/McpDocsPageTests.cs`

- [ ] **Step 1: Write the failing test**

Create `baseball-history-tests/Pages/McpDocsPageTests.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class McpDocsPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task McpDocs_FullPage_RendersTitleToolsAndResources()
    {
        var html = await GetStringAsync("/McpDocs");

        Assert.Contains("MCP Server", html);
        Assert.Contains("search_players", html);
        Assert.Contains("get_batting_leaders", html);
        Assert.Contains("get_server_diagnostics", html);
        Assert.Contains("baseball-history://server/info", html);
        Assert.Contains("ConnectionStrings__Lahman", html);
    }

    [Fact]
    public async Task ShellHeader_IncludesMcpNavLink()
    {
        var html = await GetStringAsync("/ApiDocs");

        Assert.Contains("href=\"/McpDocs\"", html);
        Assert.Contains(">MCP</a>", html);
    }
}
```

Note: Razor renders the `asp-page="/McpDocs"` tag helper to `href="/McpDocs"`, so the nav test asserts on the rendered `href`. (See Task 3 for the matching markup; this test stays red until then.)

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test baseball-history-tests --filter McpDocsPageTests`
Expected: FAIL — `/McpDocs` returns 404, so `GetStringAsync` (which calls `EnsureSuccessStatusCode`) throws and the assertions never pass.

---

## Task 2: Create the McpDocs page (green for rendering test)

**Files:**
- Create: `baseball-history-web/Pages/McpDocs.cshtml.cs`
- Create: `baseball-history-web/Pages/McpDocs.cshtml`

- [ ] **Step 1: Create the code-behind**

Create `baseball-history-web/Pages/McpDocs.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace baseball_history_web.Pages;

public class McpDocsModel : PageModel
{
    public void OnGet()
    {
    }
}
```

- [ ] **Step 2: Create the page markup**

Create `baseball-history-web/Pages/McpDocs.cshtml`. Note: angle brackets and ampersands inside `<code>` blocks are HTML-encoded (`&lt;`, `&gt;`, `&amp;`) so Razor does not parse them as tags.

```cshtml
@page
@model McpDocsModel
@{
    ViewData["Title"] = "MCP Server Documentation";
}

<div class="row justify-content-center">
    <div class="col-lg-10">
        @await Html.PartialAsync("Components/_PageHeader", new baseball_history_web.ViewModels.PageHeaderModel
        {
            Title = "MCP Server",
            Subtitle = "Read-only access to over 150 years of Major League Baseball data through the Model Context Protocol, for use with AI assistants and agents.",
            Eyebrow = "Developer"
        })

        <div class="alert alert-info d-flex align-items-center mb-4">
            <span class="me-2" style="font-size: 1.2rem;">&#129302;</span>
            <div>
                <strong>What is MCP?</strong>
                The <a href="https://modelcontextprotocol.io" target="_blank" rel="noopener">Model Context Protocol</a>
                is an open standard that lets AI assistants call external tools. This server runs over
                <strong>stdio</strong>, is <strong>read-only</strong>, and has <strong>no HTTP transport and no authentication</strong>.
            </div>
        </div>

        <!-- How it works -->
        <section class="mb-5">
            <h2>How it works</h2>
            <p>
                The server exposes <strong>12 read-only tools</strong> and <strong>6 JSON resources</strong> over the
                stdio transport. The MCP client launches the server as a child process and communicates over its
                standard input and output &mdash; there is no network endpoint to call.
            </p>
            <ul>
                <li>Requires a <code>ConnectionStrings:Lahman</code> value at startup. Placeholder strings containing <code>&lt;</code> are rejected.</li>
                <li>Every tool is read-only; the server never mutates data.</li>
                <li>Start with the <code>get_server_diagnostics</code> tool or the <code>baseball-history://server/info</code> resource to discover the surface before calling domain tools.</li>
            </ul>
        </section>

        <!-- Connecting -->
        <section class="mb-5">
            <h2 class="border-bottom pb-2">Connecting</h2>
            <p>The client launches the server with <code>dotnet</code> and supplies the database connection string through an environment variable.</p>

            <h5 class="mt-4">Claude Desktop / Claude Code</h5>
            <p>Add the server to your MCP configuration:</p>
            <pre class="bg-dark text-light p-3 rounded"><code>{
  "mcpServers": {
    "baseball-history": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/baseball-history-mcp"],
      "env": {
        "ConnectionStrings__Lahman": "Host=your-host;Database=baseball-history;Username=user;Password=secret"
      }
    }
  }
}</code></pre>

            <h5 class="mt-4">VS Code</h5>
            <p>Add a <code>.vscode/mcp.json</code> file:</p>
            <pre class="bg-dark text-light p-3 rounded"><code>{
  "servers": {
    "baseball-history": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/baseball-history-mcp"],
      "env": {
        "ConnectionStrings__Lahman": "Host=your-host;Database=baseball-history;Username=user;Password=secret"
      }
    }
  }
}</code></pre>
            <p>
                The connection string is read from the <code>ConnectionStrings__Lahman</code> environment variable
                (the .NET double-underscore convention), or from user-secrets during local development.
            </p>
        </section>

        <!-- Example prompts -->
        <section class="mb-5">
            <h2 class="border-bottom pb-2">Example prompts</h2>
            <p>Once connected, ask your AI assistant questions like these. It will choose and call the right tool:</p>
            <table class="table table-sm">
                <thead><tr><th>Ask your AI assistant</th><th>Tool(s) used</th></tr></thead>
                <tbody>
                    <tr><td>&ldquo;Find players whose last name starts with R&rdquo;</td><td><code>search_players</code></td></tr>
                    <tr><td>&ldquo;Show me Babe Ruth's career stats&rdquo;</td><td><code>get_player</code></td></tr>
                    <tr><td>&ldquo;Career home run leaders since 2000&rdquo;</td><td><code>get_batting_leaders</code></td></tr>
                    <tr><td>&ldquo;Lowest career ERA, minimum 1000 innings&rdquo;</td><td><code>get_pitching_leaders</code></td></tr>
                    <tr><td>&ldquo;Who was inducted into the Hall of Fame in 1999?&rdquo;</td><td><code>list_hall_of_fame_inductees</code></td></tr>
                    <tr><td>&ldquo;Mike Trout's salary history&rdquo;</td><td><code>get_player_salary_history</code></td></tr>
                </tbody>
            </table>
        </section>

        <!-- Tools -->
        <section class="mb-5">
            <h2 class="border-bottom pb-2">Tools</h2>

            <h5 class="mt-3">Players</h5>
            <ul class="list-unstyled">
                <li class="mb-2"><span class="badge bg-secondary me-2">search_players</span> Search players by free-text query or last-name prefix with paging.</li>
                <li class="mb-2"><span class="badge bg-secondary me-2">get_player</span> Get read-only detail for one player, including career batting, career pitching, and team tenures.</li>
            </ul>

            <h5 class="mt-3">Franchises &amp; Teams</h5>
            <ul class="list-unstyled">
                <li class="mb-2"><span class="badge bg-secondary me-2">list_franchises</span> List franchise summaries with optional filters and bounded paging.</li>
                <li class="mb-2"><span class="badge bg-secondary me-2">get_franchise</span> Get one franchise with season-by-season history.</li>
                <li class="mb-2"><span class="badge bg-secondary me-2">get_team_season</span> Get one exact team-season by team id, league, and year so franchise-era lookups stay deterministic.</li>
            </ul>

            <h5 class="mt-3">Leaderboards</h5>

            <div class="mb-4">
                <h6><span class="badge bg-secondary me-2">get_batting_leaders</span></h6>
                <p>Read batting leaderboards in career or single-season form.</p>
                <table class="table table-sm">
                    <thead><tr><th>Parameter</th><th>Type</th><th>Default</th><th>Description</th></tr></thead>
                    <tbody>
                        <tr><td><code>stat</code></td><td>string</td><td>hr</td><td>Stat to rank by: <code>hr</code>, <code>h</code>, <code>r</code>, <code>rbi</code>, <code>sb</code>, <code>2b</code>, <code>3b</code>, <code>bb</code>, <code>g</code>, <code>ab</code>, <code>avg</code>, <code>obp</code>, <code>slg</code>, <code>ops</code></td></tr>
                        <tr><td><code>fromYear</code></td><td>int?</td><td>&mdash;</td><td>Lower year bound</td></tr>
                        <tr><td><code>toYear</code></td><td>int?</td><td>&mdash;</td><td>Upper year bound</td></tr>
                        <tr><td><code>league</code></td><td>string</td><td>&mdash;</td><td>League filter (AL, NL)</td></tr>
                        <tr><td><code>minAtBats</code></td><td>int</td><td>0</td><td>Minimum at-bats threshold</td></tr>
                        <tr><td><code>singleSeason</code></td><td>bool</td><td>false</td><td>Single-season vs. career totals</td></tr>
                        <tr><td><code>page</code></td><td>int</td><td>1</td><td>1-based page</td></tr>
                        <tr><td><code>pageSize</code></td><td>int</td><td>25</td><td>Page size (max 100)</td></tr>
                    </tbody>
                </table>
            </div>

            <div class="mb-4">
                <h6><span class="badge bg-secondary me-2">get_pitching_leaders</span></h6>
                <p>Read pitching leaderboards in career or single-season form. ERA, WHIP, and BB9 sort ascending (lower is better); all others descending.</p>
                <table class="table table-sm">
                    <thead><tr><th>Parameter</th><th>Type</th><th>Default</th><th>Description</th></tr></thead>
                    <tbody>
                        <tr><td><code>stat</code></td><td>string</td><td>w</td><td>Stat to rank by: <code>w</code>, <code>l</code>, <code>so</code>, <code>sv</code>, <code>cg</code>, <code>sho</code>, <code>ip</code>, <code>g</code>, <code>gs</code>, <code>hr</code>, <code>k9</code>, <code>wpct</code>, <code>era</code>, <code>whip</code>, <code>bb9</code></td></tr>
                        <tr><td><code>fromYear</code></td><td>int?</td><td>&mdash;</td><td>Lower year bound</td></tr>
                        <tr><td><code>toYear</code></td><td>int?</td><td>&mdash;</td><td>Upper year bound</td></tr>
                        <tr><td><code>league</code></td><td>string</td><td>&mdash;</td><td>League filter (AL, NL)</td></tr>
                        <tr><td><code>minInningsPitched</code></td><td>int</td><td>0</td><td>Minimum innings-pitched threshold</td></tr>
                        <tr><td><code>singleSeason</code></td><td>bool</td><td>false</td><td>Single-season vs. career totals</td></tr>
                        <tr><td><code>page</code></td><td>int</td><td>1</td><td>1-based page</td></tr>
                        <tr><td><code>pageSize</code></td><td>int</td><td>25</td><td>Page size (max 100)</td></tr>
                    </tbody>
                </table>
            </div>

            <h5 class="mt-3">Hall of Fame</h5>
            <ul class="list-unstyled">
                <li class="mb-2"><span class="badge bg-secondary me-2">list_hall_of_fame_inductees</span> List inducted Hall of Fame rows with optional year/category filters and bounded paging.</li>
                <li class="mb-2"><span class="badge bg-secondary me-2">get_hall_of_fame_voting_history</span> Get bounded Hall of Fame voting history for one player. Rows are Lahman HallOfFame ballot rows, not a prose biography.</li>
            </ul>

            <h5 class="mt-3">Salaries</h5>
            <ul class="list-unstyled">
                <li class="mb-2"><span class="badge bg-secondary me-2">get_player_salary_history</span> Get bounded salary history for one player, ordered most recent to oldest.</li>
                <li class="mb-2"><span class="badge bg-secondary me-2">get_salary_leaders</span> List highest salary rows with optional year filter and bounded paging.</li>
            </ul>

            <h5 class="mt-3">Diagnostics</h5>
            <ul class="list-unstyled">
                <li class="mb-2"><span class="badge bg-secondary me-2">get_server_diagnostics</span> Inspect safe runtime posture, configured limits, and connectivity without exposing secrets.</li>
            </ul>
        </section>

        <!-- Resources -->
        <section class="mb-5">
            <h2 class="border-bottom pb-2">Resources</h2>
            <p>Read-only JSON documents the client can fetch to discover the server surface and usage guidance.</p>
            <table class="table table-sm">
                <thead><tr><th>URI</th><th>Purpose</th></tr></thead>
                <tbody>
                    <tr><td><code>baseball-history://server/info</code></td><td>Server identity, startup requirements, and configured limits.</td></tr>
                    <tr><td><code>baseball-history://server/workflow-guide</code></td><td>Routing for common question types to the right tool or guide.</td></tr>
                    <tr><td><code>baseball-history://server/stats-catalog</code></td><td>Supported batting and pitching stat categories plus the supported year span.</td></tr>
                    <tr><td><code>baseball-history://server/diagnostics</code></td><td>Safe runtime posture, configured limits, and connectivity.</td></tr>
                    <tr><td><code>baseball-history://hall-of-fame/guide</code></td><td>Hall of Fame tool limits, year coverage, and voting-history caveats.</td></tr>
                    <tr><td><code>baseball-history://salary/guide</code></td><td>Salary tool limits, year coverage, and how salary rows are shaped.</td></tr>
                </tbody>
            </table>
        </section>

        <!-- Programmatic SDK -->
        <section class="mb-5">
            <h2 class="border-bottom pb-2">Calling the server from code</h2>
            <p>
                Use the official MCP client SDK to launch the server over stdio and call tools directly.
                This C# example uses the <code>ModelContextProtocol.Client</code> package:
            </p>
            <pre class="bg-dark text-light p-3 rounded"><code>using ModelContextProtocol.Client;

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "baseball-history",
    Command = "dotnet",
    Arguments = ["run", "--project", "/path/to/baseball-history-mcp"],
    EnvironmentVariables = new Dictionary&lt;string, string?&gt;
    {
        ["ConnectionStrings__Lahman"] = "Host=your-host;Database=baseball-history;Username=user;Password=secret"
    }
});

await using var client = await McpClientFactory.CreateAsync(transport);

// Discover the available tools
foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"{tool.Name}: {tool.Description}");
}

// Call a tool
var result = await client.CallToolAsync(
    "get_batting_leaders",
    new Dictionary&lt;string, object?&gt;
    {
        ["stat"] = "hr",
        ["fromYear"] = 2000,
        ["singleSeason"] = true,
        ["pageSize"] = 5
    });

Console.WriteLine(result.Content[0].Text);</code></pre>
            <p>
                The same connect &rarr; list &rarr; call flow is available from the official
                <a href="https://modelcontextprotocol.io" target="_blank" rel="noopener">Python and TypeScript MCP SDKs</a>.
            </p>
        </section>

        <!-- Limits & data notes -->
        <section class="mb-5">
            <h2 class="border-bottom pb-2">Limits &amp; data notes</h2>
            <ul>
                <li>Page sizes are capped: player search 100, franchise list 50, Hall of Fame 50, batting/pitching leaderboards 100, salary leaderboard 50.</li>
                <li>History is bounded: Hall of Fame voting history returns up to 25 years; salary history up to 40 seasons.</li>
                <li>All data is from the <a href="https://sabr.org/lahman-database/" target="_blank" rel="noopener">Lahman Baseball Database</a>; the server is read-only.</li>
                <li>Player IDs follow the Lahman convention (e.g. <code>ruthba01</code>, <code>troutmi01</code>); franchise IDs match Lahman codes (e.g. <code>NYY</code>); team-season lookups use team id, league, and year (e.g. <code>NYA</code>, <code>AL</code>, 1927).</li>
                <li>Salary data is available from 1985 onward. Innings pitched is stored as outs and converted to innings (outs / 3).</li>
            </ul>
        </section>
    </div>
</div>
```

- [ ] **Step 3: Run the rendering test to verify it passes**

Run: `dotnet test baseball-history-tests --filter "McpDocsPageTests.McpDocs_FullPage_RendersTitleToolsAndResources"`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add baseball-history-web/Pages/McpDocs.cshtml baseball-history-web/Pages/McpDocs.cshtml.cs baseball-history-tests/Pages/McpDocsPageTests.cs
git commit -m "Add MCP server documentation page"
```

---

## Task 3: Add the MCP nav link (green for nav test)

**Files:**
- Modify: `baseball-history-web/Pages/Shared/_ShellHeader.cshtml` (the "API" nav item, ~line 45-47)

- [ ] **Step 1: Run the nav test to confirm it currently fails**

Run: `dotnet test baseball-history-tests --filter "McpDocsPageTests.ShellHeader_IncludesMcpNavLink"`
Expected: FAIL — no `/McpDocs` link is rendered in the shell header yet.

- [ ] **Step 2: Add the nav link**

In `baseball-history-web/Pages/Shared/_ShellHeader.cshtml`, find the existing API nav item:

```html
                    <li class="nav-item">
                        <a class="nav-link" asp-area="" asp-page="/ApiDocs">API</a>
                    </li>
```

Add an MCP nav item immediately after it:

```html
                    <li class="nav-item">
                        <a class="nav-link" asp-area="" asp-page="/ApiDocs">API</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" asp-area="" asp-page="/McpDocs">MCP</a>
                    </li>
```

- [ ] **Step 3: Run the nav test to verify it passes**

Run: `dotnet test baseball-history-tests --filter "McpDocsPageTests.ShellHeader_IncludesMcpNavLink"`
Expected: PASS — the rendered shell now contains `href="/McpDocs"`.

- [ ] **Step 4: Commit**

```bash
git add baseball-history-web/Pages/Shared/_ShellHeader.cshtml
git commit -m "Add MCP nav link to shell header"
```

---

## Task 4: Full verification

- [ ] **Step 1: Build the solution**

Run: `dotnet build baseball-history.sln --nologo`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`.

- [ ] **Step 2: Run the full test suite**

Run: `dotnet test baseball-history-tests --nologo --logger "console;verbosity=minimal"`
Expected: PASS — all tests pass (the previous baseline was 386; this adds 2 new tests for 388 total).

- [ ] **Step 3: Final commit (only if anything is uncommitted)**

```bash
git status --short
# If clean, nothing to do. Otherwise:
# git add -A && git commit -m "Finalize MCP documentation page"
```

---

## Self-Review Notes

- **Spec coverage:** All 9 spec sections map to Task 2 markup (header, info alert, how-it-works, connecting JSON, example prompts, tools, resources, SDK, limits/data notes). Nav (spec "Navigation") → Task 3. Testing (spec "Testing") → Tasks 1 & 3.
- **Accuracy:** Tool names, descriptions, the two stat lists, 6 resource URIs, and limit caps are taken verbatim from the source-of-truth files listed in the header. Verify each against those files during implementation before committing.
- **Razor encoding:** `<`, `>`, `&` inside `<code>`/`<pre>` blocks are HTML-encoded so Razor does not parse them as tags. The only generics needing this are `Dictionary&lt;string, string?&gt;` and `Dictionary&lt;string, object?&gt;` in the SDK sample.
- **Type/name consistency:** `McpDocsModel` (code-behind) matches `@model McpDocsModel`; route `/McpDocs` is consistent across page, nav link, and both tests.
