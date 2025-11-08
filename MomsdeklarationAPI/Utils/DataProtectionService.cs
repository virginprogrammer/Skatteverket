using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace MomsdeklarationAPI.Utils;

public interface IDataProtectionService
{
    string Protect(string data, TimeSpan? expiration = null);
    string Unprotect(string protectedData);
    bool TryUnprotect(string protectedData, out string? data);
    string ProtectSensitive(string sensitiveData);
    string UnprotectSensitive(string protectedData);
}

public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _generalProtector;
    private readonly IDataProtector _sensitiveProtector;
    private readonly IDataProtector _timeLimitedProtector;

    public DataProtectionService(IDataProtectionProvider dataProtectionProvider)
    {
        _generalProtector = dataProtectionProvider.CreateProtector("MomsdeklarationAPI.General");
        _sensitiveProtector = dataProtectionProvider.CreateProtector("MomsdeklarationAPI.Sensitive");
        _timeLimitedProtector = dataProtectionProvider.CreateProtector("MomsdeklarationAPI.TimeLimited");
    }

    public string Protect(string data, TimeSpan? expiration = null)
    {
        if (string.IsNullOrEmpty(data))
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        if (expiration.HasValue)
        {
            var timeLimitedProtector = _timeLimitedProtector.ToTimeLimitedDataProtector();
            return timeLimitedProtector.Protect(data, DateTimeOffset.UtcNow.Add(expiration.Value));
        }

        return _generalProtector.Protect(data);
    }

    public string Unprotect(string protectedData)
    {
        if (string.IsNullOrEmpty(protectedData))
            throw new ArgumentException("Protected data cannot be null or empty", nameof(protectedData));

        try
        {
            return _generalProtector.Unprotect(protectedData);
        }
        catch (Exception)
        {
            var timeLimitedProtector = _timeLimitedProtector.ToTimeLimitedDataProtector();
            return timeLimitedProtector.Unprotect(protectedData);
        }
    }

    public bool TryUnprotect(string protectedData, out string? data)
    {
        data = null;
        
        if (string.IsNullOrEmpty(protectedData))
            return false;

        try
        {
            data = Unprotect(protectedData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string ProtectSensitive(string sensitiveData)
    {
        if (string.IsNullOrEmpty(sensitiveData))
            throw new ArgumentException("Sensitive data cannot be null or empty", nameof(sensitiveData));

        var dataBytes = Encoding.UTF8.GetBytes(sensitiveData);
        var protectedBytes = _sensitiveProtector.Protect(dataBytes);
        
        Array.Clear(dataBytes, 0, dataBytes.Length);
        
        return Convert.ToBase64String(protectedBytes);
    }

    public string UnprotectSensitive(string protectedData)
    {
        if (string.IsNullOrEmpty(protectedData))
            throw new ArgumentException("Protected data cannot be null or empty", nameof(protectedData));

        try
        {
            var protectedBytes = Convert.FromBase64String(protectedData);
            var dataBytes = _sensitiveProtector.Unprotect(protectedBytes);
            var result = Encoding.UTF8.GetString(dataBytes);
            
            Array.Clear(dataBytes, 0, dataBytes.Length);
            Array.Clear(protectedBytes, 0, protectedBytes.Length);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to unprotect sensitive data", ex);
        }
    }
}