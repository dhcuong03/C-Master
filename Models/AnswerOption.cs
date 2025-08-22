using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class AnswerOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    // THAY ĐỔI DUY NHẤT Ở ĐÂY: Bỏ dấu chấm hỏi (?) để đổi từ bool? sang bool
    public bool IsCorrect { get; set; }

    public int? MatchId { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
