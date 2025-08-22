using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class UserTestSession
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public int TestId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string Status { get; set; } = null!;

    public decimal? FinalScore { get; set; }

    public bool? IsPassed { get; set; }

    public virtual Test Test { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
