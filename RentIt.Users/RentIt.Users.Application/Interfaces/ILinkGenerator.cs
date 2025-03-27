namespace RentIt.Users.Application.Interfaces
{
    public interface ILinkGenerator
    {
        string GenerateConfirmationLink(Guid userId, string token);
        string GenerateResetPasswordLink(string email, string token);
    }
}
