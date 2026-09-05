namespace baseball_history_web.Services;

/// <summary>
/// For 453 players — mostly segregation-era, some 19th-century — only a
/// surname or a bare initial survived the original box scores, so their
/// records must be presented as historically incomplete rather than as
/// fully documented careers (see docs/incomplete-record-badge-spec.md).
/// </summary>
public static class PlayerRecordFacts
{
    /// <summary>
    /// Explanation shown wherever a partial record is badged. Copy is an
    /// acceptance criterion on issue #71 — change it here, nowhere else.
    /// </summary>
    public const string PartialRecordExplanation =
        "Historically incomplete record. Only a partial name survived in the original sources for this player — " +
        "most such records come from segregation-era Black baseball, where box scores and rosters were unevenly " +
        "preserved; a few date to the 19th century. Documented statistics may understate this player's actual career.";

    public static bool IsPartialName(string? nameFirst)
    {
        var trimmed = nameFirst?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return true;

        return trimmed.Length switch
        {
            1 => char.IsLetter(trimmed[0]),
            2 => char.IsLetter(trimmed[0]) && trimmed[1] == '.',
            _ => false
        };
    }
}
