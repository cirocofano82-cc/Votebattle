using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Comments;

public class CreateCommentRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 3,
        ErrorMessage = "Comments must be between 3 and 1000 characters.")]
    public string Content { get; set; } = string.Empty;
}
