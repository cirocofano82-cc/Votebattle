using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Admin;

public class VotePackageAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int Credits { get; set; }
    public bool IsActive { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateVotePackageRequest
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";

    [Range(1, 1000000)]
    public int Credits { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateVotePackageRequest
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";

    [Range(1, 1000000)]
    public int Credits { get; set; }

    public bool IsActive { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
}
