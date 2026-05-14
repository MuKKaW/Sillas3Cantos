namespace SillasTresCantos.Api.Configuration;

public class AuthOptions
{
    public const string SectionName = "Auth";
    public List<AuthUserOptions> Users { get; set; } = [];
}

public class AuthUserOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
