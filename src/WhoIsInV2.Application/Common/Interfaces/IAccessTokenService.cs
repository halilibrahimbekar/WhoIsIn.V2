using WhoIsInV2.Domain.Entities;

namespace WhoIsInV2.Application.Common.Interfaces;

public interface IAccessTokenService
{
    AccessTokenPair GenerateTokenPair(User user);
    string ComputeTokenHash(string token);
}

public sealed record AccessTokenPair(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string RefreshTokenHash);