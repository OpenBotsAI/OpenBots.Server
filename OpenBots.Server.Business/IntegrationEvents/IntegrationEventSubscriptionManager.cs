using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBots.Server.Business
{
    public class IntegrationEventSubscriptionManager : BaseManager, IIntegrationEventSubscriptionManager
    {
        private readonly IIntegrationEventSubscriptionRepository _repo;
        public IntegrationEventSubscriptionManager(IIntegrationEventSubscriptionRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Takes an IntegrationEventSubscription and returns it for addition
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns>The IntegrationEventSubscription to be added</returns>
        public IntegrationEventSubscription AddIntegrationEventSubscription(IntegrationEventSubscription eventSubscription)
        {
            var existingIntegrationEventSubscription = _repo.Find(null, d => d.Name.ToLower() == eventSubscription.Name.ToLower())?.Items?.FirstOrDefault();
            if (existingIntegrationEventSubscription != null)
            {
                throw new EntityAlreadyExistsException("IntegrationEventSubscription name already exists");
            }

            return eventSubscription;
        }

        /// <summary>
        /// Updates an IntegrationEventSubscription entity 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public IntegrationEventSubscription UpdateIntegrationEventSubscription(string id, IntegrationEventSubscription request)
        {
            Guid entityId = new Guid(id);

            var existingEventSubscription = _repo.GetOne(entityId);
            if (existingEventSubscription == null)
            {
                throw new EntityDoesNotExistException("No IntegrationEventSubscription exists for the specified id");
            }

            var namedIntegrationEventSubscription = _repo.Find(null, d => d.Name.ToLower() == request.Name.ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
            if (namedIntegrationEventSubscription != null && namedIntegrationEventSubscription.Id != entityId)
            {
                throw new EntityAlreadyExistsException("IntegrationEventSubscription already exists");
            }

            existingEventSubscription.Name = request.Name;
            existingEventSubscription.EntityType = request.EntityType;
            existingEventSubscription.IntegrationEventName = request.IntegrationEventName;
            existingEventSubscription.EntityID = request.EntityID;
            existingEventSubscription.EntityName = request.EntityName;
            existingEventSubscription.TransportType = request.TransportType;
            existingEventSubscription.HTTP_URL = request.HTTP_URL;
            existingEventSubscription.HTTP_AddHeader_Key = request.HTTP_AddHeader_Key;
            existingEventSubscription.HTTP_AddHeader_Value = request.HTTP_AddHeader_Value;
            existingEventSubscription.Max_RetryCount = request.Max_RetryCount;
            existingEventSubscription.QUEUE_QueueID = request.QUEUE_QueueID;

            return existingEventSubscription;
        }

        /// <summary>
        /// Verifies that the patch update can be completed
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        public void AttemptPatchUpdate(JsonPatchDocument<IntegrationEventSubscription> request, string id)
        {
            for (int i = 0; i < request.Operations.Count; i++)
            {
                Guid entityId = new Guid(id);

                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    var namedIntegrationEventSubscription = _repo.Find(null, d => d.Name.ToLower() == request.Operations[i].value.ToString().ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
                    if (namedIntegrationEventSubscription != null && namedIntegrationEventSubscription.Id != entityId)
                    {
                        throw new EntityAlreadyExistsException("IntegrationEventSubscription name already exists");
                    }
                }
            }
        }
    }
}
