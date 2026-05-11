using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Users;
using Npgsql;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT user_id, password_hash, name, role, created_at, updated_at
            FROM users
            WHERE email = @email;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("email", email);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new User(
            reader.GetInt32(0),
            email,
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetFieldValue<DateTimeOffset>(4),
            reader.GetFieldValue<DateTimeOffset>(5));
    }

    public async Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO users (name, email, password_hash)
            VALUES (@name, @email, @password_hash)
            RETURNING user_id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("email", email);
        command.Parameters.AddWithValue("password_hash", passwordHash);

        var id = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
        return new UserProfile(id, name, email);
    }

    public async Task<UserProfile?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT user_id, name, email
            FROM users
            WHERE user_id = @user_id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("user_id", userId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? new UserProfile(reader.GetInt32(0), reader.GetString(1), reader.GetString(2))
            : null;
    }

    public async Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT user_id, name, email
            FROM users
            WHERE name like '%' || @username || '%'
            LIMIT 1;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("username", username);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? new UserProfile(reader.GetInt32(0), reader.GetString(1), reader.GetString(2))
            : null;
    }
}
