using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Comments;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Core.Options;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class CommentService : ICommentService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _db;
    private readonly CommentOptions _options;

    public CommentService(AppDbContext db, IOptions<CommentOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<PagedResult<CommentDto>> GetCommentsAsync(
        Guid battleId, int page, int pageSize, Guid? currentUserId, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var q = _db.Comments.AsNoTracking().Where(c => c.BattleId == battleId && !c.IsDeleted);
        var totalCount = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                BattleId = c.BattleId,
                Username = c.User!.UserName ?? "user",
                AvatarUrl = c.User.AvatarUrl,
                Content = c.Content,
                LikeCount = c.LikeCount,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        if (currentUserId is not null && rows.Count > 0)
        {
            var ids = rows.Select(r => r.Id).ToList();
            var likedIds = await _db.CommentLikes.AsNoTracking()
                .Where(l => l.UserId == currentUserId && ids.Contains(l.CommentId))
                .Select(l => l.CommentId)
                .ToListAsync(ct);
            var likedSet = likedIds.ToHashSet();
            foreach (var r in rows)
                r.LikedByMe = likedSet.Contains(r.Id);
        }

        return new PagedResult<CommentDto>(rows, page, pageSize, totalCount);
    }

    public async Task<Result<CommentDto>> CreateCommentAsync(
        Guid userId, Guid battleId, CreateCommentRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var content = request.Content.Trim();
        if (content.Length < 3 || content.Length > 1000)
            return Result<CommentDto>.Failure(ErrorType.Validation, "Comments must be between 3 and 1000 characters.");

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result<CommentDto>.Failure(ErrorType.Unauthorized, "Not authenticated.");
        if (user.Status != UserStatus.Active)
            return Result<CommentDto>.Failure(ErrorType.Forbidden, "Your account cannot comment.");

        var battle = await _db.Battles.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == battleId, ct);
        if (battle is null || battle.Status is not (BattleStatus.Active or BattleStatus.Ended))
            return Result<CommentDto>.Failure(ErrorType.NotFound, "Battle not found.");

        var now = DateTimeOffset.UtcNow;

        // Anti-spam: rate limit (minimum interval between comments).
        var rateSince = now.AddSeconds(-_options.MinIntervalSeconds);
        var postedRecently = await _db.Comments
            .AnyAsync(c => c.UserId == userId && c.CreatedAt > rateSince, ct);
        if (postedRecently)
            return Result<CommentDto>.Failure(ErrorType.TooManyRequests,
                "You're commenting too fast. Please wait a moment and try again.");

        // Anti-spam: duplicate detection.
        var dupSince = now.AddMinutes(-_options.DuplicateWindowMinutes);
        var isDuplicate = await _db.Comments
            .AnyAsync(c => c.UserId == userId && c.Content == content && c.CreatedAt > dupSince, ct);
        if (isDuplicate)
            return Result<CommentDto>.Failure(ErrorType.Conflict, "You've already posted this comment.");

        var comment = new Comment
        {
            BattleId = battleId,
            UserId = userId,
            Content = content,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);

        return Result<CommentDto>.Success(new CommentDto
        {
            Id = comment.Id,
            BattleId = battleId,
            Username = user.UserName ?? "user",
            AvatarUrl = user.AvatarUrl,
            Content = content,
            LikeCount = 0,
            LikedByMe = false,
            CreatedAt = now
        }, "Comment posted.");
    }

    public async Task<Result<LikeResultDto>> ToggleLikeAsync(Guid userId, Guid commentId, CancellationToken ct = default)
    {
        var comment = await _db.Comments.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted, ct);
        if (comment is null)
            return Result<LikeResultDto>.Failure(ErrorType.NotFound, "Comment not found.");

        var existing = await _db.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId, ct);

        bool liked;
        if (existing is not null)
        {
            _db.CommentLikes.Remove(existing);
            await _db.SaveChangesAsync(ct);
            liked = false;
        }
        else
        {
            _db.CommentLikes.Add(new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            });
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                // Concurrent like by the same user; treat as already liked.
            }
            liked = true;
        }

        // Recompute the count from the source of truth and persist it.
        var count = await _db.CommentLikes.CountAsync(l => l.CommentId == commentId, ct);
        await _db.Comments
            .Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, count), ct);

        return Result<LikeResultDto>.Success(new LikeResultDto { Liked = liked, LikeCount = count });
    }
}
