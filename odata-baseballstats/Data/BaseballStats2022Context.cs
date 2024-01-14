using Microsoft.EntityFrameworkCore;
using odata_baseballstats.Entities;

namespace odata_baseballstats.Data;

public partial class BaseballStats2022Context(DbContextOptions<BaseballStats2022Context> options) : DbContext(options)
{
    public virtual DbSet<AllstarFull> AllstarFulls { get; set; }
    public virtual DbSet<Appearance> Appearances { get; set; }
    public virtual DbSet<AwardsManager> AwardsManagers { get; set; }
    public virtual DbSet<AwardsPlayer> AwardsPlayers { get; set; }
    public virtual DbSet<AwardsShareManager> AwardsShareManagers { get; set; }
    public virtual DbSet<AwardsSharePlayer> AwardsSharePlayers { get; set; }
    public virtual DbSet<Batting> Battings { get; set; }
    public virtual DbSet<BattingPost> BattingPosts { get; set; }
    public virtual DbSet<CollegePlaying> CollegePlayings { get; set; }
    public virtual DbSet<Fielding> Fieldings { get; set; }
    public virtual DbSet<FieldingOf> FieldingOfs { get; set; }
    public virtual DbSet<FieldingOfsplit> FieldingOfsplits { get; set; }
    public virtual DbSet<FieldingPost> FieldingPosts { get; set; }
    public virtual DbSet<HallOfFame> HallOfFames { get; set; }
    public virtual DbSet<HomeGame> HomeGames { get; set; }
    public virtual DbSet<Manager> Managers { get; set; }
    public virtual DbSet<ManagersHalf> ManagersHalves { get; set; }
    public virtual DbSet<Park> Parks { get; set; }
    public virtual DbSet<Person> People { get; set; }
    public virtual DbSet<Pitching> Pitchings { get; set; }
    public virtual DbSet<PitchingPost> PitchingPosts { get; set; }
    public virtual DbSet<PlayerBattingTotal> PlayerBattingTotals { get; set; }
    public virtual DbSet<PlayerFieldingTotal> PlayerFieldingTotals { get; set; }
    public virtual DbSet<PlayerPitchingTotal> PlayerPitchingTotals { get; set; }
    public virtual DbSet<Salary> Salaries { get; set; }
    public virtual DbSet<School> Schools { get; set; }
    public virtual DbSet<SeriesPost> SeriesPosts { get; set; }
    public virtual DbSet<Team> Teams { get; set; }
    public virtual DbSet<TeamBattingTotal> TeamBattingTotals { get; set; }
    public virtual DbSet<TeamFieldingTotal> TeamFieldingTotals { get; set; }
    public virtual DbSet<TeamPitchingTotal> TeamPitchingTotals { get; set; }
    public virtual DbSet<TeamsFranchise> TeamsFranchises { get; set; }
    public virtual DbSet<TeamsHalf> TeamsHalves { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AllstarFull>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.GameId });

            entity.ToTable("AllstarFull");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.GameId)
                .HasMaxLength(12)
                .HasColumnName("gameID");
            entity.Property(e => e.GameNum).HasColumnName("gameNum");
            entity.Property(e => e.Gp).HasColumnName("GP");
            entity.Property(e => e.StartingPos).HasColumnName("startingPos");

            entity.HasOne(d => d.Player).WithMany(p => p.AllstarFulls)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllstarFull_People");

            entity.HasOne(d => d.Team).WithMany(p => p.AllstarFulls)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllstarFull_Teams");
        });

        modelBuilder.Entity<Appearance>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId });

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.G1b).HasColumnName("G_1b");
            entity.Property(e => e.G2b).HasColumnName("G_2b");
            entity.Property(e => e.G3b).HasColumnName("G_3b");
            entity.Property(e => e.GAll).HasColumnName("G_all");
            entity.Property(e => e.GBatting).HasColumnName("G_batting");
            entity.Property(e => e.GC).HasColumnName("G_c");
            entity.Property(e => e.GCf).HasColumnName("G_cf");
            entity.Property(e => e.GDefense).HasColumnName("G_defense");
            entity.Property(e => e.GDh).HasColumnName("G_dh");
            entity.Property(e => e.GLf).HasColumnName("G_lf");
            entity.Property(e => e.GOf).HasColumnName("G_of");
            entity.Property(e => e.GP).HasColumnName("G_p");
            entity.Property(e => e.GPh).HasColumnName("G_ph");
            entity.Property(e => e.GPr).HasColumnName("G_pr");
            entity.Property(e => e.GRf).HasColumnName("G_rf");
            entity.Property(e => e.GSs).HasColumnName("G_ss");
            entity.Property(e => e.Gs).HasColumnName("GS");

            entity.HasOne(d => d.Player).WithMany(p => p.Appearances)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appearances_People");

            entity.HasOne(d => d.Team).WithMany(p => p.Appearances)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appearances_Teams");
        });

        modelBuilder.Entity<AwardsManager>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.LgId, e.YearId, e.AwardId });

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.AwardId)
                .HasMaxLength(50)
                .HasColumnName("awardID");
            entity.Property(e => e.Notes)
                .HasMaxLength(100)
                .HasColumnName("notes");
            entity.Property(e => e.Tie)
                .HasMaxLength(1)
                .HasColumnName("tie");

            entity.HasOne(d => d.Player).WithMany(p => p.AwardsManagers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AwardsManagers_People");
        });

        modelBuilder.Entity<AwardsPlayer>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.LgId, e.YearId, e.AwardId });

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.AwardId)
                .HasMaxLength(25)
                .HasColumnName("awardID");
            entity.Property(e => e.Notes)
                .HasMaxLength(50)
                .HasColumnName("notes");
            entity.Property(e => e.Tie)
                .HasMaxLength(1)
                .HasColumnName("tie");

            entity.HasOne(d => d.Player).WithMany(p => p.AwardsPlayers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AwardsPlayers_People");
        });

        modelBuilder.Entity<AwardsShareManager>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.LgId, e.YearId, e.AwardId });

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.AwardId)
                .HasMaxLength(25)
                .HasColumnName("awardID");
            entity.Property(e => e.PointsMax).HasColumnName("pointsMax");
            entity.Property(e => e.PointsWon).HasColumnName("pointsWon");
            entity.Property(e => e.VotesFirst).HasColumnName("votesFirst");

            entity.HasOne(d => d.Player).WithMany(p => p.AwardsShareManagers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AwardsShareManagers_People");
        });

        modelBuilder.Entity<AwardsSharePlayer>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.LgId, e.YearId, e.AwardId });

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.AwardId)
                .HasMaxLength(25)
                .HasColumnName("awardID");
            entity.Property(e => e.PointsMax).HasColumnName("pointsMax");
            entity.Property(e => e.PointsWon).HasColumnName("pointsWon");
            entity.Property(e => e.VotesFirst).HasColumnName("votesFirst");

            entity.HasOne(d => d.Player).WithMany(p => p.AwardsSharePlayers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AwardsSharePlayers_People");
        });

        modelBuilder.Entity<Batting>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Stint });

            entity.ToTable("Batting");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Stint).HasColumnName("stint");
            entity.Property(e => e.Ab).HasColumnName("AB");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Hbp).HasColumnName("HBP");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.Rbi).HasColumnName("RBI");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e._2b).HasColumnName("2B");
            entity.Property(e => e._3b).HasColumnName("3B");

            entity.HasOne(d => d.Player).WithMany(p => p.Battings)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Batting_People");

            entity.HasOne(d => d.Team).WithMany(p => p.Battings)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Batting_Teams");
        });

        modelBuilder.Entity<BattingPost>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Round });

            entity.ToTable("BattingPost");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Round)
                .HasMaxLength(10)
                .HasColumnName("round");
            entity.Property(e => e.Ab).HasColumnName("AB");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Hbp).HasColumnName("HBP");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.Rbi).HasColumnName("RBI");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e._2b).HasColumnName("2B");
            entity.Property(e => e._3b).HasColumnName("3B");

            entity.HasOne(d => d.Player).WithMany(p => p.BattingPosts)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BattingPost_People");

            entity.HasOne(d => d.Team).WithMany(p => p.BattingPosts)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BattingPost_Teams");
        });

        modelBuilder.Entity<CollegePlaying>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.SchoolId });

            entity.ToTable("CollegePlaying");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.SchoolId)
                .HasMaxLength(15)
                .HasColumnName("schoolID");

            entity.HasOne(d => d.Player).WithMany(p => p.CollegePlayings)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollegePlaying_People");

            entity.HasOne(d => d.School).WithMany(p => p.CollegePlayings)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollegePlaying_Schools");
        });

        modelBuilder.Entity<Fielding>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Stint, e.Pos });

            entity.ToTable("Fielding");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Stint).HasColumnName("stint");
            entity.Property(e => e.Pos)
                .HasMaxLength(2)
                .HasColumnName("POS");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Dp).HasColumnName("DP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Pb).HasColumnName("PB");
            entity.Property(e => e.Po).HasColumnName("PO");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Wp).HasColumnName("WP");
            entity.Property(e => e.Zr).HasColumnName("ZR");

            entity.HasOne(d => d.Player).WithMany(p => p.Fieldings)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fielding_People");

            entity.HasOne(d => d.Team).WithMany(p => p.Fieldings)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fielding_Teams");
        });

        modelBuilder.Entity<FieldingOf>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.Stint });

            entity.ToTable("FieldingOF");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Stint).HasColumnName("stint");

            entity.HasOne(d => d.Player).WithMany(p => p.FieldingOfs)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FieldingOF_People");
        });

        modelBuilder.Entity<FieldingOfsplit>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Stint, e.Pos });

            entity.ToTable("FieldingOFsplit");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Stint).HasColumnName("stint");
            entity.Property(e => e.Pos)
                .HasMaxLength(2)
                .HasColumnName("POS");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Dp).HasColumnName("DP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Pb).HasColumnName("PB");
            entity.Property(e => e.Po).HasColumnName("PO");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Wp).HasColumnName("WP");
            entity.Property(e => e.Zr).HasColumnName("ZR");

            entity.HasOne(d => d.Player).WithMany(p => p.FieldingOfsplits)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FieldingOFsplit_People");

            entity.HasOne(d => d.Team).WithMany(p => p.FieldingOfsplits)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FieldingOFsplit_Teams");
        });

        modelBuilder.Entity<FieldingPost>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Round, e.Pos });

            entity.ToTable("FieldingPost");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Round)
                .HasMaxLength(10)
                .HasColumnName("round");
            entity.Property(e => e.Pos)
                .HasMaxLength(2)
                .HasColumnName("POS");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Dp).HasColumnName("DP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Pb).HasColumnName("PB");
            entity.Property(e => e.Po).HasColumnName("PO");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Tp).HasColumnName("TP");

            entity.HasOne(d => d.Player).WithMany(p => p.FieldingPosts)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FieldingPost_People");

            entity.HasOne(d => d.Team).WithMany(p => p.FieldingPosts)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FieldingPost_Teams");
        });

        modelBuilder.Entity<HallOfFame>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.YearId, e.VotedBy });

            entity.ToTable("HallOfFame");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.VotedBy)
                .HasMaxLength(64)
                .HasColumnName("votedBy");
            entity.Property(e => e.Ballots).HasColumnName("ballots");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .HasColumnName("category");
            entity.Property(e => e.Inducted)
                .HasMaxLength(1)
                .HasColumnName("inducted");
            entity.Property(e => e.Needed).HasColumnName("needed");
            entity.Property(e => e.NeededNote)
                .HasMaxLength(25)
                .HasColumnName("needed_note");
            entity.Property(e => e.Votes).HasColumnName("votes");

            entity.HasOne(d => d.Player).WithMany(p => p.HallOfFames)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HallOfFame_People");
        });

        modelBuilder.Entity<HomeGame>(entity =>
        {
            entity.HasKey(e => new { e.YearId, e.LgId, e.TeamId, e.ParkId });

            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.ParkId)
                .HasMaxLength(6)
                .HasColumnName("parkID");
            entity.Property(e => e.Attendance).HasColumnName("attendance");
            entity.Property(e => e.Games).HasColumnName("games");
            entity.Property(e => e.Openings).HasColumnName("openings");
            entity.Property(e => e.SpanFirst).HasColumnName("spanFirst");
            entity.Property(e => e.SpanLast).HasColumnName("spanLast");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Inseason });

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Inseason).HasColumnName("inseason");
            entity.Property(e => e.PlyrMgr)
                .HasMaxLength(1)
                .HasColumnName("plyrMgr");
            entity.Property(e => e.Rank).HasColumnName("rank");

            entity.HasOne(d => d.Player).WithMany(p => p.Managers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Managers_People");

            entity.HasOne(d => d.Team).WithMany(p => p.Managers)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Managers_Teams");
        });

        modelBuilder.Entity<ManagersHalf>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Inseason, e.Half });

            entity.ToTable("ManagersHalf");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Inseason).HasColumnName("inseason");
            entity.Property(e => e.Half).HasColumnName("half");
            entity.Property(e => e.Rank).HasColumnName("rank");

            entity.HasOne(d => d.Player).WithMany(p => p.ManagersHalves)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ManagersHalf_People");

            entity.HasOne(d => d.Team).WithMany(p => p.ManagersHalves)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ManagersHalf_Teams");
        });

        modelBuilder.Entity<Park>(entity =>
        {
            entity.Property(e => e.ParkId)
                .HasMaxLength(6)
                .HasColumnName("parkID");
            entity.Property(e => e.Alias)
                .HasMaxLength(50)
                .HasColumnName("alias");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .HasColumnName("state");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.PlayerId);

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.Bats)
                .HasMaxLength(2)
                .HasColumnName("bats");
            entity.Property(e => e.BirthCity)
                .HasMaxLength(255)
                .HasColumnName("birthCity");
            entity.Property(e => e.BirthCountry)
                .HasMaxLength(255)
                .HasColumnName("birthCountry");
            entity.Property(e => e.BirthDay).HasColumnName("birthDay");
            entity.Property(e => e.BirthMonth).HasColumnName("birthMonth");
            entity.Property(e => e.BirthState)
                .HasMaxLength(255)
                .HasColumnName("birthState");
            entity.Property(e => e.BirthYear).HasColumnName("birthYear");
            entity.Property(e => e.DeathCity)
                .HasMaxLength(255)
                .HasColumnName("deathCity");
            entity.Property(e => e.DeathCountry)
                .HasMaxLength(255)
                .HasColumnName("deathCountry");
            entity.Property(e => e.DeathDay).HasColumnName("deathDay");
            entity.Property(e => e.DeathMonth).HasColumnName("deathMonth");
            entity.Property(e => e.DeathState)
                .HasMaxLength(255)
                .HasColumnName("deathState");
            entity.Property(e => e.DeathYear).HasColumnName("deathYear");
            entity.Property(e => e.Debut).HasColumnName("debut");
            entity.Property(e => e.FinalGame).HasColumnName("finalGame");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.NameFirst)
                .HasMaxLength(255)
                .HasColumnName("nameFirst");
            entity.Property(e => e.NameGiven)
                .HasMaxLength(255)
                .HasColumnName("nameGiven");
            entity.Property(e => e.NameLast)
                .HasMaxLength(255)
                .HasColumnName("nameLast");
            entity.Property(e => e.Throws)
                .HasMaxLength(2)
                .HasColumnName("throws");
            entity.Property(e => e.Weight).HasColumnName("weight");
        });

        modelBuilder.Entity<Pitching>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Stint });

            entity.ToTable("Pitching");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Stint).HasColumnName("stint");
            entity.Property(e => e.Baopp).HasColumnName("BAOpp");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Bfp).HasColumnName("BFP");
            entity.Property(e => e.Bk).HasColumnName("BK");
            entity.Property(e => e.Cg).HasColumnName("CG");
            entity.Property(e => e.Er).HasColumnName("ER");
            entity.Property(e => e.Era).HasColumnName("ERA");
            entity.Property(e => e.Gf).HasColumnName("GF");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Hbp).HasColumnName("HBP");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.Ipouts).HasColumnName("IPouts");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.Sho).HasColumnName("SHO");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e.Sv).HasColumnName("SV");
            entity.Property(e => e.Wp).HasColumnName("WP");

            entity.HasOne(d => d.Player).WithMany(p => p.Pitchings)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pitching_People");

            entity.HasOne(d => d.Team).WithMany(p => p.Pitchings)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pitching_Teams");
        });

        modelBuilder.Entity<PitchingPost>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId, e.Round });

            entity.ToTable("PitchingPost");

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Round)
                .HasMaxLength(10)
                .HasColumnName("round");
            entity.Property(e => e.Baopp).HasColumnName("BAOpp");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Bfp).HasColumnName("BFP");
            entity.Property(e => e.Bk).HasColumnName("BK");
            entity.Property(e => e.Cg).HasColumnName("CG");
            entity.Property(e => e.Er).HasColumnName("ER");
            entity.Property(e => e.Era).HasColumnName("ERA");
            entity.Property(e => e.Gf).HasColumnName("GF");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Hbp).HasColumnName("HBP");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.Ipouts).HasColumnName("IPouts");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.Sho).HasColumnName("SHO");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e.Sv).HasColumnName("SV");
            entity.Property(e => e.Wp).HasColumnName("WP");

            entity.HasOne(d => d.Player).WithMany(p => p.PitchingPosts)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PitchingPost_People");

            entity.HasOne(d => d.Team).WithMany(p => p.PitchingPosts)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PitchingPost_Teams");
        });

        modelBuilder.Entity<PlayerBattingTotal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("PlayerBattingTotals");

            entity.Property(e => e.Ab).HasColumnName("AB");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Hpb).HasColumnName("HPB");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.Rbi).HasColumnName("RBI");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e._2b).HasColumnName("2B");
            entity.Property(e => e._3b).HasColumnName("3B");
        });

        modelBuilder.Entity<PlayerFieldingTotal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("PlayerFieldingTotals");

            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Dp).HasColumnName("DP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Pb).HasColumnName("PB");
            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.Po).HasColumnName("PO");
            entity.Property(e => e.Pos)
                .HasMaxLength(2)
                .HasColumnName("POS");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Wp).HasColumnName("WP");
            entity.Property(e => e.Zr).HasColumnName("ZR");
        });

        modelBuilder.Entity<PlayerPitchingTotal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("PlayerPitchingTotals");

            entity.Property(e => e.Baopp).HasColumnName("BAOpp");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Bfp).HasColumnName("BFP");
            entity.Property(e => e.Bk).HasColumnName("BK");
            entity.Property(e => e.Cg).HasColumnName("CG");
            entity.Property(e => e.Er).HasColumnName("ER");
            entity.Property(e => e.Gf).HasColumnName("GF");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Hbp).HasColumnName("HBP");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.Ipouts).HasColumnName("IPouts");
            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.Sho).HasColumnName("SHO");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e.Sv).HasColumnName("SV");
            entity.Property(e => e.Wp).HasColumnName("WP");
        });

        modelBuilder.Entity<Salary>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.TeamId, e.LgId, e.YearId });

            entity.Property(e => e.PlayerId)
                .HasMaxLength(9)
                .HasColumnName("playerID");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Salary1).HasColumnName("salary");

            entity.HasOne(d => d.Player).WithMany(p => p.Salaries)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Salaries_People");

            entity.HasOne(d => d.Team).WithMany(p => p.Salaries)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Salaries_Teams");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.Property(e => e.SchoolId)
                .HasMaxLength(15)
                .HasColumnName("schoolID");
            entity.Property(e => e.City)
                .HasMaxLength(55)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(55)
                .HasColumnName("country");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.State)
                .HasMaxLength(55)
                .HasColumnName("state");
        });

        modelBuilder.Entity<SeriesPost>(entity =>
        {
            entity.HasKey(e => new { e.TeamIdwinner, e.LgIdwinner, e.YearId, e.Round });

            entity.ToTable("SeriesPost");

            entity.HasIndex(e => new { e.TeamIdloser, e.LgIdloser, e.YearId, e.Round }, "IX_SeriesPostLoser").IsUnique();

            entity.Property(e => e.TeamIdwinner)
                .HasMaxLength(3)
                .HasColumnName("teamIDwinner");
            entity.Property(e => e.LgIdwinner)
                .HasMaxLength(2)
                .HasColumnName("lgIDwinner");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Round)
                .HasMaxLength(5)
                .HasColumnName("round");
            entity.Property(e => e.LgIdloser)
                .HasMaxLength(2)
                .HasColumnName("lgIDloser");
            entity.Property(e => e.Losses).HasColumnName("losses");
            entity.Property(e => e.TeamIdloser)
                .HasMaxLength(3)
                .HasColumnName("teamIDloser");
            entity.Property(e => e.Ties).HasColumnName("ties");
            entity.Property(e => e.Wins).HasColumnName("wins");

            entity.HasOne(d => d.Team).WithMany(p => p.SeriesPostTeams)
                .HasForeignKey(d => new { d.TeamIdloser, d.LgIdloser, d.YearId })
                .HasConstraintName("FK_SeriesPost_Teams1");

            entity.HasOne(d => d.TeamNavigation).WithMany(p => p.SeriesPostTeamNavigations)
                .HasForeignKey(d => new { d.TeamIdwinner, d.LgIdwinner, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeriesPost_Teams");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.LgId, e.YearId });

            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Ab).HasColumnName("AB");
            entity.Property(e => e.Attendance).HasColumnName("attendance");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Bba).HasColumnName("BBA");
            entity.Property(e => e.Bpf).HasColumnName("BPF");
            entity.Property(e => e.Cg).HasColumnName("CG");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.DivId)
                .HasMaxLength(1)
                .HasColumnName("divID");
            entity.Property(e => e.DivWin).HasMaxLength(1);
            entity.Property(e => e.Dp).HasColumnName("DP");
            entity.Property(e => e.Er).HasColumnName("ER");
            entity.Property(e => e.Era).HasColumnName("ERA");
            entity.Property(e => e.Fp).HasColumnName("FP");
            entity.Property(e => e.FranchId)
                .HasMaxLength(3)
                .HasColumnName("franchID");
            entity.Property(e => e.Ha).HasColumnName("HA");
            entity.Property(e => e.Hbp).HasColumnName("HBP");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Hra).HasColumnName("HRA");
            entity.Property(e => e.Ipouts).HasColumnName("IPouts");
            entity.Property(e => e.LgWin).HasMaxLength(1);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Park)
                .HasMaxLength(255)
                .HasColumnName("park");
            entity.Property(e => e.ParkId)
                .HasMaxLength(6)
                .HasColumnName("parkID");
            entity.Property(e => e.Ppf).HasColumnName("PPF");
            entity.Property(e => e.Ra).HasColumnName("RA");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sho).HasColumnName("SHO");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e.Soa).HasColumnName("SOA");
            entity.Property(e => e.Sv).HasColumnName("SV");
            entity.Property(e => e.Wcwin)
                .HasMaxLength(1)
                .HasColumnName("WCWin");
            entity.Property(e => e.Wswin)
                .HasMaxLength(1)
                .HasColumnName("WSWin");
            entity.Property(e => e._2b).HasColumnName("2B");
            entity.Property(e => e._3b).HasColumnName("3B");

            entity.HasOne(d => d.Franch).WithMany(p => p.Teams)
                .HasForeignKey(d => d.FranchId)
                .HasConstraintName("FK_Teams_TeamsFranchises");

            entity.HasOne(d => d.ParkNavigation).WithMany(p => p.Teams)
                .HasForeignKey(d => d.ParkId)
                .HasConstraintName("FK_Teams_Parks");
        });

        modelBuilder.Entity<TeamBattingTotal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TeamBattingTotals");

            entity.Property(e => e.Ab).HasColumnName("AB");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Hpb).HasColumnName("HPB");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.Rbi).HasColumnName("RBI");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e._2b).HasColumnName("2B");
            entity.Property(e => e._3b).HasColumnName("3B");
        });

        modelBuilder.Entity<TeamFieldingTotal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TeamFieldingTotals");

            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Dp).HasColumnName("DP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.Pb).HasColumnName("PB");
            entity.Property(e => e.Po).HasColumnName("PO");
            entity.Property(e => e.Sb).HasColumnName("SB");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.Wp).HasColumnName("WP");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Zr).HasColumnName("ZR");
        });

        modelBuilder.Entity<TeamPitchingTotal>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TeamPitchingTotals");

            entity.Property(e => e.Baopp).HasColumnName("BAOpp");
            entity.Property(e => e.Bb).HasColumnName("BB");
            entity.Property(e => e.Bfp).HasColumnName("BFP");
            entity.Property(e => e.Bk).HasColumnName("BK");
            entity.Property(e => e.Cg).HasColumnName("CG");
            entity.Property(e => e.Er).HasColumnName("ER");
            entity.Property(e => e.Gf).HasColumnName("GF");
            entity.Property(e => e.Gidp).HasColumnName("GIDP");
            entity.Property(e => e.Gs).HasColumnName("GS");
            entity.Property(e => e.Hbp).HasColumnName("HBP");
            entity.Property(e => e.Hr).HasColumnName("HR");
            entity.Property(e => e.Ibb).HasColumnName("IBB");
            entity.Property(e => e.Ipouts).HasColumnName("IPouts");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.Sf).HasColumnName("SF");
            entity.Property(e => e.Sh).HasColumnName("SH");
            entity.Property(e => e.Sho).HasColumnName("SHO");
            entity.Property(e => e.So).HasColumnName("SO");
            entity.Property(e => e.Sv).HasColumnName("SV");
            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.Wp).HasColumnName("WP");
            entity.Property(e => e.YearId).HasColumnName("yearID");
        });

        modelBuilder.Entity<TeamsFranchise>(entity =>
        {
            entity.HasKey(e => e.FranchId);

            entity.Property(e => e.FranchId)
                .HasMaxLength(3)
                .HasColumnName("franchID");
            entity.Property(e => e.Active)
                .HasMaxLength(2)
                .HasColumnName("active");
            entity.Property(e => e.FranchName)
                .HasMaxLength(50)
                .HasColumnName("franchName");
            entity.Property(e => e.Naassoc)
                .HasMaxLength(3)
                .HasColumnName("NAassoc");
        });

        modelBuilder.Entity<TeamsHalf>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.LgId, e.YearId, e.Half });

            entity.ToTable("TeamsHalf");

            entity.Property(e => e.TeamId)
                .HasMaxLength(3)
                .HasColumnName("teamID");
            entity.Property(e => e.LgId)
                .HasMaxLength(2)
                .HasColumnName("lgID");
            entity.Property(e => e.YearId).HasColumnName("yearID");
            entity.Property(e => e.Half).HasMaxLength(1);
            entity.Property(e => e.DivId)
                .HasMaxLength(1)
                .HasColumnName("divID");
            entity.Property(e => e.DivWin).HasMaxLength(1);

            entity.HasOne(d => d.Team).WithMany(p => p.TeamsHalves)
                .HasForeignKey(d => new { d.TeamId, d.LgId, d.YearId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeamsHalf_Teams");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
