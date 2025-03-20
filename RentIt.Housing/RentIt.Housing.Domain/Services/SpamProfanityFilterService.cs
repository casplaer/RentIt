using Microsoft.AspNetCore.Hosting;

namespace RentIt.Housing.Domain.Services
{
    public class SpamProfanityFilterService
    {
        private readonly List<string> _spamAndProfanityWords;
        private readonly string _spamWordsFilePath;

        public SpamProfanityFilterService(IWebHostEnvironment environment)
        {
            _spamWordsFilePath = Path.Combine(environment.ContentRootPath, "wwwroot", "spam-and-profanity.txt");

            _spamAndProfanityWords = LoadSpamAndProfanityWords(_spamWordsFilePath);
        }

        private List<string> LoadSpamAndProfanityWords(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл со списком спам-слов не найден.");
            }

            return File.ReadAllLines(filePath).Select(word => word.Trim().ToLower()).ToList();
        }

        public bool ContainsSpamOrProfanity(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var lowerCaseText = text.ToLower();

            return _spamAndProfanityWords.Any(spamWord => lowerCaseText.Contains(spamWord.ToLower()));
        }
    }
}