using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Resentencizer2.Database;
using Resentencizer2.Database.Model;

namespace Resentencizer2
{
	public class ResentencizerService : IHostedService
	{
		private readonly IConfiguration configuration;
		private readonly DiscordRestClient client;
		private readonly SqliteOldSentenceAccess oldSentenceAccess;

		public ResentencizerService(IConfiguration configuration, DiscordRestClient client, SqliteOldSentenceAccess oldSentenceAccess)
		{
			this.configuration = configuration;
			this.client = client;
			this.oldSentenceAccess = oldSentenceAccess;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("starting ...");

			client.Log += Log;

			var token = configuration["Discord-API-Key"];

			await client.LoginAsync(TokenType.Bot, token);

			Console.WriteLine("hello");

			await Looper();
		}

		private async Task Looper()
		{
			bool finished = false;
			while (!finished)
			{
				IEnumerable<OldSentence> oldSentences = await oldSentenceAccess.ReadOldSentences();
				int currentBatch = 0;
				if (oldSentences.Any())
				{
					currentBatch++;
					await Task.WhenAll(Resentencizer2(oldSentences, currentBatch), Task.Delay(TimeSpan.FromMinutes(1)));
				} else
				{
					finished = true;
				}
			}
		}

		private async Task Resentencizer2(IEnumerable<OldSentence> oldSentences, int currentBatch)
		{
			Console.WriteLine($"Batch {currentBatch}: beginning Resentencization ...");
			var groupedByGuild = oldSentences.GroupBy(s => s.ServerID);
			var guilds = await client.GetGuildsAsync();
			foreach (var guildGroup in groupedByGuild)
			{
				Console.WriteLine($"Batch {currentBatch}: Processing for ServerID {guildGroup.Key} ...");
				IEnumerable<OldSentence> sentencesInGuild = guildGroup;
				if (guilds.Any(g => g.Id == (ulong)guildGroup.Key))
				{
					var guild = guilds.Where(g => g.Id == (ulong)guildGroup.Key).FirstOrDefault();
					Console.WriteLine($"Batch {currentBatch}: Server with ID {guild!.Id} \"{guild.Name}\" found ...");
					var groupedByChannel = sentencesInGuild.GroupBy(s => s.ChannelID);
					var channels = await guild.GetChannelsAsync();
					foreach (var channelGroup in groupedByChannel)
					{
						Console.WriteLine($"Batch {currentBatch}: Processing for ChannelID {channelGroup.Key} ...");
						IEnumerable<OldSentence> sentencesInChannel = channelGroup;
						if (channels.Any(c => c.Id == (ulong)channelGroup.Key))
						{
							var channel = channels.Where(c => c.Id == (ulong)channelGroup.Key).FirstOrDefault() as IMessageChannel;
							Console.WriteLine($"Batch {currentBatch}: Channel with ID {channel!.Id} \"{channel.Name}\" found ...");
							Console.WriteLine($"Batch {currentBatch}: Attempting to get each individual message from Discord, processing from current database those that are not found ...");
							IEnumerable<IMessage> foundMessages = [];
							IEnumerable<OldSentence> unfoundOldSentences = [];
							foreach (var oldSentence in sentencesInChannel)
							{
								if (!foundMessages.Any(m => m?.Id == (ulong)oldSentence.MessageID) && !unfoundOldSentences.Any(s => s?.MessageID == oldSentence.MessageID))
								{
									var message = await channel.GetMessageAsync((ulong)oldSentence.MessageID);
									if (message is not null)
									{
										foundMessages = foundMessages.Append(message!);
									} else
									{
										unfoundOldSentences = unfoundOldSentences.Append(oldSentence);
									}
								} else if (unfoundOldSentences.Any(s => s.MessageID == oldSentence.MessageID))
								{
									unfoundOldSentences = unfoundOldSentences.Append(oldSentence);
								} else
								{
									// If the message ID is found in foundMessages, we don't need to re-process. Continue to next.
								}
							}
							if (foundMessages.Any())
							{
								Console.WriteLine($"Batch {currentBatch}: {foundMessages.Count()} messages queried from Discord, processing ...");
								await ProcessFromDiscordMessage(foundMessages);
							} else
							{
								Console.WriteLine($"Batch {currentBatch}: No messages could queried from Discord ...");
							}
							if (unfoundOldSentences.Any())
							{
								Console.WriteLine($"Batch {currentBatch}: {unfoundOldSentences.Count()} sentences unable to be queried, processing from current database Sentences ...");
								await ProcessFromDatabase(unfoundOldSentences);
							} else
							{
								Console.WriteLine($"Batch {currentBatch}: No messages needed to be processed from current database Sentences ...");
							}
						} else
						{
							Console.WriteLine($"Batch {currentBatch}: no Channel with ID {channelGroup.Key} found, processing from current database Sentences ...");
							await ProcessFromDatabase(sentencesInChannel);
						}
					}
				} else
				{
					Console.WriteLine($"Batch {currentBatch}: no Server with ID {guildGroup.Key} found, processing from current database Sentences ...");
					await ProcessFromDatabase(sentencesInGuild);
				}
			}
		}

		private async Task ProcessFromDiscordMessage(IEnumerable<IMessage> foundMessages)
		{
			throw new NotImplementedException();
		}

		public async Task ProcessFromDatabase(IEnumerable<OldSentence> oldSentences)
		{
			throw new NotImplementedException();
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
