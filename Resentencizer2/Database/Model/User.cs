namespace Resentencizer2.Database.Model
{
	public record User
	{
		public User(ulong ID, string Username)
		{
			this.ID = ID;
			this.Username = Username;
		}

		public User() : this(0, "Unknown") { }

		public ulong ID { get; init; }
		public string Username { get; init; }
	}
}