using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Resentencizer2
{
	public class ResentencizerService : IHostedService
	{
		private readonly IConfiguration configuration;
		private readonly DiscordRestClient client;

		public ResentencizerService(IConfiguration configuration, DiscordRestClient client)
		{
			this.configuration = configuration;
			this.client = client;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("starting ...");

			client.Log += Log;

			var token = configuration["Discord-API-Key"];

			await client.LoginAsync(TokenType.Bot, token);

			Console.WriteLine("hello");

			//await Looper();
		}

		private async Task Looper()
		{
			while (true)
			{
				await Task.WhenAll(Resentencizer2(), Task.Delay(TimeSpan.FromMinutes(1)));
			}
		}

		private async Task Resentencizer2()
		{
			var channel = await client.GetChannelAsync(1226673059314794638) as IMessageChannel;
			var message = await channel.GetMessageAsync(1268977833636528150);
			Console.WriteLine(message);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await client.LogoutAsync();
			Console.WriteLine("goodbye");
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
