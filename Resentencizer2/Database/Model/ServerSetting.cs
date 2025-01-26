namespace Resentencizer2.Database.Model
{
	public record ServerSetting(
		ulong ID,
		bool GlobalEnabled = false,
		string RetortCommand = "speak",
		bool ReactsEnabled = false,
		bool WebhooksEnabled = false
	);
}
