using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace CreateInvoiceSystem.Identity.Services;

public class JwtProvider(IConfiguration _configuration) : IJwtProvider
{
    public TokenResponse Generate(IdentityUserModel userModel)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userModel.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, userModel.Email),
            new("company_name", userModel.CompanyName),
            new("nip", userModel.Nip)
        };

        foreach (var role in userModel.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = _configuration.GetValue<double>("Jwt:ExpiryMinutes");

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            null,
            DateTime.UtcNow.AddMinutes(expiryMinutes),
            credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Guid.NewGuid();

        return new TokenResponse(accessToken, refreshToken);
    }
    public string GenerateActivationToken(string email, int expiresHours)
    {        
        var secretKey = _configuration["Jwt:Key"] ?? throw new Exception("Nie znaleziono Jwt:Key w konfiguracji");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
     
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, email),
            new("purpose", "activation")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? GetEmailFromActivationToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        var secretKey = _configuration["Jwt:Key"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],

                ValidateLifetime = true, 
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            
            var purposeClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "purpose")?.Value;
            if (purposeClaim != "activation")
            {
                return null;
            }
            
            var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value;

            return emailClaim;
        }
        catch (Exception)
        {            
            return null;
        }
    }
}

