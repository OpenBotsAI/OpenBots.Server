using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Takes a business event and returns it for addition
        /// </summary>
        /// <param name="createBusinessEventView"></param>
        /// <returns>The business event to be added</returns>
        public IntegrationEvent AddBusinessEvent(CreateBusinessEventViewModel createBusinessEventView)
        {
            var existingEvent = _integrationEventRepository.Find(null, d => d.Name.ToLower() == createBusinessEventView.Name.ToLower())?.Items?.FirstOrDefault();
            if (existingEvent != null)
            {
                throw new EntityAlreadyExistsException("Business event already exists");
            }
            IntegrationEvent newEvent = createBusinessEventView.Map(createBusinessEventView); //assign request to schedule entity

            return newEvent;
        }

        /// <summary>
        /// Updates a business event entity 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public IntegrationEvent UpdateBusinessEvent(string id, CreateBusinessEventViewModel request)
        {
            Guid entityId = new Guid(id);

            var businessEvent = _integrationEventRepository.GetOne(entityId);
            if (businessEvent == null)
            {
                throw new EntityDoesNotExistException("No business event exists for the specified id");
            }

            var namedIntegrationEvent = _integrationEventRepository.Find(null, d => d.Name.ToLower() == request.Name.ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
            if (namedIntegrationEvent != null && namedIntegrationEvent.Id != entityId)
            {
                throw new EntityAlreadyExistsException("Business event already exists");
            }

            businessEvent.Name = request.Name;
            businessEvent.Description = request.Description;
            businessEvent.EntityType = request.EntityType;
            businessEvent.PayloadSchema = request.PayloadSchema;
            businessEvent.Name = request.Name;

            return businessEvent;
        }

        /// <summary>
        /// Verifies that the patch update can be completed
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        public void AttemptPatchUpdate(JsonPatchDocument<IntegrationEvent> request, string id)
        {
            for (int i = 0; i < request.Operations.Count; i++)
            {
                Guid entityId = new Guid(id);

                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    var namedIntegrationEvent = _integrationEventRepository.Find(null, d => d.Name.ToLower() == request.Operations[i].value.ToString().ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
                    if (namedIntegrationEvent != null && namedIntegrationEvent.Id != entityId)
                    {
                        throw new EntityAlreadyExistsException("Business event name already exists");
                    }
                }
            }
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
