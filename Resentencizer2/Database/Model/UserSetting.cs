namespace Resentencizer2.Database.Model
{
	public record UserSetting
	{
		public UserSetting(ulong ServerID, ulong UserID, string? RetortCommand)
		{
			this.ServerID = ServerID;
			this.UserID = UserID;
			this.RetortCommand = RetortCommand;
		}

		public UserSetting() : this(0, 0, null) { }

		public ulong ServerID { get; init; }
		public ulong UserID { get; init; }
		public string? RetortCommand { get; init; }
	}
}