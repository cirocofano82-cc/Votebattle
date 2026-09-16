using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Comments;

namespace VoteBattle.Core.Interfaces;

public interface ICommentService
{
    Task<PagedResult<CommentDto>> GetCommentsAsync(
        Guid battleId, int page, int pageSize, Guid? currentUserId, CancellationToken ct = default);

    Task<Result<CommentDto>> CreateCommentAsync(
        Guid userId, Guid battleId, CreateCommentRequest request, string? ipAddress, CancellationToken ct = default);

    Task<Result<LikeResultDto>> ToggleLikeAsync(Guid userId, Guid commentId, CancellationToken ct = default);
}
