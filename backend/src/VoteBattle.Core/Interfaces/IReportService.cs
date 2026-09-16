using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Reports;

namespace VoteBattle.Core.Interfaces;

public interface IReportService
{
    Task<Result> CreateReportAsync(Guid userId, CreateReportRequest request, CancellationToken ct = default);
}
