using DotNetEnv;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LanguageBot
{
	class Program
	{
		public static void Main(string[] args)
		{
			Env.Load();

			string? botToken = Environment.GetEnvironmentVariable("BOT-TOKEN");
			if (botToken == null)
			{
				Console.WriteLine("bot token doesn't exist");
				return;
			}

			TelegramBotClient bot = new(botToken);

			bot.StartReceiving(Update, Error);
			Console.ReadKey();
		}

		private static Task Error(ITelegramBotClient bot, Exception exception, CancellationToken token)
		{
			throw new NotImplementedException();
		}

		private static async Task Update(ITelegramBotClient bot, Update update, CancellationToken token)
		{
			if (update.Type != UpdateType.Message || update.Message?.Text == null) return;
			Message message = update.Message;
			long chatId = message.Chat.Id;
			var username = message.From?.Username;

			CreateDictionary(username);

			switch (message.Text.ToLower())
			{
				case "/start":
					string greetings = "Hi! I am Language Bot and I will help you to improve your vocabulary. To explore commands use /help";
					await bot.SendTextMessageAsync(chatId, greetings);
					return;

				case string s when s.Contains("/add"):
					if (!IsAddCommandValid(s))
						await bot.SendTextMessageAsync(chatId, "Hmm... it doesn't seem like /add {word} {translate}. Maybe, you missed something");
					else
						await WordsOperations.AddWord(s, bot, chatId, $"{username}.txt");
					return;

				case string s when s.Contains("/delete"):
					if (!IsDeleteCommandValid(s))
						await bot.SendTextMessageAsync(chatId, "What do you want to delete? I need only /delete {word}, not only /delete and not a whole roman");
					else
						await WordsOperations.RemoveWord(s, bot, chatId, $"{username}.txt");
					return;

				case "/rand":
					await WordsOperations.ThrowRandomWord(bot, chatId, $"{username}.txt");
					return;

				case "/list":
					await WordsOperations.ShowAllWords(bot, chatId, $"{username}.txt");
					return;

				case "/help":
					await bot.SendTextMessageAsync(chatId, @"Here are some commands to operate with dictionary:
						/add {word} {translate} - write new word in dictionary
						/delete {word} - delete word from dictionary
						/rand - throw a random word from dictionary (without a translation)
						/list - show all words in dictionary with translation
						/help - all commands");
					return;

				default:
					Console.WriteLine("не те");
					return;
			}
		}

		public static string? GetEnvironmentVariable(string varName)
		{
			string? variableValue = Environment.GetEnvironmentVariable(varName);
			if (variableValue == null)
				Console.WriteLine($"environmental variable {variableValue} doesn't exist");

			return variableValue;
		}

		private static bool IsAddCommandValid(string command) => command.Split(' ').Length >= 3;
		private static bool IsDeleteCommandValid(string command) => command.Split(' ').Length == 2;

		private static void CreateDictionary(string? username)
		{
			if (username == null)
			{
				Console.WriteLine("username doesn't exist");
				return;
			}

			string directory = "Users";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			string filePath = Path.Combine(directory, $"{username}.txt");
			if (!System.IO.File.Exists(filePath))
			{
				using (FileStream fs = System.IO.File.Create(filePath)) { }
				Console.WriteLine($"File for user {username} created");
			}
		}
	}
}
