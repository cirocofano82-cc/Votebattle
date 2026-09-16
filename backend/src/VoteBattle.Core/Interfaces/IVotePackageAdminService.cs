using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Admin;

namespace VoteBattle.Core.Interfaces;

public interface IVotePackageAdminService
{
    Task<IReadOnlyList<VotePackageAdminDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result<VotePackageAdminDto>> CreateAsync(CreateVotePackageRequest request, CancellationToken ct = default);
    Task<Result<VotePackageAdminDto>> UpdateAsync(int id, UpdateVotePackageRequest request, CancellationToken ct = default);

    /// <summary>Deactivates a package (soft). Historical payments are never modified.</summary>
    Task<Result> DeactivateAsync(int id, CancellationToken ct = default);
}
