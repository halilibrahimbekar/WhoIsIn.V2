using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WhoIsInV2.Application.Common.Interfaces;
using WhoIsInV2.Domain.Entities;

namespace WhoIsInV2.Api.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IAccessTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessTokenPair GenerateTokenPair(User user)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiresAtUtc = now.AddMinutes(_options.AccessTokenMinutes);
        var refreshTokenExpiresAtUtc = now.AddDays(_options.RefreshTokenDays);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName), new(JwtRegisteredClaimNames.FamilyName, user.LastName), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, now, accessTokenExpiresAtUtc, new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new AccessTokenPair(new JwtSecurityTokenHandler().WriteToken(token), accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc, ComputeTokenHash(refreshToken));
    }

    public string ComputeTokenHash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}