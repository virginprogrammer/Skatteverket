namespace MomsdeklarationAPI.Configuration;

public class SkatteverketApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string TestBaseUrl { get; set; } = string.Empty;
    public bool UseTestEnvironment { get; set; } = true;
    public OAuthSettings OAuth { get; set; } = new();
    public CertificateSettings Certificate { get; set; } = new();
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}

public class OAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string TestTokenEndpoint { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TestAuthorizationEndpoint { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string GrantType { get; set; } = "client_credentials";
}

public class CertificateSettings
{
    public string Path { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Thumbprint { get; set; } = string.Empty;
    public string StoreName { get; set; } = "My";
    public string StoreLocation { get; set; } = "CurrentUser";
    public string ValidationMode { get; set; } = "ChainTrust";
}