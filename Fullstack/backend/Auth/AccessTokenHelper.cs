using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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




    public async Task<bool> ValidateTokenAsync(string token)
    {
        byte[] tokenBytes = Convert.FromBase64String(token);

        // Check if token is in the database
        string hashedToken = HashToken(tokenBytes);

        var storedToken = await _context.AccessTokens
            .Where(t => t.TokenHash == hashedToken)
            .FirstOrDefaultAsync();

        if (storedToken == null)
        {
            // Token not found
            return false;
        }

        // Check if the token has expired
        if (storedToken.Expires < DateTime.UtcNow)
        {
            _context.AccessTokens.Remove(storedToken);
            return false;
        }

        // Token is valid
        return true;
    }




}
