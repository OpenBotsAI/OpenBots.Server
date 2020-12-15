﻿using Hangfire;
using Newtonsoft.Json;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Webhooks;
using System;
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
        private readonly IIntegrationEventSubscriptionAttemptManager attemptManager;
        private readonly IIntegrationEventSubscriptionAttemptRepository attemptRepository;

        public WebhookSender(IIntegrationEventSubscriptionAttemptManager eventSubscriptionAttemptManager,
            IIntegrationEventSubscriptionAttemptRepository attemptRepository)
        {
            this.attemptManager = eventSubscriptionAttemptManager;
            this.attemptRepository = attemptRepository;
        }

        public async Task SendWebhook(IntegrationEventSubscription eventSubscription, WebhookPayload payload,
            IntegrationEventSubscriptionAttempt subscriptionAttempt)
        {
            var attemptCount = attemptManager.SaveAndGetAttemptCount(subscriptionAttempt, eventSubscription.HTTP_Max_RetryCount);
            payload.AttemptCount = attemptCount;

            if (attemptCount > eventSubscription.HTTP_Max_RetryCount)
            {
                return;
            }

            bool isSuccessful;
            try
            {
                isSuccessful = await SendWebhookAsync(payload, eventSubscription.HTTP_URL);
            }
            catch (Exception exception)// an internal error occurred. 
            {
                throw exception;
            }

            if (!isSuccessful)
            {
                throw new Exception($"Webhook sending attempt failed.");
            }
            else
            {
                var existingAttempt = attemptManager.GetLastAttempt(subscriptionAttempt);
                existingAttempt.Status = "Completed";
                attemptRepository.Update(existingAttempt);
            }

            return;
        }

        public async Task<bool> SendWebhookAsync(WebhookPayload payload, string url)
        {          
            string payloadString = JsonConvert.SerializeObject(payload);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            string myJson = payloadString;
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    url,
                     new StringContent(myJson, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
        }
    }
}
