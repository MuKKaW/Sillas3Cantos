namespace SillasTresCantos.Api.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    public string ConnectionStringName { get; set; } = "DefaultConnection";
}
