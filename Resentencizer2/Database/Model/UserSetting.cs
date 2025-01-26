namespace Resentencizer2.Database.Model
{
	public record UserSetting(
		ulong ServerID,
		ulong UserID,
		string? RetortCommand
	);
}
