using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using static System.Text.Encoding;
using System.Text.Json;
using System.Net;


#nullable enable

namespace DiscordWebsocketServer
{
	public class DiscordWebsocketServer
	{
		public readonly DiscordInterface applicationServer;
		private readonly string bot_token;
		private int? heartbeat_interval;
		private Task? heartbeatTask;
		private int? last_sequence_received;

		private ClientWebSocket socket = new();
		readonly Uri url = new("wss://gateway.discord.gg/?v=9&encoding=json");

		public readonly JsonSerializerOptions jsonSerializerOptions = new();


		// Yay DI
		public DiscordWebsocketServer(DiscordInterface applicationServer, string bot_token)
		{
			this.applicationServer = applicationServer;
			this.bot_token = bot_token;

			jsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault;
		}

		public async Task Start()
		{
			// Signal start to user
			Console.WriteLine("Webhook Server connecting...");

			// Setup WebSocket
			socket = new();
			await socket.ConnectAsync(url, System.Threading.CancellationToken.None);
			Console.WriteLine("Webhook Server connected.");

			while (socket.State != WebSocketState.Closed)
			{
				var buf = new byte[4096];
				var res = await socket.ReceiveAsync(buf, System.Threading.CancellationToken.None);
				var resStr = UTF8.GetString(new ArraySegment<byte>(buf, 0, res.Count));

				Payload? resPayload = null;
				try
				{
					resPayload = JsonSerializer.Deserialize(resStr, typeof(Payload)) as Payload;
				}
				catch
				{
					Console.WriteLine("FAILED TO PARSE RESPONSE");
				}

				last_sequence_received = resPayload?.s;

				Console.WriteLine($"received status: {res.CloseStatus}: {res.CloseStatusDescription}, data: {resStr}");


				if (resPayload is not null)
				{
					switch (resPayload.op)
					{
						case 1:
							_ = SendHeartbeat();
							break;
						case 10:
							DoLogin();
							var hello_payload = JsonSerializer.Deserialize(resStr, typeof(Payload<Hello>)) as Payload<Hello>;

							heartbeat_interval = hello_payload?.d?.heartbeat_interval;
							if (heartbeat_interval is not null)
							{
								heartbeatTask = new Task(async () =>
								{
									do
									{
										await Task.Delay(heartbeat_interval ?? 30000);
										Console.WriteLine("heartbeat:");
										await SendHeartbeat();
									} while (true);
								});
								heartbeatTask.Start();
							}
							break;
						case 11:
							Console.WriteLine("ACK (heartbeat)");
							break;
						case 0:
							applicationServer.Process(resPayload, resStr);
							break;
						default:
							break;
					}
				}
			}
		}
		private async void DoLogin()
		{
			Properties props = new() { os = "Windows", browser = "wat", device = "wat" };
			Identify id = new() { token = bot_token, properties = props, intents = 512 };
			Payload<Identify> payload = new(id) { op = 2 };

			await SendWebsocket(payload);
		}

		private async Task SendHeartbeat()
		{
			await SendWebsocket(new Payload() { op = 1, d = last_sequence_received });
		}

		/// <summary>
		/// Send response json on WebSocket connection.
		/// </summary>
		/// <param name="rawData"></param>
		/// <param name="messageType"></param>
		/// <returns></returns>
		public Task SendWebsocket(object obj)
		{
			try
			{
				var jsonStr = JsonSerializer.Serialize(obj, jsonSerializerOptions);
				Console.WriteLine("sending (websocket): " + jsonStr);
				return socket.SendAsync(UTF8.GetBytes(jsonStr), WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				throw;
			}
		}

		/// <summary>
		/// Send HTTP POST request to API endpoint.
		/// </summary>
		/// <param name="rawData">The data to send</param>
		/// <param name="uri">The api endpoint to send to</param>
		public void SendHTTP(object obj, Uri uri)
		{
			WebHeaderCollection headers = new();
			headers.Add(HttpRequestHeader.Authorization, $"Bot {bot_token}");
			headers.Add(HttpRequestHeader.UserAgent, "asdasd");
			headers.Add(HttpRequestHeader.ContentType, "application/json");

			using WebClient client = new();
			client.Headers = headers;
			var jsonStr = JsonSerializer.Serialize(obj, jsonSerializerOptions);
			client.UploadStringAsync(uri, jsonStr);
			Console.WriteLine("sending (http): " + jsonStr);
		}
	}
}
