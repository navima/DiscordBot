using System;
using System.Threading.Tasks;
using System.Text.Json;
using DiscordWebsocketServer;
using System.Collections.Generic;
using System.Net;
using RiotClasses;
using System.Linq;

namespace Server
{
	public class Program
	{
		public static async Task Main()
		{
			// Setup Unicode console encoding
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			// Get token from env
			var bot_token = Environment.GetEnvironmentVariable("token");

			// Setup application server
			var applServer = new ApplicationServer();

			// Setup Webhook Server
			var webServer = new DiscordWebsocketServer.DiscordWebsocketServer(applServer, bot_token);

			// Setup Events
			applServer.RespondWebsocketHandler += (sender, e) => { webServer.SendWebsocket(e); };
			applServer.RespondHTTPHandler += (sender, e) => { webServer.SendHTTP(e.data, e.uri); };

			// Start Webhook Server
			var webServerTask = webServer.Start();

			// Register/Update Slash Commands
			var application_id = Environment.GetEnvironmentVariable("application_id");
			List<ApplicationCommand> commands = new();
			commands.Add(new ApplicationCommand
			{
				name = "blep",
				type = ApplicationCommandType.CHAT_INPUT,
				description = "Send an animal photo",
				options = new ApplicationCommandOption[] {
					new ApplicationCommandOption {
						name= "animal",
						description="The type of animal",
						type = 3,
						required = true,
						choices = new Choice[] {
							new Choice { name="Dog", value="animal_dog" }
						}
					},
					new ApplicationCommandOption {
						name = "only_smol",
						description = "Whether to show only baby animals",
						type = 5,
						required = false
					}
				}
			});
			commands.Add(new ApplicationCommand
			{
				name = "status",
				type = ApplicationCommandType.CHAT_INPUT,
				description = "Get the status of the Riot servers."
			});
			commands.Add(new ApplicationCommand
			{
				name = "whois",
				type = ApplicationCommandType.CHAT_INPUT,
				description = "Get info of Summoner by Name",
				options = new ApplicationCommandOption[]
				{
					new ApplicationCommandOption
					{
						name = "summoner_name",
						description = "The summoner name",
						type = 3,
						required = true
					}
				}
			});
			commands.ForEach(command =>
			{
				//webServer.SendHTTP(command, new Uri($"https://discord.com/api/v8/applications/{application_id}/commands"));
			});



			/* */
			await webServerTask;
		}
	}
	class ApplicationServer : DiscordInterface
	{
		override public void Process(Payload payload, string rawData)
		{
			switch (payload.t)
			{
				case "MESSAGE_CREATE":
					var message_create_payload = JsonSerializer.Deserialize(rawData, typeof(Payload<Message>)) as Payload<Message>;
					if (message_create_payload.d.author.id != self_id)
					{
						Message message = message_create_payload.d;
						Console.WriteLine(message.content);


						//OnRespondHTTP(new Message() { content = message.content }, new Uri($"https://discordapp.com/api/channels/{message.channel_id}/messages"));
					}
					break;
				case "INTERACTION_CREATE":
					var interaction_create_payload = JsonSerializer.Deserialize(rawData, typeof(Payload<InteractionObject>)) as Payload<InteractionObject>;
					Console.WriteLine($"Received Slash Command: {interaction_create_payload.d.data.name}");
					var interactionObject = interaction_create_payload.d;
					ProcessSlash(interactionObject.data.name, interactionObject);
					break;
				case "READY":
					var ready_payload = JsonSerializer.Deserialize(rawData, typeof(Payload<Ready>)) as Payload<Ready>;
					var ready = ready_payload?.d;
					session_id = ready.session_id;
					self_id = ready.user.id;
					break;
				default:
					break;
			}
		}
#nullable enable
		public void ProcessSlash(string name, InteractionObject interactionObject)
		{
			var interaction_id = interactionObject.id;
			var interaction_token = interactionObject.token;

			InteractionResponse? interaction_response = null;


			if (interactionObject.data is not null)
			{
				if (SlashMethods.TryGetValue(name, out Func<DiscordInterface, InteractionObject, InteractionResponse?>? f))
				{
					interaction_response = f?.Invoke(this, interactionObject);

					if (interaction_response is not null)
					{
						Console.WriteLine("Responded.");
						OnRespondHTTP(interaction_response,
									  new Uri($"https://discord.com/api/v8/interactions/{interaction_id}/{interaction_token}/callback"));
					}
					else
					{
						Console.WriteLine($"No response from Slash method handler ({name})");
						InteractionResponse defResponse = new()
						{
							type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
							data = new InteractionCallbackData()
							{
								content = $"Error! Something unexpected happened! ({$"No response from Slash method handler ({name})"})"
							}
						};
						OnRespondHTTP(defResponse,
									  new Uri($"https://discord.com/api/v8/interactions/{interaction_id}/{interaction_token}/callback"));
					}
				}
				else
				{
					Console.WriteLine($"Slash method handler not found! ({name})");
					InteractionResponse defResponse = new()
					{
						type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
						data = new InteractionCallbackData()
						{
							content = $"Error! Something unexpected happened! ({$"Slash method handler not found! ({name})"})"
						}
					};
					OnRespondHTTP(defResponse,
									  new Uri($"https://discord.com/api/v8/interactions/{interaction_id}/{interaction_token}/callback"));
				}
			}
		}


