namespace Resentencizer2.Database.Model
{
	public record UserPermission(
		ulong ServerID,
		ulong UserID,
		bool UserAllowed = false,
		bool ServerAllowed = false,
		bool GlobalAllowed = false,
		bool MimicMeAllowed = false
	);
}
