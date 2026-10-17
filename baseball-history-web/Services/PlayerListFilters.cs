using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;

namespace baseball_history_web.Services;

/// <summary>
/// The Players browser's name/position/era filters and sort, shared by the
/// /Players page and /api/players so both return the same rows.
/// </summary>
public static class PlayerListFilters
{
    public const string DefaultSort = "name";

    public static bool IsValidPosition(string? position) =>
        PlayerListViewModel.PositionOptions.Any(o => o.Value == position);

    public static bool IsValidSort(string? sort) =>
        PlayerListViewModel.SortOptions.Any(o => o.Value == sort);

    /// <summary>
    /// A name search spans all players; otherwise rows are limited to last
    /// names starting with the letter. Position matches any fielding row at
    /// that position (OF includes LF/CF/RF). Era keeps players active at any
    /// point during the decade.
    /// </summary>
    public static IQueryable<People> Apply(IQueryable<People> people, char letter, string? searchQuery,
        string? position, int? eraDecade, string sortBy)
    {
        var query = searchQuery != null
            ? people.Where(p =>
                ((p.NameFirst ?? "") + " " + (p.NameLast ?? "")).ToLower().Contains(searchQuery.ToLower()))
            : people.Where(p =>
                p.NameLast != null && p.NameLast.ToUpper().StartsWith(letter.ToString()));

        if (position != null)
        {
            query = position == "OF"
                ? query.Where(p => p.Fieldings.Any(f =>
                    f.Pos == "OF" || f.Pos == "LF" || f.Pos == "CF" || f.Pos == "RF"))
                : query.Where(p => p.Fieldings.Any(f => f.Pos == position));
        }

        if (eraDecade.HasValue)
        {
            var eraStart = new DateOnly(eraDecade.Value, 1, 1);
            var eraEnd = new DateOnly(eraDecade.Value + 9, 12, 31);
            query = query.Where(p => p.Debut != null && p.FinalGame != null &&
                                     p.Debut <= eraEnd && p.FinalGame >= eraStart);
        }

        return sortBy switch
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
}
