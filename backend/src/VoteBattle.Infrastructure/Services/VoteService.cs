using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Battles;
using VoteBattle.Core.DTOs.Votes;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class VoteService : IVoteService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<VoteService> _logger;

    public VoteService(AppDbContext db, IAuditService audit, ILogger<VoteService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public async Task<Result<VoteResultDto>> CastVoteAsync(
        Guid userId, CreateVoteRequest request, string? ipAddress, CancellationToken ct = default)
    {
        // 1. Validate the acting user.
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result<VoteResultDto>.Failure(ErrorType.Unauthorized, "Not authenticated.");
        if (user.Status != UserStatus.Active)
            return Result<VoteResultDto>.Failure(ErrorType.Forbidden, "Your account cannot vote.");

        // 2. Validate the battle and the chosen contender.
        var battle = await _db.Battles.AsNoTracking()
            .Include(b => b.Participants)
            .FirstOrDefaultAsync(b => b.Id == request.BattleId, ct);
        if (battle is null)
            return Result<VoteResultDto>.Failure(ErrorType.NotFound, "Battle not found.");
        if (battle.Status != BattleStatus.Active)
            return Result<VoteResultDto>.Failure(ErrorType.Validation, "This battle is not open for voting.");
        if (battle.EndDate is not null && battle.EndDate < DateTimeOffset.UtcNow)
            return Result<VoteResultDto>.Failure(ErrorType.Validation, "This battle has ended.");

        var participant = battle.Participants.FirstOrDefault(p => p.Id == request.BattleParticipantId);
        if (participant is null)
            return Result<VoteResultDto>.Failure(ErrorType.Validation, "Invalid contender for this battle.");

        var now = DateTimeOffset.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // 3. Atomic conditional decrement. Only succeeds if the user still has a
            //    credit; the row lock serializes concurrent votes by the same user.
            var affected = await _db.Users
                .Where(u => u.Id == userId && u.VoteCredits >= 1)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.VoteCredits, u => u.VoteCredits - 1)
                    .SetProperty(u => u.UpdatedAt, now), ct);

            if (affected == 0)
            {
                await tx.RollbackAsync(ct);
                return Result<VoteResultDto>.Failure(ErrorType.InsufficientCredits,
                    "You're out of Vote Credits. Get more credits to keep voting.");
            }

            // 4. Read the resulting balance for the ledger row and the response.
            var newBalance = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.VoteCredits)
                .FirstAsync(ct);

            // 5. Ledger row (VOTE_SPENT, -1) + the vote, linked via navigation.
            var ledger = new VoteCreditTransaction
            {
                UserId = userId,
                Type = CreditTransactionType.VoteSpent,
                Amount = -1,
                BalanceAfter = newBalance,
                ReferenceType = "Battle",
                ReferenceId = battle.Id.ToString(),
                CreatedAt = now
            };
            var vote = new Vote
            {
                BattleId = battle.Id,
                BattleParticipantId = participant.Id,
                UserId = userId,
                CreditTransaction = ledger,
                CreatedAt = now
            };
            _db.VoteCreditTransactions.Add(ledger);
            _db.Votes.Add(vote);

            // 6. Update denormalized counters.
            await _db.BattleParticipants
                .Where(p => p.Id == participant.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.VoteCount, p => p.VoteCount + 1), ct);
            await _db.Battles
                .Where(b => b.Id == battle.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.TotalVotes, b => b.TotalVotes + 1)
                    .SetProperty(b => b.TotalAmountSpent, b => b.TotalAmountSpent + 1m)
                    .SetProperty(b => b.UpdatedAt, now), ct);

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            await _audit.LogAsync(AuditEventType.VoteCreated, userId, ipAddress,
                new { battleId = battle.Id, participantId = participant.Id }, ct);

            // 7. Build the up-to-date result in memory (counters were incremented by 1).
            var result = BuildResult(battle, participant.Id, newBalance);
            return Result<VoteResultDto>.Success(result, "Vote counted.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Vote transaction failed for user {UserId} on battle {BattleId}.",
                userId, request.BattleId);
            throw;
        }
    }

    public async Task<PagedResult<UserVoteDto>> GetUserVotesAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var q = _db.Votes.AsNoTracking().Where(v => v.UserId == userId);
        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new UserVoteDto
            {
                Id = v.Id,
                BattleId = v.BattleId,
                BattleTitle = v.Battle!.Title,
                BattleSlug = v.Battle.Slug,
                ParticipantName = v.BattleParticipant!.Name,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<UserVoteDto>(items, page, pageSize, totalCount);
    }

    private static VoteResultDto BuildResult(Battle battle, Guid votedParticipantId, int newBalance)
    {
        var newTotal = battle.TotalVotes + 1;
        var participants = battle.Participants
            .OrderBy(p => p.Position)
            .Select(p =>
            {
                var count = p.VoteCount + (p.Id == votedParticipantId ? 1 : 0);
                return new BattleParticipantDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Position = p.Position,
                    VoteCount = count,
                    Percentage = newTotal > 0 ? Math.Round(count * 100.0 / newTotal, 1) : 0
                };
            })
            .ToList();

        return new VoteResultDto
        {
            NewBalance = newBalance,
            BattleTotalVotes = newTotal,
            BattleTotalAmountSpent = battle.TotalAmountSpent + 1m,
            Participants = participants
        };
    }
}
