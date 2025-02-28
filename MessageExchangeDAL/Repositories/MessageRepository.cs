using MessageExchangeDAL.Models;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Logging;

namespace MessageExchangeDAL.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(string connectionString, ILogger<MessageRepository> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddMessageAsync(MessageDTO message)
        {
            _logger.LogInformation("Attempting to add message to database: {Text}, Sequence: {SequenceNumber}, Created At: {CreatedAt}",
                                   message.Text, message.SequenceNumber, message.CreatedAt);

            const string query = "INSERT INTO messages (SequenceNumber, Text, CreatedAt) VALUES (@SequenceNumber, @Text, @CreatedAt)";
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@SequenceNumber", message.SequenceNumber);
                command.Parameters.AddWithValue("@Text", message.Text);
                command.Parameters.AddWithValue("@CreatedAt", message.CreatedAt);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Message successfully added to database.");
                }
                else
                {
                    _logger.LogWarning("Message insertion returned no affected rows.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding message to database.");
                throw;
            }
        }

        public async Task<IEnumerable<MessageDTO>> GetMessagesByDateRangeAsync(DateTime from, DateTime to)
        {
            _logger.LogInformation("Fetching messages from {From} to {To}", from, to);

            const string query = "SELECT SequenceNumber, Text, CreatedAt FROM messages WHERE CreatedAt BETWEEN @From AND @To";
            var messages = new List<MessageDTO>();

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@From", from);
                command.Parameters.AddWithValue("@To", to);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    messages.Add(new MessageDTO
                    {
                        SequenceNumber = reader.GetInt32(0),
                        Text = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    });
                }

                _logger.LogInformation("Fetched {MessageCount} messages from the database.", messages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching messages from the database.");
                throw;
            }

            return messages;
        }
    }
}
