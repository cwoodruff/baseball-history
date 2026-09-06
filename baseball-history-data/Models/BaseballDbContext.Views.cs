using Microsoft.EntityFrameworkCore;

namespace BaseballHistory.Data.Models;

public partial class BaseballDbContext
{
    public virtual DbSet<PlayerSeasonRate> PlayerSeasonRates { get; set; } = null!;

    public virtual DbSet<CareerBattingSummary> CareerBattingSummaries { get; set; } = null!;

    public virtual DbSet<PlayerSeasonPitchingAdvanced> PlayerSeasonPitchingAdvanced { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerSeasonRate>(entity =>
        {
            entity.HasNoKey().ToView("v_player_season_rates");

            entity.Property(e => e.PlayerId).HasColumnName("playerid");
            entity.Property(e => e.YearId).HasColumnName("yearid");
            entity.Property(e => e.LgId).HasColumnName("lgid");
            entity.Property(e => e.G).HasColumnName("g");
            entity.Property(e => e.Pa).HasColumnName("pa");
            entity.Property(e => e.Ab).HasColumnName("ab");
            entity.Property(e => e.H).HasColumnName("h");
            entity.Property(e => e.Hr).HasColumnName("hr");
            entity.Property(e => e.Tb).HasColumnName("tb");
            entity.Property(e => e.TeamGamesWtd).HasColumnName("team_games_wtd");
            entity.Property(e => e.PaThreshold).HasColumnName("pa_threshold");
            entity.Property(e => e.Qualified).HasColumnName("qualified");
            entity.Property(e => e.Avg).HasColumnName("avg");
            entity.Property(e => e.Obp).HasColumnName("obp");
            entity.Property(e => e.Slg).HasColumnName("slg");
            entity.Property(e => e.Iso).HasColumnName("iso");
            entity.Property(e => e.Babip).HasColumnName("babip");
            entity.Property(e => e.BbPct).HasColumnName("bb_pct");
            entity.Property(e => e.KPct).HasColumnName("k_pct");
            entity.Property(e => e.OpsIndex).HasColumnName("ops_index");
            entity.Property(e => e.HrPer162).HasColumnName("hr_per_162");
            entity.Property(e => e.HPer162).HasColumnName("h_per_162");
            entity.Property(e => e.RbiPer162).HasColumnName("rbi_per_162");
        });

        modelBuilder.Entity<CareerBattingSummary>(entity =>
        {
            entity.HasNoKey().ToView("v_career_batting");

            entity.Property(e => e.PlayerId).HasColumnName("playerid");
            entity.Property(e => e.FirstYear).HasColumnName("first_year");
            entity.Property(e => e.LastYear).HasColumnName("last_year");
            entity.Property(e => e.Seasons).HasColumnName("seasons");
            entity.Property(e => e.Pa).HasColumnName("pa");
            entity.Property(e => e.Ab).HasColumnName("ab");
            entity.Property(e => e.CareerPaThreshold).HasColumnName("career_pa_threshold");
            entity.Property(e => e.Qualified).HasColumnName("qualified");
            entity.Property(e => e.PctOfThreshold).HasColumnName("pct_of_threshold");
            entity.Property(e => e.Avg).HasColumnName("avg");
            entity.Property(e => e.Obp).HasColumnName("obp");
            entity.Property(e => e.Slg).HasColumnName("slg");
            entity.Property(e => e.OpsIndex).HasColumnName("ops_index");
        });

        modelBuilder.Entity<PlayerSeasonPitchingAdvanced>(entity =>
        {
            entity.HasNoKey().ToView("v_player_season_pitching");

            entity.Property(e => e.PlayerId).HasColumnName("playerid");
            entity.Property(e => e.YearId).HasColumnName("yearid");
            entity.Property(e => e.LgId).HasColumnName("lgid");
            entity.Property(e => e.G).HasColumnName("g");
            entity.Property(e => e.Gs).HasColumnName("gs");
            entity.Property(e => e.Ipouts).HasColumnName("ipouts");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.TeamGamesWtd).HasColumnName("team_games_wtd");
            entity.Property(e => e.IpoutsThreshold).HasColumnName("ipouts_threshold");
            entity.Property(e => e.Qualified).HasColumnName("qualified");
            entity.Property(e => e.Era).HasColumnName("era");
            entity.Property(e => e.Whip).HasColumnName("whip");
            entity.Property(e => e.K9).HasColumnName("k9");
            entity.Property(e => e.Bb9).HasColumnName("bb9");
        });
    }
}
