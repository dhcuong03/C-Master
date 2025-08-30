using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class AnswerOption
{
    [Key]
    [Column("option_id")]
    public int OptionId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("option_text")]
    public string OptionText { get; set; } = null!;

    // === SỬA LỖI: Đổi từ bool? thành bool ===
    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    [Column("match_id")]
    public int? MatchId { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("AnswerOptions")]
    public virtual Question Question { get; set; } = null!;

    [InverseProperty("ChosenOption")]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
