using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class AuditLog
{
    [Key]
    [Column("log_id")]
    public int LogId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("action")]
    [StringLength(255)]
    public string Action { get; set; } = null!;

    [Column("target_type")]
    [StringLength(50)]
    public string? TargetType { get; set; }

    [Column("target_id")]
    public int? TargetId { get; set; }

    [Column("details")]
    public string? Details { get; set; }

    [Column("log_time")]
    public DateTime? LogTime { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AuditLogs")]
    public virtual User? User { get; set; }
}
