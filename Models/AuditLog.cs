using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class AuditLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? TargetType { get; set; }

    public int? TargetId { get; set; }

    public string? Details { get; set; }

    public DateTime? LogTime { get; set; }

    public virtual User? User { get; set; }
}
