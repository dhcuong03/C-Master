using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class TestAssignment
{
    [Key]
    [Column("assignment_id")]
    public int AssignmentId { get; set; }

    // === SỬA LỖI: Cho phép TestId có thể null để xử lý dữ liệu cũ ===
    [Column("test_id")]
    public int? TestId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("assigned_by")]
    public int? AssignedBy { get; set; }

    [Column("assigned_at")]
    public DateTime? AssignedAt { get; set; }

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    public int? LevelId { get; set; }

    [ForeignKey("AssignedBy")]
    [InverseProperty("TestAssignmentAssignedByNavigations")]
    public virtual User? AssignedByNavigation { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("TestAssignments")]
    public virtual Department? Department { get; set; }

    [ForeignKey("TestId")]
    [InverseProperty("TestAssignments")]
    public virtual Test? Test { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TestAssignmentUsers")]
    public virtual User? User { get; set; }
}
