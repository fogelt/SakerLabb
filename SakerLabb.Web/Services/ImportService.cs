using System.Diagnostics;
using System.Xml;
using Newtonsoft.Json;
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
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = new XmlUrlResolver()
        };

        var document = new XmlDocument { XmlResolver = new XmlUrlResolver() };
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        document.Load(reader);

        return document.DocumentElement?.InnerText ?? "";
    }

    public object? ImportJson(string json)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        return JsonConvert.DeserializeObject(json, settings);
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
            ? $"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms"
            : $"Ping failed: {reply.Status}";
    }
}
