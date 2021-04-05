using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Webhooks;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Business
{
    public class IntegrationEventManager : BaseManager, IIntegrationEventManager
    {
        private readonly IIntegrationEventRepository _integrationEventRepository;
        private readonly IWebhookPublisher _webhookPublisher;
        public IntegrationEventManager(
            IIntegrationEventRepository integrationEventRepository,
            IWebhookPublisher webhookPublisher)
        {
            _integrationEventRepository = integrationEventRepository;
            _webhookPublisher = webhookPublisher;
        }

        public IntegrationEvent UpdateBusinessEvent(string id, CreateBusinessEventViewModel request)
        {
            Guid entityId = new Guid(id);
            var existingEvent = _integrationEventRepository.GetOne(entityId);
            VerifyBusinessEvent(existingEvent);

            existingEvent.Name = request.Name;
            existingEvent.Description = request.Description;
            existingEvent.EntityType = request.EntityType;
            existingEvent.PayloadSchema = request.PayloadSchema;
            existingEvent.Name = request.Name;

            return existingEvent;

        }

        public void RaiseBusinessEvent(string id, RaiseBusinessEventViewModel request)
        {
            Guid entityId = new Guid(id);
            var existingEvent = _integrationEventRepository.GetOne(entityId);
            VerifyBusinessEvent(existingEvent);
            
            _webhookPublisher.PublishAsync(existingEvent.Name, request.EntityId.ToString(), request.EntityName).ConfigureAwait(false);

        }

        public void DeleteBusinessEvent(string id)
        {
            Guid entityId = new Guid(id);
            var existingEvent = _integrationEventRepository.GetOne(entityId);
            VerifyBusinessEvent(existingEvent);

            _integrationEventRepository.SoftDelete(entityId);
        }

        /// <summary>
        /// Verify that the IntegrationEvent is not a system event
        /// </summary>
        /// <param name="businessEvent">Event to be verified</param>
        private void VerifyBusinessEvent(IntegrationEvent businessEvent)
        {
            if (businessEvent == null) throw new EntityDoesNotExistException($"IntegrationEvent with id {businessEvent.Id} could not be found");

            if (businessEvent.IsSystem == true) throw new UnauthorizedOperationException($"System events can't be updated", EntityOperationType.Update);
        }
    }
}
