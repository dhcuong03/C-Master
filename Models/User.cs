using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

[Index("Email", Name = "UQ__Users__AB6E6164A5F57578", IsUnique = true)]
[Index("Username", Name = "UQ__Users__F3DBC5721C180112", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("full_name")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("level_id")]
    public int? LevelId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? ResetTokenExpires { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [ForeignKey("DepartmentId")]
    [InverseProperty("Users")]
    public virtual Department? Department { get; set; }

    [ForeignKey("LevelId")]
    [InverseProperty("Users")]
    public virtual Level? Level { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role? Role { get; set; }

    [InverseProperty("AssignedByNavigation")]
    public virtual ICollection<TestAssignment> TestAssignmentAssignedByNavigations { get; set; } = new List<TestAssignment>();

    [InverseProperty("User")]
    public virtual ICollection<TestAssignment> TestAssignmentUsers { get; set; } = new List<TestAssignment>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    [InverseProperty("GradedByNavigation")]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    [InverseProperty("User")]
    public virtual ICollection<UserTestSession> UserTestSessions { get; set; } = new List<UserTestSession>();

    // === DÒNG MỚI CẦN THÊM VÀO ĐÂY ===
    [InverseProperty("User")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
