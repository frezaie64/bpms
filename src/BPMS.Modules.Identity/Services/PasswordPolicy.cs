using System.Text.RegularExpressions;

namespace BPMS.Modules.Identity.Services;

public partial class PasswordPolicy : IPasswordPolicy
{
    public void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters long.");

        if (!HasUppercase().IsMatch(password))
            throw new InvalidOperationException("Password must contain at least one uppercase letter.");

        if (!HasLowercase().IsMatch(password))
            throw new InvalidOperationException("Password must contain at least one lowercase letter.");

        if (!HasDigit().IsMatch(password))
            throw new InvalidOperationException("Password must contain at least one digit.");

        if (!HasSpecialChar().IsMatch(password))
            throw new InvalidOperationException("Password must contain at least one special character.");
    }

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex HasUppercase();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex HasLowercase();

    [GeneratedRegex(@"[0-9]")]
    private static partial Regex HasDigit();

    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex HasSpecialChar();
}