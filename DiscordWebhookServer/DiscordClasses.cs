using System;
using System.Text.Json.Serialization;

#nullable enable
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


namespace DiscordWebsocketServer
{
	public class Payload<T>
	{
		public int op { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public T? d { get; set; }
		public int? s { get; set; }
		public string? t { get; set; }
		public Payload() { }
		public Payload(T data) { d = data; }
	}
	public class Payload : Payload<dynamic> { }

	public class Identify
	{

		public string token { get; set; }
		public Properties properties { get; set; }
		public bool? compress { get; set; }
		public int? large_threshold { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int[] shard { get; set; }
		public Presence? presence { get; set; }
		public int intents { get; set; }
	}

	public class Properties
	{
		[JsonPropertyName("$os")]
		public string os { get; set; }
		[JsonPropertyName("$browser")]
		public string browser { get; set; }
		[JsonPropertyName("$device")]
		public string device { get; set; }
	}

	public class Presence
	{
		public Activity[] activities { get; set; }
		public string status { get; set; }
		public int? since { get; set; }
		public bool afk { get; set; }
	}

	public class Activity
	{
		public string name { get; set; }
		public int type { get; set; }
	}


	public class Message
	{
		public Reaction[] reactions { get; set; }
		public object[] attachments { get; set; }
		public bool tts { get; set; }
		public object[] embeds { get; set; }
		public DateTime timestamp { get; set; }
		public bool mention_everyone { get; set; }
		public string id { get; set; }
		public bool pinned { get; set; }
		public DateTime? edited_timestamp { get; set; }
		public User author { get; set; }
		public string[] mention_roles { get; set; }
		public object[]? mention_channels { get; set; }
		public string content { get; set; }
		public string channel_id { get; set; }
		public string? guild_id { get; set; }
		public User[] mentions { get; set; }
		public int type { get; set; }
		public GuildMember? member { get; set; }
	}

	public class Reaction
	{
		public int count { get; set; }
		public bool me { get; set; }
		public Emoji emoji { get; set; }
	}

	public class Emoji
	{
		public object id { get; set; }
		public string name { get; set; }
	}


	public class Ready
	{
		public int v { get; set; }
		public User_Settings user_settings { get; set; }
		public User user { get; set; }
		public string session_id { get; set; }
		public object[] relationships { get; set; }
		public object[] private_channels { get; set; }
		public object[] presences { get; set; }
		public Guild[] guilds { get; set; }
		public object[] guild_join_requests { get; set; }
		public string[] geo_ordered_rtc_regions { get; set; }
		public Application application { get; set; }
		public string[] _trace { get; set; }
	}

	public class User_Settings
	{
	}

	public class User
	{
		public bool verified { get; set; }
		public string username { get; set; }
		public bool mfa_enabled { get; set; }
		public string id { get; set; }
		public int flags { get; set; }
		public object email { get; set; }
		public string discriminator { get; set; }
		public bool bot { get; set; }
		public object avatar { get; set; }
	}

	public class Application
	{
		public string id { get; set; }
		public int flags { get; set; }
	}

	public class Guild
	{
		public bool unavailable { get; set; }
		public string id { get; set; }
	}

	public enum ApplicationCommandType
	{
		CHAT_INPUT = 1,
		USER = 2,
		MESSAGE = 3,
	}

	public class ApplicationCommand
	{
		public string id { get; set; }
		public string name { get; set; }
		public ApplicationCommandType? type { get; set; }
		public string description { get; set; }
		public ApplicationCommandOption[] options { get; set; }
	}

	public class ApplicationCommandOption
	{
		public string name { get; set; }
		public string description { get; set; }
		public int type { get; set; }
		public bool required { get; set; }
		public Choice[] choices { get; set; }
	}

	public class Choice
	{
		public string name { get; set; }
		public string value { get; set; }
	}

	public class GuildMember
	{
		public User? user { get; set; }
		public string? nick { get; set; }
		public string[] roles { get; set; }
		public DateTime joined_at { get; set; }
		public DateTime? premium_since { get; set; }
		public bool deaf { get; set; }
		public bool mute { get; set; }
		public bool? pending { get; set; }
		public bool? is_pending { get; set; }
		public string? permissions { get; set; }
		public object avatar { get; set; }
	}

	public enum InteractionType
	{
		PING = 1,
		APPLICATION_COMMAND = 2,
		MESSAGE_COMPONENT = 3,
	}

	public class InteractionObject
	{
		public int version { get; set; }
		public InteractionType type { get; set; }
		public string token { get; set; }
		public GuildMember? member { get; set; }
		public User? user { get; set; }
		public string id { get; set; }
		public string? guild_id { get; set; }
		public InteractionData? data { get; set; }
		public string? channel_id { get; set; }
		public string application_id { get; set; }
		public Message? message { get; set; }

		public User GetUser()
		{
#pragma warning disable CS8603 // Possible null reference return.
			return member?.user ?? user;
#pragma warning restore CS8603 // Possible null reference return.
		}
	}


	public class InteractionData
	{
		public int type { get; set; }
		public ApplicationCommandInteractionDataOption[]? options { get; set; }
		public string name { get; set; }
		public string id { get; set; }
		public object? resolved { get; set; }
		public string? custom_id { get; set; }
		public int? component_type { get; set; }
		public SelectOption[]? values { get; set; }
		public string target_id { get; set; }
	}

	public class SelectOption
	{
		public string description { get; set; }
		public Emoji emoji { get; set; }
		public string label { get; set; }
		public string value { get; set; }
	}

	public class ApplicationCommandInteractionDataOption
	{
		public System.Text.Json.JsonElement? value { get; set; }
		public ApplicationCommandOptionType type { get; set; }
		public string name { get; set; }
	}

	public enum ApplicationCommandOptionType
	{
		SUB_COMMAND = 1,
		SUB_COMMAND_GROUP = 2,
		STRING = 3,
		INTEGER = 4,    //Any integer between -2^53 and 2^53
		BOOLEAN = 5,
		USER = 6,
		CHANNEL = 7,    //Includes all channel types + categories
		ROLE = 8,
		MENTIONABLE = 9,    //Includes users and roles
		NUMBER = 10,    //Any double between -2^53 and 2^53
	}

	public class InteractionResponse
	{
		public InteractionCallbackType type { get; set; }
		public InteractionCallbackData data { get; set; }
	}

	public class InteractionCallbackData
	{
		public bool? tts { get; set; }
		public string? content { get; set; }
		public Embed[]? embeds { get; set; }
		public AllowedMentions allowed_mentions { get; set; }
		public InteractionCallbackDataFlags? flags { get; set; }
		public Component[]? components { get; set; }
	}

	[Flags]
	public enum InteractionCallbackDataFlags
	{
		EPHEMERAL = 1 << 6 // only the user receiving the message can see it
	}

	public class Component
	{
	}

	public class AllowedMentions
	{
	}

	public class Embed { }

	public enum InteractionCallbackType
	{
		PONG = 1,   //ACK a Ping
		CHANNEL_MESSAGE_WITH_SOURCE = 4,    //respond to an interaction with a message
		DEFERRED_CHANNEL_MESSAGE_WITH_SOURCE = 5,   //ACK an interaction and edit a response later, the user sees a loading state
		DEFERRED_UPDATE_MESSAGE = 6,    //for components, ACK an interaction and edit the original message later; the user does not see a loading state
		UPDATE_MESSAGE = 7, //for components, edit the message the component was attached to
	}

	public class Hello
	{
		public int heartbeat_interval { get; set; }
	}
}