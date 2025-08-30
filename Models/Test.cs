using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class Test
{
    [Key]
    [Column("test_id")]
    public int TestId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; }

    [Column("passing_score", TypeName = "decimal(5, 2)")]
    public decimal PassingScore { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trình độ cho bài test.")]
    [Column("level_id")]
    public int? LevelId { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Tests")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Test")]
    public virtual ICollection<TestAssignment> TestAssignments { get; set; } = new List<TestAssignment>();

    [InverseProperty("Test")]
    public virtual ICollection<UserTestSession> UserTestSessions { get; set; } = new List<UserTestSession>();

    [InverseProperty("Test")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [ForeignKey("TestId")]
    [InverseProperty("Tests")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [ForeignKey("LevelId")]
    // === SỬA LỖI: Cho phép thuộc tính Level có thể null ===
    public virtual Level? Level { get; set; }
}
