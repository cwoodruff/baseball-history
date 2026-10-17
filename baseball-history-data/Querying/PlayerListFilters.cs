using BaseballHistory.Data.Models;

namespace BaseballHistory.Data.Querying;

/// <summary>
/// The Players browser's name/position/era filters and sort, shared by the
/// /Players page, /api/players and the MCP search_players tool so all three
/// return the same rows.
/// </summary>
public static class PlayerListFilters
{
    public const string DefaultSort = "name";

    /// <summary>Position codes; OF also matches LF/CF/RF fielding rows</summary>
    public static readonly IReadOnlyList<string> Positions = ["P", "C", "1B", "2B", "3B", "SS", "OF", "DH"];

    /// <summary>name (last, first), or career hr/hits/games descending</summary>
    public static readonly IReadOnlyList<string> Sorts = ["name", "hr", "hits", "games"];

    public static bool IsValidPosition(string? position) => position != null && Positions.Contains(position);

    public static bool IsValidSort(string? sort) => sort != null && Sorts.Contains(sort);

    /// <summary>
    /// A name search spans all players; otherwise rows are limited to last
    /// names starting with the letter. Then position, era and sort apply.
    /// </summary>
    public static IQueryable<People> Apply(IQueryable<People> people, char letter, string? searchQuery,
        string? position, int? eraDecade, string sortBy)
    {
        var query = searchQuery != null
            ? people.Where(p =>
                ((p.NameFirst ?? "") + " " + (p.NameLast ?? "")).ToLower().Contains(searchQuery.ToLower()))
            : people.Where(p =>
                p.NameLast != null && p.NameLast.ToUpper().StartsWith(letter.ToString()));

        return ApplySort(ApplyEra(ApplyPosition(query, position), eraDecade), sortBy);
    }

    /// <summary>Keeps players with any fielding row at the position (OF includes LF/CF/RF)</summary>
    public static IQueryable<People> ApplyPosition(IQueryable<People> query, string? position)
    {
        if (position == null)
            return query;

        return position == "OF"
            ? query.Where(p => p.Fieldings.Any(f =>
                f.Pos == "OF" || f.Pos == "LF" || f.Pos == "CF" || f.Pos == "RF"))
            : query.Where(p => p.Fieldings.Any(f => f.Pos == position));
    }

    /// <summary>Keeps players active at any point during the decade</summary>
    public static IQueryable<People> ApplyEra(IQueryable<People> query, int? eraDecade)
    {
        if (!eraDecade.HasValue)
            return query;

        var eraStart = new DateOnly(eraDecade.Value, 1, 1);
        var eraEnd = new DateOnly(eraDecade.Value + 9, 12, 31);
        return query.Where(p => p.Debut != null && p.FinalGame != null &&
                                p.Debut <= eraEnd && p.FinalGame >= eraStart);
    }

    public static IOrderedQueryable<People> ApplySort(IQueryable<People> query, string sortBy) => sortBy switch
    {
        "hr" => query.OrderByDescending(p => p.Battings.Sum(b => (int?)b.Hr) ?? 0)
            .ThenBy(p => p.NameLast).ThenBy(p => p.NameFirst),
        "hits" => query.OrderByDescending(p => p.Battings.Sum(b => (int?)b.H) ?? 0)
            .ThenBy(p => p.NameLast).ThenBy(p => p.NameFirst),
        "games" => query.OrderByDescending(p =>
                (p.Battings.Sum(b => (int?)b.G) ?? 0) + (p.Pitchings.Sum(pi => (int?)pi.G) ?? 0))
            .ThenBy(p => p.NameLast).ThenBy(p => p.NameFirst),
        _ => query.OrderBy(p => p.NameLast).ThenBy(p => p.NameFirst)
    };
}
