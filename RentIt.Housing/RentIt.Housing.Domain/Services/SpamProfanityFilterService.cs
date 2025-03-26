using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace RentIt.Housing.Domain.Services
{
    public class SpamProfanityFilterService
    {
        private readonly List<string> _spamAndProfanityWords;
        private readonly string _spamWordsFilePath;
        private readonly ILogger _logger;

        public SpamProfanityFilterService(IWebHostEnvironment environment, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _spamWordsFilePath = Path.Combine(environment.ContentRootPath, "wwwroot", "spam-and-profanity.txt");
            _logger.Information("Путь к файлу спам-слов: {FilePath}", _spamWordsFilePath);

            _spamAndProfanityWords = LoadSpamAndProfanityWords(_spamWordsFilePath);
            _logger.Information("Загружено {Count} слов для фильтрации спама и нецензурщины", _spamAndProfanityWords.Count);
        }

        private List<string> LoadSpamAndProfanityWords(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.Error("Файл со списком спам-слов не найден по пути: {FilePath}", filePath);
                throw new FileNotFoundException("Файл со списком спам-слов не найден.", filePath);
            }

            _logger.Information("Начинается загрузка слов для фильтрации из файла: {FilePath}", filePath);
            var words = File.ReadAllLines(filePath)
                            .Select(word => word.Trim().ToLower())
                            .ToList();
            _logger.Information("Успешно загружено {Count} слов", words.Count);
            return words;
        }

        public bool ContainsSpamOrProfanity(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _logger.Debug("Проверяемый текст пуст или равен null. Возвращаем false.");
                return false;
            }

            var lowerCaseText = text.ToLower();
            bool contains = _spamAndProfanityWords.Any(spamWord => lowerCaseText.Contains(spamWord));
            _logger.Debug("Проверка текста на наличие спам/нецензурщины завершена. Результат: {Result}", contains);
            return contains;
        }
    }
}