		public Dictionary<string, Func<DiscordInterface, InteractionObject, InteractionResponse?>> SlashMethods = new()
		{
			["blep"] = (@self, interactionObject) =>
			{
				string? username = interactionObject.GetUser().username;
				InteractionResponse response = new()
				{
					type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
					data = new InteractionCallbackData { content = $"\\**bleps {username ?? "I have no idea who called me"}\\**" }
				};
				return response;
			},
			["status"] = (@self, interactionObject) =>
			{
				Uri url = new("https://eun1.api.riotgames.com/lol/status/v4/platform-data");
				var riot_token = Environment.GetEnvironmentVariable("riot_token");


				WebHeaderCollection headers = new();
				headers.Add("X-Riot-Token", riot_token);


				using WebClient client = new();
				client.Headers = headers;

				string resStr;
				try
				{
					resStr = client.DownloadString(url);
				}
				catch (WebException e)
				{
					return new InteractionResponse { type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE, data = new InteractionCallbackData { content = $"Error! Riot api returned non-2 http code! ({e.Response})" } };
				}
				Console.WriteLine("Riot API returned: " + resStr);


				PlatformDataDto? res = JsonSerializer.Deserialize(resStr, typeof(PlatformDataDto)) as PlatformDataDto;

				string contentStr = string.Join("\n", res?.maintenances.Select(x =>
				{
					var title = x.titles.Where(y => y.locale == "en_GB").FirstOrDefault()?.content ?? "No english locale found";
					var contents = string.Join("\n", x.updates.Select(z => z.translations.Where(a => a.locale == "en_GB").FirstOrDefault()?.content ?? "No english locale found"));
					return title+ ": " + contents;
				}) ?? new List<string> { "No maintenance updates" });

				InteractionResponse response = new()
				{
					type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
					data = new InteractionCallbackData()
					{
						content = contentStr
					}
				};
				return response;
			},
			["whois"] = (@self, interactionObject) =>
			{

				string? name = interactionObject.data?.options?[0].value?.GetString();
				if (name is null)
					return new InteractionResponse
					{
						type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
						data = new InteractionCallbackData
						{
							content = "Error! Failed to parse provided name!"
						}
					};


				Uri url = new($"https://eun1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{name}");
				var riot_token = Environment.GetEnvironmentVariable("riot_token");


				WebHeaderCollection headers = new();
				headers.Add("X-Riot-Token", riot_token);


				using WebClient client = new();
				client.Headers = headers;


				Console.WriteLine("GETting from: " + url);
				string resStr;
				try
				{
					resStr = client.DownloadString(url);
				}
				catch (WebException e)
				{
					return new InteractionResponse { type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE, data = new InteractionCallbackData { content = $"Error! Riot api returned non-2 http code! ({e.Response})" } };
				}

				var res = JsonSerializer.Deserialize(resStr, typeof(SummonerDto)) as SummonerDto;

				InteractionResponse response = new()
				{
					type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
					data = new InteractionCallbackData()
					{
						content = $"```json\n{resStr.Replace(",", ",\n")}```"
					}
				};
				return response;
			}
		};
	}
}