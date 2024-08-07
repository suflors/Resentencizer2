using AtelierTomato.Markov.Core;
using AtelierTomato.Markov.Model;
using AtelierTomato.Markov.Model.ObjectOID;
using AtelierTomato.Markov.Service.Discord;
using AtelierTomato.Markov.Storage;
using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Resentencizer2.Database;
using Resentencizer2.Database.Model;
using System.Text.RegularExpressions;

namespace Resentencizer2
{
	public class ResentencizerService : IHostedService
	{
		private readonly IConfiguration configuration;
		private readonly DiscordRestClient client;
		private readonly SqliteOldSentenceAccess oldSentenceAccess;
		private readonly OldSentenceRenderer oldSentenceRenderer;
		private readonly SentenceParser sentenceParser;
		private readonly IWordStatisticAccess wordStatisticAccess;
		private readonly ISentenceAccess sentenceAccess;
		private readonly ResentencizerOptions resentencizerOptions;
		private readonly DiscordSentenceParser discordSentenceParser;

		private readonly Regex RemoveEscapement = new Regex(@"\\(.)", RegexOptions.Compiled);
		public ResentencizerService(IConfiguration configuration, DiscordRestClient client, SqliteOldSentenceAccess oldSentenceAccess, OldSentenceRenderer oldSentenceRenderer, SentenceParser sentenceParser, IWordStatisticAccess wordStatisticAccess, ISentenceAccess sentenceAccess, IOptions<ResentencizerOptions> resentencizerOptions, DiscordSentenceParser discordSentenceParser)
		{
			this.configuration = configuration;
			this.client = client;
			this.oldSentenceAccess = oldSentenceAccess;
			this.oldSentenceRenderer = oldSentenceRenderer;
			this.sentenceParser = sentenceParser;
			this.wordStatisticAccess = wordStatisticAccess;
			this.sentenceAccess = sentenceAccess;
			this.resentencizerOptions = resentencizerOptions.Value;
			this.discordSentenceParser = discordSentenceParser;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			Console.WriteLine("starting ...");

			client.Log += Log;

			var token = configuration["Discord-API-Key"];

			await client.LoginAsync(TokenType.Bot, token);

			Console.WriteLine("hello");

			_ = Looper(cancellationToken);
		}

