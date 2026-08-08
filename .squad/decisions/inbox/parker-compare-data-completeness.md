# Parker decision note: compare data completeness

- Added two supplemental postseason comparison cards: one for batting and one for pitching, matching the existing compare-card/table pattern and showing only the summary fields called for by the plan.
- Postseason batting appears only when at least one player has career postseason at-bats; players without postseason batting data render as an em dash instead of zeroes.
- Postseason pitching appears only when at least one player has career postseason innings pitched (IPouts > 0); players without postseason pitching data render as an em dash instead of zeroes.
- Used existing compare-table arrow/highlight behavior for postseason cards: higher is better for most stats, while lower is better for postseason losses and ERA.
- Fielding is summarized at each player's primary position, defined as the position with the highest total games across all career Fielding rows; ties fall back to position code alphabetical order because the LINQ orders by descending games then position.
- Fielding Po, A, E, and Dp values are parsed defensively in memory with the same int.TryParse + invariant-culture approach used by PlayerEndpoints.GetPlayerFielding, so null, empty-string, or malformed Lahman values become 0 instead of throwing.
- The fielding card intentionally does not use comparison arrows because primary positions may differ between players, making most cross-player fielding totals misleading; the table follows the simpler awards-style presentation with plain values.
- Fielding percentage is computed as (Po + A) / (Po + A + E) when the denominator is positive, otherwise 0, and formatted in .000 style.
- A player with postseason rows totaling zero AB or zero IPouts is treated as not having displayable postseason batting/pitching for section-visibility purposes, matching the requirement to gate on real at-bats / innings.
