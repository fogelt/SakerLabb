using SakerLabb.Web.Infrastructure.Logging;

namespace SakerLabb.Web.Services;

public class FileService
{
    private readonly string _root;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _root = Path.GetFullPath(Path.Combine(environment.WebRootPath, "files"));
        _logger = logger;
        Directory.CreateDirectory(_root);
    }

    public string ReadDocument(string name)
    {
        var safePath = GetSafePath(name);
        _logger.LogInformation("Läser bilaga {Name} från {Path}", LogCleaner.Clean(name), LogCleaner.Clean(safePath));
        return File.ReadAllText(safePath);
    }

    public byte[] ReadBytes(string name)
    {
        var safePath = GetSafePath(name);
        return File.ReadAllBytes(safePath);
    }

    public IEnumerable<string> List()
    {
        return Directory.EnumerateFiles(_root)
            .Select(Path.GetFileName)
            .Where(f => f is not null)!;
    }

    public async Task<string> SaveUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Filen är tom eller saknas.", nameof(file));
        }

        var originalFileName = Path.GetFileName(file.FileName);
        var sanitizedFileName = string.Concat(originalFileName.Split(Path.GetInvalidFileNameChars()));

        var safeFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
        var target = GetSafePath(safeFileName);

        await using var stream = File.Create(target);
        await file.CopyToAsync(stream);

        _logger.LogInformation("Bilaga sparad som {Target}", LogCleaner.Clean(target));
        return safeFileName;
    }

    public void Delete(string name)
    {
        var safePath = GetSafePath(name);
        if (File.Exists(safePath))
        {
            File.Delete(safePath);
        }
    }

    private string GetSafePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Filnamn kan inte vara tomt.", nameof(fileName));
        }

        var combinedPath = Path.Combine(_root, Path.GetFileName(fileName));
        var fullPath = Path.GetFullPath(combinedPath);

        if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Försök till path traversal upptäckt med filnamn: {FileName}", LogCleaner.Clean(fileName));
            throw new UnauthorizedAccessException("Åtkomst nekar: Ogiltig sökväg.");
        }

        return fullPath;
    }
}