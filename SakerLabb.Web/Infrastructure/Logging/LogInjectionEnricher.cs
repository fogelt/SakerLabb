using Serilog.Core;
using Serilog.Events;

namespace SakerLabb.Web.Infrastructure.Logging;

public class LogInjectionEnricher : ILogEventEnricher
{
  public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
  {
    foreach (var property in logEvent.Properties.ToList())
    {
      if (property.Value is ScalarValue { Value: string strValue } && (strValue.Contains('\n') || strValue.Contains('\r')))
      {
        var clean = strValue.Replace("\r", string.Empty).Replace("\n", string.Empty);
        logEvent.AddOrUpdateProperty(new LogEventProperty(property.Key, new ScalarValue(clean)));
      }
    }
  }
}