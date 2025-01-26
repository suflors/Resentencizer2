namespace Resentencizer2.Database.Model
{
	public record OldSentence
	{
		public OldSentence(
			long MessageID,
			long FragmentNumber,
			long UserID,
			long ChannelID,
			long ServerID,
			string Text,
			long VersionNumber,
			long Deactivated,
			long InWordTable)
		{
			this.MessageID = MessageID;
			this.FragmentNumber = FragmentNumber;
			this.UserID = UserID;
			this.ChannelID = ChannelID;
			this.ServerID = ServerID;
			this.Text = Text;
			this.VersionNumber = VersionNumber;
			this.Deactivated = Deactivated;
			this.InWordTable = InWordTable;
		}

		public OldSentence()
			: this(0, 0, 0, 0, 0, string.Empty, 0, 0, 0) { }

		public long MessageID { get; init; }
		public long FragmentNumber { get; init; }
		public long UserID { get; init; }
		public long ChannelID { get; init; }
		public long ServerID { get; init; }
		public string Text { get; init; }
		public long VersionNumber { get; init; }
		public long Deactivated { get; init; }
		public long InWordTable { get; init; }
	}
}