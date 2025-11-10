using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Utils;

public interface ICertificateValidator
{
    ValidationResult ValidateCertificate(X509Certificate2 certificate);
    ValidationResult ValidateCertificateChain(X509Certificate2 certificate);
    bool IsCertificateExpired(X509Certificate2 certificate);
    bool IsCertificateRevoked(X509Certificate2 certificate);
}

public class CertificateValidator : ICertificateValidator
{
    private readonly ILogger<CertificateValidator> _logger;

    public CertificateValidator(ILogger<CertificateValidator> logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateCertificate(X509Certificate2 certificate)
    {
        var result = new ValidationResult();

        if (certificate == null)
        {
            result.Errors.Add("Certificate is null");
            return result;
        }

        try
        {
            if (IsCertificateExpired(certificate))
            {
                result.Errors.Add("Certificate has expired");
            }

            if (!HasValidKeyUsage(certificate))
            {
                result.Errors.Add("Certificate does not have valid key usage");
            }

            if (!HasValidExtendedKeyUsage(certificate))
            {
                result.Errors.Add("Certificate does not have valid extended key usage");
            }

            var chainResult = ValidateCertificateChain(certificate);
            result.Errors.AddRange(chainResult.Errors);
            result.Warnings.AddRange(chainResult.Warnings);

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating certificate");
            result.Errors.Add($"Validation failed: {ex.Message}");
        }

        return result;
    }

    public ValidationResult ValidateCertificateChain(X509Certificate2 certificate)
    {
        var result = new ValidationResult();

        try
        {
            using var chain = new X509Chain();
            
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            chain.ChainPolicy.VerificationTime = DateTime.Now;

            bool chainIsValid = chain.Build(certificate);

            if (!chainIsValid)
            {
                foreach (var status in chain.ChainStatus)
                {
                    switch (status.Status)
                    {
                        case X509ChainStatusFlags.RevocationStatusUnknown:
                        case X509ChainStatusFlags.OfflineRevocation:
                            result.Warnings.Add($"Chain warning: {status.StatusInformation}");
                            break;
                        default:
                            result.Errors.Add($"Chain error: {status.StatusInformation}");
                            break;
                    }
                }
            }

            ValidateCertificateAuthorities(chain, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating certificate chain");
            result.Errors.Add($"Chain validation failed: {ex.Message}");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    public bool IsCertificateExpired(X509Certificate2 certificate)
    {
        var now = DateTime.UtcNow;
        return now < certificate.NotBefore || now > certificate.NotAfter;
    }

    public bool IsCertificateRevoked(X509Certificate2 certificate)
    {
        try
        {
            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            
            bool chainIsValid = chain.Build(certificate);

            return chain.ChainStatus.Any(status => 
                status.Status == X509ChainStatusFlags.Revoked ||
                status.Status == X509ChainStatusFlags.RevocationStatusUnknown);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check revocation status");
            return false;
        }
    }

    private bool HasValidKeyUsage(X509Certificate2 certificate)
    {
        var keyUsageExtension = certificate.Extensions.OfType<X509KeyUsageExtension>().FirstOrDefault();
        if (keyUsageExtension == null)
            return true;

        return keyUsageExtension.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature) ||
               keyUsageExtension.KeyUsages.HasFlag(X509KeyUsageFlags.KeyAgreement);
    }

    private bool HasValidExtendedKeyUsage(X509Certificate2 certificate)
    {
        var extendedKeyUsageExtension = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
        if (extendedKeyUsageExtension == null)
            return true;

        return extendedKeyUsageExtension.EnhancedKeyUsages
            .Cast<System.Security.Cryptography.Oid>()
            .Any(oid => oid.Value == "1.3.6.1.5.5.7.3.2" || // Client Authentication
                       oid.Value == "1.3.6.1.5.5.7.3.4");   // Email Protection
    }

    private void ValidateCertificateAuthorities(X509Chain chain, ValidationResult result)
    {
        var trustedIssuers = new[]
        {
            "CN=Skatteverket",
            "CN=Swedish Government Root CA",
            "CN=E-legitimationsnämnden"
        };

        bool hasTrustedIssuer = false;
        
        foreach (var chainElement in chain.ChainElements)
        {
            var certificate = chainElement.Certificate;
            
            if (trustedIssuers.Any(issuer => 
                certificate.Issuer.Contains(issuer, StringComparison.OrdinalIgnoreCase) ||
                certificate.Subject.Contains(issuer, StringComparison.OrdinalIgnoreCase)))
            {
                hasTrustedIssuer = true;
                break;
            }
        }

        if (!hasTrustedIssuer)
        {
            result.Warnings.Add("Certificate chain does not contain a recognized Swedish government authority");
        }
    }
}