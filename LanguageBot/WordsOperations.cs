using System.Text;
using Telegram.Bot;

namespace LanguageBot
{
    static class WordsOperations
    {
        public static async Task AddWord(string command, ITelegramBotClient bot, long chatId)
        {
            string database = "DB-FILE";
            string? fileName = Program.IsEnvironmentalVariableExists(database);
            if (fileName == null) return;

            string wordToAdd = command.Replace("/add", "").Trim();

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            using (StreamWriter sw = new(fullPath, true))
            {
                sw.WriteLine(wordToAdd);
            }
            await bot.SendTextMessageAsync(chatId, $"word {wordToAdd} added");
            Console.WriteLine($"word {wordToAdd} added");
        }

        public static async Task RemoweWord(string command, ITelegramBotClient bot, long chatId)
        {
            string database = "DB-FILE";
            string? fileName = Program.IsEnvironmentalVariableExists(database);
            if (fileName == null) return;

            string wordToDelete = command.Replace("/delete", "").Trim();

            string fullPathDelete = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            List<string> wordsInDB = new(File.ReadAllLines(fullPathDelete));
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
                    File.WriteAllLines(fullPathDelete, wordsInDB);
                    Console.WriteLine("word deleted");
                    await bot.SendTextMessageAsync(chatId, $"Word {wordToDelete} deleted succesfully");
                }
            }
        }

        public static async Task ThrowRandomWord(ITelegramBotClient bot, long chatId)
        {
            Random random = new();

            string database = "DB-FILE";
            string? fileName = Program.IsEnvironmentalVariableExists(database);
            if (fileName == null) return;

            string fullPathDelete = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            List<string> wordsInDB = new(File.ReadAllLines(fullPathDelete));
            int indexOfWord = random.Next(0, wordsInDB.Count);

            string randomWord = wordsInDB[indexOfWord].Split(' ')[0];
            await bot.SendTextMessageAsync(chatId, $"Random word: {randomWord}");
            Console.WriteLine("random word sent");
        }

        public static async Task ShowAllWords(ITelegramBotClient bot, long chatId)
        {
            string database = "DB-FILE";
            string? fileName = Program.IsEnvironmentalVariableExists(database);
            if (fileName == null) return;

            string fullPathDelete = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            List<string> wordsInDB = new(File.ReadAllLines(fullPathDelete));

            StringBuilder allWords = new();

            foreach (var word in wordsInDB)
            {
                allWords.Append(word);
                allWords.Append('\n');
            }

            await bot.SendTextMessageAsync(chatId, $"All words in your dictionary:\n{allWords}");
        }
    }
}