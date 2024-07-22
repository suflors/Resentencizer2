namespace Resentencizer2.Database.Model
{
	public record OldSentence(
		ulong MessageID,
		int FragmentNumber,
		ulong UserID,
		ulong ChannelID,
		ulong ServerID,
		string Text,
		int VersionNumber,
		bool Deactivated = false,
		bool InWordTable = false
	);
}
