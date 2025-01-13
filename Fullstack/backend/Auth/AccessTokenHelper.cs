using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AccessTokenHelper
{

    public JwtSecurityToken GenerateAccessToken(int userId, int expirationInHours = 720) // default 30 days
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("TokenType", "PAT")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey_CLI")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "CLIIssuer",
            audience: "CLIAudience",
            claims: claims,
            expires: DateTime.Now.AddHours(expirationInHours),
            signingCredentials: creds
        );

        return token;
    }


}
