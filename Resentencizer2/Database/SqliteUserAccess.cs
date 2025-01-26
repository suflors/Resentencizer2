using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Resentencizer2.Database.Model;

namespace Resentencizer2.Database
{
	public class SqliteUserAccess
	{
		private readonly ResentencizerOptions options;

		public SqliteUserAccess(IOptions<ResentencizerOptions> options)
		{
			this.options = options.Value;
		}

		public async Task<IEnumerable<User>> ReadAllUsers()
		{
			await using var connection = new SqliteConnection(options.OldConnectionString);
			connection.Open();

			var result = await connection.QueryAsync<User>($@"select {nameof(User.ID)}, {nameof(User.Username)} from {nameof(User)}");

			connection.Close();

			return result;
		}
	}
}
