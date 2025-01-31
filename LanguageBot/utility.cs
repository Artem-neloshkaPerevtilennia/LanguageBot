using System.Text.RegularExpressions;
namespace LanguageBot;

public static class Utility
{
  public static string? GetEnvironmentVariable(string varName)
  {
    string? variableValue = Environment.GetEnvironmentVariable(varName);
    if (variableValue == null)
      Console.WriteLine($"environmental variable {variableValue} doesn't exist");

    return variableValue;
  }

  public static bool IsAddCommandValid(string command)
  {
    Regex fullCommand = new(@"^/add\s+[a-zA-Z\s]+\s+-+\s[\p{IsCyrillic}a-zA-Z\s,'-]+$");

    return fullCommand.IsMatch(command);
  }

  public static bool IsDeleteCommandValid(string command)
  {
    Regex validCommand = new(@"^/delete\s+[a-zA-Z\s]+$");

    return validCommand.IsMatch(command);
  }
}