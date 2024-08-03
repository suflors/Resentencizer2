namespace Resentencizer2.Database.Model
{
	public record OldSentence(
		long MessageID,
		long FragmentNumber,
		long UserID,
		long ChannelID,
		long ServerID,
		string Text,
		long VersionNumber,
		long Deactivated,
		long InWordTable
	);
}
