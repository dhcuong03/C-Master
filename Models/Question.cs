using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class Question
{
    [Key]
    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("question_type")]
    [StringLength(20)]
    public string QuestionType { get; set; } = null!;

    [Column("skill_id")]
    public int? SkillId { get; set; }

    [Column("difficulty")]
    [StringLength(10)]
    public string Difficulty { get; set; } = null!;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Question")]
    public virtual List<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("Questions")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("SkillId")]
    [InverseProperty("Questions")]
    public virtual Skill? Skill { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    [ForeignKey("QuestionId")]
    [InverseProperty("Questions")]
    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();


}
