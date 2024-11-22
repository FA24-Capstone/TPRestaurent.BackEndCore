using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/discord")]
    [ApiController]
    public class ExpoBuildNotificationController : ControllerBase
    {
        private const string DiscordWebhookShipperUrl =
            "https://discord.com/api/webhooks/1303209823344787477/0kIOpRLacybvRrE9v3bHs9Sj6xS7TaxDWVGeXnA-HuzLuKCYenZZZLvmrx5gzneVHJZt";

        private const string DiscordWebhookTabletUrl = "https://discord.com/api/webhooks/1303209503046500384/aXlJzYw37BO9hkSl2NnD64UjCOU6mkO0OE9vbo7Bv61rgfPqhl_WqjoZsmu_Kva2svPV";

        [HttpPost("shipper")]
        public async Task<IActionResult> ReceiveBuildNotification([FromBody] ExpoBuildPayload payload)
        {
            if (payload?.Status != "finished" || string.IsNullOrEmpty(payload.Artifacts?.BuildUrl))
            {
                return BadRequest("Build failed or incomplete data");
            }

            var embedMessage = CreateEmbedMessage(payload.Artifacts.BuildUrl, 0);

            return await SendDiscordNotification(embedMessage, 0);
        }

        [HttpPost("tablet")]
        public async Task<IActionResult> ReceiveBuildNotificationTablet([FromBody] ExpoBuildPayload payload)
        {
            if (payload?.Status != "finished" || string.IsNullOrEmpty(payload.Artifacts?.BuildUrl))
            {
                return BadRequest("Build failed or incomplete data");
            }

            var embedMessage = CreateEmbedMessage(payload.Artifacts.BuildUrl, 1);

            return await SendDiscordNotification(embedMessage, 1);
        }

        private object CreateEmbedMessage(string buildUrl, int type)
        {
            string buildTypeMessage = type == 0 ? "shipper" : "tablet";

            // Construct the embed message
            var webhookMessage = new ExpoBuildPayload.WebhookMessage
            {
                Content = "Hello, this is a message from my webhook!",
                Embeds = new List<ExpoBuildPayload.Embed>
                {
                    new ExpoBuildPayload.Embed
                    {
                        Title = $"New Build Released for {buildTypeMessage} 🎉", // Title
                        Description = $"A new build for the **{buildTypeMessage}** has been successfully released. Please find the download link below.", // Description
                        Url =buildUrl,
                        Color = 7506394,
                        Fields = new List<ExpoBuildPayload.Field>
                        {
                            new ExpoBuildPayload.Field
                            {
                                Name = "Download Link", // Field name
                                Value = $"[Download the APK here]({buildUrl})", // Field value with a link
                                Inline = true
                            },
                        },
                    }
                }
            };

            return webhookMessage; // Return only the embed object wrapped in a valid structure
        }

        private async Task<IActionResult> SendDiscordNotification(object embedMessage, int type)
        {
            var client = new RestClient();
            var request = new RestRequest(type == 0 ? DiscordWebhookShipperUrl : DiscordWebhookTabletUrl);
            request.Method = Method.Post;

            // Properly serialize the embedMessage to JSON
            request.AddJsonBody(embedMessage);
            try
            {
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return Ok("Notification sent to Discord");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // Model for deserializing the payload from Expo
    public class ExpoBuildPayload
    {
        public string Status { get; set; }
        public Artifact Artifacts { get; set; }

        public class Artifact
        {
            public string BuildUrl { get; set; }
        }

        public class WebhookMessage
        {
            public string Content { get; set; }
            public string Username { get; set; }
            public List<Embed> Embeds { get; set; }
        }

        public class Embed
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public int Color { get; set; }
            public List<Field> Fields { get; set; }
            public Footer Footer { get; set; }
        }

        public class Field
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Inline { get; set; }
        }

        public class Footer
        {
            public string Text { get; set; }
        }
    }
}