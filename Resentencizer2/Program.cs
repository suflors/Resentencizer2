using Discord.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resentencizer2.Database;

namespace Resentencizer2
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((hostContext, builder) =>
				{
					// Add other providers for JSON, etc.

					builder.AddUserSecrets<ResentencizerService>();
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<ResentencizerService>()
							.AddSingleton<DiscordRestClient>()
							.AddSingleton<SqliteOldSentenceAccess>();
					services.AddOptions<ResentencizerOptions>()
							.Bind(hostContext.Configuration.GetSection("Resentencizer"));
				});
	}
}