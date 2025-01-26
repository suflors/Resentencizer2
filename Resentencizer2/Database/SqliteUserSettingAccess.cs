using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Resentencizer2.Database.Model;

namespace Resentencizer2.Database
{
	public class SqliteUserSettingAccess
	{
		private readonly ResentencizerOptions options;

		public SqliteUserSettingAccess(IOptions<ResentencizerOptions> options)
		{
			this.options = options.Value;
		}

		public async Task<IEnumerable<UserSetting>> ReadAllUserSetting()
		{
			await using var connection = new SqliteConnection(options.OldConnectionString);
			connection.Open();

			var result = await connection.QueryAsync<UserSetting>($@"
select {nameof(UserSetting.ServerID)}, {nameof(UserSetting.UserID)}, {nameof(UserSetting.RetortCommand)}
from {nameof(UserSetting)}");

			connection.Close();

			return result;
		}
	}
}
