using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Identity.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}