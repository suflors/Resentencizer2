using AtelierTomato.Markov.Core;
using AtelierTomato.Markov.Model;
using AtelierTomato.Markov.Model.ObjectOID.Parser;
using AtelierTomato.Markov.Service.Discord;
using AtelierTomato.Markov.Storage;
using AtelierTomato.Markov.Storage.Sqlite;
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
							.AddSingleton<SqliteOldSentenceAccess>()
							.AddSingleton<SqliteUserAccess>()
							.AddSingleton<SqliteServerSettingAccess>()
							.AddSingleton<SqliteUserPermissionAccess>()
							.AddSingleton<OldSentenceRenderer>()
							.AddSingleton<SentenceParser>()
							.AddSingleton<IWordStatisticAccess, SqliteWordStatisticAccess>()
							.AddSingleton<ISentenceAccess, SqliteSentenceAccess>()
							.AddSingleton<IAuthorAccess, SqliteAuthorAccess>()
							.AddSingleton<ILocationSettingAccess, SqliteLocationSettingAccess>()
							.AddSingleton<IAuthorPermissionAccess, SqliteAuthorPermissionAccess>()
							.AddSingleton<DiscordSentenceParser>()
							.AddSingleton<DiscordObjectOIDBuilder>()
							.AddSingleton<DiscordSentenceBuilder>()
							.AddSingleton(_ => new MultiParser<IObjectOID>([new BookObjectOIDParser(), new SpecialObjectOIDParser(), new DiscordObjectOIDParser()]));
					services.AddOptions<ResentencizerOptions>()
							.Bind(hostContext.Configuration.GetSection("Resentencizer"));
					services.AddOptions<SentenceParserOptions>()
							.Bind(hostContext.Configuration.GetSection("SentenceParser"));
					services.AddOptions<SqliteAccessOptions>()
							.Bind(hostContext.Configuration.GetSection("SqliteAccess"));
					services.AddOptions<DiscordSentenceParserOptions>()
							.Bind(hostContext.Configuration.GetSection("DiscordSentenceParser"));
				});
	}
}