namespace RentIt.Users.Application.Interfaces
{
    public interface IAccountTokenGenerator
    {
        string GenerateToken(int length = 32);
    }
}
