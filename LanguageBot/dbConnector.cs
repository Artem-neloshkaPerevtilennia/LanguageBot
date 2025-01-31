using MySql.Data.MySqlClient;

namespace LanguageBot;

class dbConnector
{
	private readonly string? _dbConnectionString;

	public dbConnector()
    {
        string? host = Utility.GetEnvironmentVariable("MYSQLHOST");
        string? port = Utility.GetEnvironmentVariable("MYSQLPORT");
        string? user = Utility.GetEnvironmentVariable("MYSQLUSER");
        string? password = Utility.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD");
        string? database = Utility.GetEnvironmentVariable("MYSQL_DATABASE");

        _dbConnectionString = $"Server={host};Port={port};Database={database};User={user};Password={password};";
    }

	public string? GetConnectionString() => _dbConnectionString;

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