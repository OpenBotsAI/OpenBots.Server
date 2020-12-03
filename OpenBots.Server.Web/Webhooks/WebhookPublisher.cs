using OpenBots.Server.DataAccess.Repositories;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Model.Webhooks;
using Newtonsoft.Json;

namespace OpenBots.Server.Web.Webhooks
{
    public class WebhookPublisher : IWebhookPublisher
    {
        private readonly IIntegrationEventRepository eventRepository;
        private readonly IIntegrationEventLogRepository eventLogRepository;
        private readonly IIntegrationEventSubscriptionRepository eventSubscriptionRepository;
        private readonly IIntegrationEventSubscriptionAttemptRepository eventSubscriptionAttemptRepository;
        private readonly IWebhookSender webhookSender;

        public WebhookPublisher(
        IIntegrationEventRepository eventRepository,
        IIntegrationEventLogRepository eventLogRepository,
        IIntegrationEventSubscriptionRepository eventSubscriptionRepository,
        IIntegrationEventSubscriptionAttemptRepository eventSubscriptionAttemptRepository,
        IWebhookSender webhookSender)
        {
            this.eventRepository = eventRepository;
            this.eventLogRepository = eventLogRepository;
            this.eventSubscriptionRepository = eventSubscriptionRepository;
            this.eventSubscriptionAttemptRepository = eventSubscriptionAttemptRepository;
            this.webhookSender = webhookSender;
        }

        /// <summary>
        /// Publishes webhooks to all subscriptions
        /// </summary>
        /// <param name="integrationEventName"> Unique Name for integration event</param>
        /// <param name="entityId">Optional parameter that specifies the entity which was affected</param>
        /// <param name="entityName">Optional parameter that specifies the name of the affected entity</param>
        /// <returns></returns>
        private async Task PublishAsync(string integrationEventName, string entityId = "", string entityName = "")
        {
            //Get all subscriptions for the event.
            var eventSubscriptions = eventSubscriptionRepository.Find(0, 1).Items?.
                Where(s => s.IntegrationEventName == integrationEventName); 

            if (eventSubscriptions == null)
            {
                return;
            }

            //Get current Integration Event
            var integrationEvent = eventRepository.Find(0, 1).Items?.Where(e => e.Name == integrationEventName).FirstOrDefault();

            WebhookPayload payload = CreatePayload(integrationEvent, entityId, entityName);

            //Log Integration Event
            IntegrationEventLog eventLog = new IntegrationEventLog()
            {
                IntegrationEventName = integrationEventName,
                OccuredOnUTC = DateTime.Now,
                EntityType = integrationEvent.EntityType,
                EntityID = Guid.Parse(entityId),
                PayloadJSON = JsonConvert.SerializeObject(payload),
                CreatedOn = DateTime.UtcNow,
                Message = "",
                Status = "",
                SHA256Hash = ""
            };
            eventLogRepository.Add(eventLog);


            //TODO : filter out subscriptions which are only subscribed to specify IDs
            foreach (var eventSubscription in eventSubscriptions)
            {
                // Create new IntegrationEventSubscriptionAttempt
                //Log Integration Event
                IntegrationEventSubscriptionAttempt subscriptionAttempt = new IntegrationEventSubscriptionAttempt()
                {
                    EventLogID = eventLog.Id,
                    IntegrationEventName = eventSubscription.IntegrationEventName,
                    Status = eventLog.Status,
                    AttemptCounter = 0
                };
                eventSubscriptionAttemptRepository.Add(subscriptionAttempt);

                //create a background job to send the webhook
                BackgroundJob.Enqueue(() => SendWebhook(eventSubscription, payload, subscriptionAttempt.Id??Guid.Empty));
            }
        }


        private static WebhookPayload CreatePayload(IntegrationEvent integrationEvent, string entityId, string entityName)
        {
            //Create Payload object
            var newPayload = new WebhookPayload
            {
                EventId = integrationEvent.Id,
                EntityType = integrationEvent.EntityType,
                EventName = integrationEvent.Name,
                EntityID = Guid.Parse(entityId),
                EntityName = entityName,
                OccuredOnUTC = DateTime.Now,
                Message = "",
                Data = "",
                HMACKey = "",
                NONCE = "",
                SHA256Hash = "",
            };

            return newPayload;
        }

        private void SendWebhook(IntegrationEventSubscription eventSubscription, WebhookPayload payload, Guid attemptId)
        {
            var eventSubscriptionAttempt = eventSubscriptionAttemptRepository.GetOne(attemptId);
            var sendAttemptNumber = eventSubscriptionAttempt.AttemptCounter++;
            payload.AttemptCount = sendAttemptNumber;
            eventSubscriptionAttemptRepository.Update(eventSubscriptionAttempt);

            if (sendAttemptNumber > eventSubscription.HTTP_Max_RetryCount)
            {
                return;
            }

            try
            {
                webhookSender.SendWebhookAsync(payload, eventSubscription.HTTP_URL);
            }
            catch (Exception e)
            {
                throw; //Throws exception to re-try sending webhook. 
            }

            return;      
        }
    }
}
