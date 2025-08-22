using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class Question
{
    public Question()
    {
        AnswerOptions = new List<AnswerOption>();
        UserAnswers = new List<UserAnswer>();
        Tests = new List<Test>();
    }

    public int QuestionId { get; set; }
    public string Content { get; set; } = null!;
    public string QuestionType { get; set; } = null!;
    public int? SkillId { get; set; }
    public string Difficulty { get; set; } = null!;
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // THAY ĐỔI DUY NHẤT: Đổi từ ICollection sang List để form hoạt động
    public virtual List<AnswerOption> AnswerOptions { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
    public virtual Skill? Skill { get; set; }
    public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    public virtual ICollection<Test> Tests { get; set; }
}
