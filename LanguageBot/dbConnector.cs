using MySql.Data.MySqlClient;

namespace LanguageBot;

class dbConnector
{
	private readonly string? _dbConnectionString;

	public dbConnector()
	{
		// string? host = Utility.GetEnvironmentVariable("DB_HOST");
		// string? port = Utility.GetEnvironmentVariable("DB_PORT");
		// string? user = Utility.GetEnvironmentVariable("DB_USER");
		// string? password = Utility.GetEnvironmentVariable("DB_PASSWORD");
		// string? database = Utility.GetEnvironmentVariable("DB_NAME");

		//_dbConnectionString = $"Server={host};Port={port};Database={database};User={user};Password={password};";
		string? mysqlUrl = Utility.GetEnvironmentVariable("DB_URL");

		if (!string.IsNullOrEmpty(mysqlUrl))
		{
			var uri = new Uri(mysqlUrl);
			string userInfo = uri.UserInfo; // login:password
			string[] userPass = userInfo.Split(':');

			_dbConnectionString = $"Server={uri.Host};Port={uri.Port};" +
																$"Database={uri.AbsolutePath.TrimStart('/')};" +
																$"User={userPass[0]};Password={userPass[1]};" +
																$"SslMode=Preferred;";
		}
	}

	public bool TestConnection()
	{
		try
		{
			using (var connection = new MySqlConnection(_dbConnectionString))
			{
				connection.Open();
				Console.WriteLine("Успішне підключення до бд!");
				return true;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($@"Error: {ex.Message}");
			return false;
		}
	}

	public void ExecuteQuery(string query)
	{
		try
		{
			using (var connection = new MySqlConnection(_dbConnectionString))
			{
				connection.Open();
				using (var command = new MySqlCommand(query, connection))
				{
					command.ExecuteNonQuery();
					Console.WriteLine("Запит виконано успішно!");
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Помилка виконання запиту: {ex.Message}");
		}
	}
}