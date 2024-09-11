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
                string wordToAddWithoutExtraSpaces = string.Join(" ", wordToAdd.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                bool? isWordInDB = IsWordInDB(wordToAddWithoutExtraSpaces, fullPath);
                if (isWordInDB == null)
                {
                    await bot.SendTextMessageAsync(chatId, "Woooops... seems like you database wasn't created.");
                    Console.WriteLine($"database {username} doesn't exist");
                    return;
                }

                if (isWordInDB == true)
                {
                    await bot.SendTextMessageAsync(chatId, "Heeeey... you've added this word already!");
                    Console.WriteLine("this dumb bitch forgot what he added");
                    return;
                }

                using (StreamWriter sw = new(fullPath, true))
                {
                    sw.WriteLine(wordToAddWithoutExtraSpaces);
                }
                await bot.SendTextMessageAsync(chatId, $"word {wordToAddWithoutExtraSpaces} added");
                Console.WriteLine($"word {wordToAddWithoutExtraSpaces} added");
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
                List<string>? wordsInDB = GetAllWords(fullPath);
                if (wordsInDB == null) return;
                if (wordsInDB.Count == 0)
                {
                    await bot.SendTextMessageAsync(chatId, "Doesn't seem like you have something in your dictionary. Add some words immediately!");
                    return;
                }

                string wordToDelete = command.Replace("/delete ", "").Trim();
                string wordToDeleteWithoutExtraSpaces = string.Join(" ", wordToDelete.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                string? wordAndTranslateToDelete = wordsInDB.Find(x => x.Contains(wordToDeleteWithoutExtraSpaces, StringComparison.CurrentCultureIgnoreCase));

                if (wordAndTranslateToDelete == null)
                {
                    await bot.SendTextMessageAsync(chatId, $"Wooops... word {wordToDeleteWithoutExtraSpaces} doesn't exist in dictionary. Maybe, you meant something else");
                    Console.WriteLine("word doesn't exist");
                }
                else
                {
                    bool wordFound = wordsInDB.Remove(wordAndTranslateToDelete);
                    if (wordFound)
                    {
                        File.WriteAllLines(fullPath, wordsInDB);
                        Console.WriteLine("word deleted");
                        await bot.SendTextMessageAsync(chatId, $"Word {wordToDeleteWithoutExtraSpaces} deleted succesfully");
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
                List<string>? wordsInDB = GetAllWords(fullPath);
                if (wordsInDB == null) return;
                if (wordsInDB.Count == 0)
                {
                    await bot.SendTextMessageAsync(chatId, "Doesn't seem like you have something in your dictionary. Add some words immediately!");
                    return;
                }

                Random random = new();
                int indexOfWord = random.Next(0, wordsInDB.Count);

                string randomWord = wordsInDB[indexOfWord].Split(" -")[0];
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
                List<string>? wordsInDB = GetAllWords(fullPath);
                if (wordsInDB == null) return;
                if (wordsInDB.Count == 0)
                {
                    await bot.SendTextMessageAsync(chatId, "Doesn't seem like you have something in your dictionary. Add some words immediately!");
                    return;
                }

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

        private static bool? IsWordInDB(string newWord, string pathToDB)
        {
            List<string>? wordsInDB = GetAllWords(pathToDB);

            if (wordsInDB == null) return null;

            return wordsInDB.Count > 0 && wordsInDB.Contains(newWord);
        }

        private static string? GetPathToDB(string username)
        {
            string database = "DB-FILE";
            string? fileName = Program.GetEnvironmentVariable(database);

            return (fileName == null) ? null
            : Path.Combine(Directory.GetCurrentDirectory(), fileName + username);
        }

        private static List<string>? GetAllWords(string? fullPath)
        {
            if (fullPath == null)
            {
                Console.WriteLine("Database wasn't found");
                return null;
            }

            List<string> wordsInDB = new(File.ReadAllLines(fullPath));

            return wordsInDB;
        }
    }
}