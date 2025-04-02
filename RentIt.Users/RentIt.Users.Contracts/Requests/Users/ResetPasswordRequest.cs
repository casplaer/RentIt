namespace RentIt.Users.Contracts.Requests.Users
{
    public record ResetPasswordRequest(
        string Email,
        string Token,
        string NewPassword,
        string ConfirmPassword);
}
