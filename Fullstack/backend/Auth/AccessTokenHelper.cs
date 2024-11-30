using backend.Models;
using System.Security.Cryptography;

public class AccessTokenHelper
{

    private readonly JanusDbContext _context;

    public AccessTokenHelper(JanusDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateTokenAsync(int userId)
    {
        var tokenBytes = new byte[64]; // 512 bits Access token

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }

        string hashedToken = HashToken(tokenBytes);

        var accessToken = new AccessToken
        {
            UserId = userId,
            TokenHash = hashedToken
        };

        // Store hashed token
        _context.AccessTokens.Add(accessToken);
        await _context.SaveChangesAsync();

        // Return token for user to store locally
        return Convert.ToBase64String(tokenBytes);
    }

    private static string HashToken(byte[] token)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(token);

            return Convert.ToBase64String(hash);
        }
    }

}
