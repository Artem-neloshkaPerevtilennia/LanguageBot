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
			if (update.Type == UpdateType.Message && update.Message?.Text != null)
			{
				Message message = update.Message;
				long chatId = message.Chat.Id;

				switch (message.Text.ToLower())
				{
					case "/start":
						string greetings = "Hi! I am Language Bot and I will help you to improve your vocabulary. To add some word in your dictionary use /add";
						await bot.SendTextMessageAsync(chatId, greetings);
						return;

					case string s when s.Contains("/add"):
						await WordsOperations.AddWord(s, bot, chatId);
						return;

					case string s when s.Contains("/delete"):
						await WordsOperations.RemoweWord(s, bot, chatId);
						return;

					case "/rand":
						await WordsOperations.ThrowRandomWord(bot, chatId);
						return;

					case "/list":
						await WordsOperations.ShowAllWords(bot, chatId);
						return;

					default:
						Console.WriteLine("не те");
						return;
				}
			}

			return;
		}

		public static string? IsEnvironmentalVariableExists(string varName)
		{
			string? variableName = Environment.GetEnvironmentVariable(varName);
			if (variableName == null)
				Console.WriteLine($"environmental variable {variableName} doesn't exist");

			return variableName;
		}
	}
}
