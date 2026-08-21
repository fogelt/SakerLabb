namespace SakerLabb.Web.Infrastructure.Logging;

public static class LogCleaner
{
  public static string Clean(string? input) =>
      string.IsNullOrEmpty(input) ? string.Empty : input.Replace("\r", "").Replace("\n", "");

  public static string Clean(int? input) =>
        input?.ToString() ?? string.Empty;
}