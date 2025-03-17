using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AccessTokenHelper
{
    private readonly string _issuer = "CLIIssuer";
    private readonly string _audience = "CLIAudience";
    private readonly SymmetricSecurityKey _key;

    public AccessTokenHelper()
    {
        // Assuming you have JWT__SecretKey_CLI set in your environment variables.
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey_CLI")));
    }


    public JwtSecurityToken GenerateAccessToken(int userId, int expirationInHours = 720) // default 30 days
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", userId.ToString()),
            new Claim("TokenType", "PAT")
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.Now.AddHours(expirationInHours),
            signingCredentials: creds
        );

        return token;
    }





    public ClaimsPrincipal ValidateAccessToken(string token, out SecurityToken validatedToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // This will throw an exception if validation fails
        return tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
    }


}
