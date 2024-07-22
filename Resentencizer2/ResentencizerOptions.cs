namespace Resentencizer2
{
	public class ResentencizerOptions
	{
		public string OldConnectionString { get; set; } = "YOU NEED TO SET THIS!";
		public int BatchSize { get; set; } = 50;
		public int CurrentVersion { get; set; }
	}
}
