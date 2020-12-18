using OpenBots.Server.DataAccess.Repositories;
using Hangfire;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Model.Webhooks;
using Newtonsoft.Json;
using TransportType = OpenBots.Server.Model.Webhooks.TransportType;
using OpenBots.Server.Model;
using OpenBots.Server.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace OpenBots.Server.Web.Webhooks
{
    public class WebhookPublisher : IWebhookPublisher
    {
        private readonly IIntegrationEventRepository eventRepository;
        private readonly IIntegrationEventLogRepository eventLogRepository;
        private readonly IIntegrationEventSubscriptionRepository eventSubscriptionRepository;
        private readonly IIntegrationEventSubscriptionAttemptRepository attemptRepository;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IQueueItemRepository queueItemRepository;
        private IHubContext<NotificationHub> _hub;

        public WebhookPublisher(
        IIntegrationEventRepository eventRepository,
        IIntegrationEventLogRepository eventLogRepository,
        IIntegrationEventSubscriptionRepository eventSubscriptionRepository,
        IIntegrationEventSubscriptionAttemptRepository integrationEventSubscriptionAttemptRepository,
        IBackgroundJobClient backgroundJobClient,
        IQueueItemRepository queueItemRepository,
        IHubContext<NotificationHub> hub)
        {
            this.eventRepository = eventRepository;
            this.eventLogRepository = eventLogRepository;
            this.eventSubscriptionRepository = eventSubscriptionRepository;
            this.backgroundJobClient = backgroundJobClient;
            this.queueItemRepository = queueItemRepository;
            attemptRepository = integrationEventSubscriptionAttemptRepository;
            _hub = hub;
        }

        /// <summary>
        /// Publishes Webhooks to all subscriptions
        /// </summary>
        /// <param name="integrationEventName"> Unique Name for integration event</param>
        /// <param name="entityId">Optional parameter that specifies the entity which was affected</param>
        /// <param name="entityName">Optional parameter that specifies the name of the affected entity</param>
        /// <returns></returns>
        public async Task PublishAsync(string integrationEventName, string entityId = "", string entityName = "")
        {
            //Get all subscriptions for the event.
            var eventSubscriptions = eventSubscriptionRepository.Find(0, 1).Items?.
                Where(s => s.IntegrationEventName == integrationEventName || s.EntityID == Guid.Parse(entityId)); 

            if (eventSubscriptions == null)
            {
                return;
            }

            //Get current Integration Event
            var integrationEvent = eventRepository.Find(0, 1).Items?.Where(e => e.Name == integrationEventName).FirstOrDefault();

            if (integrationEvent == null) return;
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
            eventLog = eventLogRepository.Add(eventLog);


            // Get subscriptions that must receive webhook
            foreach (var eventSubscription in eventSubscriptions)
            {
                //Handle subscriptions that should not get notified
                if (!((eventSubscription.IntegrationEventName == integrationEventName || eventSubscription.IntegrationEventName == null)
                    && (eventSubscription.EntityID == new Guid(entityId) || eventSubscription.EntityID == null)))
                {
                    continue; //Do not create an attempt in this case
                }

                // Create new IntegrationEventSubscriptionAttempt
                IntegrationEventSubscriptionAttempt subscriptionAttempt = new IntegrationEventSubscriptionAttempt()
                {
                    EventLogID = eventLog.Id,
                    IntegrationEventName = eventSubscription.IntegrationEventName,
                    IntegrationEventSubscriptionID = eventSubscription.Id,
                    Status = "InProgress",
                    AttemptCounter = 0,
                    CreatedOn = DateTime.UtcNow,
                };

                switch (eventSubscription.TransportType)
                {
                    case TransportType.HTTPS:
                        //create a background job to send the webhook
                        backgroundJobClient.Enqueue<WebhookSender>(x => x.SendWebhook(eventSubscription, payload, subscriptionAttempt));
                        break;
                    case TransportType.Queue:
                        QueueItemModel queueItem = new QueueItemModel
                        {
                            Name = eventSubscription.Name,
                            IsLocked = false,
                            QueueId = eventSubscription.QUEUE_QueueID ?? Guid.Empty,
                            Type = "Json",
                            JsonType = "IntegrationEvent",
                            DataJson = JsonConvert.SerializeObject(payload),
                            State = "New",
                            RetryCount = eventSubscription.HTTP_Max_RetryCount ?? 1,
                            Source = eventSubscription.IntegrationEventName,
                            Event = integrationEvent.Description,
                            CreatedOn = DateTime.UtcNow,                        
                        };
                        queueItemRepository.Add(queueItem);
                        subscriptionAttempt.AttemptCounter = 1;
                        attemptRepository.Add(subscriptionAttempt);
                        break;
                    case TransportType.SignalR:
                        await _hub.Clients.All.SendAsync(integrationEventName, JsonConvert.SerializeObject(payload)).ConfigureAwait(false);
                        subscriptionAttempt.AttemptCounter = 1;
                        attemptRepository.Add(subscriptionAttempt);
                        break;
                }
            }

            return;
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
            };

            return newPayload;
        }        
    }
}
