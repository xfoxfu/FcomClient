using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FcomClient.Serialization.ApiObjects
{
	/// <summary>
	/// Represents a message forwarding request.
	/// </summary>
	class MessageForwardRequest
	{
		[JsonPropertyName("token")]
		public string Token { get; set; }

		[JsonPropertyName("discord_id")]
		public long DiscordId { get; set; }

		[JsonPropertyName("messages")]
		public List<TextMessage> Messages { get; set; }

		public MessageForwardRequest(string token, long discordId)
		{
			this.Token = token;
			this.DiscordId = discordId;
		}

		public void AppendMessage()
		{

		}
	}

	class TextMessage {

		[JsonPropertyName("timestamp")]
		public string Timestamp { get; set; }

		[JsonPropertyName("sender")]
		public string Sender { get; set; }

		[JsonPropertyName("recipient")]
		public string Recipient { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; }
	}

}
