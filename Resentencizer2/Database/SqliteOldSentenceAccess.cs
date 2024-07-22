using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Resentencizer2.Database.Model;

namespace Resentencizer2.Database
{
	public class SqliteOldSentenceAccess
	{
		private readonly ResentencizerOptions options;

		public SqliteOldSentenceAccess(IOptions<ResentencizerOptions> options)
		{
			this.options = options.Value;
		}

		public async Task<IEnumerable<OldSentence>> ReadOldSentence()
		{
			await using var connection = new SqliteConnection(options.OldConnectionString);
			connection.Open();

			var result = await connection.QueryAsync<OldSentence>($@"
SELECT * FROM Sentence
WHERE {nameof(OldSentence.VersionNumber)} < @currentVersion
GROUP BY {nameof(OldSentence.MessageID)}
LIMIT @batchSize",
			new
			{
				batchSize = options.BatchSize,
				currentVersion = options.CurrentVersion
			});

			connection.Close();

			return result;
		}
	}
}
