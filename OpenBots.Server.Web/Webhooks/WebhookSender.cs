using Newtonsoft.Json;
using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Webhooks
{
    /// <summary>
    /// Uses a new HttpClient to send integration events to subscrubed URLs
    /// </summary>
    public class WebhookSender : IWebhookSender
    {
        public WebhookSender()
        {
        }

        public async Task<(bool isSucceed, HttpStatusCode statusCode, string content)> SendWebhookAsync(WebhookPayload payload, string url)
        {
            
            string payloadString = JsonConvert.SerializeObject(payload);
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "http://yourUrl",
                     new StringContent(payloadString, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                var isSucceed = response.IsSuccessStatusCode;
                var statusCode = response.StatusCode;
                var content = await response.Content.ReadAsStringAsync();

                return (isSucceed, statusCode, content);
            }           
        }
    }
}
