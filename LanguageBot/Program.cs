using DotNetEnv;
using MySql.Data.MySqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LanguageBot
{
  class Program
  {
    public static dbConnector db = new dbConnector();

    public static void Main(string[] args)
    {
      Env.Load();

      string? botToken = Utility.GetEnvironmentVariable("BOT-TOKEN");
      if (botToken == null) return;
      if (!db.TestConnection()) return;

      TelegramBotClient bot = new(botToken);
      CreateTables();

      bot.StartReceiving(Update, Error);
      Console.WriteLine("Bot is running...");
      Thread.Sleep(Timeout.Infinite);
    }

    private static Task Error(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
      throw new NotImplementedException();
    }

    private static async Task Update(ITelegramBotClient bot, Update update, CancellationToken token)
    {
      Message message = update.Message;
      string? messageText = message.Text.ToLower();
      long chatId = message.Chat.Id;
      if (messageText == null)
      {
        await bot.SendTextMessageAsync(chatId, "Wrong message format. Please, send text non-empty messages!");
        Console.WriteLine("Wrong message format");
        return;
      }

      var username = message.From?.Username;
      if (username == null)
      {
        Console.WriteLine("username doesn't exist");
        return;
      }

      CreateDictionary(username, chatId);

      Message? isReply = message.ReplyToMessage;
      if (isReply == null)
      {
        await OnCommand(bot, message, chatId);
      }
      else
      {
        await OnReply(bot, isReply, messageText, chatId);
      }
    }

    private static async Task OnCommand(ITelegramBotClient bot, Message message, long chatId)
    {
      string messageText = message.Text.ToLower();
      switch (messageText)
      {
        case "/start":
          string greetings = "Hi! I am Language Bot and I will help you to improve your vocabulary. To explore commands use /help";
          await bot.SendTextMessageAsync(chatId, greetings);
          return;

        case string s when s.Contains("/add"):
          if (!Utility.IsAddCommandValid(s))
            await bot.SendTextMessageAsync(chatId, "Hmm... it doesn't seem like /add {word} - {translate}. Maybe, you missed something");
          else
            await WordsOperations.AddWord(db, s, bot, chatId);
          return;

        case string s when s.Contains("/delete"):
          if (!Utility.IsDeleteCommandValid(s))
            await bot.SendTextMessageAsync(chatId, "What do you want to delete? I need only /delete {word}, not only /delete and not a whole roman");
          else
            await WordsOperations.RemoveWord(db, s, bot, chatId);
          return;

        case "/rand":
          await WordsOperations.ThrowRandomWord(db, bot, chatId, message.MessageId);
          return;

        case "/list":
          await WordsOperations.ShowAllWords(db, bot, chatId);
          return;

        case "/help":
          await bot.SendTextMessageAsync(chatId, @"Here are some commands to operate with dictionary:
						/add {word} - {translate}: write new word in dictionary
						/delete {word}: delete word from dictionary
						/rand: throw a random word from dictionary (without a translation)
						/list: show all words in dictionary with translation
						/help: all commands");
          return;

        default:
          Console.WriteLine("This is not a chat to talk: use commands!\nWe learn english here, talk to some girls outside, not here!");
          return;
      }
    }

    private static async Task OnReply(ITelegramBotClient bot, Message repliedMessage, string sendMessageText, long chatId)
    {
      using (MySqlConnection connection = new(db.GetConnectionString()))
      {
        long repliedMessageId = repliedMessage.MessageId;
        string translation = string.Empty;
        connection.Open();

        string getTranslationQuery = @"
        SELECT translation
        FROM Words
        JOIN Questions ON Questions.wordId = Words.id
        WHERE Questions.chatId = @chatId AND Questions.messageId = @messageId;
        ";

        using (MySqlCommand command = new(getTranslationQuery, connection))
        {
          command.Parameters.AddWithValue("@chatId", chatId);
          command.Parameters.AddWithValue("@messageId", repliedMessageId);

          using (var reader = command.ExecuteReader())
          {
            Console.WriteLine($"chatId: {chatId}, repliedMessageId: {repliedMessageId}");

            if (reader.Read())
            {
              System.Console.WriteLine("jfjajfd");
              translation = reader.GetString(0);
            }
          }
        }

        if (string.IsNullOrEmpty(translation))
        {
          await bot.SendTextMessageAsync(chatId, "Hmm, I couldn't find the original question. Try again or start a new word.");
          Console.WriteLine("Translation not found");
          return;
        }

        if (string.Equals(translation.Trim(), sendMessageText.Trim(), StringComparison.OrdinalIgnoreCase))
        {
          await bot.SendTextMessageAsync(chatId, "Congrats! You're damn right!");
          Console.WriteLine("He's right");
        }
        else
        {
          await bot.SendTextMessageAsync(chatId, $"Naaaah, you're wroooong, the right answer was {translation}");
          Console.WriteLine("He's stupid");
        }
      }
    }

    private static void CreateTables()
    {
      string createUsersQuery = @"
      CREATE TABLE IF NOT EXISTS Users (
        id INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
        tag VARCHAR(255) NOT NULL UNIQUE,
        chatId BIGINT NOT NULL UNIQUE
      );";
      db.ExecuteQuery(createUsersQuery);

      string createWordsQuery = @"
      CREATE TABLE IF NOT EXISTS Words (
        id INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
        word VARCHAR(255) NOT NULL UNIQUE,
        translation VARCHAR(255) NOT NULL
      );";
      db.ExecuteQuery(createWordsQuery);

      string createUsers_WordsQuery = @"
      CREATE TABLE IF NOT EXISTS Users_Words (
        userId INT NOT NULL,
        wordId INT NOT NULL,

        FOREIGN KEY (userId) REFERENCES Users(id) ON DELETE CASCADE,
        FOREIGN KEY (wordId) REFERENCES Words(id) ON DELETE CASCADE,
        UNIQUE KEY (userId, wordId)
      );";
      db.ExecuteQuery(createUsers_WordsQuery);

      string createQuestionsQuery = @"
      CREATE TABLE IF NOT EXISTS Questions (
        chatId BIGINT NOT NULL,
        messageId BIGINT NOT NULL,
        userId INT NOT NULL,
        wordId INT NOT NULL,

        FOREIGN KEY (userId) REFERENCES Users(id) ON DELETE CASCADE,
        FOREIGN KEY (wordId) REFERENCES Words(id) ON DELETE CASCADE,
        PRIMARY KEY (chatId, messageId)
      );";
      db.ExecuteQuery(createQuestionsQuery);
    }

    public static void CreateDictionary(string userTag, long chatId)
    {
      using (MySqlConnection connection = new (db.GetConnectionString()))
      {
        connection.Open();
        string query = @"
        INSERT INTO Users (tag, chatId)
        SELECT * FROM (SELECT @tag AS tag, @chatId AS chatId) AS tmp
        WHERE NOT EXISTS (
            SELECT 1 FROM Users WHERE chatId = @chatId
        );";

        using (var command = new MySqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@tag", userTag);
          command.Parameters.AddWithValue("@chatId", chatId);
          command.ExecuteNonQuery();
        }
      }
    }
  }
}
