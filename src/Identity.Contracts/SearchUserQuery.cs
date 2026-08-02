using System.ComponentModel.DataAnnotations;

namespace StarterKit.Identity.Contracts;

public record SearchUserQuery
{
    [StringLength(256)]
    public string? SearchValue { get; set; }
}
