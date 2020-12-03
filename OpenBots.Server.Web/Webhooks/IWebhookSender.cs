using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Webhooks
{
    public interface IWebhookSender
    {
        Task<(bool isSucceed, HttpStatusCode statusCode, string content)> SendWebhookAsync(WebhookPayload payload, string url);

    }
}
