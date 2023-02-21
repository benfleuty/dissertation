using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace UploadFilesLibrary;

public class SqlDataAccess : ISqlDataAccess
{
	private readonly IConfiguration _config;

	public SqlDataAccess(IConfiguration config)
	{
		_config = config;
	}

	public async Task SaveData(string storedProc,
							string connName,
							object parameters)
	{
		string connStr = _config.GetConnectionString(connName)
			?? throw new Exception($"Missing connection string at {connName}");

		using var conn = new SqlConnection(connStr);

		await conn.ExecuteAsync(storedProc,
						  parameters,
						  commandType: System.Data.CommandType.StoredProcedure);


	}
}