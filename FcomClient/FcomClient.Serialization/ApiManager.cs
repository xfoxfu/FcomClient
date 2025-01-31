﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using FcomClient.Serialization.ApiObjects;
using RestSharp;

namespace FcomClient.Serialization
{
	/// <summary>
	/// A wrapper for the FCOM server API.
	/// </summary>
	class ApiManager
	{
		/// <summary>
		/// User agent string to use.
		/// </summary>
		private readonly string CLIENT_VERSION = "FcomClient/0.9.2";
		
		/// <summary>
		/// Server address, read from the file server_location.txt
		/// </summary>
		public readonly string SERVER_ADDRESS = System.IO.File.ReadAllText("server_location.txt");

		/// <summary>
		/// API endpoint for submitting messages to be forwarded.
		/// </summary>
		private readonly string MESSAGE_FORWARDING_ENDPOINT = "/api/v1/messaging";

		/// <summary>
		/// API endpoint for registering/confirming a token.
		/// </summary>
		public readonly string REGISTRATION_ENDPOINT = "/api/v1/register";

		/// <summary>
		/// API registration token, provided by the bot and entered into FcomClient by the user.
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// The online callsign the user is logged in as.
		/// </summary>
		public string Callsign;

		/// <summary>
		/// ID number of the Discord user associated with the token.
		/// </summary>
		public long DiscordId { get; }

		/// <summary>
		/// Display name of the Discord user associated with the token.
		/// </summary>
		public string DiscordName { get; }

		/// <summary>
		/// Internal object for interacting with the FCOM server API.
		/// </summary>
		private RestClient client;

		/// <summary>
		/// Whether the callsign/token associated with the ApiManager object is registered.
		/// </summary>
		public bool IsRegistered { get; }

		/// <summary>
		/// Initializes the API manager, 
		/// passing the given token to the server for confirmation.
		/// </summary>
		public ApiManager(string token, string callsign)
		{
			client = new RestClient(new RestClientOptions
			{
				BaseUrl = new Uri(SERVER_ADDRESS),
				UserAgent = CLIENT_VERSION,
			});
			RestRequest registerRequest 
				= new RestRequest(REGISTRATION_ENDPOINT, Method.Get);

			registerRequest.AddParameter("token", token);
			registerRequest.AddParameter("callsign", callsign);

			registerRequest.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

			var response = client.Execute<ServerRegistrationResponse>(registerRequest);

			if (response.IsSuccessful)
			{
				// var deserializer = new JsonDeserializer();
				ServerRegistrationResponse returnPayload = response.Data; // JsonSerializer.Deserialize<ServerRegistrationResponse>(response);

				this.Callsign = returnPayload.Callsign;
				this.DiscordId = returnPayload.DiscordId;
				this.DiscordName = returnPayload.DiscordName;
				this.Token = returnPayload.Token;

				if (this.DiscordId == 0)
					this.IsRegistered = false;
				else
					this.IsRegistered = true;
			}
			else
			{
				// do nothing
			}

		}

		/// <summary>
		/// Forwards the given FSD message to the FCOM server.
		/// </summary>
		/// <param name="pm">Message to forward.</param>
		public void ForwardMessage(FsdObject.FsdMessage pm)
		{
			// convert timestamp to Unix time
			DateTimeOffset dateTimeOffset = new DateTimeOffset(pm.Timestamp);
			long unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();

			/* Sample JSON (after server rewrite):
			   NOTE: currently, the API does not parse beyond the first message.
			{
				"token": 	"Wjve5p45aTojv6yzRr72FKs9K1py8ze2auFbB8g328o",
				"messages":	[
					{
						"timestamp": "2018-05-01 10:40:00 PM",
						"sender": "XX_SUP",
						"receiver": "DAL1107",
						"message": "IMMA SWING ZE MIGHTY BAN-HAMMER MUAHAHAHAHA"
					}
				]
			}
			 */

			Console.Write(" -- Forwarding to {0}...", SERVER_ADDRESS);

			// Build the API request
			var request = new RestRequest(MESSAGE_FORWARDING_ENDPOINT, Method.Post)
			{
				RequestFormat = DataFormat.Json
			};

			// Build the JSON object
			var messagePayload = new Message
			{
				timestamp = unixTimestamp.ToString(),
				sender = pm.Sender,
				receiver = pm.Recipient,
				message = pm.Message,
			};

			/* The API accepts a list of messages, but currently only processes the first in the list*/
			var messageList = new List<Message>
			{
				messagePayload
			};

			request.AddBody(new ForwardedMessage
			{
				token = this.Token,
				messages = messageList
			});

			var response = client.Execute(request);
			var content = response.Content;

			if (response.IsSuccessful)
			{
			}
			else
			{
				throw new FcomApiException(String.Format("[Message forwarding error] {0}", response.Content));
			}

		}
	}
}
