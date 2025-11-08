using System.Runtime.InteropServices;
using System.Security;

namespace MomsdeklarationAPI.Utils;

public static class SecureStringExtensions
{
    public static string ToUnsecureString(this SecureString secureString)
    {
        if (secureString == null)
            throw new ArgumentNullException(nameof(secureString));

        var unmanagedString = IntPtr.Zero;
        try
        {
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(unmanagedString) ?? string.Empty;
        }
        finally
        {
            if (unmanagedString != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }

    public static SecureString ToSecureString(this string plainString)
    {
        if (plainString == null)
            throw new ArgumentNullException(nameof(plainString));

        var secureString = new SecureString();
        foreach (char c in plainString)
        {
            secureString.AppendChar(c);
        }
        secureString.MakeReadOnly();
        return secureString;
    }

    public static void ClearString(ref string? sensitiveString)
    {
        if (!string.IsNullOrEmpty(sensitiveString))
        {
            unsafe
            {
                fixed (char* ptr = sensitiveString)
                {
                    for (int i = 0; i < sensitiveString.Length; i++)
                    {
                        ptr[i] = '\0';
                    }
                }
            }
            sensitiveString = null;
        }
    }
}