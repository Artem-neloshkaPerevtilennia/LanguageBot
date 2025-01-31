using System.Text;
using MySql.Data.MySqlClient;
using Telegram.Bot;

namespace LanguageBot
{
  static class WordsOperations
  {
    public static async Task AddWord(dbConnector database, string command, ITelegramBotClient bot, long chatId)
    {
      try
      {
        string wordToAdd = command.Replace("/add ", "").Trim();
        string wordToAddWithoutExtraSpaces = string.Join(" ", wordToAdd.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        string[] wordWithTranslation = wordToAddWithoutExtraSpaces.Split(" - ");
        string word = wordWithTranslation[0];
        string translation = wordWithTranslation[1];

        bool? isWordInDB = await IsWordInDB(database, word, chatId);
        if (isWordInDB == null)
        {
          await bot.SendTextMessageAsync(chatId, "Woooops... seems like you database wasn't created.");
          Console.WriteLine($"database {chatId} doesn't exist");
          return;
        }

        if (isWordInDB == true)
        {
          await bot.SendTextMessageAsync(chatId, "Heeeey... you've added this word already!");
          Console.WriteLine("this dumb bitch forgot what he added");
          return;
        }

        AddWordQuery(database, word, translation, chatId);
        await bot.SendTextMessageAsync(chatId, $"word {wordToAddWithoutExtraSpaces} added");
        Console.WriteLine($"word {wordToAddWithoutExtraSpaces} added");
      }
      catch (Exception exception)
      {
        Console.WriteLine($"An exeption thrown: {exception.Message}");
      }
    }

    public static async Task RemoveWord(dbConnector database, string command, ITelegramBotClient bot, long chatId)
    {
      try
      {
        List<string>? wordsInDB = await GetAllWordsForUser(database, chatId);
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
          DeleteWordQuery(database, wordAndTranslateToDelete, chatId);
          Console.WriteLine("word deleted");
          await bot.SendTextMessageAsync(chatId, $"Word {wordToDeleteWithoutExtraSpaces} deleted succesfully");
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine($"An exeption thrown: {exception.Message}");
      }
    }

    public static async Task ThrowRandomWord(dbConnector database, ITelegramBotClient bot, long chatId)
    {
      try
      {
        List<string>? wordsInDB = await GetAllWordsForUser(database, chatId);
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

    public static async Task ShowAllWords(dbConnector database, ITelegramBotClient bot, long chatId)
    {
      try
      {
        List<List<string>>? wordsInDB = await GetAllWordsWithTranslationForUser(database, chatId);
        if (wordsInDB == null) return;
        if (wordsInDB.Count == 0)
        {
          await bot.SendTextMessageAsync(chatId, "Doesn't seem like you have something in your dictionary. Add some words immediately!");
          return;
        }

        StringBuilder allWords = new();
        foreach (var word in wordsInDB)
        {
          string wordWithTranslation = word[0] + " - " + word[1];
          allWords.AppendLine(wordWithTranslation);
        }

        await bot.SendTextMessageAsync(chatId, $"All words in your dictionary:\n{allWords}");
      }
      catch (Exception exception)
      {
        Console.WriteLine($"An exeption thrown: {exception.Message}");
      }
    }

    private static async Task<bool?> IsWordInDB(dbConnector database, string wordToFind, long chatId)
    {
      List<string>? wordsInDB = await GetAllWordsForUser(database, chatId);

      if (wordsInDB == null) return null;

      return wordsInDB.Count > 0 && wordsInDB.Contains(wordToFind);
    }

    private static async Task<List<string>?> GetAllWordsForUser(dbConnector database, long chatId)
    {
      if (database == null)
      {
        Console.WriteLine("Database wasn't found");
        return null;
      }

      List<string> wordsInDB = [];
      using (MySqlConnection connection = new(database.GetConnectionString()))
      {
        await connection.OpenAsync();
        string getAllWordsQuery = @"
        SELECT Words.word 
        FROM Words 
        JOIN Users_Words ON Users_Words.wordId = Words.id 
        JOIN Users ON Users.id = Users_Words.userId 
        WHERE Users.chatId = @chatId";

        using (MySqlCommand command = new(getAllWordsQuery, connection))
        {
          command.Parameters.AddWithValue("@chatId", chatId);

          using (var reader = await command.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
            {
              wordsInDB.Add(reader.GetString(0));
            }
          }
        }
      }

      return wordsInDB;
    }

    private static async Task<List<List<string>>?> GetAllWordsWithTranslationForUser(dbConnector database, long chatId)
    {
      if (database == null)
      {
        Console.WriteLine("Database wasn't found");
        return null;
      }

      List<List<string>> wordsInDB = new();

      using (MySqlConnection connection = new(database.GetConnectionString()))
      {
        await connection.OpenAsync();

        string getAllWordsQuery = $@"
        SELECT Words.word, Words.translation
        FROM Words 
        JOIN Users_Words ON Users_Words.wordId = Words.id 
        JOIN Users ON Users.id = Users_Words.userId 
        WHERE Users.chatId = @chatId";

        using (MySqlCommand command = new(getAllWordsQuery, connection))
        {
          command.Parameters.AddWithValue("@chatId", chatId);

          using (var reader = await command.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
            {
              List<string> row = new();
              for (int i = 0; i < reader.FieldCount; i++)
              {
                row.Add(reader[i]?.ToString() ?? "");
              }
              wordsInDB.Add(row);
            }
          }
        }
      }

      return wordsInDB;
    }

    private static void AddWordQuery(dbConnector database, string wordToAdd, string translateToAdd, long chatId)
    {
      using (var connection = new MySqlConnection(database.GetConnectionString()))
      {
        connection.Open();

        string insertWordQuery = @"
        INSERT INTO Words (word, translation)
        SELECT * FROM (SELECT @word AS word, @translation AS translation) AS tmp
        WHERE NOT EXISTS (SELECT 1 FROM Words WHERE word = @word);";

        using (var command = new MySqlCommand(insertWordQuery, connection))
        {
          command.Parameters.AddWithValue("@word", wordToAdd);
          command.Parameters.AddWithValue("@translation", translateToAdd);
          command.ExecuteNonQuery();
        }

        string selectIdsQuery = @"
        SELECT w.id, u.id FROM Words w, Users u
        WHERE w.word = @word AND u.chatId = @chatId;";

        int wordId = 0, userId = 0;
        using (var command = new MySqlCommand(selectIdsQuery, connection))
        {
          command.Parameters.AddWithValue("@word", wordToAdd);
          command.Parameters.AddWithValue("@chatId", chatId);
          using (var reader = command.ExecuteReader())
          {
            if (reader.Read())
            {
              wordId = reader.GetInt32(0);
              userId = reader.GetInt32(1);
            }
          }
        }

        if (wordId > 0 && userId > 0)
        {
          string insertUserWordQuery = @"
            INSERT INTO Users_Words (userId, wordId)
            SELECT * FROM (SELECT @userId AS userId, @wordId AS wordId) AS tmp
            WHERE NOT EXISTS (SELECT 1 FROM Users_Words WHERE userId = @userId AND wordId = @wordId);";

          using (var command = new MySqlCommand(insertUserWordQuery, connection))
          {
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@wordId", wordId);
            command.ExecuteNonQuery();
          }
        }
      }
    }

    private static void DeleteWordQuery(dbConnector database, string wordToRemove, long chatId)
    {
      using (var connection = new MySqlConnection(database.GetConnectionString()))
      {
        connection.Open();

        string selectIdsQuery = @"
        SELECT w.id, u.id FROM Words w
        JOIN Users u ON u.chatId = @chatId
        WHERE w.word = @word;";

        int wordId = 0, userId = 0;
        using (var command = new MySqlCommand(selectIdsQuery, connection))
        {
          command.Parameters.AddWithValue("@word", wordToRemove);
          command.Parameters.AddWithValue("@chatId", chatId);
          using (var reader = command.ExecuteReader())
          {
            if (reader.Read())
            {
              wordId = reader.GetInt32(0);
              userId = reader.GetInt32(1);
            }
          }
        }

        if (wordId > 0 && userId > 0)
        {
          string deleteUserWordQuery = @"
            DELETE FROM Users_Words 
            WHERE userId = @userId AND wordId = @wordId;";

          using (var command = new MySqlCommand(deleteUserWordQuery, connection))
          {
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@wordId", wordId);
            command.ExecuteNonQuery();
          }
        }

        string deleteWordQuery = @"
        DELETE w FROM Words w
        LEFT JOIN Users_Words uw ON w.id = uw.wordId
        WHERE w.id = @wordId AND uw.wordId IS NULL;";

        using (var command = new MySqlCommand(deleteWordQuery, connection))
        {
          command.Parameters.AddWithValue("@wordId", wordId);
          command.ExecuteNonQuery();
        }
      }
    }
  }
}