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

		public async Task<IEnumerable<OldSentence>> ReadOldSentences()
		{
			await using var connection = new SqliteConnection(options.OldConnectionString);
			connection.Open();

			var result = await connection.QueryAsync<OldSentence>($@"
SELECT {nameof(OldSentence.MessageID)}, {nameof(OldSentence.FragmentNumber)}, {nameof(OldSentence.UserID)}, {nameof(OldSentence.ChannelID)}, {nameof(OldSentence.ServerID)}, {nameof(OldSentence.Text)}, {nameof(OldSentence.VersionNumber)}, {nameof(OldSentence.Deactivated)}, {nameof(OldSentence.InWordTable)} FROM Sentence
WHERE {nameof(OldSentence.VersionNumber)} < @currentVersion AND {nameof(OldSentence.ServerID)} IS 1127358996890796062
ORDER BY {nameof(OldSentence.ChannelID)}
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
