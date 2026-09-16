using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Votes;

namespace VoteBattle.Core.Interfaces;

public interface IVoteService
{
    /// <summary>
    /// Casts one vote, spending exactly one credit inside an atomic transaction.
    /// Fails (without side effects) when the user has no credits.
    /// </summary>
    Task<Result<VoteResultDto>> CastVoteAsync(Guid userId, CreateVoteRequest request, string? ipAddress, CancellationToken ct = default);

    Task<PagedResult<UserVoteDto>> GetUserVotesAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
