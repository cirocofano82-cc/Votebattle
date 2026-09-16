using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Payments;

public class CreateCheckoutRequest
{
    [Required]
    public int VotePackageId { get; set; }
}
