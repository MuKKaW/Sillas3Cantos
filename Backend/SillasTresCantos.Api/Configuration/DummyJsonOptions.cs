namespace SillasTresCantos.Api.Configuration;

public class DummyJsonOptions
{
    public const string SectionName = "ExternalApis:DummyJson";
    public string BaseUrl { get; set; } = "https://dummyjson.com/";
    public int TimeoutSeconds { get; set; } = 5;
}
