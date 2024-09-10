using System.Text;
using Telegram.Bot;

namespace LanguageBot
{
    static class WordsOperations
    {
        public static async Task AddWord(string command, ITelegramBotClient bot, long chatId, string username)
        {
            try
            {
                string? fullPath = GetPathToDB(username);
                if (fullPath == null)
                {
                    Console.WriteLine("Database wasn't found");
                    await bot.SendTextMessageAsync(chatId, "Error: Database not found.");
                    return;
                }

                string wordToAdd = command.Replace("/add ", "").Trim();

                using (StreamWriter sw = new(fullPath, true))
                {
                    sw.WriteLine(wordToAdd);
                }
                await bot.SendTextMessageAsync(chatId, $"word {wordToAdd} added");
                Console.WriteLine($"word {wordToAdd} added");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An exeption thrown: {exception.Message}");
            }
        }

        public static async Task RemoveWord(string command, ITelegramBotClient bot, long chatId, string username)
        {
            try
            {
                string? fullPath = GetPathToDB(username);
                List<string>? wordsInDB = await GetAllWords(fullPath, bot, chatId);
                if (wordsInDB == null) return;

                string wordToDelete = command.Replace("/delete ", "").Trim();
                string? wordAndTranslateToDelete = wordsInDB.Find(x => x.Contains(wordToDelete, StringComparison.CurrentCultureIgnoreCase));

                if (wordAndTranslateToDelete == null)
                {
                    await bot.SendTextMessageAsync(chatId, $"Wooops... word {wordToDelete} doesn't exist in dictionary. Maybe, you meant something else");
                    Console.WriteLine("word doesn't exist");
                }
                else
                {
                    bool wordFound = wordsInDB.Remove(wordAndTranslateToDelete);
                    if (wordFound)
                    {
                        File.WriteAllLines(fullPath, wordsInDB);
                        Console.WriteLine("word deleted");
                        await bot.SendTextMessageAsync(chatId, $"Word {wordToDelete} deleted succesfully");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An exeption thrown: {exception.Message}");
            }
        }

        public static async Task ThrowRandomWord(ITelegramBotClient bot, long chatId, string username)
        {
            try
            {
                string? fullPath = GetPathToDB(username);
                List<string>? wordsInDB = await GetAllWords(fullPath, bot, chatId);
                if (wordsInDB == null) return;

                Random random = new();
                int indexOfWord = random.Next(0, wordsInDB.Count);

                string randomWord = wordsInDB[indexOfWord].Split(' ')[0];
                await bot.SendTextMessageAsync(chatId, $"Random word: {randomWord}");
                Console.WriteLine("random word sent");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An exeption thrown: {exception.Message}");
            }
        }

        public static async Task ShowAllWords(ITelegramBotClient bot, long chatId, string username)
        {
            try
            {
                string? fullPath = GetPathToDB(username);
                List<string>? wordsInDB = await GetAllWords(fullPath, bot, chatId);
                if (wordsInDB == null) return;

                StringBuilder allWords = new();
                foreach (var word in wordsInDB)
                {
                    allWords.AppendLine(word);
                }

                await bot.SendTextMessageAsync(chatId, $"All words in your dictionary:\n{allWords}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An exeption thrown: {exception.Message}");
            }
        }

        private static string? GetPathToDB(string username)
        {
            string database = "DB-FILE";
            string? fileName = Program.GetEnvironmentVariable(database);

            return (fileName == null) ? null
            : Path.Combine(Directory.GetCurrentDirectory(), fileName + username);
        }

        private static async Task<List<string>?> GetAllWords(string? fullPath, ITelegramBotClient bot, long chatId)
        {
            if (fullPath == null)
            {
                Console.WriteLine("Database wasn't found");
                await bot.SendTextMessageAsync(chatId, "Error: Database not found.");
                return null;
            }

            List<string> wordsInDB = new(File.ReadAllLines(fullPath));
            if (wordsInDB.Count == 0)
            {
                await bot.SendTextMessageAsync(chatId, "Doesn't seem like you have something in your dictionary. Add some words immediately!");
                return null;
            }

            return wordsInDB;
        }
    }
}