namespace RentIt.Users.Contracts.Responses.RefreshToken
{
    public record ValidateRefreshTokenResponse(
        string NewAccessToken,
        string NewRefreshToken
        );
}
