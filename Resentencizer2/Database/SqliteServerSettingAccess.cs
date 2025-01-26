using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Resentencizer2.Database.Model;

namespace Resentencizer2.Database
{
	public class SqliteServerSettingAccess
	{
		private readonly ResentencizerOptions options;

		public SqliteServerSettingAccess(IOptions<ResentencizerOptions> options)
		{
			this.options = options.Value;
		}

		public async Task<IEnumerable<ServerSetting>> ReadAllServerSettings()
		{
			await using var connection = new SqliteConnection(options.OldConnectionString);
			connection.Open();

			var result = await connection.QueryAsync<ServerSetting>($@"
select {nameof(ServerSetting.ID)}, {nameof(ServerSetting.GlobalEnabled)}, {nameof(ServerSetting.RetortCommand)}, {nameof(ServerSetting.ReactsEnabled)}, {nameof(ServerSetting.WebhooksEnabled)}
from {nameof(ServerSetting)}
where {nameof(ServerSetting.ID)}");

			connection.Close();

			return result;
		}

	}
}
