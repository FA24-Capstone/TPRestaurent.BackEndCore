using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/discord")]
    [ApiController]
    public class ExpoBuildNotificationController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        private const string DiscordWebhookUrl =
            "https://discord.com/api/webhooks/1290961564211347466/TYe7ldnQh8R6NJ5VXoyjEbsZ-0s49vjIs_1T6W3kmBGR-oyvMN-MdY8kAi0sjNWMJTZI";

        public ExpoBuildNotificationController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveBuildNotification([FromBody] ExpoBuildPayload payload)
        {
            if (payload?.Status == "finished" && !string.IsNullOrEmpty(payload.Artifacts?.BuildUrl))
            {
                var embedMessage = new
                {
                    username = "Expo Build Bot",
                    avatar_url = "https://path/to/icon.png",
                    embeds = new[]
                    {
                        new
                        {
                            title = "New Expo Build Successful 🎉",
                            description = "Your Expo build has completed successfully!",
                            color = 5814783,
                            fields = new[]
                            {
                                new
                                {
                                    name = "APK Download URL",
                                    value = $"[Click here to download the APK]({payload.Artifacts.BuildUrl})"
                                }
                            },
                            footer = new
                            {
                                text = "Expo Build Bot",
                                icon_url = "https://path/to/icon.png"
                            },
                            timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                };

                try
                {
                    // Send embed message to Discord webhook
                    var response = await _httpClient.PostAsJsonAsync(DiscordWebhookUrl, embedMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok("Notification sent to Discord");
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, "Failed to send message to Discord");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return BadRequest("Build failed or incomplete data");
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
    }



}
