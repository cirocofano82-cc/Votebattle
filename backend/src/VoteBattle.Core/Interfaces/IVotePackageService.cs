using VoteBattle.Core.DTOs.Payments;

namespace VoteBattle.Core.Interfaces;

public interface IVotePackageService
{
    Task<IReadOnlyList<VotePackageDto>> GetActivePackagesAsync(CancellationToken ct = default);
}
