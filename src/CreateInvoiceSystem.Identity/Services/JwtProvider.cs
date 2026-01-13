using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace CreateInvoiceSystem.Identity.Services;

public class JwtProvider(IConfiguration configuration) : IJwtProvider
{
    public string Generate(IdentityUserModel userModel)
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
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            null,
            DateTime.UtcNow.AddMinutes(double.Parse(configuration["Jwt:ExpiryMinutes"]!)),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

