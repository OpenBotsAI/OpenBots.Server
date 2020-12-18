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
        private readonly IIntegrationEventSubscriptionAttemptRepository repo;
        private readonly IIntegrationEventSubscriptionRepository subscriptionRepository;
        public IntegrationEventSubscriptionAttemptManager(IIntegrationEventSubscriptionAttemptRepository repo,
            IIntegrationEventSubscriptionRepository subscriptionRepository)
        {
            this.repo = repo;
            this.subscriptionRepository = subscriptionRepository;
        }

        public int? SaveAndGetAttemptCount(IntegrationEventSubscriptionAttempt currentAttempt, int? maxRetryCount)
        {
            int? attemptCount = 0;
            var previousAttempt = GetLastAttempt(currentAttempt);

            //If no attempt exists, then this is the first attempt
            if (previousAttempt == null)
            {
                attemptCount = 1;
            }
            else
            {
                previousAttempt.Status = "Failed";
                attemptCount = previousAttempt.AttemptCounter;
                attemptCount++;
                repo.Update(previousAttempt);
            }
            currentAttempt.AttemptCounter = attemptCount;
            currentAttempt.CreatedOn = DateTime.Now;
            currentAttempt.Id = Guid.NewGuid();
            repo.Add(currentAttempt);
            return attemptCount;
        }

        public IntegrationEventSubscriptionAttempt GetLastAttempt(IntegrationEventSubscriptionAttempt subscriptionAttempt)
        {
            var result = repo.Find(0, 1).Items?
                .Where(a => a.EventLogID == subscriptionAttempt.EventLogID
           && a.IntegrationEventSubscriptionID == subscriptionAttempt.IntegrationEventSubscriptionID)?
                .OrderByDescending(a => a.AttemptCounter)
                .FirstOrDefault();

            return result;
        }
            public SubscriptionAttemptViewModel GetAttemptView(SubscriptionAttemptViewModel subscriptionAttempt)
        {
            subscriptionAttempt.TransportType = subscriptionRepository.GetOne(subscriptionAttempt.IntegrationEventSubscriptionID ?? Guid.Empty)?.TransportType.ToString();

            return subscriptionAttempt;
        }
    }
}
