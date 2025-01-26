namespace Resentencizer2.Database.Model
{
	public record UserPermission
	{
		public UserPermission(ulong ServerID, ulong UserID, bool UserAllowed = false, bool ServerAllowed = false, bool GlobalAllowed = false, bool MimicMeAllowed = false)
		{
			this.ServerID = ServerID;
			this.UserID = UserID;
			this.UserAllowed = UserAllowed;
			this.ServerAllowed = ServerAllowed;
			this.GlobalAllowed = GlobalAllowed;
			this.MimicMeAllowed = MimicMeAllowed;
		}

		public UserPermission() : this(0, 0, false, false, false, false) { }

		public ulong ServerID { get; init; }
		public ulong UserID { get; init; }
		public bool UserAllowed { get; init; }
		public bool ServerAllowed { get; init; }
		public bool GlobalAllowed { get; init; }
		public bool MimicMeAllowed { get; init; }
	}
}