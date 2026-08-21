using Microsoft.Data.Sqlite;
using SakerLabb.Web.Infrastructure.Logging;

namespace SakerLabb.Web.Data;

public class TicketRepository
{
    private readonly Db _db;
    private readonly ILogger<TicketRepository> _logger;

    private static readonly Dictionary<string, string> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "id", "t.Id ASC" },
        { "id_desc", "t.Id DESC" },
        { "title", "t.Title ASC" },
        { "status", "t.Status ASC" },
        { "priority", "t.Priority ASC" },
        { "created", "t.Created DESC" }
    };

    public TicketRepository(Db db, ILogger<TicketRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<Ticket> Search(string? search, string? sort)
    {
        using var connection = _db.Open();
        var command = connection.CreateCommand();

        // Validate and sanitize the sort parameter via whitelist
        var order = (sort is not null && AllowedSortColumns.TryGetValue(sort, out var mappedOrder))
            ? mappedOrder
            : "t.Id DESC";

        var query = "SELECT t.Id, t.Title, t.Body, t.Status, t.Priority, t.OwnerId, t.Internal, t.Created, u.Username " +
                    "FROM Tickets t JOIN Users u ON u.Id = t.OwnerId ";

        if (!string.IsNullOrWhiteSpace(search))
        {
            query += "WHERE (t.Title LIKE $search OR t.Body LIKE $search) ";
            command.Parameters.AddWithValue("$search", $"%{search}%");
        }

        query += $"ORDER BY {order}";
        command.CommandText = query;

        _logger.LogInformation("Ärendesökning utförd med fritext {Search} och sortering {Sort}", LogCleaner.Clean(search), LogCleaner.Clean(order));

        return Read(command);
    }

    public Ticket? GetById(string id)
    {
        using var connection = _db.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT t.Id, t.Title, t.Body, t.Status, t.Priority, t.OwnerId, t.Internal, t.Created, u.Username " +
                              "FROM Tickets t JOIN Users u ON u.Id = t.OwnerId WHERE t.Id = $id";
        command.Parameters.AddWithValue("$id", id);

        return Read(command).FirstOrDefault();
    }

    public List<Comment> GetComments(string ticketId)
    {
        using var connection = _db.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TicketId, Author, Text, Created FROM Comments WHERE TicketId = $ticketId";
        command.Parameters.AddWithValue("$ticketId", ticketId);

        var comments = new List<Comment>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            comments.Add(new Comment
            {
                Id = reader.GetInt32(0),
                TicketId = reader.GetInt32(1),
                Author = reader.GetString(2),
                Text = reader.GetString(3),
                Created = reader.GetString(4)
            });
        }

        return comments;
    }

    public void AddComment(string ticketId, string author, string text)
    {
        using var connection = _db.Open();
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Comments (TicketId, Author, Text, Created) " +
                              "VALUES ($ticketId, $author, $text, $created)";

        command.Parameters.AddWithValue("$ticketId", ticketId);
        command.Parameters.AddWithValue("$author", author);
        command.Parameters.AddWithValue("$text", text);
        command.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));

        command.ExecuteNonQuery();
    }

    public void UpdateStatus(string ticketId, string status)
    {
        using var connection = _db.Open();
        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Tickets SET Status = $status WHERE Id = $id";

        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$id", ticketId);

        command.ExecuteNonQuery();
    }

    private static List<Ticket> Read(SqliteCommand command)
    {
        var tickets = new List<Ticket>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tickets.Add(new Ticket
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Body = reader.GetString(2),
                Status = reader.GetString(3),
                Priority = reader.GetString(4),
                OwnerId = reader.GetInt32(5),
                Internal = reader.GetInt32(6) == 1,
                Created = reader.GetString(7),
                OwnerName = reader.GetString(8)
            });
        }

        return tickets;
    }
}