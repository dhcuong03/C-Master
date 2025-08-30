using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class Feedback
{
    [Key]
    [Column("feedback_id")]
    public int FeedbackId { get; set; }

    // === SỬA LỖI: Cho phép UserId có thể null ===
    [Column("user_id")]
    public int? UserId { get; set; }

    // === SỬA LỖI: Cho phép TestId có thể null ===
    [Column("test_id")]
    public int? TestId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "New";

    [ForeignKey("TestId")]
    [InverseProperty("Feedbacks")]
    // === SỬA LỖI: Cho phép Test có thể null ===
    public virtual Test? Test { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Feedbacks")]
    // === SỬA LỖI: Cho phép User có thể null ===
    public virtual User? User { get; set; }
}
