using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class Test
{
    public int TestId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal PassingScore { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<TestAssignment> TestAssignments { get; set; } = new List<TestAssignment>();

    public virtual ICollection<UserTestSession> UserTestSessions { get; set; } = new List<UserTestSession>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
