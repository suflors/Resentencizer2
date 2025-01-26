namespace Resentencizer2.Database.Model
{
	public record class ServerSetting
	{
		public ulong ID { get; init; } = 0;
		public bool GlobalEnabled { get; init; } = false;
		public string RetortCommand { get; init; } = "speak";
		public bool ReactsEnabled { get; init; } = false;
		public bool WebhooksEnabled { get; init; } = false;

		public ServerSetting() { }

		public ServerSetting(ulong id, bool globalEnabled = false, string retortCommand = "speak", bool reactsEnabled = false, bool webhooksEnabled = false)
		{
			ID = id;
			GlobalEnabled = globalEnabled;
			RetortCommand = retortCommand;
			ReactsEnabled = reactsEnabled;
			WebhooksEnabled = webhooksEnabled;
		}
	}
}