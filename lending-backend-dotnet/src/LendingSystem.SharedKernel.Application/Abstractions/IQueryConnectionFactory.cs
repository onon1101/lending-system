using System.Data;

namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IQueryConnectionFactory
{
    IDbConnection CreateConnection();
}
