namespace VoteBattle.Core.Common;

/// <summary>
/// Domain-level classification of a failed operation. The API layer maps each value
/// to an HTTP status code, so Core stays free of HTTP concerns.
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,   // 400
    Unauthorized = 2, // 401
    Forbidden = 3,    // 403
    NotFound = 4,     // 404
    Conflict = 5      // 409
}
