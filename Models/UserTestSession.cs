using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class UserTestSession
{
    [Key]
    [Column("session_id")]
    public int SessionId { get; set; }

    // === SỬA LỖI: Cho phép UserId có thể null ===
    [Column("user_id")]
    public int? UserId { get; set; }

    // === SỬA LỖI: Cho phép TestId có thể null ===
    [Column("test_id")]
    public int? TestId { get; set; }

    [Column("start_time")]
    public DateTime? StartTime { get; set; }

    [Column("end_time")]
    public DateTime? EndTime { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("final_score", TypeName = "decimal(5, 2)")]
    public decimal? FinalScore { get; set; }

    [Column("is_passed")]
    public bool? IsPassed { get; set; }

    [ForeignKey("TestId")]
    [InverseProperty("UserTestSessions")]
    public virtual Test? Test { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserTestSessions")]
    public virtual User? User { get; set; }

    [InverseProperty("Session")]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
