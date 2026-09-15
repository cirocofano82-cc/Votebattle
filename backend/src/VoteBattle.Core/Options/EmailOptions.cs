namespace VoteBattle.Core.Options;

/// <summary>
/// Email sending configuration (SMTP). Defaults target the local Mailhog catcher.
/// </summary>
public class EmailOptions
{
    public string FromAddress { get; set; } = "no-reply@votebattle.local";
    public string FromName { get; set; } = "VoteBattle";
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
}