		private async Task Looper(CancellationToken cancelToken)
		{
			bool finished = false;
			int currentBatch = 0;
			while (!finished && !cancelToken.IsCancellationRequested)
			{
				IEnumerable<OldSentence> oldSentences = await oldSentenceAccess.ReadOldSentences();
				if (oldSentences.Any())
				{
					currentBatch++;
					await Task.WhenAll(Resentencizer2(oldSentences, currentBatch), Task.Delay(TimeSpan.FromMinutes(1)));
				} else
				{
					finished = true;
				}
			}
			Console.WriteLine($"Finished processing {(currentBatch)} batches of OldSentences.");
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
						bool channelFound = channels.Any(c => c.Id == (ulong)channelGroup.Key);
						bool threadFound;
						IThreadChannel? thread = null;
						if (!channelFound)
						{
							thread = await guild.GetThreadChannelAsync((ulong)channelGroup.Key);
							if (thread is not null)
							{
								threadFound = true;
							} else
							{
								threadFound = false;
							}
						} else
						{
							threadFound = false;
						}
						if (channelFound || threadFound)
						{
							IMessageChannel? channel;
							if (channelFound)
							{
								channel = channels.Where(c => c.Id == (ulong)channelGroup.Key).FirstOrDefault() as IMessageChannel;
							} else if (threadFound)
							{
								channel = thread;
							} else
							{
								throw new InvalidOperationException("This should not be possible.");
							}
							var cahannel = channels.Where(c => c.Id == (ulong)channelGroup.Key).FirstOrDefault() as IMessageChannel;
							Console.WriteLine($"Batch {currentBatch}: Channel with ID {channel!.Id} \"{channel.Name}\" found ...");
							Console.WriteLine($"Batch {currentBatch}: Attempting to get each individual message from Discord, processing from current database those that are not found ...");
							IEnumerable<IMessage> foundMessages = [];
							IEnumerable<OldSentence> unfoundOldSentences = [];
							var botUser = await guild.GetUserAsync(client.CurrentUser.Id);
							ChannelPermissions? permissions = null;
							if (channel is IThreadChannel threadChannel)
							{
								permissions = botUser.GetPermissions(await guild.GetChannelAsync(threadChannel.CategoryId!.Value));
							} else
							{
								permissions = botUser.GetPermissions(channel as IGuildChannel);
							}

							if (permissions.Value.ViewChannel && permissions.Value.ReadMessageHistory)
							{
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
									await ProcessFromDiscordMessage(foundMessages, guild, channel);
								} else
								{
									Console.WriteLine($"Batch {currentBatch}: No messages could queried from Discord ...");
								}
								if (unfoundOldSentences.Any())
								{
									Console.WriteLine($"Batch {currentBatch}: {unfoundOldSentences.Count()} sentences unable to be queried, processing from current database Sentences ...");
									await ProcessFromDatabase(unfoundOldSentences, guild, channel);
								} else
								{
									Console.WriteLine($"Batch {currentBatch}: No messages needed to be processed from current database Sentences ...");
								}
							} else
							{
								Console.WriteLine($"Batch {currentBatch}: cannot view channel, processing all messages from channel from current database Sentences ...");
								sentencesInChannel = await oldSentenceAccess.ReadOldSentencesByChannelID(channelGroup.Key);
								await ProcessFromDatabase(sentencesInChannel, guild, channel);
							}

						} else
						{
							Console.WriteLine($"Batch {currentBatch}: no Channel with ID {channelGroup.Key} found, processing all messages from channel from current database Sentences ...");
							sentencesInChannel = await oldSentenceAccess.ReadOldSentencesByChannelID(channelGroup.Key);
							await ProcessFromDatabase(sentencesInChannel);
						}
					}
				} else
				{
					Console.WriteLine($"Batch {currentBatch}: no Server with ID {guildGroup.Key} found, processing all messages from server from current database Sentences ...");
					sentencesInGuild = await oldSentenceAccess.ReadOldSentencesByServerID(guildGroup.Key);
					await ProcessFromDatabase(sentencesInGuild);
				}
			}
			Console.WriteLine($"Batch {currentBatch}: complete!");
		}

		private async Task ProcessFromDiscordMessage(IEnumerable<IMessage> foundMessages, IGuild guild, IChannel channel)
		{
			IEnumerable<Sentence> sentences = [];
			foreach (var message in foundMessages)
			{
				if (message is IUserMessage userMessage)
				{
					IEnumerable<string> parsedMessage = discordSentenceParser.ParseIntoSentenceTexts(userMessage.Content, userMessage.Tags);
					if (parsedMessage.Any())
					{
						foreach (string parsedText in parsedMessage)
						{
							await wordStatisticAccess.WriteWordStatisticsFromString(parsedText);
						}
						sentences = sentences.Concat(await DiscordSentenceBuilder.Build(guild, channel, userMessage.Id, userMessage.Author.Id, userMessage.CreatedAt, parsedMessage));
					}
				}
			}
			await sentenceAccess.WriteSentenceRange(sentences);
			Console.WriteLine($"\tWrote {sentences.Count()} Sentences parsed from Discord to the database.");
			IEnumerable<long> messageIDs = foundMessages.Select(m => (long)m.Id);
			IEnumerable<OldSentence> oldSentences = await oldSentenceAccess.ReadOldSentencesByMessageIDs(messageIDs);
			Console.WriteLine($"\tRead {oldSentences.Count()} OldSentences from the database to update.");
			oldSentences = oldSentences.Select(s => new OldSentence(s.MessageID, s.FragmentNumber, s.UserID, s.ChannelID, s.ServerID, s.Text, resentencizerOptions.CurrentVersion, s.Deactivated, s.InWordTable));
			await oldSentenceAccess.WriteSentenceRange(oldSentences);
			Console.WriteLine($"\tUpdated version number for {oldSentences.Count()} OldSentences.");
		}

		public async Task ProcessFromDatabase(IEnumerable<OldSentence> oldSentences, IGuild? guild = null, IChannel? channel = null)
		{
			var groupedByMessage = oldSentences.GroupBy(s => s.MessageID);
			IEnumerable<Sentence> sentences = [];
			foreach (var group in groupedByMessage)
			{
				IEnumerable<OldSentence> sentencesInGroup = group;
				OldSentence firstSentence = sentencesInGroup.First();
				IEnumerable<string> renderedTexts = sentencesInGroup.Select(s => RemoveEscapement.Replace(oldSentenceRenderer.Render(s.Text), m => m.Groups[1].Value));
				IEnumerable<string> reparsedTexts = renderedTexts.SelectMany(discordSentenceParser.ParseIntoSentenceTexts); // need the discord sentence parser because of escapement
				reparsedTexts = reparsedTexts.SelectMany(sentenceParser.ParseIntoSentenceTexts); // after removing escapement, we need to re-parse it. discord parser is not necessary
				DiscordObjectOID messageOID;
				if (guild is not null && channel is not null)
				{
					messageOID = await DiscordObjectOIDBuilder.Build(guild, channel, (ulong)firstSentence.MessageID);
				} else
				{
					messageOID = DiscordObjectOID.ForMessage("discord.com", (ulong)firstSentence.ServerID, 0, (ulong)firstSentence.ChannelID, 0, (ulong)firstSentence.MessageID);
				}
				AuthorOID author = new(ServiceType.Discord, "discord.com", firstSentence.UserID.ToString());
				IEnumerable<Sentence> newSentences = reparsedTexts.Select((text, index) =>
					new Sentence(
						messageOID.WithSentence(index),
						author,
						SnowflakeUtils.FromSnowflake((ulong)firstSentence.MessageID),
						text
					)
				);
				sentences = sentences.Concat(newSentences);
			}
			Console.WriteLine($"\tCreated {sentences.Count()} Sentences, attempting to write to the new database ...");
			await wordStatisticAccess.WriteWordStatisticsFromString(string.Join(' ', sentences.Select(s => s.Text)));
			Console.WriteLine($"\tWrote WordStatistics from {sentences.Count()} Sentences to the database.");
			await sentenceAccess.WriteSentenceRange(sentences);
			Console.WriteLine($"\tWrote {sentences.Count()} Sentences to the database.");
			oldSentences = oldSentences.Select(s => new OldSentence(s.MessageID, s.FragmentNumber, s.UserID, s.ChannelID, s.ServerID, s.Text, resentencizerOptions.CurrentVersion, s.Deactivated, s.InWordTable));
			await oldSentenceAccess.WriteSentenceRange(oldSentences);
			Console.WriteLine($"\tUpdated version number for {oldSentences.Count()} OldSentences.");

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
