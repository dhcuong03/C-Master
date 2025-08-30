using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class UserAnswer
{
    [Key]
    [Column("user_answer_id")]
    public int UserAnswerId { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("chosen_option_id")]
    public int? ChosenOptionId { get; set; }

    [Column("answer_text")]
    public string? AnswerText { get; set; }

    [Column("score", TypeName = "decimal(5, 2)")]
    public decimal? Score { get; set; }

    [Column("grader_notes")]
    public string? GraderNotes { get; set; }

    [Column("graded_by")]
    public int? GradedBy { get; set; }

    [Column("graded_at")]
    public DateTime? GradedAt { get; set; }

    [ForeignKey("ChosenOptionId")]
    [InverseProperty("UserAnswers")]
    public virtual AnswerOption? ChosenOption { get; set; }

    [ForeignKey("GradedBy")]
    [InverseProperty("UserAnswers")]
    public virtual User? GradedByNavigation { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("UserAnswers")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("SessionId")]
    [InverseProperty("UserAnswers")]
    public virtual UserTestSession Session { get; set; } = null!;
}
