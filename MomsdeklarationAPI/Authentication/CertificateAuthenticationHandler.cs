using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Serilog;

namespace MomsdeklarationAPI.Authentication;

public class CertificateAuthenticationHandler : AuthenticationHandler<CertificateAuthenticationOptions>
{
    private readonly ILogger _logger;

    public CertificateAuthenticationHandler(
        IOptionsMonitor<CertificateAuthenticationOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock,
        ILogger logger)
        : base(options, loggerFactory, encoder, clock)
    {
        _logger = logger;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var clientCertificate = Context.Connection.ClientCertificate;

            if (clientCertificate == null)
            {
                _logger.Warning("No client certificate provided");
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!ValidateCertificate(clientCertificate))
            {
                _logger.Warning("Invalid client certificate: {Subject}", clientCertificate.Subject);
                return Task.FromResult(AuthenticateResult.Fail("Invalid certificate"));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, clientCertificate.Subject),
                new Claim(ClaimTypes.NameIdentifier, clientCertificate.Thumbprint ?? string.Empty),
                new Claim("CertificateSerialNumber", clientCertificate.SerialNumber ?? string.Empty),
                new Claim("CertificateIssuer", clientCertificate.Issuer),
                new Claim(ClaimTypes.AuthenticationMethod, "Certificate")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.Information("Certificate authentication successful for {Subject}", clientCertificate.Subject);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Certificate authentication failed");
            return Task.FromResult(AuthenticateResult.Fail("Authentication failed"));
        }
    }

    private bool ValidateCertificate(X509Certificate2 certificate)
    {
        if (certificate == null)
            return false;

        if (DateTime.UtcNow < certificate.NotBefore || DateTime.UtcNow > certificate.NotAfter)
        {
            _logger.Warning("Certificate expired or not yet valid");
            return false;
        }

        if (!string.IsNullOrEmpty(Options.RequiredThumbprint) && 
            !string.Equals(certificate.Thumbprint, Options.RequiredThumbprint, StringComparison.OrdinalIgnoreCase))
        {
            _logger.Warning("Certificate thumbprint mismatch");
            return false;
        }

        if (!string.IsNullOrEmpty(Options.RequiredIssuer) && 
            !certificate.Issuer.Contains(Options.RequiredIssuer, StringComparison.OrdinalIgnoreCase))
        {
            _logger.Warning("Certificate issuer mismatch");
            return false;
        }

        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = Options.RevocationMode;
        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

        if (Options.TrustedIssuers.Any())
        {
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            foreach (var issuer in Options.TrustedIssuers)
            {
                chain.ChainPolicy.CustomTrustStore.Add(issuer);
            }
        }

        bool chainValid = chain.Build(certificate);

        if (!chainValid && Options.AllowSelfSignedCertificates)
        {
            var selfSignedValid = chain.ChainStatus.Length == 1 && 
                chain.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot;
            
            if (selfSignedValid)
            {
                _logger.Warning("Accepting self-signed certificate");
                return true;
            }
        }

        return chainValid;
    }
}

public class CertificateAuthenticationOptions : AuthenticationSchemeOptions
{
    public string? RequiredThumbprint { get; set; }
    public string? RequiredIssuer { get; set; }
    public X509RevocationMode RevocationMode { get; set; } = X509RevocationMode.Online;
    public bool AllowSelfSignedCertificates { get; set; } = false;
    public List<X509Certificate2> TrustedIssuers { get; set; } = new();
}

public static class CertificateAuthenticationExtensions
{
    public static AuthenticationBuilder AddCertificate(
        this AuthenticationBuilder builder,
        Action<CertificateAuthenticationOptions> configureOptions)
    {
        return builder.AddScheme<CertificateAuthenticationOptions, CertificateAuthenticationHandler>(
            "Certificate", configureOptions);
    }
}