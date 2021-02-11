using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBots.Server.Business
{
    public class IntegrationEventSubscriptionAttemptManager: BaseManager, IIntegrationEventSubscriptionAttemptManager
    {
        private readonly IIntegrationEventSubscriptionAttemptRepository _repo;
        private readonly IIntegrationEventSubscriptionRepository _subscriptionRepository;
        public IntegrationEventSubscriptionAttemptManager(IIntegrationEventSubscriptionAttemptRepository repo,
            IIntegrationEventSubscriptionRepository subscriptionRepository)
        {
            _repo = repo;
            _subscriptionRepository = subscriptionRepository;
        }

        public int? SaveAndGetAttemptCount(IntegrationEventSubscriptionAttempt currentAttempt, int? maxRetryCount)
        {
            int? attemptCount = 0;
            var previousAttempt = GetLastAttempt(currentAttempt);

            //if no attempt exists, then this is the first attempt
            if (previousAttempt == null)
            {
                attemptCount = 1;
            }
            else
            {
                previousAttempt.Status = "Failed";
                attemptCount = previousAttempt.AttemptCounter;
                attemptCount++;
                _repo.Update(previousAttempt);
            }
            currentAttempt.AttemptCounter = attemptCount;
            currentAttempt.CreatedOn = DateTime.UtcNow;
            currentAttempt.Id = Guid.NewGuid();
            _repo.Add(currentAttempt);
            return attemptCount;
        }

        public IntegrationEventSubscriptionAttempt GetLastAttempt(IntegrationEventSubscriptionAttempt subscriptionAttempt)
        {
            var result = _repo.Find(0, 1).Items?
                .Where(a => a.EventLogID == subscriptionAttempt.EventLogID
           && a.IntegrationEventSubscriptionID == subscriptionAttempt.IntegrationEventSubscriptionID)?
                .OrderByDescending(a => a.AttemptCounter)
                .FirstOrDefault();

            return result;
        }
            public SubscriptionAttemptViewModel GetAttemptView(SubscriptionAttemptViewModel subscriptionAttempt)
        {
            subscriptionAttempt.TransportType = _subscriptionRepository.GetOne(subscriptionAttempt.IntegrationEventSubscriptionID ?? Guid.Empty)?.TransportType.ToString();

            return subscriptionAttempt;
        }
    }
}
