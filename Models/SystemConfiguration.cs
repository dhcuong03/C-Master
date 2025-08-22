using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class SystemConfiguration
{
    public string ConfigKey { get; set; } = null!;

    public string? ConfigValue { get; set; }

    public string? Description { get; set; }
}
