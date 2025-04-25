namespace RentIt.Housing.Domain.Services.Interfaces
{
    public interface ISpamProfanityFilterService
    {
        bool ContainsSpamOrProfanity(string text);
    }
}
