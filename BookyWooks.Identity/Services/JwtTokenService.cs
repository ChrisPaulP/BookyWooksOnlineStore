using BookyWooks.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookyWooks.Identity.Services;

public class JwtTokenService
{
    public const string SecurityKey = "secretJWTsigningKey@123";
    //private readonly List<ApplicationUser> _users = new()
    //{
    //    new ApplicationUser{ UserName = "leo@gmail.com", Password = "aDm1n", Email = "leo@gmail.com", Role = "Administrator", Scopes = ["order_scope"] },
    //    new ApplicationUser{ UserName = "belle@gmail.com", Password = "u$3r01", Email = "belle@gmail.com", Role = "User",  Scopes = ["order_scope"]}
    //};

    public Models.AuthenticationToken? GenerateAuthToken(LoginModel loginModel)
    {
        //var user = _users.FirstOrDefault(u => u.UserName == loginModel.Username && u.Password == loginModel.Password);

        //if (user is null)
        //{
        //    return null;
        //}

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecurityKey));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var expirationTimeStamp = DateTime.Now.AddMinutes(5);

        //var claims = new List<Claim>
        //{
        //    new Claim(JwtRegisteredClaimNames.Name, user.UserName),
        //    new Claim("role", user.Role),
        //    new Claim("scope", string.Join(" ", user.Scopes))
        //};

        var tokenOptions = new JwtSecurityToken(
            issuer: "https://localhost:5002",
            claims: new List<Claim> { }, // claims,
            expires: expirationTimeStamp,
            signingCredentials: signingCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        //return new Models.AuthenticationToken(tokenString, (int)expirationTimeStamp.Subtract(DateTime.Now).TotalSeconds);

        return new Models.AuthenticationToken(tokenString, (int)expirationTimeStamp.Subtract(DateTime.Now).TotalSeconds);
    }
}
