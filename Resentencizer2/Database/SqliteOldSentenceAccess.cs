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
WHERE {nameof(OldSentence.VersionNumber)} < @currentVersion
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

		public async Task WriteSentenceRange(IEnumerable<OldSentence> sentences)
		{
			await using var connection = new SqliteConnection(options.OldConnectionString);
			connection.Open();
			await using var transaction = await connection.BeginTransactionAsync();

			foreach (OldSentence sentence in sentences)
			{
				await WriteCore(sentence, connection);
			}

			await transaction.CommitAsync();
		}

		private static async Task WriteCore(OldSentence sentence, SqliteConnection connection)
		{
			await connection.ExecuteAsync($@"
insert into Sentence ( {nameof(OldSentence.MessageID)}, {nameof(OldSentence.FragmentNumber)}, {nameof(OldSentence.UserID)}, {nameof(OldSentence.ChannelID)}, {nameof(OldSentence.ServerID)}, {nameof(OldSentence.Text)}, {nameof(OldSentence.VersionNumber)}, {nameof(OldSentence.Deactivated)}, {nameof(OldSentence.InWordTable)})
Values ( @messageID, @fragmentNumber , @userID , @channelID , @serverID , @text, @versionNumber, @deactivated, @inWordTable )
on conflict ({nameof(OldSentence.MessageID)}, {nameof(OldSentence.FragmentNumber)}) do update set
{nameof(OldSentence.VersionNumber)} = excluded.{nameof(OldSentence.VersionNumber)}
",
			new
			{
				messageID = sentence.MessageID,
				fragmentNumber = sentence.FragmentNumber,
				userID = sentence.UserID,
				channelID = sentence.ChannelID,
				serverID = sentence.ServerID,
				text = sentence.Text,
				versionNumber = sentence.VersionNumber,
				deactivated = sentence.Deactivated,
				inWordTable = sentence.InWordTable
			});
		}
	}
}
