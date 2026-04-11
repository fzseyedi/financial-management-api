using System.Data;

namespace FinancialManagementApi.Application.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}