using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Models;

public partial class BaseballDbContext : DbContext
{
    public BaseballDbContext()
    {
    }

    public BaseballDbContext(DbContextOptions<BaseballDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AllstarFull> AllstarFull { get; set; }

    public virtual DbSet<Appearances> Appearances { get; set; }

    public virtual DbSet<AwardsManagers> AwardsManagers { get; set; }

    public virtual DbSet<AwardsPlayers> AwardsPlayers { get; set; }

    public virtual DbSet<AwardsShareManagers> AwardsShareManagers { get; set; }

    public virtual DbSet<AwardsSharePlayers> AwardsSharePlayers { get; set; }

    public virtual DbSet<Batting> Batting { get; set; }

    public virtual DbSet<BattingPost> BattingPost { get; set; }

    public virtual DbSet<CollegePlaying> CollegePlaying { get; set; }

    public virtual DbSet<Fielding> Fielding { get; set; }

    public virtual DbSet<FieldingOf> FieldingOf { get; set; }

    public virtual DbSet<FieldingOfsplit> FieldingOfsplit { get; set; }

    public virtual DbSet<FieldingPost> FieldingPost { get; set; }

    public virtual DbSet<HallOfFame> HallOfFame { get; set; }

    public virtual DbSet<HomeGames> HomeGames { get; set; }

    public virtual DbSet<Managers> Managers { get; set; }

    public virtual DbSet<ManagersHalf> ManagersHalf { get; set; }

    public virtual DbSet<Parks> Parks { get; set; }

    public virtual DbSet<People> People { get; set; }

    public virtual DbSet<Pitching> Pitching { get; set; }

    public virtual DbSet<PitchingPost> PitchingPost { get; set; }

    public virtual DbSet<Salaries> Salaries { get; set; }

    public virtual DbSet<Schools> Schools { get; set; }

    public virtual DbSet<SeriesPost> SeriesPost { get; set; }

    public virtual DbSet<Teams> Teams { get; set; }

    public virtual DbSet<TeamsFranchises> TeamsFranchises { get; set; }

    public virtual DbSet<TeamsHalf> TeamsHalf { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AllstarFull>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.LgId, e.TeamId, e.GameId });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.GameId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("gameID");
            entity.Property(e => e.GameNum)
                .HasColumnType("tinyint")
                .HasColumnName("gameNum");
            entity.Property(e => e.Gp)
                .HasColumnType("tinyint")
                .HasColumnName("GP");
            entity.Property(e => e.StartingPos)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("startingPos");
        });

        modelBuilder.Entity<Appearances>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.LgId, e.TeamId, e.YearId });

            entity.Property(e => e.PlayerId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.G1b)
                .HasColumnType("smallint")
                .HasColumnName("G_1b");
            entity.Property(e => e.G2b)
                .HasColumnType("smallint")
                .HasColumnName("G_2b");
            entity.Property(e => e.G3b)
                .HasColumnType("smallint")
                .HasColumnName("G_3b");
            entity.Property(e => e.GAll)
                .HasColumnType("smallint")
                .HasColumnName("G_all");
            entity.Property(e => e.GBatting)
                .HasColumnType("smallint")
                .HasColumnName("G_batting");
            entity.Property(e => e.GC)
                .HasColumnType("smallint")
                .HasColumnName("G_c");
            entity.Property(e => e.GCf)
                .HasColumnType("smallint")
                .HasColumnName("G_cf");
            entity.Property(e => e.GDefense)
                .HasColumnType("smallint")
                .HasColumnName("G_defense");
            entity.Property(e => e.GDh)
                .HasColumnType("smallint")
                .HasColumnName("G_dh");
            entity.Property(e => e.GLf)
                .HasColumnType("smallint")
                .HasColumnName("G_lf");
            entity.Property(e => e.GOf)
                .HasColumnType("smallint")
                .HasColumnName("G_of");
            entity.Property(e => e.GP)
                .HasColumnType("smallint")
                .HasColumnName("G_p");
            entity.Property(e => e.GPh)
                .HasColumnType("smallint")
                .HasColumnName("G_ph");
            entity.Property(e => e.GPr)
                .HasColumnType("smallint")
                .HasColumnName("G_pr");
            entity.Property(e => e.GRf)
                .HasColumnType("smallint")
                .HasColumnName("G_rf");
            entity.Property(e => e.GSs)
                .HasColumnType("smallint")
                .HasColumnName("G_ss");
            entity.Property(e => e.Gs)
                .HasColumnType("smallint")
                .HasColumnName("GS");
        });

        modelBuilder.Entity<AwardsManagers>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.AwardId, e.YearId, e.LgId, e.Tie });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.AwardId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("awardID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Tie)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)")
                .HasColumnName("tie");
            entity.Property(e => e.Notes)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("notes");
        });

        modelBuilder.Entity<AwardsPlayers>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.AwardId, e.LgId, e.Tie, e.Notes });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("INT")
                .HasColumnName("yearID");
            entity.Property(e => e.AwardId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("awardID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Tie)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)")
                .HasColumnName("tie");
            entity.Property(e => e.Notes)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("notes");
        });

        modelBuilder.Entity<AwardsShareManagers>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.LgId, e.YearId, e.AwardId });

            entity.Property(e => e.PlayerId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.AwardId)
                .HasColumnType("nvarchar(255)")
                .HasColumnName("awardID");
            entity.Property(e => e.PointsMax)
                .HasColumnType("smallint")
                .HasColumnName("pointsMax");
            entity.Property(e => e.PointsWon)
                .HasColumnType("smallint")
                .HasColumnName("pointsWon");
            entity.Property(e => e.VotesFirst)
                .HasColumnType("smallint")
                .HasColumnName("votesFirst");
        });

        modelBuilder.Entity<AwardsSharePlayers>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.LgId, e.YearId, e.AwardId });

            entity.Property(e => e.PlayerId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.AwardId)
                .HasColumnType("nvarchar(255)")
                .HasColumnName("awardID");
            entity.Property(e => e.PointsMax)
                .HasColumnType("smallint")
                .HasColumnName("pointsMax");
            entity.Property(e => e.PointsWon)
                .HasColumnType("smallint")
                .HasColumnName("pointsWon");
            entity.Property(e => e.VotesFirst)
                .HasColumnType("smallint")
                .HasColumnName("votesFirst");
        });

        modelBuilder.Entity<Batting>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.Stint, e.TeamId, e.LgId });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Stint)
                .HasColumnType("tinyint")
                .HasColumnName("stint");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Ab)
                .HasColumnType("smallint")
                .HasColumnName("AB");
            entity.Property(e => e.Bb)
                .HasColumnType("smallint")
                .HasColumnName("BB");
            entity.Property(e => e.Cs)
                .HasColumnType("smallint")
                .HasColumnName("CS");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Gidp)
                .HasColumnType("smallint")
                .HasColumnName("GIDP");
            entity.Property(e => e.H).HasColumnType("smallint");
            entity.Property(e => e.Hbp)
                .HasColumnType("smallint")
                .HasColumnName("HBP");
            entity.Property(e => e.Hr)
                .HasColumnType("smallint")
                .HasColumnName("HR");
            entity.Property(e => e.Ibb)
                .HasColumnType("smallint")
                .HasColumnName("IBB");
            entity.Property(e => e.R).HasColumnType("smallint");
            entity.Property(e => e.Rbi)
                .HasColumnType("smallint")
                .HasColumnName("RBI");
            entity.Property(e => e.Sb)
                .HasColumnType("smallint")
                .HasColumnName("SB");
            entity.Property(e => e.Sf)
                .HasColumnType("smallint")
                .HasColumnName("SF");
            entity.Property(e => e.Sh)
                .HasColumnType("smallint")
                .HasColumnName("SH");
            entity.Property(e => e.So)
                .HasColumnType("smallint")
                .HasColumnName("SO");
            entity.Property(e => e._2b)
                .HasColumnType("smallint")
                .HasColumnName("2B");
            entity.Property(e => e._3b)
                .HasColumnType("smallint")
                .HasColumnName("3B");
        });

        modelBuilder.Entity<BattingPost>(entity =>
        {
            entity.HasKey(e => new { e.YearId, e.Round, e.PlayerId, e.TeamId, e.LgId });

            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Round)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("round");
            entity.Property(e => e.PlayerId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Ab)
                .HasColumnType("smallint")
                .HasColumnName("AB");
            entity.Property(e => e.Bb)
                .HasColumnType("smallint")
                .HasColumnName("BB");
            entity.Property(e => e.Cs)
                .HasColumnType("smallint")
                .HasColumnName("CS");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Gidp)
                .HasColumnType("smallint")
                .HasColumnName("GIDP");
            entity.Property(e => e.H).HasColumnType("smallint");
            entity.Property(e => e.Hbp)
                .HasColumnType("smallint")
                .HasColumnName("HBP");
            entity.Property(e => e.Hr)
                .HasColumnType("smallint")
                .HasColumnName("HR");
            entity.Property(e => e.Ibb)
                .HasColumnType("smallint")
                .HasColumnName("IBB");
            entity.Property(e => e.R).HasColumnType("smallint");
            entity.Property(e => e.Rbi)
                .HasColumnType("smallint")
                .HasColumnName("RBI");
            entity.Property(e => e.Sb)
                .HasColumnType("smallint")
                .HasColumnName("SB");
            entity.Property(e => e.Sf)
                .HasColumnType("smallint")
                .HasColumnName("SF");
            entity.Property(e => e.Sh)
                .HasColumnType("smallint")
                .HasColumnName("SH");
            entity.Property(e => e.So)
                .HasColumnType("smallint")
                .HasColumnName("SO");
            entity.Property(e => e._2b)
                .HasColumnType("smallint")
                .HasColumnName("2B");
            entity.Property(e => e._3b)
                .HasColumnType("smallint")
                .HasColumnName("3B");
        });

        modelBuilder.Entity<CollegePlaying>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.SchoolId, e.YearId });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.SchoolId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("schoolID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
        });

        modelBuilder.Entity<Fielding>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.Stint, e.TeamId, e.LgId, e.Pos });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Stint)
                .HasColumnType("tinyint")
                .HasColumnName("stint");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Pos)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(2)")
                .HasColumnName("POS");
            entity.Property(e => e.A).HasColumnType("smallint");
            entity.Property(e => e.Cs)
                .HasColumnType("smallint")
                .HasColumnName("CS");
            entity.Property(e => e.Dp)
                .HasColumnType("smallint")
                .HasColumnName("DP");
            entity.Property(e => e.E).HasColumnType("smallint");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Gs)
                .HasColumnType("smallint")
                .HasColumnName("GS");
            entity.Property(e => e.InnOuts).HasColumnType("smallint");
            entity.Property(e => e.Pb)
                .HasColumnType("smallint")
                .HasColumnName("PB");
            entity.Property(e => e.Po)
                .HasColumnType("smallint")
                .HasColumnName("PO");
            entity.Property(e => e.Sb)
                .HasColumnType("smallint")
                .HasColumnName("SB");
            entity.Property(e => e.Wp)
                .HasColumnType("smallint")
                .HasColumnName("WP");
            entity.Property(e => e.Zr)
                .HasColumnType("float")
                .HasColumnName("ZR");
        });

        modelBuilder.Entity<FieldingOf>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.Stint });

            entity.ToTable("FieldingOF");

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Stint)
                .HasColumnType("tinyint")
                .HasColumnName("stint");
            entity.Property(e => e.Gcf).HasColumnType("smallint");
            entity.Property(e => e.Glf).HasColumnType("smallint");
            entity.Property(e => e.Grf).HasColumnType("smallint");
        });

        modelBuilder.Entity<FieldingOfsplit>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.Stint, e.TeamId, e.LgId, e.Pos });

            entity.ToTable("FieldingOFsplit");

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Stint)
                .HasColumnType("tinyint")
                .HasColumnName("stint");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Pos)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(2)")
                .HasColumnName("POS");
            entity.Property(e => e.A).HasColumnType("smallint");
            entity.Property(e => e.Cs)
                .HasColumnType("smallint")
                .HasColumnName("CS");
            entity.Property(e => e.Dp)
                .HasColumnType("smallint")
                .HasColumnName("DP");
            entity.Property(e => e.E).HasColumnType("smallint");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Gs)
                .HasColumnType("smallint")
                .HasColumnName("GS");
            entity.Property(e => e.InnOuts).HasColumnType("smallint");
            entity.Property(e => e.Pb)
                .HasColumnType("smallint")
                .HasColumnName("PB");
            entity.Property(e => e.Po)
                .HasColumnType("smallint")
                .HasColumnName("PO");
            entity.Property(e => e.Sb)
                .HasColumnType("smallint")
                .HasColumnName("SB");
            entity.Property(e => e.Wp)
                .HasColumnType("smallint")
                .HasColumnName("WP");
            entity.Property(e => e.Zr)
                .HasColumnType("float")
                .HasColumnName("ZR");
        });

        modelBuilder.Entity<FieldingPost>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.TeamId, e.LgId, e.Round, e.Pos });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Round)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("round");
            entity.Property(e => e.Pos)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(2)")
                .HasColumnName("POS");
            entity.Property(e => e.A).HasColumnType("smallint");
            entity.Property(e => e.Cs)
                .HasColumnType("smallint")
                .HasColumnName("CS");
            entity.Property(e => e.Dp)
                .HasColumnType("smallint")
                .HasColumnName("DP");
            entity.Property(e => e.E).HasColumnType("smallint");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Gs)
                .HasColumnType("smallint")
                .HasColumnName("GS");
            entity.Property(e => e.InnOuts).HasColumnType("smallint");
            entity.Property(e => e.Pb)
                .HasColumnType("smallint")
                .HasColumnName("PB");
            entity.Property(e => e.Po)
                .HasColumnType("smallint")
                .HasColumnName("PO");
            entity.Property(e => e.Sb)
                .HasColumnType("smallint")
                .HasColumnName("SB");
            entity.Property(e => e.Tp)
                .HasColumnType("smallint")
                .HasColumnName("TP");
        });

        modelBuilder.Entity<HallOfFame>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.Yearid, e.VotedBy });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.Yearid)
                .HasColumnType("smallint")
                .HasColumnName("yearid");
            entity.Property(e => e.VotedBy)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("votedBy");
            entity.Property(e => e.Ballots)
                .HasColumnType("smallint")
                .HasColumnName("ballots");
            entity.Property(e => e.Category)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("category");
            entity.Property(e => e.Inducted)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)")
                .HasColumnName("inducted");
            entity.Property(e => e.Needed)
                .HasColumnType("smallint")
                .HasColumnName("needed");
            entity.Property(e => e.NeededNote)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1000)")
                .HasColumnName("needed_note");
            entity.Property(e => e.Votes)
                .HasColumnType("smallint")
                .HasColumnName("votes");
        });

        modelBuilder.Entity<HomeGames>(entity =>
        {
            entity.HasKey(e => new { e.Yearkey, e.Leaguekey, e.Teamkey, e.Parkkey });

            entity.Property(e => e.Yearkey)
                .HasColumnType("INT")
                .HasColumnName("yearkey");
            entity.Property(e => e.Leaguekey)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("leaguekey");
            entity.Property(e => e.Teamkey)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamkey");
            entity.Property(e => e.Parkkey)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("parkkey");
            entity.Property(e => e.Attendance)
                .HasColumnType("INT")
                .HasColumnName("attendance");
            entity.Property(e => e.Games)
                .HasColumnType("smallint")
                .HasColumnName("games");
            entity.Property(e => e.Openings)
                .HasColumnType("smallint")
                .HasColumnName("openings");
            entity.Property(e => e.Spanfirst)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("spanfirst");
            entity.Property(e => e.Spanlast)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("spanlast");
        });

        modelBuilder.Entity<Managers>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.TeamId, e.LgId, e.Inseason });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Inseason)
                .HasColumnType("tinyint")
                .HasColumnName("inseason");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.L).HasColumnType("smallint");
            entity.Property(e => e.PlyrMgr)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)")
                .HasColumnName("plyrMgr");
            entity.Property(e => e.Rank)
                .HasColumnType("tinyint")
                .HasColumnName("rank");
            entity.Property(e => e.W).HasColumnType("smallint");
        });

        modelBuilder.Entity<ManagersHalf>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.TeamId, e.LgId, e.Half });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.Half)
                .HasColumnType("tinyint")
                .HasColumnName("half");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Inseason)
                .HasColumnType("tinyint")
                .HasColumnName("inseason");
            entity.Property(e => e.L).HasColumnType("smallint");
            entity.Property(e => e.Rank)
                .HasColumnType("tinyint")
                .HasColumnName("rank");
            entity.Property(e => e.W).HasColumnType("smallint");
        });

        modelBuilder.Entity<Parks>(entity =>
        {
            entity.HasIndex(e => e.Parkkey, "IX_Parks_parkkey").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("INT")
                .HasColumnName("ID");
            entity.Property(e => e.City)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("country");
            entity.Property(e => e.Parkalias)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(512)")
                .HasColumnName("parkalias");
            entity.Property(e => e.Parkkey)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("parkkey");
            entity.Property(e => e.Parkname)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("parkname");
            entity.Property(e => e.State)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("state");
        });

        modelBuilder.Entity<People>(entity =>
        {
            entity.HasKey(e => e.PlayerId);

            entity.Property(e => e.PlayerId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.Bats)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("bats");
            entity.Property(e => e.BbrefId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("bbrefID");
            entity.Property(e => e.BirthCity)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("birthCity");
            entity.Property(e => e.BirthCountry)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("birthCountry");
            entity.Property(e => e.BirthDay)
                .HasColumnType("INT")
                .HasColumnName("birthDay");
            entity.Property(e => e.BirthMonth)
                .HasColumnType("INT")
                .HasColumnName("birthMonth");
            entity.Property(e => e.BirthState)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("birthState");
            entity.Property(e => e.BirthYear)
                .HasColumnType("INT")
                .HasColumnName("birthYear");
            entity.Property(e => e.DeathCity)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("deathCity");
            entity.Property(e => e.DeathCountry)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("deathCountry");
            entity.Property(e => e.DeathDay)
                .HasColumnType("INT")
                .HasColumnName("deathDay");
            entity.Property(e => e.DeathMonth)
                .HasColumnType("INT")
                .HasColumnName("deathMonth");
            entity.Property(e => e.DeathState)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("deathState");
            entity.Property(e => e.DeathYear)
                .HasColumnType("INT")
                .HasColumnName("deathYear");
            entity.Property(e => e.Debut)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("debut");
            entity.Property(e => e.FinalGame)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("finalGame");
            entity.Property(e => e.Height)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("height");
            entity.Property(e => e.Id)
                .HasColumnType("INT")
                .HasColumnName("ID");
            entity.Property(e => e.NameFirst)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("nameFirst");
            entity.Property(e => e.NameGiven)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("nameGiven");
            entity.Property(e => e.NameLast)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("nameLast");
            entity.Property(e => e.RetroId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("retroID");
            entity.Property(e => e.Throws)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("throws");
            entity.Property(e => e.Weight)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("weight");
        });

        modelBuilder.Entity<Pitching>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.Stint });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Stint)
                .HasColumnType("tinyint")
                .HasColumnName("stint");
            entity.Property(e => e.Baopp)
                .HasColumnType("float")
                .HasColumnName("BAOpp");
            entity.Property(e => e.Bb)
                .HasColumnType("smallint")
                .HasColumnName("BB");
            entity.Property(e => e.Bfp)
                .HasColumnType("smallint")
                .HasColumnName("BFP");
            entity.Property(e => e.Bk)
                .HasColumnType("smallint")
                .HasColumnName("BK");
            entity.Property(e => e.Cg)
                .HasColumnType("smallint")
                .HasColumnName("CG");
            entity.Property(e => e.Er)
                .HasColumnType("smallint")
                .HasColumnName("ER");
            entity.Property(e => e.Era)
                .HasColumnType("float")
                .HasColumnName("ERA");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Gf)
                .HasColumnType("smallint")
                .HasColumnName("GF");
            entity.Property(e => e.Gidp)
                .HasColumnType("smallint")
                .HasColumnName("GIDP");
            entity.Property(e => e.Gs)
                .HasColumnType("smallint")
                .HasColumnName("GS");
            entity.Property(e => e.H).HasColumnType("smallint");
            entity.Property(e => e.Hbp)
                .HasColumnType("smallint")
                .HasColumnName("HBP");
            entity.Property(e => e.Hr)
                .HasColumnType("smallint")
                .HasColumnName("HR");
            entity.Property(e => e.Ibb)
                .HasColumnType("smallint")
                .HasColumnName("IBB");
            entity.Property(e => e.Ipouts)
                .HasColumnType("smallint")
                .HasColumnName("IPouts");
            entity.Property(e => e.L).HasColumnType("smallint");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.R).HasColumnType("smallint");
            entity.Property(e => e.Sf)
                .HasColumnType("smallint")
                .HasColumnName("SF");
            entity.Property(e => e.Sh)
                .HasColumnType("smallint")
                .HasColumnName("SH");
            entity.Property(e => e.Sho)
                .HasColumnType("smallint")
                .HasColumnName("SHO");
            entity.Property(e => e.So)
                .HasColumnType("smallint")
                .HasColumnName("SO");
            entity.Property(e => e.Sv)
                .HasColumnType("smallint")
                .HasColumnName("SV");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.W).HasColumnType("smallint");
            entity.Property(e => e.Wp)
                .HasColumnType("smallint")
                .HasColumnName("WP");
        });

        modelBuilder.Entity<PitchingPost>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.Round });

            entity.Property(e => e.PlayerId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Round)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("round");
            entity.Property(e => e.Baopp)
                .HasColumnType("float")
                .HasColumnName("BAOpp");
            entity.Property(e => e.Bb)
                .HasColumnType("smallint")
                .HasColumnName("BB");
            entity.Property(e => e.Bfp)
                .HasColumnType("smallint")
                .HasColumnName("BFP");
            entity.Property(e => e.Bk)
                .HasColumnType("smallint")
                .HasColumnName("BK");
            entity.Property(e => e.Cg)
                .HasColumnType("smallint")
                .HasColumnName("CG");
            entity.Property(e => e.Er)
                .HasColumnType("smallint")
                .HasColumnName("ER");
            entity.Property(e => e.Era)
                .HasColumnType("float")
                .HasColumnName("ERA");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Gf)
                .HasColumnType("smallint")
                .HasColumnName("GF");
            entity.Property(e => e.Gidp)
                .HasColumnType("smallint")
                .HasColumnName("GIDP");
            entity.Property(e => e.Gs)
                .HasColumnType("smallint")
                .HasColumnName("GS");
            entity.Property(e => e.H).HasColumnType("smallint");
            entity.Property(e => e.Hbp)
                .HasColumnType("smallint")
                .HasColumnName("HBP");
            entity.Property(e => e.Hr)
                .HasColumnType("smallint")
                .HasColumnName("HR");
            entity.Property(e => e.Ibb)
                .HasColumnType("smallint")
                .HasColumnName("IBB");
            entity.Property(e => e.Ipouts)
                .HasColumnType("INT")
                .HasColumnName("IPouts");
            entity.Property(e => e.L).HasColumnType("smallint");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.R).HasColumnType("smallint");
            entity.Property(e => e.Sf)
                .HasColumnType("smallint")
                .HasColumnName("SF");
            entity.Property(e => e.Sh)
                .HasColumnType("smallint")
                .HasColumnName("SH");
            entity.Property(e => e.Sho)
                .HasColumnType("smallint")
                .HasColumnName("SHO");
            entity.Property(e => e.So)
                .HasColumnType("smallint")
                .HasColumnName("SO");
            entity.Property(e => e.Sv)
                .HasColumnType("smallint")
                .HasColumnName("SV");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.W).HasColumnType("smallint");
            entity.Property(e => e.Wp)
                .HasColumnType("smallint")
                .HasColumnName("WP");
        });

        modelBuilder.Entity<Salaries>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId });

            entity.Property(e => e.PlayerId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(20)")
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Salary)
                .HasColumnType("bigint")
                .HasColumnName("salary");
        });

        modelBuilder.Entity<Schools>(entity =>
        {
            entity.HasKey(e => e.SchoolId);

            entity.Property(e => e.SchoolId)
                .HasColumnType("nvarchar(20)")
                .HasColumnName("schoolID");
            entity.Property(e => e.City)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("country");
            entity.Property(e => e.NameFull)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("name_full");
            entity.Property(e => e.State)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(50)")
                .HasColumnName("state");
        });

        modelBuilder.Entity<SeriesPost>(entity =>
        {
            entity.HasKey(e => new { e.TeamIdwinner, e.LgIdwinner, e.YearId, e.Round });

            entity.Property(e => e.TeamIdwinner)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamIDwinner");
            entity.Property(e => e.LgIdwinner)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgIDwinner");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Round)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(10)")
                .HasColumnName("round");
            entity.Property(e => e.LgIdloser)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgIDloser");
            entity.Property(e => e.Losses)
                .HasColumnType("smallint")
                .HasColumnName("losses");
            entity.Property(e => e.TeamIdloser)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamIDloser");
            entity.Property(e => e.Ties)
                .HasColumnType("smallint")
                .HasColumnName("ties");
            entity.Property(e => e.Wins)
                .HasColumnType("smallint")
                .HasColumnName("wins");
        });

        modelBuilder.Entity<Teams>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.LgId, e.YearId });

            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Ab)
                .HasColumnType("smallint")
                .HasColumnName("AB");
            entity.Property(e => e.Attendance)
                .HasColumnType("INT")
                .HasColumnName("attendance");
            entity.Property(e => e.Bb)
                .HasColumnType("smallint")
                .HasColumnName("BB");
            entity.Property(e => e.Bba)
                .HasColumnType("smallint")
                .HasColumnName("BBA");
            entity.Property(e => e.Bpf)
                .HasColumnType("tinyint")
                .HasColumnName("BPF");
            entity.Property(e => e.Cg)
                .HasColumnType("smallint")
                .HasColumnName("CG");
            entity.Property(e => e.Cs)
                .HasColumnType("smallint")
                .HasColumnName("CS");
            entity.Property(e => e.DivId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(2)")
                .HasColumnName("divID");
            entity.Property(e => e.DivWin)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)");
            entity.Property(e => e.Dp)
                .HasColumnType("smallint")
                .HasColumnName("DP");
            entity.Property(e => e.E).HasColumnType("smallint");
            entity.Property(e => e.Er)
                .HasColumnType("smallint")
                .HasColumnName("ER");
            entity.Property(e => e.Era)
                .HasColumnType("float")
                .HasColumnName("ERA");
            entity.Property(e => e.Fp)
                .HasColumnType("float")
                .HasColumnName("FP");
            entity.Property(e => e.FranchId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("franchID");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.Ghome).HasColumnType("smallint");
            entity.Property(e => e.H).HasColumnType("smallint");
            entity.Property(e => e.Ha)
                .HasColumnType("smallint")
                .HasColumnName("HA");
            entity.Property(e => e.Hbp)
                .HasColumnType("smallint")
                .HasColumnName("HBP");
            entity.Property(e => e.Hr)
                .HasColumnType("smallint")
                .HasColumnName("HR");
            entity.Property(e => e.Hra)
                .HasColumnType("smallint")
                .HasColumnName("HRA");
            entity.Property(e => e.Ipouts)
                .HasColumnType("smallint")
                .HasColumnName("IPouts");
            entity.Property(e => e.L).HasColumnType("smallint");
            entity.Property(e => e.LgWin)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)");
            entity.Property(e => e.Name)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("name");
            entity.Property(e => e.Park)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(255)")
                .HasColumnName("park");
            entity.Property(e => e.Ppf)
                .HasColumnType("tinyint")
                .HasColumnName("PPF");
            entity.Property(e => e.R).HasColumnType("smallint");
            entity.Property(e => e.Ra)
                .HasColumnType("smallint")
                .HasColumnName("RA");
            entity.Property(e => e.Rank).HasColumnType("tinyint");
            entity.Property(e => e.Sb)
                .HasColumnType("smallint")
                .HasColumnName("SB");
            entity.Property(e => e.Sf)
                .HasColumnType("smallint")
                .HasColumnName("SF");
            entity.Property(e => e.Sho)
                .HasColumnType("smallint")
                .HasColumnName("SHO");
            entity.Property(e => e.So)
                .HasColumnType("smallint")
                .HasColumnName("SO");
            entity.Property(e => e.Soa)
                .HasColumnType("smallint")
                .HasColumnName("SOA");
            entity.Property(e => e.Sv)
                .HasColumnType("smallint")
                .HasColumnName("SV");
            entity.Property(e => e.TeamIdbr)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamIDBR");
            entity.Property(e => e.TeamIdlahman45)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamIDlahman45");
            entity.Property(e => e.TeamIdretro)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamIDretro");
            entity.Property(e => e.W).HasColumnType("smallint");
            entity.Property(e => e.Wcwin)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)")
                .HasColumnName("WCWin");
            entity.Property(e => e.Wswin)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)")
                .HasColumnName("WSWin");
            entity.Property(e => e._2b)
                .HasColumnType("smallint")
                .HasColumnName("2B");
            entity.Property(e => e._3b)
                .HasColumnType("smallint")
                .HasColumnName("3B");
        });

        modelBuilder.Entity<TeamsFranchises>(entity =>
        {
            entity.HasKey(e => e.FranchId);

            entity.Property(e => e.FranchId)
                .HasColumnType("nvarchar(4)")
                .HasColumnName("franchID");
            entity.Property(e => e.Active)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(2)")
                .HasColumnName("active");
            entity.Property(e => e.FranchName)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(100)")
                .HasColumnName("franchName");
            entity.Property(e => e.Naassoc)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("NAassoc");
        });

        modelBuilder.Entity<TeamsHalf>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.LgId, e.YearId, e.Half });

            entity.Property(e => e.TeamId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(4)")
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(3)")
                .HasColumnName("lgID");
            entity.Property(e => e.YearId)
                .HasColumnType("smallint")
                .HasColumnName("yearID");
            entity.Property(e => e.Half).HasColumnType("tinyint");
            entity.Property(e => e.DivId)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(2)")
                .HasColumnName("divID");
            entity.Property(e => e.DivWin)
                .UseCollation("NOCASE")
                .HasColumnType("nvarchar(1)");
            entity.Property(e => e.G).HasColumnType("smallint");
            entity.Property(e => e.L).HasColumnType("smallint");
            entity.Property(e => e.Rank).HasColumnType("tinyint");
            entity.Property(e => e.W).HasColumnType("smallint");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
