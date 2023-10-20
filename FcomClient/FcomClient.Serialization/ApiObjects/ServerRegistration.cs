using System.Text.Json.Serialization;

namespace FcomClient.Serialization.ApiObjects
{
	/// <summary>
	/// Represents API response containing the server registration.
	/// </summary>
	class ServerRegistrationResponse
	{
		[JsonPropertyName("token")]
		public string Token { get; set; }

		[JsonPropertyName("callsign")]
		public string Callsign { get; set; }

		[JsonPropertyName("discord_id")]
		public long DiscordId { get; set; }

		[JsonPropertyName("discord_name")]
		public string DiscordName { get; set; }

	}
}
