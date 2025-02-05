using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtHelper
{
    public JwtSecurityToken GenerateJwtToken(int userId, string username)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("Username", username),
            new Claim("TokenType", "User")
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey_Web")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "FrontendIssuer",
            audience: "FrontendAudience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(20),
            signingCredentials: creds
        );

        return token;
    }



    public static bool ValidateRefreshToken(string refreshToken)
    {
        // Validate the refresh token
        return true;
    }

}
