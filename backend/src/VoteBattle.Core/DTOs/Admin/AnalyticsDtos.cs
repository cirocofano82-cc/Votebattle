namespace VoteBattle.Core.DTOs.Admin;

public class TopBattleDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public decimal TotalAmountSpent { get; set; }
}

/// <summary>Business + product metrics for the admin dashboard.</summary>
public class DashboardDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueToday { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBattles { get; set; }
    public int TotalVotes { get; set; }
    public int VotesToday { get; set; }

    public int PayingUsers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageRevenuePerUser { get; set; }

    /// <summary>Share of all users who have made at least one payment (0-100).</summary>
    public double ConversionRate { get; set; }

    /// <summary>Share of bonus-receiving users who became paying users (0-100).</summary>
    public double FreeToPaidConversionRate { get; set; }

    public int FreeCreditsDistributed { get; set; }
    public int PurchasedCredits { get; set; }
    public int CreditsSpent { get; set; }

    public List<TopBattleDto> TopBattles { get; set; } = new();
}

public class SuspiciousUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Security &amp; abuse overview for the admin panel.</summary>
public class SecurityOverviewDto
{
    public int SuspiciousUsers { get; set; }
    public int BannedUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int RecentRegistrations { get; set; }   // last 7 days
    public int FailedLogins24h { get; set; }
    public int RegistrationBonusesGranted { get; set; }
    public List<SuspiciousUserDto> RecentUsers { get; set; } = new();
}
