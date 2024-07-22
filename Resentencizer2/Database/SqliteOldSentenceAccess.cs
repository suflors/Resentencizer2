using Microsoft.Extensions.Options;

namespace Resentencizer2.Database
{
	public class SqliteOldSentenceAccess
	{
		private readonly ResentencizerOptions options;

		public SqliteOldSentenceAccess(IOptions<ResentencizerOptions> options)
		{
			this.options = options.Value;
		}
	}
}
