using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

public partial class SystemConfiguration
{
    [Key]
    [Column("config_key")]
    [StringLength(100)]
    public string ConfigKey { get; set; } = null!;

    [Column("config_value")]
    public string? ConfigValue { get; set; }

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }
}
