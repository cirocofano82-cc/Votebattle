using VoteBattle.Core.DTOs.Admin;

namespace VoteBattle.Core.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<SecurityOverviewDto> GetSecurityOverviewAsync(CancellationToken ct = default);
}
