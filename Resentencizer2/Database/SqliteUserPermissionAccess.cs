using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Resentencizer2.Database.Model;

namespace Resentencizer2.Database
{
	public class SqliteUserPermissionAccess
	{
		private readonly ResentencizerOptions options;

		public SqliteUserPermissionAccess(IOptions<ResentencizerOptions> options)
		{
			this.options = options.Value;
		}

		public async Task<IEnumerable<UserPermission>> ReadAllUserPermissions()
		{
			await using var connection = new SqliteConnection(options.OldConnectionString);
			connection.Open();

			var result = await connection.QueryAsync<UserPermission>($@"
select {nameof(UserPermission.ServerID)}, {nameof(UserPermission.UserID)}, {nameof(UserPermission.UserAllowed)}, {nameof(UserPermission.ServerAllowed)}, {nameof(UserPermission.GlobalAllowed)}, {nameof(UserPermission.MimicMeAllowed)}
from {nameof(UserPermission)}
			");

			connection.Close();

			return result;
		}
	}
}
