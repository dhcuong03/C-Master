using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class EmployeeAssessmentContext : DbContext
{
    public EmployeeAssessmentContext()
    {
    }

    public EmployeeAssessmentContext(DbContextOptions<EmployeeAssessmentContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<TestAssignment> TestAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserTestSession> UserTestSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Program;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__AnswerOp__F4EACE1B3F78314F");

            entity.Property(e => e.IsCorrect).HasDefaultValue(false);

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions).HasConstraintName("FK__AnswerOpt__quest__5629CD9C");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__9E2397E072B8516E");

            entity.Property(e => e.LogTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs).HasConstraintName("FK__AuditLogs__user___787EE5A0");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C2232422A93CF3E4");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__7A6B2B8C820CF398");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PK__Levels__03461643A6A3A106");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__2EC215492E95801C");

            entity.ToTable(tb => tb.HasTrigger("trg_Questions_Update"));

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Questions).HasConstraintName("FK__Questions__creat__5165187F");

            entity.HasOne(d => d.Skill).WithMany(p => p.Questions).HasConstraintName("FK__Questions__skill__5070F446");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__760965CCF08437ED");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Skills__FBBA83795E6B6217");
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.ConfigKey).HasName("PK__SystemCo__BDF6033C006EE312");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Tests__F3FF1C0246DCF165");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tests).HasConstraintName("FK__Tests__created_b__59FA5E80");

            entity.HasMany(d => d.Questions).WithMany(p => p.Tests)
                .UsingEntity<Dictionary<string, object>>(
                    "TestQuestion",
                    r => r.HasOne<Question>().WithMany()
                        .HasForeignKey("QuestionId")
                        .HasConstraintName("FK__TestQuest__quest__5DCAEF64"),
                    l => l.HasOne<Test>().WithMany()
                        .HasForeignKey("TestId")
                        .HasConstraintName("FK__TestQuest__test___5CD6CB2B"),
                    j =>
                    {
                        j.HasKey("TestId", "QuestionId").HasName("PK__TestQues__71133D5699F7479C");
                        j.ToTable("TestQuestions");
                        j.IndexerProperty<int>("TestId").HasColumnName("test_id");
                        j.IndexerProperty<int>("QuestionId").HasColumnName("question_id");
                    });
        });

        modelBuilder.Entity<TestAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__TestAssi__DA891814AE1FA72E");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.TestAssignmentAssignedByNavigations).HasConstraintName("FK__TestAssig__assig__6477ECF3");

            entity.HasOne(d => d.Department).WithMany(p => p.TestAssignments).HasConstraintName("FK__TestAssig__depar__6383C8BA");

            entity.HasOne(d => d.Test).WithMany(p => p.TestAssignments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestAssig__test___619B8048");

            entity.HasOne(d => d.User).WithMany(p => p.TestAssignmentUsers).HasConstraintName("FK__TestAssig__user___628FA481");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FA0D1BDB3");

            entity.ToTable(tb => tb.HasTrigger("trg_Users_Update"));

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Department).WithMany(p => p.Users).HasConstraintName("FK__Users__departmen__00200768");

            entity.HasOne(d => d.Level).WithMany(p => p.Users).HasConstraintName("FK__Users__level_id__01142BA1");

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("FK__Users__role_id__02084FDA");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("PK__UserAnsw__6BBB07503291EA6A");

            entity.HasOne(d => d.ChosenOption).WithMany(p => p.UserAnswers).HasConstraintName("FK__UserAnswe__chose__6EF57B66");

            entity.HasOne(d => d.GradedByNavigation).WithMany(p => p.UserAnswers).HasConstraintName("FK__UserAnswe__grade__6FE99F9F");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAnswe__quest__6E01572D");

            entity.HasOne(d => d.Session).WithMany(p => p.UserAnswers).HasConstraintName("FK__UserAnswe__sessi__6D0D32F4");
        });

        modelBuilder.Entity<UserTestSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__UserTest__69B13FDC49934A5E");

            entity.Property(e => e.Status).HasDefaultValue("NOT_STARTED");

            entity.HasOne(d => d.Test).WithMany(p => p.UserTestSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserTestS__test___6A30C649");

            entity.HasOne(d => d.User).WithMany(p => p.UserTestSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserTestS__user___693CA210");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
