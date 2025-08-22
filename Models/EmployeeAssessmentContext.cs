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
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Program;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__AnswerOp__F4EACE1B3F78314F");

            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");
            entity.Property(e => e.MatchId).HasColumnName("match_id");
            entity.Property(e => e.OptionText).HasColumnName("option_text");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__AnswerOpt__quest__5629CD9C");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__9E2397E072B8516E");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(255)
                .HasColumnName("action");
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.LogTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("log_time");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .HasColumnName("target_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AuditLogs__user___787EE5A0");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C2232422A93CF3E4");

            entity.HasIndex(e => e.DepartmentName, "UQ__Departme__226ED1571B582033").IsUnique();

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .HasColumnName("department_name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__7A6B2B8C820CF398");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Test).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__test___74AE54BC");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__user___73BA3083");
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PK__Levels__03461643A6A3A106");

            entity.HasIndex(e => e.LevelName, "UQ__Levels__F94299E9ACA4381A").IsUnique();

            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.LevelName)
                .HasMaxLength(50)
                .HasColumnName("level_name");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__2EC215492E95801C");

            entity.ToTable(tb => tb.HasTrigger("trg_Questions_Update"));

            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Difficulty)
                .HasMaxLength(10)
                .HasColumnName("difficulty");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(20)
                .HasColumnName("question_type");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Questions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Questions__creat__5165187F");

            entity.HasOne(d => d.Skill).WithMany(p => p.Questions)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK__Questions__skill__5070F446");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__760965CCF08437ED");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__783254B1858A4FDB").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Skills__FBBA83795E6B6217");

            entity.HasIndex(e => e.SkillName, "UQ__Skills__73C038ADA28711FD").IsUnique();

            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.SkillName)
                .HasMaxLength(100)
                .HasColumnName("skill_name");
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.ConfigKey).HasName("PK__SystemCo__BDF6033C006EE312");

            entity.Property(e => e.ConfigKey)
                .HasMaxLength(100)
                .HasColumnName("config_key");
            entity.Property(e => e.ConfigValue).HasColumnName("config_value");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Tests__F3FF1C0246DCF165");

            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.PassingScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("passing_score");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tests)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Tests__created_b__59FA5E80");

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

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("assigned_at");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.TestAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK__TestAssig__assig__6477ECF3");

            entity.HasOne(d => d.Department).WithMany(p => p.TestAssignments)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__TestAssig__depar__6383C8BA");

            entity.HasOne(d => d.Test).WithMany(p => p.TestAssignments)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestAssig__test___619B8048");

            entity.HasOne(d => d.User).WithMany(p => p.TestAssignmentUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TestAssig__user___628FA481");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FA0D1BDB3");

            entity.ToTable(tb => tb.HasTrigger("trg_Users_Update"));

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164A5F57578").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Users__F3DBC5721C180112").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Users__departmen__44FF419A");

            entity.HasOne(d => d.Level).WithMany(p => p.Users)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK__Users__level_id__45F365D3");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__role_id__440B1D61");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("PK__UserAnsw__6BBB07503291EA6A");

            entity.Property(e => e.UserAnswerId).HasColumnName("user_answer_id");
            entity.Property(e => e.AnswerText).HasColumnName("answer_text");
            entity.Property(e => e.ChosenOptionId).HasColumnName("chosen_option_id");
            entity.Property(e => e.GradedAt).HasColumnName("graded_at");
            entity.Property(e => e.GradedBy).HasColumnName("graded_by");
            entity.Property(e => e.GraderNotes).HasColumnName("grader_notes");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");
            entity.Property(e => e.SessionId).HasColumnName("session_id");

            entity.HasOne(d => d.ChosenOption).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.ChosenOptionId)
                .HasConstraintName("FK__UserAnswe__chose__6EF57B66");

            entity.HasOne(d => d.GradedByNavigation).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.GradedBy)
                .HasConstraintName("FK__UserAnswe__grade__6FE99F9F");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAnswe__quest__6E01572D");

            entity.HasOne(d => d.Session).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__UserAnswe__sessi__6D0D32F4");
        });

        modelBuilder.Entity<UserTestSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__UserTest__69B13FDC49934A5E");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.FinalScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("final_score");
            entity.Property(e => e.IsPassed).HasColumnName("is_passed");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("NOT_STARTED")
                .HasColumnName("status");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Test).WithMany(p => p.UserTestSessions)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserTestS__test___6A30C649");

            entity.HasOne(d => d.User).WithMany(p => p.UserTestSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserTestS__user___693CA210");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
