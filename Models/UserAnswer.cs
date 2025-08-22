using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int SessionId { get; set; }

    public int QuestionId { get; set; }

    public int? ChosenOptionId { get; set; }

    public string? AnswerText { get; set; }

    public decimal? Score { get; set; }

    public string? GraderNotes { get; set; }

    public int? GradedBy { get; set; }

    public DateTime? GradedAt { get; set; }

    public virtual AnswerOption? ChosenOption { get; set; }

    public virtual User? GradedByNavigation { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual UserTestSession Session { get; set; } = null!;
}
