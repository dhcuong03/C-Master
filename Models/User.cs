using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? RoleId { get; set; }

    public int? DepartmentId { get; set; }

    public int? LevelId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Level? Level { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<TestAssignment> TestAssignmentAssignedByNavigations { get; set; } = new List<TestAssignment>();

    public virtual ICollection<TestAssignment> TestAssignmentUsers { get; set; } = new List<TestAssignment>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public virtual ICollection<UserTestSession> UserTestSessions { get; set; } = new List<UserTestSession>();
}
