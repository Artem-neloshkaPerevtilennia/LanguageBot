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
			if (update.Type == UpdateType.Message && update.Message.Text == "/start")
			{
				await bot.SendTextMessageAsync(update.Message.Chat.Id, "cheeseburger");
			}

			return;
		}
	}
}
