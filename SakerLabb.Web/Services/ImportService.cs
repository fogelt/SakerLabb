using System.Xml;
using System.Text.Json;
using System.Net.NetworkInformation;

namespace SakerLabb.Web.Services;

public class ImportService
{
    private readonly ILogger<ImportService> _logger;
    private readonly HttpClient _http;

    public ImportService(ILogger<ImportService> logger, HttpClient http)
    {
        _logger = logger;
        _http = http;
    }

    public string ImportXml(string xml)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        var document = new XmlDocument { XmlResolver = null };
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        document.Load(reader);

        return document.DocumentElement?.InnerText ?? "";
    }
    public string ImportJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "No JSON input provided.";
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(doc.RootElement, options);
        }
        catch (JsonException ex)
        {
            return $"Invalid JSON format: {ex.Message}";
        }
    }

    public async Task<string> FetchRemote(string url)
    {
        _logger.LogInformation("Hämtar fjärresurs {Url}", url);
        var response = await _http.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public string Ping(string host)
    {
        using var ping = new Ping();
        PingReply reply = ping.Send(host, 5000);
        return reply.Status == IPStatus.Success
            ? $"Reply: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms"
            : $"Ping failed: {reply.Status}";
    }
}
